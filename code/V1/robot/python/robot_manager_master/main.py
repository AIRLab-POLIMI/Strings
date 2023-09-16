
from classes.maestro import Maestro
from configs.robots.test_robot import test_robot
from configs.controllers.test_controller import test_controller_key, TestControllerKeys
from configs.dof_keys import *

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SETUP

quest_ip = "192.168.0.101"


# setup robot and default controllers
# robot = siid
robot = test_robot

# setup the orchestrator of the robot
maestro = Maestro(robot, quest_ip)

# setup the controllers: 'add_control(self, controller_key, control_id, dof_key)'
#
# siid
# maestro.add_control(quest_controller_key, QuestControllerKeys.JOY_Y_LEFT.value, forward_dof_key)
# maestro.add_control(quest_controller_key, QuestControllerKeys.JOY_X_LEFT.value, rotation_dof_key)
# maestro.add_control(quest_controller_key, QuestControllerKeys.HEAD_X.value, eye_x_dof_key)
# maestro.add_control(quest_controller_key, QuestControllerKeys.HEAD_Y.value, eye_y_dof_key)
# maestro.add_control(quest_controller_key, QuestControllerKeys.TRIG_RIGHT.value, petals_dof_key)
# maestro.add_control(quest_controller_key, QuestControllerKeys.GRIP_RIGHT.value, led_dof_key)

# test
maestro.add_control(test_controller_key, TestControllerKeys.K_1.value, petals_dof_key)
maestro.add_control(test_controller_key, TestControllerKeys.K_2.value, busto_dof_key)


# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN


def main():
    maestro.setup()

    while True:
        maestro.loop()


if __name__ == "__main__":
    main()
