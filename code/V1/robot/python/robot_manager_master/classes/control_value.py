

class ControlValue:
    def __init__(self, key):
        self.key = key
        self.current_value = None

    def on_value_rcv(self, new_value):
        self.current_value = new_value
