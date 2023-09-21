
from classes.controller import Controller
from enum import Enum

single_value_controller_key = 'sv'


class SingleValueControllerKeys(Enum):
    VAL = 0


class SingleValueController(Controller):
    def __init__(self):
        super().__init__(SingleValueControllerKeys)
