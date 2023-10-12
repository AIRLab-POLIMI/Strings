

# when UDP messages come:
# - key-value message try to be extracted
#    - for EACH k-v message received:
#       - update the corresponding IN-DOF class
#          - at every SERIAL step, compute the OUT-DOFs

# A "Controller" is a class that:
# - awaits for one or more specific KEY-VALUEs from UDP controllers
# - combines these values to compute the values of one or mode DOFs of the robot.
# It's an intermediate class between the RAW control signals and the RAW DOF values.

# Contains:
# - list of ControlValues
# - list of DOFs
# - functions to map ControlValues to DOFs

# Controllers are defined within the ROBOT configuration class.
# Each "robot" knows which Controls Signals to expect and how to combine them into DOFs.


class Controller:
    def __init__(self):
        pass




class PassThroughController(Controller):
    def __init__(self):
        super(PassThroughController, self).__init__()
    #
    # def map_control_to_dof(self, control_value):
    #     return control_value


# 1. pass through: the most basic mapping is a 1-1 mapping between one control value and one DOF value.
# def pass_through(value):
#     return value