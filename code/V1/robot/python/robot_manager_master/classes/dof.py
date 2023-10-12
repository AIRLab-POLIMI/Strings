
# DOF is the class with the value of a single SLAVE DOF. The SLAVE (e.g. Arduino) receives these values,
# rescales them, and then assigns them to the corresponding actuator (e.g. Motor, Light, Sound..) without
# any additional computation.

# the most basic DOF just sends to arduino the raw control value in [0, 255], and arduino will rescale it to its range


class Dof:

    def __init__(self):
        self.current_value = 0

    def on_new_value(self, value):
        # [safe programming] clamp value between 0 and 255
        if value < 0:
            value = 0
        elif value > 255:
            value = 255

        self.current_value = value

    def get_message(self):
        return self.current_value
