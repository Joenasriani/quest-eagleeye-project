import json,hashlib,pathlib,struct,socket,subprocess,threading,time,sys
ROOT=pathlib.Path(__file__).resolve().parent
ASSEMBLY=ROOT/'bin'/'Release'/'net8.0'/'Qee.Desktop.dll'
def packet():
 data=struct.pack('<HHHH',100,300,500,700)
 meta={'schema_version':1,'source':'synthetic-fixture','frame_id':0,'width':2,'height':2,'pixel_format':'uint16-le','capture_clock_domain':'fixture-process-monotonic','capture_mono_ns':1234567,'synthetic':True,'sha256':hashlib.sha256(data).hexdigest()}
 b=json.dumps(meta,sort_keys=True,separators=(',',':')).encode('ascii')
 return b'QEE1'+struct.pack('!II',len(b),len(data))+b+data
def run(args):
 p=subprocess.run(['dotnet',str(ASSEMBLY)]+args,capture_output=True,text=True,timeout=40)
 print(args,p.returncode,p.stdout,p.stderr,flush=True)
 return p
def serve(payloads):
 s=socket.socket();s.bind(('127.0.0.1',0));s.listen(1);s.settimeout(12)
 port=s.getsockname()[1]
 def serve_():
  try:
   c,_=s.accept()
   with c:
    for d in payloads:
     c.sendall(d);time.sleep(.075)
  finally:s.close()
 t=threading.Thread(target=serve_,daemon=True);t.start()
 return port,t

def serve_reconnect():
 s=socket.socket();s.setsockopt(socket.SOL_SOCKET,socket.SO_REUSEADDR,1);s.bind(('127.0.0.1',0));s.listen(5);s.settimeout(9)
 port=s.getsockname()[1]
 def worker():
  try:
   for session in range(2):
    c,_=s.accept()
    with c:
     c.settimeout(3)
     for frame_id in range(140):
      try:
       c.sendall(packet_with_id(frame_id))
      except (BrokenPipeError,ConnectionResetError): break
      time.sleep(.002)
     time.sleep(.35)
   time.sleep(.25)
  finally:s.close()
 t=threading.Thread(target=worker,daemon=True);t.start()
 return port,t
def packet_with_id(fid):
 data=struct.pack('<HHHH',100,300,500,700)
 meta={'schema_version':1,'source':'synthetic-fixture','frame_id':fid,'width':2,'height':2,'pixel_format':'uint16-le','capture_clock_domain':'fixture-process-monotonic','capture_mono_ns':1234567+fid,'synthetic':True,'sha256':hashlib.sha256(data).hexdigest()}
 b=json.dumps(meta,sort_keys=True,separators=(',',':')).encode('ascii')
 return b'QEE1'+struct.pack('!II',len(b),len(data))+b+data

def main():
 assert ASSEMBLY.is_file()
 good=packet()
 bad=[('magic',b'BAD!'+good[4:]),('sha',good[:-1]+bytes([good[-1]^255])),('truncate',good[:-2]),('oversized',b'QEE1'+struct.pack('!II',9999999,8))]
 checks=[]
 for name,data in [('valid',good)]+bad:
  f=ROOT/f'case_{name}.qee';f.write_bytes(data)
  p=run(['decode',str(f)])
  ok=p.returncode==(0 if name=='valid' else 1)
  checks.append((name,ok))
  assert ok,name
 for mode,count in [('tcp',8),('receiver',12)]:
  port,t=serve([good]*count)
  p=run([mode,str(port)]);t.join(5)
  ok=p.returncode==0 and (('TCP_PASS:8' in p.stdout) if mode=='tcp' else ('RECEIVER_PASS:' in p.stdout and ':applies=' in p.stdout and ':stale=True' in p.stdout))
  checks.append((mode,ok));assert ok,mode
 port,t=serve_reconnect()
 p=run(['phase11',str(port)]);t.join(5)
 ok=p.returncode==0 and 'PHASE11_PASS:' in p.stdout and ':maxqueue=' in p.stdout and ':sessions=' in p.stdout
 checks.append(('phase11_reconnect_cancellation_queue',ok));assert ok,'phase11'
 (ROOT/'compiled_runtime_results.json').write_text(json.dumps(checks,indent=2))
 print('C# COMPILED INTEGRATION PASS',checks)
if __name__=='__main__':main()
