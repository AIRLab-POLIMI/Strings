
from classes.robot import Robot
from classes.dof import Dof
from configs.keys.control_value_keys import *


# config file for test robot made of 2 servos

# 0. IP
test_robot_ip = "192.168.0.102"

# A. DOFS
servo1_dof = Dof(petals_dof_key)
servo2_dof = Dof(busto_dof_key)

# B. CONTROL DICT
control_dict = {
    "/dev/cu.usbmodem14201": [servo1_dof, servo2_dof]
}

# C. SENSORS
# bump_sensor = InSensorSerial('b')

# D. SENSING DICT
sensing_dict = {
    # "/dev/cu.usbmodem14101": [bump_sensor]
}

# E. ROBOT
test_robot = Robot(test_robot_ip, control_dict, sensing_dict)
