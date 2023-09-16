
from configs.controllers.quest_controller import OculusQuestController, quest_controller_key
from configs.controllers.test_controller import TestController, test_controller_key

all_controllers = {
    quest_controller_key: OculusQuestController,
    test_controller_key: TestController
}
