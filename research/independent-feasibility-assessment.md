# QUEST-EAGLEEYE — Independent Phase 0 and engineering investigation

**Assessment date:** 2026-10-21  
**Source revision:** `261c599118c0e6bea03e0ad5cffb581265504223` (`main`)  
**Author of original concept:** Joseph Ribal Nasr  
**Assessment status:** DEVELOP FURTHER / NOT READY FOR PUBLICATION  
**Nature of this file:** Technical investigation and proposed validation protocol, not measured experimental results.

## Research qualification

The tracked repository at the inspected revision contains five files: `Readme.md`, `index.html`, `research.html`, `CITATION.cff`, and `sitemap.xml`. Its code examples are embedded in the HTML document; the repository contains no integrated Unity application, Android thermal capture implementation, compiled headset build, calibrated sensor dataset, Meshtastic gateway implementation, or experimental logs. The work is a **conceptual systems-integration and feasibility study**, not an empirical prototype evaluation, and "preprint" would describe a dissemination stage rather than evidence quality.

**Originality:** limited/not established. Consumer passthrough MR, USB thermal imagers, LoRa/Meshtastic messaging, and sensor registration are existing technologies; a combination of these parts is not by itself scientific novelty. A reproducible interface-contract comparison, calibration/error budget, or negative compatibility result could become a reusable contribution.

**Beneficiaries:** XR systems engineers, HCI researchers, sensor-integration developers and technical educators.  
**Publication value:** a precise, reproducible engineering report could help others distinguish achievable visualizations from unsupported hardware assumptions.  
**Verdict:** **DEVELOP FURTHER**. Zenodo Publication → Report is a plausible eventual destination, but neither integration evidence nor final publication audit is complete. Do not deposit based on this file.

## Evidence register

| Claim | Status for QUEST-EAGLEEYE | Boundary |
|---|---|---|
| Quest 3 color passthrough and spatial presentation | Third-party documented capability; not a project implementation | Meta documentation describes passthrough and environment depth |
| Forward-facing RGB camera access | Third-party documented capability; not a project implementation | Meta Passthrough Camera API requires supported Horizon OS, device and permission |
| TC001 thermal resolution 256×192, nominal 25 Hz | Manufacturer specification, not project measurement | No evidence of direct Quest acquisition |
| Unity `WebCamTexture` enumerates and decodes TC001 | **UNSUPPORTED** | Device enumeration, transport class, decoder and pixel format must be tested independently |
| A thermal image rendered on a headset-fixed panel | DOCUMENTED DESIGN / technically plausible | Not thermal-to-world registration |
| Thermal image precisely registered to physical objects | TECHNICALLY PLAUSIBLE BUT UNTESTED | Needs intrinsics, extrinsics, timestamps, depth and quantitative errors |
| Heltec V3 has onboard GNSS | CONTRADICTED | Meshtastic hardware listing says no GPS |
| Meshtastic position updates from external coordinates | Third-party-supported concept; unimplemented here | GNSS/paired source, message integration, packet age and coordinate mapping required |
| Quest 3 native eye tracking | CONTRADICTED | Meta states Quest 3 and 3S lack eye-tracking hardware |
| Head-directed ray/controller/hand interaction | Documented platform capability and project design | Project has not demonstrated integration |
| Generic USB-C PD hub simultaneously provides Quest charging and USB thermal capture | UNSUPPORTED | Headset port role, hub negotiation and actual load require bench checks |
| EagleEye military-system equivalence | UNSUPPORTED / outside scope | Only selected interface functions are comparable |

## Reference-system boundary

Anduril's 2025 EagleEye disclosures and contemporaneous reporting describe a modular soldier-worn display family and intended integration of situational overlays, shared context, sensing and communication. These are **vendor/public descriptions**, not a disclosed, independently benchmarked specification or a common baseline against which Quest 3 can be certified. The civilian research target is display and coordination functionality only; no military reliability, night-vision equivalence, sensor-fusion accuracy or ballistic capability is inferred.

