
from classes.maestro import Maestro

from configs.robots.odile import odile

from configs.keys.control_value_keys import *

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - SETUP

# setup robot and default controllers
# robot = siid
# robot = test_robot
robot = odile

# setup the orchestrator of the robot
maestro = Maestro(robot)


# setup the controllers:
# 'maestro.add_control(AllController.yourcontrollername.value.id, control_id, dof_key)'
#
# test robot
def setup_controls():
    maestro.set_control_mapping(, petals_dof_key)

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN


def main():
    setup_controls()

    maestro.setup()

    while True:
        maestro.loop()


if __name__ == "__main__":
    main()
