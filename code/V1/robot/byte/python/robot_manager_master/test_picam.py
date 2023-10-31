
import io
import picamera
import socket
import time
import threading


# Define the camera resolution and frame rate
width = 200
height = 150
framerate = 10

# Define the UDP server address and port
server_address = ('192.168.0.2', 25666)

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print("AA")


# Create a thread for capturing and sending frames
def capture_and_send():
    print("0")

    with picamera.PiCamera() as camera:

        print("1")

        camera.resolution = (width, height)

        print("2")

        camera.framerate = framerate

        # Wait for the camera to warm up
        time.sleep(2)

        print("A")

        stream = io.BytesIO()

        print("B")

        for _ in camera.capture_continuous(stream, format='jpeg', use_video_port=True):
            # Rewind the stream for reading
            stream.seek(0)

            print("C")
            # Read the frame data
            frame_data = stream.read()

            # Send the frame data over UDP
            sock.sendto(frame_data, server_address)

            print("D")
            # Reset the stream for the next frame
            stream.seek(0)
            stream.truncate()

            print("E")


# Create and start the capture thread
capture_thread = threading.Thread(target=capture_and_send)
print("AAAAA")

capture_thread.start()

try:
    # Keep the main program running
    while True:
        pass
except KeyboardInterrupt:
    pass
finally:
    sock.close()
