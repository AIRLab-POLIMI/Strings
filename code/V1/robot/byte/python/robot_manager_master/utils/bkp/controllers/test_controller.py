
from enum import Enum
from utils.bkp.controller_byte import Controller

test_controller_key = 't'


class TestControllerKeys(Enum):
    K_1 = 0
    K_2 = 1
    K_3 = 2


class TestController(Controller):
    def __init__(self, unique_id, static_ip=True):
        super().__init__(TestControllerKeys, unique_id, static_ip)
