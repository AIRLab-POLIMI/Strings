
from classes.robot import Robot
from classes.dof import Dof
from classes.control_channels.control_channel import SingleControlChannel
from classes.control_channels.triskar_control_channel import TriskarControlChannel
from configs.keys.control_value_keys import *
from utils.constants import ROBOT_RADIUS_BIG, WHEEL_RADIUS_BIG
from classes.python_sensor import CameraPythonSensor


# config file for robot ODILE

# 0. IP
# odile_ip = "192.168.0.101"
odile_ip = "192.168.0.2"

# A. DOFS
# forward_dof = Dof()
# strafe_dof = Dof()
# rotation_dof = Dof()

# triskar base
wheel_left_dof = Dof()
wheel_right_dof = Dof()
wheel_back_dof = Dof()

# arms
head_x_dof = Dof()
head_y_dof = Dof()
neck_forward_dof = Dof()
arm_horizontal_dof = Dof()
arm_shoulder_dof = Dof()
arm_elbow_dof = Dof()

# A1. CONTROL CHANNELS
# forward_cv = SingleControlChannel(forward_dof_key, forward_dof)
# strafe_cv = SingleControlChannel(strafe_dof_key, strafe_dof)
# rotation_cv = SingleControlChannel(rotation_dof_key, rotation_dof)

triskar_cc = TriskarControlChannel(wheel_left_dof, wheel_right_dof, wheel_back_dof, WHEEL_RADIUS_BIG, ROBOT_RADIUS_BIG)

head_x_cc = SingleControlChannel(eye_x_dof_key, head_x_dof)
head_y_cc = SingleControlChannel(eye_y_dof_key, head_y_dof)
neck_forward_cc = SingleControlChannel(neck_forward_dof_key, neck_forward_dof)
arm_horizontal_cc = SingleControlChannel(arm_horizontal_dof_key, arm_horizontal_dof)
arm_shoulder_cc = SingleControlChannel(arm_shoulder_dof_key, arm_shoulder_dof)
arm_elbow_cc = SingleControlChannel(arm_elbow_dof_key, arm_elbow_dof)

control_channels = [triskar_cc,
                    head_x_cc, head_y_cc, neck_forward_cc, arm_horizontal_cc, arm_shoulder_cc, arm_elbow_cc]

# B. CONTROL DICT
control_dict = {
    # "/dev/cu.usbmodem14101": [forward_dof, strafe_dof, rotation_dof],
    "/dev/usb_device12": [wheel_right_dof, wheel_left_dof, wheel_back_dof],

    # "/dev/cu.usbmodem14102": [head_x_dof, head_y_dof,
    #                           neck_forward_dof, arm_horizontal_dof, arm_shoulder_dof, arm_elbow_dof]
}

# C. SENSORS
# none

# C1. PYTHON SENSORS
python_sensors = [CameraPythonSensor()]


# D. SENSING DICT
sensing_dict = {
    # "/dev/cu.usbmodem14101": [bump_sensor]
}


# E. ROBOT
# --> ip, control_dict, sensing_dict, control_channels, python_sensors <--
odile = Robot(odile_ip, control_dict, sensing_dict, control_channels, python_sensors)
