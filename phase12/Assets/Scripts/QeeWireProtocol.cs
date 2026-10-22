// QUEST-EAGLEEYE Phase 8. Synthetic QEE1 data ONLY; no TC001 or Quest hardware verification.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace QuestEagleEye {
    [Serializable]
    public sealed class QeeMetadata {
        public int schema_version;
        public string source;
        public long frame_id;
        public int width;
        public int height;
        public string pixel_format;
        public string capture_clock_domain;
        public long capture_mono_ns;
        public bool synthetic;
        public string sha256;
    }
    public sealed class QeeFrame {
        public readonly QeeMetadata Metadata;
        public readonly byte[] Pixels;
        public QeeFrame(QeeMetadata metadata, byte[] pixels) { Metadata = metadata; Pixels = pixels; }
    }
    public static class QeeWireProtocol {
        public const int MaxMetadata = 4096;
        public const int MaxData = 1048576;
        public const int MaxSide = 1024;
        private static readonly HashSet<string> Required = new HashSet<string> {
            "schema_version", "source", "frame_id", "width", "height", "pixel_format",
            "capture_clock_domain", "capture_mono_ns", "synthetic", "sha256"
        };
        static byte[] ReadExactly(Stream stream, int count) {
            var result = new byte[count]; int offset = 0;
            while (offset < count) {
                int n = stream.Read(result, offset, count - offset);
                if (n <= 0) throw new EndOfStreamException("Truncated QEE1 frame");
                offset += n;
            }
            return result;
        }
        static uint ReadUInt32BE(byte[] bytes, int start) {
            return ((uint)bytes[start] << 24) | ((uint)bytes[start+1] << 16) |
                   ((uint)bytes[start+2] << 8) | bytes[start+3];
        }
        public static QeeFrame ReadFrame(Stream stream) {
            var header = ReadExactly(stream, 12);
            if (header[0] != (byte)'Q' || header[1] != (byte)'E' || header[2] != (byte)'E' || header[3] != (byte)'1')
                throw new InvalidDataException("QEE1 magic mismatch");
            uint mlen = ReadUInt32BE(header, 4), plen = ReadUInt32BE(header, 8);
            if (mlen == 0 || mlen > MaxMetadata || plen == 0 || plen > MaxData)
                throw new InvalidDataException("QEE1 length out of bounds");
            byte[] rawMetadata = ReadExactly(stream, checked((int)mlen));
            // ASCII JSON only, per Phase 7. Reject bytes outside ASCII (no silent substitution).
            foreach (byte b in rawMetadata) if (b > 127) throw new InvalidDataException("Non-ASCII metadata");
            string json = Encoding.ASCII.GetString(rawMetadata);
            // JsonUtility ignores extra fields. Verify top-level property set before deserializing.
            // Unity JsonUtility is used only for parsing known fields; this scanner handles strings and nested values.
            var names = ExtractTopLevelNames(json);
            if (!names.SetEquals(Required)) throw new InvalidDataException("Metadata fields mismatch");
            var meta = JsonUtility.FromJson<QeeMetadata>(json);
            if (meta == null || meta.schema_version != 1 || meta.source != "synthetic-fixture" || !meta.synthetic ||
                meta.frame_id < 0 || meta.capture_mono_ns < 0 ||
                string.IsNullOrWhiteSpace(meta.capture_clock_domain) || meta.capture_clock_domain.Length > 80 ||
                meta.width < 1 || meta.height < 1 || meta.width > MaxSide || meta.height > MaxSide ||
                meta.pixel_format != "uint16-le" || meta.sha256 == null || meta.sha256.Length != 64 || !IsLowerHex(meta.sha256))
                throw new InvalidDataException("Invalid or non-synthetic metadata");
            if ((long)meta.width * meta.height * 2 != plen) throw new InvalidDataException("Pixel dimensions mismatch");
            byte[] pixels = ReadExactly(stream, checked((int)plen));
            byte[] digest;
            using (var sha = SHA256.Create()) digest = sha.ComputeHash(pixels);
            var hex = BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant();
            if (!string.Equals(hex, meta.sha256, StringComparison.Ordinal))
                throw new InvalidDataException("QEE1 checksum mismatch");
            return new QeeFrame(meta, pixels);
        }
        static bool IsLowerHex(string value) {
            foreach (char c in value) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
            return true;
        }
        // Strict flat JSON token and type validation. No nested values, duplicate keys or coercions.
        static HashSet<string> ExtractTopLevelNames(string json) {
            var names = new HashSet<string>(StringComparer.Ordinal);
            int i = 0;
            Skip(); Expect('{'); Skip();
            if (i < json.Length && json[i] == '}') throw new InvalidDataException("Empty metadata");
            while (true) {
                Skip(); string key = ReadString();
                if (!names.Add(key) || !Required.Contains(key)) throw new InvalidDataException("Unknown or duplicate key");
                Skip(); Expect(':'); Skip();
                bool expectsString = key == "source" || key == "pixel_format" || key == "capture_clock_domain" || key == "sha256";
                bool expectsBoolean = key == "synthetic";
                if (expectsString) { ReadString(); }
                else if (expectsBoolean) {
                    if (Match("true")) { } else if (Match("false")) { }
                    else throw new InvalidDataException("Expected JSON boolean");
                } else {
                    int start = i;
                    if (i < json.Length && json[i] == '-') i++;
                    if (i >= json.Length || json[i] < '0' || json[i] > '9') throw new InvalidDataException("Expected integer");
                    if (json[i] == '0') i++;
                    else while (i < json.Length && json[i] >= '0' && json[i] <= '9') i++;
                    if (i < json.Length && (json[i] == '.' || json[i] == 'e' || json[i] == 'E')) throw new InvalidDataException("Integer expected");
                    if (!long.TryParse(json.Substring(start,i-start), out _)) throw new InvalidDataException("Integer overflow");
                }
                Skip();
                if (i < json.Length && json[i] == '}') { i++; break; }
                Expect(',');
                Skip();
                if (i < json.Length && json[i] == '}') throw new InvalidDataException("Trailing comma");
            }
            Skip(); if (i != json.Length) throw new InvalidDataException("Trailing JSON");
            return names;
            void Skip() { while (i < json.Length && (json[i] == ' ' || json[i] == '\t' || json[i] == '\r' || json[i] == '\n')) i++; }
            void Expect(char c) { if (i >= json.Length || json[i++] != c) throw new InvalidDataException("Invalid JSON punctuation"); }
            bool Match(string token) {
                if (i + token.Length > json.Length || string.CompareOrdinal(json,i,token,0,token.Length) != 0) return false;
                i += token.Length; return true;
            }
            string ReadString() {
                Expect('"'); var b = new StringBuilder();
                while (i < json.Length) {
                    char c = json[i++]; if (c == '"') return b.ToString();
                    if (c == '\\') {
                        if (i >= json.Length) throw new InvalidDataException("Invalid escape");
                        char e = json[i++];
                        if (e == '"' || e == '\\' || e == '/') b.Append(e);
                        else throw new InvalidDataException("Unsupported escape");
                    } else { if (c < 32 || c > 127) throw new InvalidDataException("Invalid character"); b.Append(c); }
                }
                throw new InvalidDataException("Unterminated string");
            }
        }
    }
}