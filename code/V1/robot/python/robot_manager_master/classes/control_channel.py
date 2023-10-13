
from classes.control_value import ControlValue
from configs.keys.control_value_keys import *


# when UDP messages come:
# - key-value message try to be extracted
#    - for EACH k-v message received:
#       - update the corresponding IN-DOF class
#          - at every SERIAL step, compute the OUT-DOFs

# A "ControlChannel" is a class that:
# - awaits for one or more specific KEY-VALUEs from UDP controllers
# - combines these values to compute the values of one or mode DOFs of the robot.
# It's an intermediate class between the RAW control signals and the RAW DOF values.

# Contains:
# - list of ControlValues
# - list of DOFs
# - functions to map ControlValues to DOFs

# ControlChannel are defined within the ROBOT configuration class.
# Each "robot" knows which Controls Signals to expect and how to combine them into DOFs.


# - ControlValues are updated every time a UDP message is received (on msg received)
# - DOF values are updated every time a SERIAL message needs to be sent (update dofs)
# new values are computed only if at least one ControlValue has changed since the last SERIAL message was sent.

# NB both the values from UDP and the DOF values are in range [0, 255],
#    so no rescaling is required for pass-through channels.


class ControlChannel:
    def __init__(self):
        self.control_value_dict = dict()

    def on_value_rcv(self, control_value_key, value):
        pass

    def update_dofs(self):
        pass


class SingleControlChannel(ControlChannel):
    def __init__(self, control_value_key, dof):
        super(SingleControlChannel, self).__init__()
        self.control_value = ControlValue(control_value_key)
        self.control_value_dict[control_value_key] = self.control_value
        self.dof = dof

    def update_dofs(self):

        dirty = False

        for control_value in self.control_value_dict.values():
            if control_value.dirty:
                dirty = True
                break

        if dirty:
            self.dof.value = self.control_value.get_current_value()  # this method cleans the dirty flag
            print(f"[SingleControlChannel][update_dofs] - "
                  f"key: {self.control_value.key} - "
                  f"signal key: {self.control_value.control_signal_key} "
                  f"new val: {self.dof.value} - dirty: {self.dof.dirty}")


class TriskarControlChannel(ControlChannel):
    def __init__(self, left_motor_dof, right_motor_dof, back_motor_dof, wheel_radius, robot_radius):
        super(TriskarControlChannel, self).__init__()
        self.control_value_dict[forward_dof_key] = ControlValue(forward_dof_key)
        self.control_value_dict[strafe_dof_key] = ControlValue(strafe_dof_key)
        self.control_value_dict[rotation_dof_key] = ControlValue(rotation_dof_key)

        self.left_motor_dof = left_motor_dof
        self.right_motor_dof = right_motor_dof
        self.back_motor_dof = back_motor_dof

        self._wheelRadius = wheel_radius  # 3.5f //cm
        self._robotRadius = robot_radius  # 12.5f  //cm
        self._m1_R = -1.0 / wheel_radius
        self._mL_R = -robot_radius / wheel_radius
        self._C60_R = 0.500000000 / wheel_radius   # cos(60°) / R
        self._C30_R = 0.866025404 / wheel_radius   # cos(30°) / R

    def update_dofs(self):

        dirty = False

        for control_value in self.control_value_dict.values():
            if control_value.dirty:
                dirty = True
                break

        if dirty:
            self.compute_motors()

    def compute_motors(self):
        # all the hard-coded values are test values with no particular meaning... but they work <3

        dx12 = self._C60_R * self.control_value_dict[strafe_dof_key].get_current_value() * 10
        dy12 = self._C30_R * self.control_value_dict[forward_dof_key].get_current_value() * 15
        dthz123 = self._mL_R * self.control_value_dict[rotation_dof_key].get_current_value() * 2

        self.right_motor_dof.value = dx12 + dy12 + dthz123
        self.left_motor_dof.value = dx12 - dy12 + dthz123
        self.back_motor_dof.value = (self._m1_R * self.control_value_dict[strafe_dof_key].get_current_value() * 10) \
                                    + dthz123