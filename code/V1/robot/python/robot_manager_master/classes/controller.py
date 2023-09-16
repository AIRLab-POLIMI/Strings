
from utils.byte_helper import bytes_to_ints


# dof messages are always all sent from raspberry to arduino, in the right order, even if a specific dof wasn't updated.
# what changes is which control values are received from the controllers through UDP.

# a controller can always change two possible things:
# - which values it is sending to the robot
# - which dofs correspond to each incoming value

# A controller is a virtual object that is:
# - an IP (the ip from whence the message is expected)
# - the names of the controls, in the right order in which they arrive.
#   This is used just for a more readable code and setup.
# - a "pos-dof" dict, that maps the position of the control value in the message to the dof it is mapped to.
# NB one controller is always one IP. The IP identifies the controller uniquely.

# The controller always sends all its possible values.
# in the config, each controller needs to be setup with the right amount of incoming values.
# the config then needs to map a value in a specific POSITION of the input message (like, "the third byte") to a
# specific DOF. This way, the controller can send all its values, and the config can decide which values to use.
# All bytes without an assigned dof are discarded.

# the control values are always enums, with name:position couples. For each controller, each control value is
# uniquely defined by its position value.


class Controller:
    def __init__(self, control_values, ip=None):
        self.control_keys = control_values
        self.ip = ip
        self.position_dof_dict = dict()

    def set_ip(self, ip):
        self.ip = ip

    def add_control(self, control_id, dof):
        # control id is already the position of the control value in the message
        # check if the control_id is valid
        valid = False
        for position in self.control_keys:
            if position.value == control_id:
                valid = True
                break

        if not valid:
            print(f"[CONTROLLER][ADD CONTROL] - ip: '{self.ip}' ERROR: control_id '{control_id}' is not valid. "
                  f"Valid values are: {[key.value for key in self.control_keys]}")
            return
        self.position_dof_dict[control_id] = dof

    def on_msg_received(self, bytes_msg):
        # 1. separate the bytes in a list of integers in [0, 255], one for each byte
        values = bytes_to_ints(bytes_msg)

        # 2. update the DOF values with the new values at the corresponding position n the position_dof_dict
        for position in self.position_dof_dict.keys():
            self.position_dof_dict[position].on_new_value(values[position])
