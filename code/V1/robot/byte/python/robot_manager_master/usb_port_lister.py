
import serial.tools.list_ports

ports = list(serial.tools.list_ports.comports())
for port, desc, hwid in sorted(ports):
    print(f"{port}: {desc} [{hwid}]")