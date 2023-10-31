#
# import cv2
# import socket
# import pickle
# import struct
# import time
#
# # This streams the camera over udp
# # You can change the Quality and Size of the JPG you are sending to test tradeoffs on packet size
#
# # Define the UDP IP address and port to send the stream to
# UDP_IP = '192.168.0.102'  # Change this to the IP address of the receiving machine
# UDP_PORT = 25666  # Change this to an available UDP port on the receiving machine
#
# # Before creating the socket, set the buffer size
# sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
# sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)  # Set buffer size to 65536 bytes
#
# # Open the laptop's camera
# cap = cv2.VideoCapture(-1)
# QUALITY = 5
# # create trackbar for quality
# # cv2.namedWindow('frame')
# # cv2.createTrackbar('QUALITY', 'frame', 1, 100, lambda x: None)
# # cv2.setTrackbarPos('QUALITY', 'frame', QUALITY)
# RESIZE = 10
# # trackbar for resize
# # cv2.createTrackbar('RESIZE', 'frame', 1, 100, lambda x: None)
# # cv2.setTrackbarPos('QUALITY', 'frame', RESIZE)
#
# # trackbar for len
# # cv2.createTrackbar('PACKET_SIZE', 'frame', 1, 10000, lambda x: None)
#
# while True:
#     try:
#         # Capture a frame from the camera
#         ret, frame = cap.read()
#         print(len(frame))
#         # show
#         # cv2.imshow('frame',frame)
#         # print resolution
#         # print(frame.shape)
#         frame = cv2.resize(frame, (0, 0), fx=RESIZE / 100, fy=RESIZE / 100)
#         # send frame as jpg over udp
#         encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), QUALITY]
#         frame = cv2.imencode('.jpg', frame, encode_param)[1].tobytes()
#         print(len(frame))
#         # if len > 60000 dont sent
#         if len(frame) < 60000:
#             sock.sendto(frame, (UDP_IP, UDP_PORT))
#         # string saying "time " and current time
#         timestring = "time " + str(time.time())
#         # send string "time"+ time now
#         sock.sendto(timestring.encode(), (UDP_IP, UDP_PORT))
#
#         # slider to set quality
#         # QUALITY = cv2.getTrackbarPos('QUALITY', 'frame')
#         # RESIZE = cv2.getTrackbarPos('RESIZE', 'frame')
#         if (RESIZE == 0):
#             RESIZE = 1
#         # set PACKET_SIZE trackbar value to len(frame)
#         # cv2.setTrackbarPos('PACKET_SIZE', 'frame', len(frame))
#
#         # print image size in bytes
#         # sleep 0.05
#         cv2.waitKey(50)
#     except Exception as e:
#         print(f"Error sending frame. Error: '{e}'")
#
# # Release the camera and close the socket when done
# cap.release()
# sock.close()
#
# if __name__ == "__main__":
#     print("DIOCANE")
#     main()





import cv2
import socket
import subprocess
import time


def reset_usb_device(bus_number, device_number):
    try:
        authorized_path = f"/sys/bus/usb/devices/{bus_number}-{device_number}/authorized"
        subprocess.run(['sudo', 'sh', '-c', f'echo 0 > {authorized_path}'], check=True)
        subprocess.run(['sudo', 'sh', '-c', f'echo 1 > {authorized_path}'], check=True)
        print("USB device reset successful.")
    except subprocess.CalledProcessError as e:
        print(f"Error resetting USB device: {e}")


def main():
    # Define the UDP IP address and port to send the stream to
    UDP_IP = '192.168.0.102'  # Change this to the IP address of the receiving machine 101
    UDP_PORT = 25666      # Change this to an available UDP port on the receiving machine

    RESOLUTION_SCALE = 0.15 #0.20
    QUALITY = 20 #30 #4

    # Before creating the socket, set the buffer size
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_SNDBUF, 65536)  # Set buffer size to 65536 bytes

    # Bind the socket to the IP address and port
    # sock.bind(("192.168.0.115", 25666))

    error_count = 0
    MAX_ERROR_COUNT = 10

    # Open the laptop's camera
    cap = cv2.VideoCapture(-1)

    # Replace 'Bus 001' and 'Device 012' with your specific bus and device numbers.
    bus_number = '1'
    device_number = '1.2'
    #reset_usb_device(bus_number, device_number)
    DELAY_TIME = 0.0006

    while True:
        try:
            # Capture a frame from the camera
            ret, frame = cap.read()
            #reduce frame size to 1/2
            frame = cv2.resize(frame, (0,0), fx=RESOLUTION_SCALE, fy=RESOLUTION_SCALE)
            # cv2.imshow('frame', frame)

            print(f"sending frame: {len(frame)}")

            # send frame as jpg over udp
            #frame = cv2.imencode('.jpg', frame)[1].tobytes()
            # send frame as jpg over udp with quality
            encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), QUALITY]
            frame = cv2.imencode('.jpg', frame, encode_param)[1].tobytes()
            #print(len(frame))
            sock.sendto(frame, (UDP_IP, UDP_PORT))
            #print image size in bytes
            # sleep 0.05
            error_count = 0
            time.sleep(DELAY_TIME)

        except Exception as e:
            print(f"error showing frame: {e}")
            error_count += 1
            if (error_count > MAX_ERROR_COUNT):
                break

    # Release the camera and close the socket when done
    cap.release()
    sock.close()


#if main
if __name__ == "__main__":
    main()