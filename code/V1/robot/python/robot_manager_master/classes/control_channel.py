
from classes.control_value import ControlValue


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


class PassThroughControlChannel(ControlChannel):
    def __init__(self):
        super(PassThroughControlChannel, self).__init__()
    #
    # def map_control_to_dof(self, control_value):
    #     return control_value


# 1. pass through: the most basic mapping is a 1-1 mapping between one control value and one DOF value.
# def pass_through(value):
#     return value
