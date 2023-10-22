
import serial
import time
from utils.constants import \
    serial_default_baud, serial_default_timeout, serial_default_delay_after_setup, ARDUINO_READY_FLAG
from utils.byte_helper import int_list_to_bytes, int_to_byte, string_line_to_bytes


class SerialChannel:

    def __init__(self,
                 serial_port,
                 baud=serial_default_baud,
                 timeout=serial_default_timeout,
                 delay_after_setup=serial_default_delay_after_setup):
        self.port = serial_port
        self.baud = baud
        self.timeout = timeout
        self.delay_after_setup = delay_after_setup

        # declare SER to None; it will be initialized in "setup_serial"
        self.ser = None

    def setup_serial(self):

        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - START setting up SERIAL COMM")

        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - "
              "attempting SERIAL connection at PORT: ", self.port, " - with BAUD: ", self.baud)

        # wait until serial is available
        while True:
            try:
                self.ser = serial.Serial(self.port, self.baud, timeout=self.timeout)
                time.sleep(self.delay_after_setup)
                self.ser.reset_input_buffer()
                self.ser.reset_output_buffer()
                time.sleep(self.delay_after_setup)

                # self.ser.close()  # RESET IT, them start again
                #
                # time.sleep(self.delay_after_setup)
                #
                # self.ser = serial.Serial(self.port, self.baud, timeout=self.timeout)
                # time.sleep(self.delay_after_setup)
                # self.ser.reset_input_buffer()
                # self.ser.reset_output_buffer()
                # time.sleep(self.delay_after_setup)

                print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - connected SUCCESSFULLY")
                break
            except Exception as e:
                print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - connection FAILED with error: '", e,
                      "'.\nTrying again in 1s..")
                time.sleep(1)

        # awaiting ARDUINO
        # if any message is received though serial, it means arduino is UP and RUNNING
        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - AWAITING for ARDUINO to complete setup")
        while True:
            self.write_string(ARDUINO_READY_FLAG)
            serial_msg = self.read_serial_blocking()
            if serial_msg and len(serial_msg) > 0 and serial_msg == ARDUINO_READY_FLAG:
                print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - ARDUINO has completed setup SUCCESSFULLY")
                break
            else:
                print(f"[SETUP SERIAL][PORT '{self.port}'] - "
                      f"ARDUINO setup is not complete: serial msg is: {serial_msg}. Checking again in 1s..")
                time.sleep(1)  # sleep time in seconds

        # setup complete
        print(f"[SERIALCHANNEL][PORT '{self.port}'][SETUP SERIAL] - serial setup COMPLETE\n")

    def read_serial_blocking(self):
        # awaits for the line to be complete("\n" character) before returning
        try:
            return self.ser.readline().decode('utf-8').rstrip()
        except Exception as e:
            return f"NO MSG. Error: {e}"

    def read_serial_non_blocking(self):
        # checks if there is something in the buffer at the time in which the method is called.
        # - if there is, awaits for the line to be complete("\n" character) before returning
        # - if there isn't, return None
        try:
            if self.ser.in_waiting > 0:
                return self.ser.readline().decode('utf-8').rstrip()
            else:
                return None
        except Exception as e:
            print(f"[SERIALCHANNEL][PORT '{self.port}'][READ SERIAL NON BLOCKING] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_output_buffer()
            # self.ser.reset_input_buffer()
            return None

    def read_serial_byte_non_blocking(self):
        # checks if there is something in the buffer at the time in which the method is called.
        # - if there is, awaits for a byte to be complete before returning
        # - if there isn't, return None
        try:
            if self.ser.in_waiting > 0:
                return self.ser.read(1)
            else:
                return None
        except Exception as e:
            print(f"[SERIALCHANNEL][PORT '{self.port}'][READ SERIAL BYTES NON BLOCKING] - "
                  f"ABORTED: an error occurred: '{e}'")
            self.ser.reset_output_buffer()
            # self.ser.reset_input_buffer()
            return None

    def write_byte_int(self, value):
        # convert input value into a single byte and send it via serial port
        print(f"[SERIALCHANNEL][write_byte_int] WRITE ----------------------------- int msg: '{value}'")
        packed_data = int_to_byte(value)
        self.ser.write(packed_data)

    def write_bytes_int(self, values):
        # input is a list of integers. Create a single message that is the concatenation of all the bytes
        # and send it via serial
        print(f"[SERIALCHANNEL][write_bytes_int] WRITE ----------------------------- int msg: '{values}'")
        packed_data = int_list_to_bytes(values)
        self.ser.write(packed_data)

    def write_string(self, msg):
        try:
            print(f"[SERIALCHANNEL][write_serial] WRITE ----------------------------- msg: '{msg}'")
            self.ser.write(string_line_to_bytes(msg))
            # print(f"[SERIALCHANNEL][write_serial] - COMPLETE\n")

        except Exception as e:
            print(f"[SERIALCHANNEL][PORT '{self.port}'][WRITE SERIAL] - ABORTED: an error occurred: '{e}'")
            self.ser.reset_input_buffer()
            self.ser.reset_output_buffer()

    def cleanup(self):
        self.ser.close()
