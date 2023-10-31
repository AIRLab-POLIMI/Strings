
from enum import Enum

from utils.bkp.controllers.quest_controller import OculusQuestController
from utils.bkp.controllers.single_value_controller import SingleValueController


# this is a dictionary of all the controllers.
# here, controllers are linked to their IP uniquely.
# so that given an IP, the Maestro knows exactly what type of controller it is.
# if specific controllers have NO STATIC IP, use a unique identifier here instead,
# and the Maestro will know how to setup each of those controllers.

# set through enum for easy manual setup in main.py if needed

# enum of the unique identifiers of the controllers that have NO static IP
class DynamicIpControllerKeys(Enum):
    OculusQuest = "oq"


# assign UNIQUE controller enum name to its Controller object.
# - if IPs are static: use as IP the IP, and "static_ip" is True by default
# - if IPs are dynamic: use as IP a unique identifier, and "static_ip" is False by default
#   the maestro will check with all of them in SETUP and knows how to set the ip
class AllControllers(Enum):
    SingleValueController1 = SingleValueController("192.168.0.105")
    OculusQuestController = OculusQuestController(DynamicIpControllerKeys.OculusQuest.value, static_ip=False)


def get_controlled_by_id(controller_id):
    for controller in AllControllers:
        if controller.value.id == controller_id:
            return controller.value
    return None
