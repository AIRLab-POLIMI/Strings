
from configs.keys.control_value_keys import *
from utils.constants import max_centered_control_value
from classes.control_channels.control_channel import ControlChannel
from classes.control_value import CenteredControlValue


class TriskarControlChannel(ControlChannel):
    def __init__(self, left_motor_dof, right_motor_dof, back_motor_dof, wheel_radius, robot_radius):
        super(TriskarControlChannel, self).__init__()

        left_motor_dof.set_bounds(-max_centered_control_value, max_centered_control_value)
        right_motor_dof.set_bounds(-max_centered_control_value, max_centered_control_value)
        back_motor_dof.set_bounds(-max_centered_control_value, max_centered_control_value)

        self.control_value_dict[forward_dof_key] = CenteredControlValue(forward_dof_key)
        self.control_value_dict[strafe_dof_key] = CenteredControlValue(strafe_dof_key)
        self.control_value_dict[rotation_dof_key] = CenteredControlValue(rotation_dof_key)

        self.left_motor_dof = left_motor_dof
        self.right_motor_dof = right_motor_dof
        self.back_motor_dof = back_motor_dof

        self._wheelRadius = wheel_radius  # 3.5f //cm
        self._robotRadius = robot_radius  # 12.5f  //cm
        self._m1_R = -1.0 / wheel_radius
        self._mL_R = -robot_radius / wheel_radius
        self._C60_R = 0.500000000 / wheel_radius   # cos(60°) / R
        self._C30_R = 0.866025404 / wheel_radius   # cos(30°) / R

        self._1_C30 = 1.154700538  # 1/COS(30)

        print("C I A IA I")

    def update_dofs(self):

        dirty = False

        for control_value in self.control_value_dict.values():
            if control_value.dirty:
                dirty = True
                break

        if dirty:
            self.compute_motors()

    def compute_motors(self):

        # this method translated the CONTROL VALUES into DOF SETPOINTS for the single motors.
        # A. take the max of the three setpoints and set that as the maximum wrt the max possible value.
        #    - find the M_CV = max_i(abs(CV_i)). That will be the max abs value of the sum of the setpoints

        # if references are FS, SS and AS:
        #  - the amount of linear forward speed is given by the front wheels,
        #    which however multiply the reference by COS(30)
        #  - the amount of strafe is given by the back wheel entirely,
        #    and the front wheels follow by giving half of the setpoint as well
        #  - the amount of instantaneous linear speed from the angular setpoint
        #    is given direclty by all the wheels in the same way

        #  to euqalise the contributions, it should be:

        #  back_motor_setpoint = - strafe + angular
        #  front_motor_setpoints = strafe/2 +/- forward/COS(30) + angular (to make forard the same as the others)

        # so we compute the setpoints for both to see if one caps:
        # RELATIVE WEIGHTS FRONT WHEELS:
        # - find the one with highest absolute value (without coefficients) MCV,
        #   with max possible value = max_control_value
        # - sum s = (strafe/2 + forward/COS(30) + angular)
        # - find the relative weights r_i = CV_i / s  (NB signs are preserved!)
        # - get the new value for each signal: CV'_i = r_i * MCV * (their coefficient, if present: strafe has 1/2;)
        # - the max possible value is therefore max_control_value

        # IF 2 * CV'_strafe + CV'_angular < max_control_value, use these values;
        # ELSE, they must be further rescaled to accomodate the BACK wheel:

        # coeff = max_control_value / (2 * CV'_strafe + CV'_angular)
        # CV''_i = CV'_i * coeff

        # this way, the back wheel will be CAPPED, while the front wheels will have a lesser setpoint

        forward_target = self.control_value_dict[forward_dof_key].get_current_value()
        strafe_target = self.control_value_dict[strafe_dof_key].get_current_value()
        rot_target = self.control_value_dict[rotation_dof_key].get_current_value()

        if forward_target == 0 and strafe_target == 0 and rot_target == 0:
            # then no setpoint is given, set all the motors to zero and return
            self.right_motor_dof.on_new_value(0)
            self.left_motor_dof.on_new_value(0)
            self.back_motor_dof.on_new_value(0)

            return

        # get the max of the three targets
        max_abs = max([abs(forward_target), abs(strafe_target), abs(rot_target)])

        # sum values
        s = forward_target * self._1_C30 + strafe_target * 0.5 + rot_target

        # relative weights
        rw_f = forward_target / s
        rw_s = strafe_target / s
        rw_r = rot_target / s

        # new targets
        new_f = rw_f * max_centered_control_value * self._1_C30
        new_s = rw_s * max_centered_control_value * 0.5
        new_r = rw_r * max_centered_control_value

        # back check
        temp_back_target = - new_s * 2 + new_r
        if abs(temp_back_target) > max_centered_control_value:
            coeff = max_centered_control_value / temp_back_target
            new_f *= coeff
            new_s *= coeff
            new_r *= coeff

        self.right_motor_dof.on_new_value(new_f + new_s + new_r)
        self.left_motor_dof.on_new_value(-new_f + new_s + new_r)
        self.back_motor_dof.on_new_value(-2*new_s + new_r)

        print(f"[TriskarControlChannel][compute_motors] - "
              f"right motor value: '{self.right_motor_dof.get_message()}' - "
              f"left motor value: '{self.left_motor_dof.get_message()}' - "
              f"back motor value: '{self.back_motor_dof.get_message()}'")
