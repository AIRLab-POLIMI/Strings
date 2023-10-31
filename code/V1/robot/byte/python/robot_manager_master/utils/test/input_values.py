
import keyboard


# declare a class called inputValue
class InputValue:
    def __init__(self, key0, key180):
        self.key0 = key0
        self.key180 = key180
        self.current_value = 0

    def loop(self):
        if keyboard.is_pressed(self.key0):
            self.current_value = 0
        elif keyboard.is_pressed(self.key180):
            self.current_value = 180
        else:
            self.current_value = 90


class InputValues:
    def __init__(self):
        self.values = []

    def add_input_value(self, value):
        self.values.append(value)

    def get_message(self):
        message = []
        for value in self.values:
            value.loop()
            message.append(value.current_value)
        return message
