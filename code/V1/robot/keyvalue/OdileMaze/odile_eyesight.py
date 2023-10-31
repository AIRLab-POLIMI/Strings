# Part 01 using opencv access webcam and transmit the video in HTML
import time
import cv2
import pyshine as ps  # pip3 install pyshine==0.0.9
import urllib.request

HTML = """
<html>
<head>
<title>Odile's Eyesight</title>
</head>

<body>
<center><img src="stream.mjpg" width='100%' height='100%' autoplay playsinline></center>
</body>
</html>
"""


def main():
    StreamProps = ps.StreamProps
    StreamProps.set_Page(StreamProps, HTML)
    address = ('192.168.0.103', 9000)
    try:
        StreamProps.set_Mode(StreamProps, 'cv2')
        capture = cv2.VideoCapture(-1)
        time.sleep(2)
        capture.set(cv2.CAP_PROP_BUFFERSIZE, 4)
        capture.set(cv2.CAP_PROP_FRAME_WIDTH, 720)
        capture.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
        capture.set(cv2.CAP_PROP_FPS, 70)
        StreamProps.set_Capture(StreamProps, capture)
        StreamProps.set_Quality(StreamProps, 90)
        server = ps.Streamer(address, StreamProps)
        print('Server started at', 'http://' + address[0] + ':' + str(address[1]))
        server.serve_forever()

    except KeyboardInterrupt:
        capture.release()
        server.socket.close()


if __name__ == '__main__':
    main()
