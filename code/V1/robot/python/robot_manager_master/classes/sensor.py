

class Sensor:
    def __init__(self, key):
        self.current_value = 0
        self.key = key

    def on_new_value(self, value):
        self.current_value = value

    def get_message(self):
        return self.current_value


# sensor values coming from arduino serial port. They are in the range [0, 255]

class InSensorSerial(Sensor):
    def __init__(self, key):
        super().__init__(key)

    def on_new_value(self, value):
        # [safe programming] clamp value between 0 and 255
        if value < 0:
            value = 0
        elif value > 255:
            value = 255

        self.current_value = value
