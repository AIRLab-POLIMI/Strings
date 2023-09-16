
from classes.controller import Controller
from enum import Enum

quest_controller_key = 'q'


class QuestControllerKeys(Enum):
    JOY_X_RIGHT = 0
    JOY_Y_RIGHT = 1
    TRIG_RIGHT = 2
    GRIP_RIGHT = 3
    ANGLE_X_RIGHT = 4
    ANGLE_Y_RIGHT = 5
    ANGLE_Z_RIGHT = 6
    JOY_X_LEFT = 7
    JOY_Y_LEFT = 8
    TRIG_LEFT = 9
    GRIP_LEFT = 10
    ANGLE_X_LEFT = 11
    ANGLE_Y_LEFT = 12
    ANGLE_Z_LEFT = 13
    HEAD_X = 14
    HEAD_Y = 15


class OculusQuestController(Controller):
    def __init__(self):
        super().__init__(QuestControllerKeys)
