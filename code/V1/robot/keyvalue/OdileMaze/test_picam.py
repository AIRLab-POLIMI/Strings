#
# import picamera
#
# # Create a PiCamera object
# camera = picamera.PiCamera()
#
# # Capture a still image and save it to a file
# camera.capture('image.jpg')
#
# # Release the camera when done
# camera.close()
#


import io
import picamera
import socket
import time
import threading

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Create a thread for capturing and sending frames
def capture_and_send():

    # Define the camera resolution and frame rate
    width = 70
    height = 60

    framerate = 10

    # Define the UDP server address and port
    server_address = ('192.168.0.100', 25666)

    # Create a UDP socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    print("AA")

    with picamera.PiCamera() as camera:

        # print("1")

        camera.resolution = (width, height)

        # print("2")

        camera.framerate = framerate

        # Wait for the camera to warm up
        time.sleep(2)

        # print("A")

        stream = io.BytesIO()

        # print("B")

        for _ in camera.capture_continuous(stream, format='jpeg', use_video_port=True):
            # Rewind the stream for reading
            stream.seek(0)

            # print("C")
            # Read the frame data
            frame_data = stream.read()

            # print(f"sending frame: '{len(frame_data)}'")

            # Send the frame data over UDP
            sock.sendto(frame_data, server_address)

            # print("D")
            # Reset the stream for the next frame
            stream.seek(0)
            stream.truncate()

            # print("E")


if __name__ == "__main__":

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
