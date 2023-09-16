
from enum import Enum
from classes.controller import Controller

test_controller_key = 't'
test_controller_ip = "192.168.1.64"


class TestControllerKeys(Enum):
    K_1 = 0
    K_2 = 1
    K_3 = 2


class TestController(Controller):
    def __init__(self):
        super().__init__(TestControllerKeys, test_controller_ip)
