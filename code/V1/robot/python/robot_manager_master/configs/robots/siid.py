
from classes.robot import Robot
from classes.dof import Dof
from classes.sensor import InSensorSerial
from configs.dof_keys import *


# config file for robot SIID

# 0. IP
siid_ip = "192.168.0.100"

# A. DOFS
forward_dof = Dof(forward_dof_key)
strafe_dof = Dof(strafe_dof_key)
rotation_dof = Dof(rotation_dof_key)
petals_dof = Dof(petals_dof_key)
eye_x_dof = Dof(eye_x_dof_key)
eye_y_dof = Dof(eye_y_dof_key)
led_dof = Dof(led_dof_key)

# B. CONTROL DICT
control_dict = {
    "/dev/cu.usbmodem14101": [forward_dof, strafe_dof, rotation_dof, petals_dof, eye_x_dof, eye_y_dof, led_dof]
}

# C. SENSORS
bump_sensor = InSensorSerial('b')

# D. SENSING DICT
sensing_dict = {
    # "/dev/cu.usbmodem14101": [bump_sensor]
}

# E. ROBOT
siid = Robot(siid_ip, control_dict, sensing_dict)