## Defensible experimental architecture

The **preferred first experiment** avoids assuming that a proprietary USB thermal camera can work directly with Horizon OS.

```text
TOPDON TC001 ─USB─> compatible Android/Windows gateway
                            │
                       verified frame capture
                            │
                      timestamps + encoding
                            │
                        local Wi-Fi
                            ▼
Quest 3 app <──────── frame-stream receiver
   │                    │
   ├─ passthrough       └─ 2D thermal panel (stage A)
   ├─ head pose
   ├─ controller/hand/head ray
   └─ optional calibrated thermal camera mount
                │
       intrinsics/extrinsics + pose + depth
                ▼
       world registration (stage B; not tested)

External GNSS / phone ─> Meshtastic node / Heltec V3
                                │
                       BLE/USB/network gateway
                                │
                  position reports + packet age
                                ▼
                      Quest 3 status display
```

**Stage A:** prove that a supported external host actually decodes frames and that the Quest renders them with measured latency. This is an image-display demonstration, **not** registration.  
**Stage B:** separately establish rigid mounting, lens intrinsics, camera-to-headset extrinsics, synchronised timestamps, world transforms and registration error under controlled movement.  
**Stage C:** decode Meshtastic position packets from a paired host; annotate only reports with validated coordinates, age and source. Demonstrate messaging before attempting any geographic marker alignment.

### Interface contracts

| Link | Proposed payload | Proven support / outstanding dependency |
|---|---|---|
| TC001 → gateway | Frame bytes, capture time, calibration and format | Android/Windows compatibility claimed by TOPDON; acquisition SDK/driver/interface requires verification |
| Gateway → Quest | Versioned packet: frame ID, monotonic timestamp, frame format, dimensions, optional thermal scale | Application-level protocol is proposed, not implemented |
| Thermal sensor → headset | Rigid transform, intrinsic matrix, distortion model, timestamp | Entire calibration and motion-stability chain untested |
| Heltec → companion | Meshtastic protobuf position/report packets | Meshtastic client interface documented; actual device/client versions and transport not tested |
| Companion → Quest | Node ID, WGS84 position, fix type, source, message timestamp, reception timestamp | Local-to-world origin, uncertainty and stale-data policy remain to be implemented |
| Power hub → headset | USB data host role and negotiated supply | Android USB host general behavior does not guarantee Quest PD passthrough |

### Spatial analysis and failure boundaries

Use a declared convention, such as right-handed metres with world frame W, headset H and thermal camera T. Register a thermal pixel by undistorting it with calibrated thermal intrinsics `K_T`, estimating range from a supported depth or target model and applying the time-matched transform `T_W_H(t) * T_H_T`. Without depth, a thermal pixel defines a ray, not a unique world position. Independent pose tracking drift, headset/display latency, parallax, thermal sensor response and extrinsic mount movement all contribute to projection error.

A global WGS84 GNSS coordinate cannot be applied directly to an arbitrary headset-local origin. The test must establish local origin, orientation/heading, reference frame and uncertainty. Reject invalid or stale positions rather than display them as real-time ground truth.

### Power and wearability

Treat USB-C cable/hub/host topology, supply power, headset charging status and connected sensor operation as one test, not individual component specifications. Log charge state and power draw under headset + wireless + peripheral load. Use strain relief, protect USB connectors, avoid mains-powered improvised harnesses, and do not claim ballistic protection, night-operation suitability or safety-critical reliability.

## Reproducible test gates

