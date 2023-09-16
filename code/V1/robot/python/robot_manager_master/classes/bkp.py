
import struct
import serial
import keyboard
import time


# Open the serial port for communication with Arduino
# ser = serial.Serial('/dev/ttyS0', 9600)  # Replace with your actual serial port and baud rate
ser = serial.Serial('/dev/tty.usbmodem14301', 115200)  # Replace with your actual serial port and baud rate

# Define the minimum and maximum values for rescaling
min_value1 = 0
max_value1 = 180  # Replace with your specific range
min_value2 = 0
max_value2 = 180  # Replace with your specific range
min_value3 = 0
max_value3 = 180  # Replace with your specific range

value1 = 0
value2 = 0
value3 = 0


def rescale(value, old_min, old_max, new_min, new_max):
    # Rescale 'value' from the old range to the new range
    return int((value - old_min) / (old_max - old_min) * (new_max - new_min) + new_min)


def send_servo_values():
    global value1, value2, value3

    # Rescale values to 0-255 range
    scaled_value1 = rescale(value1, min_value1, max_value1, 0, 255)
    scaled_value2 = rescale(value2, min_value2, max_value2, 0, 255)
    scaled_value3 = rescale(value3, min_value3, max_value3, 0, 255)

    # Pack values into binary format
    # packed_data = struct.pack('BBB', scaled_value1, scaled_value2, scaled_value3)
    # Send the packed data to Arduino
    # ser.write(packed_data)

    # convert scaled_value_2 to 1 byte and send it via serial port
    packed_data = struct.pack('B', scaled_value2)
    ser.write(packed_data)

    # print current message and binary version
    # print('value1: ', value1, 'value2: ', value2, 'value3: ', value3, 'packed_data: ', packed_data)

    print('value2: ', value2, 'packed_data: ', packed_data)

    # read from serial port and print message in a non blocking way
    if ser.in_waiting > 0:
        print(ser.readline().decode('utf-8').rstrip())


def main():
    global value1, value2, value3

    last_sent_time = time.time()

    while True:

        if keyboard.is_pressed('q'):
            exit()

        # Capture keyboard events for servo control
        if keyboard.is_pressed('r'):
            value1 = 180
        elif keyboard.is_pressed('t'):
            value1 = 0
        else:
            value1 = 90

        # Capture keyboard events for servo control
        if keyboard.is_pressed('a'):
            value2 = 180
        elif keyboard.is_pressed('s'):
            value2 = 0
        else:
            value2 = 90

        # Capture keyboard events for servo control
        if keyboard.is_pressed('n'):
            value3 = 180
        elif keyboard.is_pressed('m'):
            value3 = 0
        else:
            value3 = 90

        # Wait for 30 milliseconds before sending the next message
        now = time.time()

        if now - last_sent_time >= 0.03:
            # Send the rescaled values to Arduino
            send_servo_values()
            last_sent_time = now
        # time.sleep(0.03)


if __name__ == '__main__':
    main()


    #
    # def read_serial(self):
    #     self.index += 1  # for debugging
    #     # print(f"[MAIN][read_serial] --- READ {index}: ")
    #
    #     # for each arduino channel of the sensor channels, read as many bytes as there are Sensors in the list
    #     for arduino_channel in self.sensing_dict.keys():
    #         while True:
    #             # read one byte at a time.
    #             line = arduino_channel.read_serial_byte_non_blocking()
    #             if line is not None:
    #                 # convert byte to integer between 0 and 255
    #                 line = int.from_bytes(line, byteorder='big')
    #                 print(f"      --- READ {self.index} --- mgs: {line}")
    #                 pass
    #             else:
    #                 break