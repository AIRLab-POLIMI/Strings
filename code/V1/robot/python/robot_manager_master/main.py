
from classes.maestro import Maestro

from configs.robots.test_robot import test_robot

from configs.controllers.all_controllers import *
from configs.controllers.single_value_controller import SingleValueControllerKeys

from configs.dof_keys import *

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SETUP

quest_ip = "192.168.0.101"


# setup robot and default controllers
# robot = siid
robot = test_robot

# setup the orchestrator of the robot
maestro = Maestro(robot, quest_ip)


# setup the controllers:
# 'maestro.add_control(AllController.yourcontrollername.value.id, control_id, dof_key)'
#
# test robot
def setup_controls():
    maestro.add_control(AllControllers.SingleValueController1.value.id,
                        SingleValueControllerKeys.VAL.value, petals_dof_key)

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN


def main():
    setup_controls()

    maestro.setup()

    while True:
        maestro.loop()


if __name__ == "__main__":
    main()
