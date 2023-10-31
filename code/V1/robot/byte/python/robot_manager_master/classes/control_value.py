
# TODO have an input to the constructor that specifies the INITIAL VALUE
# TODO so that arduino is initialised correclty when the robot is booted

from utils.constants import min_control_value, max_control_value, center_control_value, zero_threshold_control_value


class ControlValue:
    def __init__(self, key):
        self.key = key
        self.previous_value = -1000  # initialise to a value that is a number but different from any possible real val
        self.current_value = None
        self.dirty = True

        self.reset_current_value()

    def reset_current_value(self):
        self.current_value = 0

    def get_current_value(self):
        self.dirty = False
        return self.current_value

    def on_msg_received(self, new_value):
        self.current_value = new_value
        if self.current_value != self.previous_value:
            self.previous_value = self.current_value
            self.dirty = True
        print(f"[ControlValue][on_msg_received] key: {self.key} - received: '{new_value}' - dirty: {self.dirty}")


class CenteredControlValue(ControlValue):
    def __init__(self, key):
        super(CenteredControlValue, self).__init__(key)

        self.current_value_raw = None
        self.previous_value_raw = -10000

    def reset_current_value(self):
        self.current_value_raw = 0
        super(CenteredControlValue, self).reset_current_value()

    def on_msg_received(self, new_value):
        self.current_value_raw = new_value
        if self.current_value_raw != self.previous_value_raw:
            self.previous_value_raw = self.current_value_raw

            # center the value
            self.current_value = new_value - center_control_value
            self.previous_value = self.current_value
            self.dirty = True

        print(f"[CenteredControlValue][on_msg_received] "
              f"key: {self.key} - received: '{new_value}' - current val: {self.current_value} - dirty: {self.dirty}")


class ScaledControlValue(ControlValue):
    def __init__(self, key, min_out, max_out):
        super(ScaledControlValue, self).__init__(key)

        self.min_out = min_out
        self.max_out = max_out

        self.range_ratio = (max_out - min_out) / (max_control_value - min_control_value)
        
        self.current_value_raw = None
        self.previous_value_raw = -10000
        
    def reset_current_value(self):
        self.current_value_raw = 0
        super(ScaledControlValue, self).reset_current_value()

    def on_msg_received(self, new_value):
        self.current_value_raw = new_value
        if self.current_value_raw != self.previous_value_raw:
            self.previous_value_raw = self.current_value_raw

            # map the value
            self.current_value = (new_value - min_control_value) * self.range_ratio + self.min_out
            if abs(self.current_value < zero_threshold_control_value):
                self.current_value = 0
            self.previous_value = self.current_value
            self.dirty = True

        print(f"[ScaledControlValue][on_msg_received] "
              f"key: {self.key} - received: '{new_value}' - current val: {self.current_value} - dirty: {self.dirty}")
