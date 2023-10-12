

class ControlValue:
    def __init__(self, key):
        self.key = key
        self.control_signal_key = None
        self.previous_value = -1000  # initialise to a value that is a number but different from any possible real val
        self.current_value = None
        self.dirty = True

    def set_control_signal_key(self, control_signal_key):
        self.control_signal_key = control_signal_key

    def get_current_value(self):
        self.dirty = False
        return self.current_value

    def on_value_rcv(self, new_value):
        self.current_value = new_value
        if self.current_value != self.previous_value:
            self.previous_value = self.current_value
            self.dirty = True
        self.dirty = False
