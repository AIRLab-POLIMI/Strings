
from classes.robot import Robot
from classes.dof import Dof
from classes.control_channel import SingleControlChannel
from configs.keys.control_value_keys import *


# config file for robot ODILE

# 0. IP
odile_ip = "192.168.0.101"

# A. DOFS
forward_dof = Dof()
strafe_dof = Dof()
rotation_dof = Dof()
head_x_dof = Dof()
head_y_dof = Dof()
neck_forward_dof = Dof()
arm_horizontal_dof = Dof()
arm_shoulder_dof = Dof()
arm_elbow_dof = Dof()

# A1. CONTROL CHANNELS
forward_cv = SingleControlChannel(forward_dof_key, forward_dof)
strafe_cv = SingleControlChannel(strafe_dof_key, strafe_dof)
rotation_cv = SingleControlChannel(rotation_dof_key, rotation_dof)
head_x_cv = SingleControlChannel(eye_x_dof_key, head_x_dof)
head_y_cv = SingleControlChannel(eye_y_dof_key, head_y_dof)
neck_forward_cv = SingleControlChannel(neck_forward_dof_key, neck_forward_dof)
arm_horizontal_cv = SingleControlChannel(arm_horizontal_dof_key, arm_horizontal_dof)
arm_shoulder_cv = SingleControlChannel(arm_shoulder_dof_key, arm_shoulder_dof)
arm_elbow_cv = SingleControlChannel(arm_elbow_dof_key, arm_elbow_dof)

control_channels = [forward_cv, strafe_cv, rotation_cv, head_x_cv, head_y_cv,
                    neck_forward_cv, arm_horizontal_cv, arm_shoulder_cv, arm_elbow_cv]

# B. CONTROL DICT
control_dict = {
    "/dev/cu.usbmodem14101": [forward_dof, strafe_dof, rotation_dof],

    "/dev/cu.usbmodem14102": [head_x_dof, head_y_dof,
                              neck_forward_dof, arm_horizontal_dof, arm_shoulder_dof, arm_elbow_dof]
}

# C. SENSORS
# none

# D. SENSING DICT
sensing_dict = {
    # "/dev/cu.usbmodem14101": [bump_sensor]
}

# E. ROBOT
# --> ip, control_dict, sensing_dict, control_channels <--
odile = Robot(odile_ip, control_dict, sensing_dict, control_channels)
