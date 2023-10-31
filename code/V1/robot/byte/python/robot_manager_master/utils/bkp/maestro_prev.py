import time

from classes.serial_channel import SerialChannel
from classes.network_channel import NetworkChannel
from utils.constants import DEFAULT_SERIAL_ELAPSED, DEFAULT_NETWORK_SEND_ELAPSED_SEC, DEFAULT_MAX_CONSECUTIVE_MSG_READS
from utils.bkp.controllers.all_controllers import AllControllers, DynamicIpControllerKeys, get_controlled_by_id


class Maestro:
    def __init__(self,
                 robot,
                 quest_ip=None,
                 serial_elapsed=DEFAULT_SERIAL_ELAPSED,
                 network_send_elapsed=DEFAULT_NETWORK_SEND_ELAPSED_SEC):

        self.robot = robot
        self.quest_ip = quest_ip
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

        # --- CHECKS
        # all controllers with dynamic ip flag must have an id that is present in the DynamicIpControllerKeys enum.
        # if this is not the case, print and exit the program
        for controller in AllControllers:
            if not controller.value.static_ip:
                all_good = False
                for dynamic_ip_controller_key in DynamicIpControllerKeys:
                    if controller.value.id == dynamic_ip_controller_key.value:
                        all_good = True
                        break
                if not all_good:
                    print(
                        f"[ERROR][Maestro] Controller {controller.value.unique_id} has dynamic IP but its id is not in "
                        f"DynamicIpControllerKeys enum. Please add it there.")
                    exit(1)

    # - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SERIAL

    def write_serial(self):
        # for each arduino channel of the control_channels, write as many bytes as there are DOFs in the list
        for arduino_channel in self.control_dict.keys():
            # write a single message with all the bytes, one for each DOF of this channel
            # pass
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

    def add_control(self, controller_id, control_id, dof_key):
        # input:
        # - controller_id(string): the unique id of the controller that sends the control signal/s.
        #               It's the IP for static ip controllers, and a unique key for dynamic ip controllers
        # - control_id(int): the ID of the specific control signal. It's the value of the enum of the control keys
        #               corresponding to the position of that control signal in the message.
        # - dof_key(string): the string key identifying the DOF that this control signal is mapped to

        # 1. check if the corresponding controller object is already present in the list of active controllers-
        #    a controller is uniquely identified by its ip. The only exception is quest controller, which my have no ip
        #    at this point before setup. In that case, it is identified by its key.
        #    if not, create it
        if controller_id not in self.controllers_by_id.keys():
            new_controller = get_controlled_by_id(controller_id)
            if new_controller is None:
                print(f"[MAESTRO][ADD CONTROL] - ERROR: controller with id '{controller_id}' not found in "
                      f"'AllControllers' Enum in 'all_controllers.py'")
                return

            self.controllers_by_id[controller_id] = new_controller

            # print info of the new controller, including if it has static ip
            print(f"[Maestro][add_control] adding new controller with ip: {new_controller.ip} - "
                  f"static ip: {new_controller.static_ip}")

            # 2. if it wasn't present among the controllers:
            #    if it's a static ip controller, add it to the IP:control dict.
            #    if it's a dynamic ip controller, add it to the ID:control dict ONLY if setup has been completed.
            if self.controllers_by_id[controller_id].static_ip:
                self.controllers_by_ip[new_controller.ip] = new_controller
            elif self.setup_complete:
                try:
                    self.controllers_by_ip[self.controller_id_to_ip[controller_id]] = new_controller
                except Exception as e:
                    print(
                        f"[MAESTRO][ADD CONTROL] - ERROR: setup complete but controller with id '{controller_id}' not found in "
                        f"'controller_id_ip' dict. Exception: {e}. Exiting program.")
                    exit(1)

        # 3. add the position:dof couple to the dict
        try:
            self.controllers_by_id[controller_id].add_control(control_id, self.robot.dofs[dof_key])

        except Exception as e:
            print(f"[MAESTRO][ADD CONTROL] - ERROR: dof with key '{dof_key}' not found in "
                  f"robot.dofs. Exception: {e}")
            return

    def setup_controllers(self):
        # setup the IPs of all the controllers in the controller_id dict that have dynamic ip,
        # populating the controller_id_ip dict.
        # This needs to be done one by one.

        # -- QUEST --
        # LOGIC TO GET DYNAMIC QUEST IP HERE (or in a previous setup method)
        # then, use the quest_ip
        if self.quest_ip is not None:
            self.controller_id_to_ip[DynamicIpControllerKeys.OculusQuest.value] = self.quest_ip
        if DynamicIpControllerKeys.OculusQuest.value in self.controllers_by_id.keys():
            self.controllers_by_ip[self.quest_ip] = self.controllers_by_id[DynamicIpControllerKeys.OculusQuest.value]

        # -- FINAL CHECK --
        # check that all the controllers in the controller_id dict are also in the controller_ip dict
        # if not, it means that they are dynamic ip controllers and setup has not been completed:
        # in that case, print and exit
        for ip_id, controller in self.controllers_by_id.items():
            if controller not in self.controllers_by_ip.values():
                print(f"[MAESTRO][SETUP CONTROLLERS] - ERROR: controller with id '{ip_id}' not found in "
                      f"'controller_ip' dict. Setup has not been completed. Exiting program.")
                exit(1)

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
            # print(f"[maestro][network_communication] - "
            #       f"message from IP: ", sender_ip,
            #       " - and PORT: ", self.network_channel.udp_data[1][1])

            if sender_ip in self.controllers_by_ip.keys():
                # print(f"[maestro][network_communication] - "
                #       f"received data: '{self.network_channel.udp_data[0]} from controller with ip: '{sender_ip}")
                self.controllers_by_ip[sender_ip].on_msg_received(self.network_channel.udp_data[0])

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

        self.setup_controllers()

        self.setup_complete = True

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
