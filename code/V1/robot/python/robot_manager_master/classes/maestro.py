
import time

from classes.serial_channel import SerialChannel
from classes.network_channel import NetworkChannel
from utils.constants import DEFAULT_SERIAL_ELAPSED, DEFAULT_NETWORK_SEND_ELAPSED_SEC, DEFAULT_MAX_CONSECUTIVE_MSG_READS
from configs.controllers.all_controllers import all_controllers
from configs.controllers.quest_controller import quest_controller_key


class Maestro:
    def __init__(self,
                 robot,
                 quest_ip,
                 serial_elapsed=DEFAULT_SERIAL_ELAPSED,
                 network_send_elapsed=DEFAULT_NETWORK_SEND_ELAPSED_SEC):

        self.robot = robot
        self.quest_ip = quest_ip

        # --- CONTROLLERS
        self.controllers = dict()
        self.controllers_ip = dict()

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

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SERIAL

    def write_serial(self):
        # for each arduino channel of the control channels, write as many bytes as there are DOFs in the list
        for arduino_channel in self.control_dict.keys():
            # write a single message with all the bytes, one for each DOF of this channel
            arduino_channel.write_bytes_int([dof.get_message() for dof in self.control_dict[arduino_channel]])

    def read_serial(self):
        self.index += 1  # for debugging
        # print(f"[MAIN][read_serial] --- READ {index}: ")

        # for each arduino channel of the sensor channels, read as many bytes as there are Sensors in the list
        for arduino_channel in self.sensing_dict.keys():
            # read one byte at a time, one for each sensor
            for in_sensor_serial in self.sensing_dict[arduino_channel]:
                print(f"      --- READ {self.index} --- for sensor: {in_sensor_serial.key}")
                while True:
                    line = arduino_channel.read_serial_byte_non_blocking()
                    if line is not None:
                        # convert byte to integer between 0 and 255 and update the sensor value class
                        line = int.from_bytes(line, byteorder='big')
                        print(f"      --- READ {self.index} --- mgs: {line} -- for sensor: {in_sensor_serial.key}")
                        in_sensor_serial.on_new_value(line)
                        break

            # TODO - code HANGS if there are not at least as many bytes to read as there are sensors for this channel:
            # TODO - arduino should write immediately after receiving (1 ms after),
            # TODO - so that the channel is ready to be read from

    def serial_communication(self):
        # can perform a SERIAL ACTION only every 'self.last_serial_time' seconds (~10ms usually)
        if time.time() - self.last_serial_time < self.serial_elapsed:
            # print(".. NO SERIAL COMM ..")
            return

        else:
            # print(f"[maestro][serial_communication] write: {self.write}")
            if self.write:
                self.write_serial()
                self.write = False
            else:
                self.read_serial()
                self.write = True
            self.last_serial_time = time.time()

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - CONTROLLERS

    def add_control(self, controller_key, control_id, dof_key):
        # input:
        # - controller_id(string): the ID of the controller that sends the control signal/s
        # - control_id(int): the ID of the specific control signal. It's the value of the enum of the control keys
        #               corresponding to the position of that control signal in the message.
        # - dof_key(string): the string key identifying the DOF that this control signal is mapped to

        # 1. check if the corresponding controller object is already present in the list of active controllers
        #    if not, create it
        if controller_key not in self.controllers.keys():
            try:
                new_controller = all_controllers[controller_key]()
                if controller_key == quest_controller_key:
                    new_controller.set_ip(self.quest_ip)

                    # TODO if self.quest_ip is set later,
                    # TODO this if will just skip over and when quest ip is set this controller needs to be updated

                # else it should already have a set ip
                elif new_controller.ip is None:
                    print(f"[MAESTRO][ADD CONTROL] - ERROR: controller with key '{controller_key}' has no IP set it. "
                          f"Skipping setup")
                    return

                self.controllers[controller_key] = new_controller
                self.controllers_ip[new_controller.ip] = new_controller

            except Exception as e:
                print(f"[MAESTRO][ADD CONTROL] - ERROR: controller with key '{controller_key}' not found in "
                      f"all_controllers.py. Exception: {e}")
                return

        # 2. add the position:dof couple to the dict
        try:
            self.controllers[controller_key].add_control(control_id, self.robot.dofs[dof_key])

        except Exception as e:
            print(f"[MAESTRO][ADD CONTROL] - ERROR: dof with key '{dof_key}' not found in "
                  f"robot.dofs. Exception: {e}")
            return

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - NETWORK

    # network communication does two things:
    #   - it receives messages from the controllers and updates the corresponding current DOF values
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
            sender_ip = self.network_channel.udp_data[1][0]
            print(f"[maestro][network_communication] - "
                  f"message from IP: ", sender_ip,
                  " - and PORT: ", self.network_channel.udp_data[1][1])

            if sender_ip in self.controllers.keys():
                self.controllers_ip[sender_ip].on_msg_received(self.network_channel.udp_data[0])

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
        pass

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN

    def setup(self):
        for arduino_channel in self.arduino_channels.values():
            arduino_channel.setup_serial()

        self.network_channel.setup_udp()

    def loop(self):
        # if keyboard.is_pressed('q'):
        #     self.close_program()

        # A. NETWORK COMMUNICATION
        self.network_communication()

        # B. SERIAL COMMUNICATION
        self.serial_communication()

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - UTILS

    def close_program(self):
        for arduino_channel in self.arduino_channels.values():
            arduino_channel.cleanup()
        exit()
