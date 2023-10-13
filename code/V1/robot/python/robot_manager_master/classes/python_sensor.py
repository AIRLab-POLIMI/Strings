
import io
import socket
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

    def get_udp_message(self):
        # the method must return the value to send as bytes
        # in this version, this method also collects the data from the hardware and returns it directly.
        # If there is no valid data, it returns None
        pass


class CameraPythonSensor(PythonSensor):

    def __init__(self):
        super().__init__()
        # max size of the buffer is 1so that the latest frame is always the one that is sent
        self.frame_queue = queue.Queue(maxsize=1)
        self.thread = None

    def setup(self):
        self.thread = threading.Thread(target=self.capture_and_stream, args=(self.frame_queue,))
        self.thread.start()

    def get_udp_message(self):
        try:
            return self.frame_queue.get(timeout=1.0)  # Get a frame from the queue
        except queue.Empty:
            # Handle the case where the queue is empty
            return None

    # Function to capture and stream video frames
    def capture_and_stream(self):
        with picamera.PiCamera() as camera:
            camera.resolution = (640, 480)  # Adjust the resolution as needed
            camera.framerate = 30  # Adjust the framerate as needed

            stream = io.BytesIO()
            for _ in camera.capture_continuous(stream, format='jpeg'):
                frame = stream.getvalue()
                self.frame_queue.put(frame)  # Put the frame into the queue
                stream.seek(0)
                stream.truncate()
                time.sleep(1.0 / 30)  # Synchronize with the 30 FPS capture rate
