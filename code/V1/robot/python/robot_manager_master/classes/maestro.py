
import time

from classes.serial_channel import SerialChannel
from classes.network_channel import NetworkChannel
from utils.constants import DEFAULT_SERIAL_ELAPSED, DEFAULT_NETWORK_SEND_ELAPSED_SEC, DEFAULT_MAX_CONSECUTIVE_MSG_READS
from utils.messaging_helper import parse_byte_message, bytes_to_unicode_str


class Maestro:
    def __init__(self,
                 robot,
                 serial_elapsed=DEFAULT_SERIAL_ELAPSED,
                 network_send_elapsed=DEFAULT_NETWORK_SEND_ELAPSED_SEC):

        self.robot = robot
        # --- FLAGS
        self.setup_complete = False

        # --- CONTROLLERS
        self.controllers_by_ip = dict()  # {controller_ip: controller object}
        self.controllers_by_id = dict()  # {controller_id: controller object}
        # we could avoid using two dicts if we make a search based on id on every "add_control" call.
        self.controller_id_to_ip = dict()  # {controller_id: controller_ip} dict. For dynamic ip controllers

        # --- NETWORK COMMUNICATION
        self.network_channel = NetworkChannel(self.robot.ip)
        self.last_network_time = time.time()
        self.network_send_elapsed = network_send_elapsed

        # --- SERIAL COMMUNICATION
        self.last_serial_time = time.time()
        self.serial_elapsed = serial_elapsed
        self.write = True
        self.index = 0
        # build a serial channel for each arduino port
        self.arduino_channels = dict()
        for arduino_port in self.robot.all_arduino_ports:
            self.arduino_channels[arduino_port] = SerialChannel(arduino_port)

        self.control_dict = dict()
        for arduino_port in self.robot.control_arduino_ports:
            self.control_dict[self.arduino_channels[arduino_port]] = self.robot.control_dict[arduino_port]
        
        self.sensing_dict = dict()
        for arduino_port in self.robot.sensor_arduino_ports:
            self.sensing_dict[self.arduino_channels[arduino_port]] = self.robot.sensing_dict[arduino_port]

        # --- CONTROL CHANNELS
        # maps the keys of the control signal (the incoming value)
        # to the ControlChannel object containing the corresponding ControlValue object
        self.control_values = dict()  # {Control signal key: ControlChannel object}
        # this will be updated every time a new mapping is assigned in the "set_control_mapping" method

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SERIAL

    def write_serial(self):
        # A - for each control channel, call the UPDATE DOF method to get the current dof values
        for control_channel in self.robot.control_channels:
            control_channel.update_dofs()

        # B - for each arduino channel of the control_channels, write as many bytes as there are DOFs in the list
        for arduino_channel in self.control_dict.keys():
            # write a single message with all the bytes, one for each DOF of this channel
            # pass
            arduino_channel.write_bytes_int([dof.get_message() for dof in self.control_dict[arduino_channel]])

    def read_serial(self):
        # self.index += 1  # for debugging
        # print(f"[MAIN][read_serial] --- READ {index}: ")

        # for each arduino channel of the sensor channels, read as many bytes as there are Sensors in the list
        for arduino_channel in self.sensing_dict.keys():
            # read one byte at a time, one for each sensor
            for in_sensor_serial in self.sensing_dict[arduino_channel]:
                # print(f"      --- READ {self.index} --- for sensor: {in_sensor_serial.key}")
                while True:
                    line = arduino_channel.read_serial_byte_non_blocking()
                    if line is not None:
                        # convert byte to integer between 0 and 255 and update the sensor value class
                        line = int.from_bytes(line, byteorder='big')
                        # print(f"      --- READ {self.index} --- mgs: {line} -- for sensor: {in_sensor_serial.key}")
                        in_sensor_serial.on_new_value(line)
                        break

            # TODO - code HANGS if there are not at least as many bytes to read as there are sensors for this channel:
            # TODO - arduino should write immediately after receiving (1 ms after),
            # TODO - so that the channel is ready to be read from

    def serial_communication(self):
        # can perform a SERIAL ACTION only every 'self.last_serial_time' seconds (~10ms usually)
        if time.time() - self.last_serial_time < self.serial_elapsed:
            # print(f".. NO SERIAL COMM ..")
            return

        else:
            # print(f"[maestro][serial_communication] write: {self.write}")
            if self.write:
                self.write_serial()
                self.write = False
            else:
                self.read_serial()
                self.write = True
                # print("read from serial")
            self.last_serial_time = time.time()

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - CONTROLLERS

    def set_control_mapping(self, control_signal_key, control_value_key):
        # get the ControlChannel object corresponding to the control value key
        control_channel = self.robot.control_key_channels[control_value_key]

        # assign to the control channel dict the control signal key
        self.control_values[control_signal_key] = control_channel.control_value_dict[control_value_key]

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - PYTHON SENSORS

    # this portion contains computations for all the data that is collected by the sensors connected to Python direclty.
    # e.g. the Camera.

    def setup_python_sensors(self):
        for python_sensor in self.robot.python_sensors:
            python_sensor.setup()

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - NETWORK

    # network communication does two things:
    #   - it receives messages from the remote controllers and passes them to the corresponding ControlChannel
    #   - it sends messages to the feedback system with the OutSensor values
    # the two tasks are performed with different frequencies:
    #   - checking input UDP messages is done at every loop iteration
    #   - sending output UDP messages is done at a fixed frequency, controlled by the 'last_network_time' variable
    # NB since "read udp" is called at every iteration,
    #    it's not implemented as a separate method because it would be inefficient in python
    def network_communication(self):
        # print("[maestro][network_communication] - network communication --- BEGIN")
        # A read from udp, at every iteration read everything there is to read
        num_read = 0
        while self.network_channel.read_udp_non_blocking():

            # 1. try to get key-value messages
            key_value_msgs = parse_byte_message(bytes_to_unicode_str(self.network_channel.udp_data[0]))
            # print(f"[MAESCRO][network communication] - received message: '{key_value_msgs}'")

            # 2. if there is at least a key-value message, update the corresponding controller
            if key_value_msgs is not None:
                for key_value_msg in key_value_msgs:
                    # print(f"[maestro][network_communication] - "
                    #       f"received key: '{key}' with value: '{value}' "
                    #       f"from controller with ip: {self.network_channel.udp_data[1][0]}")
                    if key_value_msg.key in self.control_values.keys():
                        self.control_values[key_value_msg.key].on_msg_received(float(key_value_msg.value))
                    else:
                        pass
                        # print(f"[maestro][network_communication] - "
                        #       f"received key: '{key_value_msg.key}' with value: '{key_value_msg.value}' "
                        #       f"from controller with ip: '{self.network_channel.udp_data[1][0]}' "
                        #       f"but this key is not in the control_keys_dict. "
                        #       f"Please add it to the control_keys_dict in the constructor of Maestro.")
                num_read += 1
                if num_read > DEFAULT_MAX_CONSECUTIVE_MSG_READS:
                    break

            num_read += 1
            if num_read > DEFAULT_MAX_CONSECUTIVE_MSG_READS:
                break

        # B write to udp, every 'self.network_send_elapsed' seconds
        if time.time() - self.last_network_time > self.network_send_elapsed:
            # print("[maestro][network_communication] - sending udp message")
            # send the message to the feedback system
            self.write_udp()
            self.last_network_time = time.time()

    def write_udp(self):
        # self.network_channel.write_udp(....)
        for python_sensor in self.robot.python_sensors:
            msg = python_sensor.get_udp_message()
            if msg is not None:
                self.network_channel.write_udp(msg, "192.168.0.101", 12345)

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN

    def setup(self):
        for arduino_channel in self.arduino_channels.values():
            arduino_channel.setup_serial()

        self.network_channel.setup_udp()

        self.setup_python_sensors()

        self.setup_complete = True

    def loop(self):
        # if keyboard.is_pressed('q'):
        #     self.close_program()

        # 0. Python sensor update
        # TODO if it's a bit slow, do the PSLoop only inside the Network loop when you're about to send;
        # TODO and/or send with higher frequency
        # self.python_sensor_loop()

        # A. NETWORK COMMUNICATION
        self.network_communication()

        # B. SERIAL COMMUNICATION
        self.serial_communication()

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - UTILS

    def close_program(self):
        for arduino_channel in self.arduino_channels.values():
            arduino_channel.cleanup()
        exit()
