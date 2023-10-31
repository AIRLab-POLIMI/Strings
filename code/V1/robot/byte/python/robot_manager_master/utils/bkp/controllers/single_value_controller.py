
from utils.bkp.controller_byte import ControllerByte
from enum import Enum


single_value_controller_key = 'sv'


class SingleValueControllerKeys(Enum):
    VAL = 0


class SingleValueController(ControllerByte):
    def __init__(self, unique_id, static_ip=True):
        super().__init__(SingleValueControllerKeys, unique_id, static_ip)
