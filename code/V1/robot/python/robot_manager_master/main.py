
import time

from classes.maestro import Maestro

from configs.robots.odile import odile

from configs.keys.control_value_keys import *
from configs.keys.control_signal_keys import *

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
    # --> control_signal_key, control_value_key <--

    # head
    maestro.set_control_mapping(HEAD_X_KEY, eye_x_dof_key)
    maestro.set_control_mapping(HEAD_Y_KEY, eye_y_dof_key)

    maestro.set_control_mapping(JOY_LEFT_VR_X_KEY, rotation_dof_key)
    maestro.set_control_mapping(JOY_LEFT_VR_Y_KEY, forward_dof_key)

# - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - -  - - - - - MAIN

def main():
    setup_controls()

    maestro.setup()

    while True:

        # a = time.time()

        maestro.loop()

        # print(f"loop time: '{time.time() - a}")


if __name__ == "__main__":
    main()