| Gate | Apparatus / method | Record and acceptance condition | Result |
|---|---|---|---|
| H1 USB compatibility | Exact Quest 3 OS/build, TC001 hardware ID, USB hub, Android logs | Enumerated VID/PID, interfaces, endpoint descriptors, permission decision and frame acquisition evidence; do not infer frames from enumeration | NOT TESTED |
| H2 Gateway thermal capture | TC001 + confirmed-supported Android/Windows host | Record real frame dimensions, pixel format, sequence/timestamps, sustained 10-minute decode and drop counts | NOT TESTED |
| H3 Stream latency | Optical/timecode stimulus visible to thermal capture, synchronized clocks | Median, p95 and maximum end-to-end latency over ≥100 frames, with method uncertainty | NOT TESTED |
| H4 Registration | Calibrated fiducial/thermal targets at multiple depths and headset orientations | Intrinsics/extrinsics, mean and p95 pixel or angular error, drift and repeatability; prespecify acceptable tolerances before collecting data | NOT TESTED |
| H5 Position transport | ≥2 Meshtastic nodes, one source with demonstrably valid GNSS/phone coordinate | Source and receive timestamps, sequence IDs, loss and age distributions, coordinate residual to a stated reference | NOT TESTED |
| H6 Power topology | USB power meter, battery and hub part numbers, Quest charging indication | Stable sensor data while external power applied, charge trend, temperature and fault log; explicit pass/fail | NOT TESTED |
| H7 Interaction/workload | Head-ray/controller interaction tasks, consent-based participants if recruited | Selection success, task time, misses, subjective workload instrument; report sample and ethics conditions | NOT TESTED |

No numerical outcome may be asserted until raw records are preserved and the apparatus is reproducible.

## Source register and technical prior art starting points

- Meta, *Passthrough Camera API Overview*: https://developers.meta.com/vr/documentation/spatial-sdk/spatial-sdk-pca-overview/ . Official platform constraints; consult version-specific requirements.
- Meta, *Immersive Features*: https://developers.meta.com/vr/essentials/horizon-os-immersive-features/ . Official Quest 3 eye-tracking limitation.
- TOPDON, *TC001 specification*: https://www.topdon.us/products/tc001 . Manufacturer specification, not interoperability proof.
- Heltec, *WiFi LoRa 32 V3*: https://heltec.org/project/wifi-lora-32-v3/ . Board hardware documentation.
- Meshtastic, *HELTEC LoRa 32*: https://meshtastic.org/docs/hardware/devices/heltec-automation/lora32/ . V3 GNSS absence and firmware support.
- Meshtastic, *Serial Module*: https://meshtastic.org/docs/configuration/module/serial/ . Protobuf/NMEA interface documentation, not headset compatibility.
- Android Developers, *USB host overview*: https://developer.android.com/develop/connectivity/usb/host . USB permission, enumeration and interface access.
- Gentex, *EagleEye partnership announcement*, 2025-10-13: https://www.gentexcorp.com/news/gentex-expands-partnership-with-anduril-to-deliver-the-ai-driven-eagleeye-system-for-the-modern-warfighter/ . Vendor announcement, not independently measured performance.
- Myers, Meghann, *Anduril debuts Eagle Eye, a modular, AI-powered soldier headset*, Defense One, 2025-10-13: https://www.defenseone.com/defense-systems/2025/10/anduril-debuts-eagle-eye-modular-ai-powered-soldier-headset/408775/ . Contemporary reporting.

These are a **verified technical source seed list**, not an exhaustive peer-reviewed literature review. Scholarly literature/DOI checks remain incomplete.

## Audit blockers

1. No documented working thermal frame acquisition on the specified gateway/Quest configuration.
2. No controlled registration/latency/power measurements, and thus no empirical results.
3. Missing reviewed scholarly prior art with accurate finding-level citations and DOI validation.
4. No independently validated USB-C power/data compatibility for the exact Quest/hub combination.
5. No reproducible software bundle, versioned interface contracts or raw experimental evidence.
6. No resolved author-approved new manuscript licensing, release metadata and final PDF preflight.
7. The original December 10, 2025 date is historical and should not automatically be reused as a new technical-report release date.

**Independent audit determination:** research development justified; **NOT READY** for Zenodo approval or release. Maintain this file as a non-empirical working investigation until the gates above are resolved.
