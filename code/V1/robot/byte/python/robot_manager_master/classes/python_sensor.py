
import io
import picamera
import threading
import queue
import time


class PythonSensor:
    def __init__(self):
        pass

    def setup(self):
        pass

    # def collect_data(self):
    #     # method to update the value to be sent via udp
    #     pass

    def send_udp_message(self, send_method, destination_ip, destination_port):
        # the method must return the value to send as bytes
        # in this version, this method also collects the data from the hardware and returns it directly.
        # If there is no valid data, it returns None
        pass


class CameraPythonSensor(PythonSensor):

    def __init__(self):
        super().__init__()

        # Set your desired frame capture frequency (frames per second)
        self.frame_rate = 10

        # Initialize the camera
        self.camera = picamera.PiCamera()
        self.camera.resolution = (300, 200)  # Adjust resolution as needed
        self.camera.framerate = self.frame_rate

    def setup(self):
        pass

    def send_udp_message(self, send_method, destination_ip, destination_port):
        stream = io.BytesIO()
        self.camera.capture(stream, format='jpeg', use_video_port=True)
        stream.seek(0)
        frame_data = stream.read()

        if frame_data is not None and len(frame_data):
            send_method(frame_data, destination_ip, destination_port)
