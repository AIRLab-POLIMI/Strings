
from classes.robot import Robot
from classes.dof import Dof
from classes.sensor import InSensorSerial
from classes.control_channel import SingleControlChannel
from configs.keys.control_value_keys import *


# config file for robot SIID

# 0. IP
siid_ip = "192.168.0.100"

# A. DOFS
forward_dof = Dof()
strafe_dof = Dof()
rotation_dof = Dof()
petals_dof = Dof()
eye_x_dof = Dof()
eye_y_dof = Dof()
led_dof = Dof()

# A1. CONTROL CHANNELS
forward_cv = SingleControlChannel(forward_dof_key, forward_dof)
strafe_cv = SingleControlChannel(strafe_dof_key, strafe_dof)
rotation_cv = SingleControlChannel(rotation_dof_key, rotation_dof)
petals_cv = SingleControlChannel(petals_dof_key, petals_dof)
eye_x_cv = SingleControlChannel(eye_x_dof_key, eye_x_dof)
eye_y_cv = SingleControlChannel(eye_y_dof_key, eye_y_dof)
led_cv = SingleControlChannel(led_dof_key, led_dof)

control_channels = [forward_cv, strafe_cv, rotation_cv, petals_cv, eye_x_cv, eye_y_cv, led_cv]

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
# --> ip, control_dict, sensing_dict, control_channels <--
siid = Robot(siid_ip, control_dict, sensing_dict, control_channels)
