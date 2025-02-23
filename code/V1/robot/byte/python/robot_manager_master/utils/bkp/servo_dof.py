
from classes.dof import Dof
from utils.math_helper import map_range


# FOR INSANE ATTEMPTS AT OPTIMISATION
# Class to represent a degree of freedom composed of a single servo motor. It contains the min and max values.
# Based on the min and max values, the DOF knows how to behave when sending values to the servo motor.
# Values sent are always between 0 and 255. min >= 0 always, they are always positive angles (0 to 360 degrees).
#
# map the current value from the sensor range [0, 255] to the dof range [min, max]
# A. if max <= 255: scale the value from [0, 255] to [min, max]. Arduino will use the value as is.
# B. if max >= 255 AND (max-min) <= 255: scale the value from [0, 255] to [0, max-min]. Arduino will add min.
# C. pass through. Arduino will scale the value from [0, 255] to its own [min, max]

class ServoDof(Dof):
    def __init__(self, key, min_val, max_val):
        super().__init__(key)
        self.min = min_val
        self.max = max_val
        self.range = max_val - min_val

        self.scale = None
        if self.max <= 255:
            self.scale = self.scale_min_max
        elif self.range <= 255:
            self.scale = self.scale_min
        else:
            self.scale = self.pass_through

    def pass_through(self):
        return self.current_value

    def scale_min(self):
        return map_range(self.current_value, 0, 255, 0, self.range)

    def scale_min_max(self):
        return map_range(self.current_value, 0, 255, self.min, self.max)

    def get_message(self):
        return self.scale()
