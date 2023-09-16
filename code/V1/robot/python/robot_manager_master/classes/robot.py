
# a Robot is just a CONFIGURATION class that the MAESTRO will use to do its operations.
# It is a composition of

# - for CONTROL: "arduino serial port : dof list" DICT. The DOF LIST must be in the CORRECT ORDER wrt to the control
#                message that each arduino is expecting
# - for SENSING: "arduino serial port : sensor list" DICT. The SENSOR LIST must be in the CORRECT ORDER wrt to the
#                sensing message that each arduino is sending. NB the sensors are always only InSensorSerial sensors,
#                the only ones coming from arduino serial port by definition


class Robot:
    def __init__(self, ip, control_dict, sensing_dict):
        self.control_dict = control_dict
        self.sensing_dict = sensing_dict

        self.control_arduino_ports = list(self.control_dict.keys())
        self.sensor_arduino_ports = list(self.sensing_dict.keys())

        # make a unique list of all the arduino ports from both control and sensing dicts
        self.all_arduino_ports = list(set(self.control_arduino_ports + self.sensor_arduino_ports))

        # compose a [dof_key:dof] dict for easy fast access
        self.dofs = dict()
        for arduino_port in self.control_arduino_ports:
            for dof in self.control_dict[arduino_port]:
                self.dofs[dof.key] = dof

        # compose a [sensor_key:sensor] dict for easy fast access
        self.sensors = dict()
        for arduino_port in self.sensor_arduino_ports:
            for sensor in self.sensing_dict[arduino_port]:
                self.sensors[sensor.key] = sensor

        self.num_dofs = len(self.dofs)
        self.num_sensors = len(self.sensors)

        self.ip = ip
