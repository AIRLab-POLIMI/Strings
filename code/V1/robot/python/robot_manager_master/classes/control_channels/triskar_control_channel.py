
from configs.keys.control_value_keys import *
from utils.constants import max_control_value, center_control_value
from classes.control_channels.control_channel import ControlChannel
from classes.control_value import CenteredControlValue


class TriskarControlChannel(ControlChannel):
    def __init__(self, left_motor_dof, right_motor_dof, back_motor_dof, wheel_radius, robot_radius):
        super(TriskarControlChannel, self).__init__()

        left_motor_dof.set_bounds(-max_control_value, max_control_value)
        right_motor_dof.set_bounds(-max_control_value, max_control_value)
        back_motor_dof.set_bounds(-max_control_value, max_control_value)

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

        # get the max of the three targets
        max_abs = max([abs(forward_target), abs(strafe_target), abs(rot_target)])

        # sum values
        s = forward_target * self._1_C30 + strafe_target * 0.5 + rot_target

        # relative weights
        rw_f = forward_target / s
        rw_s = strafe_target / s
        rw_r = rot_target / s

        # new targets
        new_f = rw_f * max_control_value * self._1_C30
        new_s = rw_s * max_control_value * 0.5
        new_r = rw_r * max_control_value

        # back check
        temp_back_target = - new_s * 2 + new_r
        if abs(temp_back_target) > max_control_value:
            coeff = max_control_value / temp_back_target
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






        #
    # def compute_motors(self):
    #     # all the hard-coded values are test values with no particular meaning... but they work <3
    #
    #     print(f"[TriskarControlChannel][compute_motors] - "
    #           f"vals ::: C60_R: {self._C60_R} - C30_R: {self._C30_R} - m1R: '{self._m1_R}' - mLR: {self._mL_R} - "
    #           f"wheel radius: {self._wheelRadius} - robot radius: {self._robotRadius} - "
    #           f"strafe target: {self.control_value_dict[strafe_dof_key].get_current_value()} - "
    #           f"forward target: '{self.control_value_dict[forward_dof_key].get_current_value()}' - "
    #           f"rot target: {self.control_value_dict[rotation_dof_key].get_current_value()}")
    #
    #     dx12 = self._C60_R * self.control_value_dict[strafe_dof_key].get_current_value()
    #     dy12 = self._C30_R * self.control_value_dict[forward_dof_key].get_current_value()
    #     dthz123 = self._mL_R * self.control_value_dict[rotation_dof_key].get_current_value()
    #
    #     print(f"a: {dx12 + dy12 + dthz123} - b: {dx12 - dy12 + dthz123}")
    #
    #     self.right_motor_dof.on_new_value(dx12 + dy12 + dthz123)
    #     self.left_motor_dof.on_new_value(dx12 - dy12 + dthz123)
    #     self.back_motor_dof.on_new_value(
    #         (self._m1_R * self.control_value_dict[strafe_dof_key].get_current_value()) + dthz123)
    #
    #     print(f"[TriskarControlChannel][compute_motors] - "
    #           f"right motor value: '{self.right_motor_dof.get_message()}' - "
    #           f"left motor value: '{self.left_motor_dof.get_message()}' - "
    #           f"back motor value: '{self.back_motor_dof.get_message()}'")
    #
    #
    #
    #






    # the MAX possible absolute value for RIGHT and LEFT wheels is
    # max_abs(dx12) + max_abs(dy12) + max_abs(dthz123)
    # - max_abs(dx12) = abs(C60_R) * max_abs_strafe_speed_target
    # - max_abs(dy12) = abs(C30_R) * max_abs_forward_speed_target
    # - max_abs(dthz123) = abs(mL_R) * max_abs_rot_speed_target
    #
    # all of the three rotation targets maximum absolute values
    # are the max value of the control signal - the center value (control values are "centered control values").
    # if we consider that in case the control signal range is not EVEN, the CENTER VALUE is half the range rounded DOWN,
    # the max abs value of the signal is "MAX_CONTROL_SIGNAL_VALUE - CONTROL_SIGNAL_CENTER_VALUE".

    # the MAX possible absolute value for BACK wheel is
    # (abs(m1_R) * max_abs_strafe_speed_target) + max_abs(dthz123)
    # all values we already talked about

        max_abs_speed_target = max_control_value - center_control_value

        self.max_abs_RL_motor_val = (abs(self._C60_R) + abs(self._C30_R) + abs(self._mL_R)) * max_abs_speed_target
        self.max_abs_B_motor_val = (abs(self._m1_R) + abs(self._mL_R)) * max_abs_speed_target

        # LR_max_forward_target = abs(self._C60_R) * max_abs_speed_target
        # LR_max_strafe_target = abs(self._C60_R) * max_abs_speed_target
        #
        # B_max_strafe_target = abs(self._m1_R) * max_abs_speed_target
        #
        # max_rot_target = abs(self._mL_R) * max_abs_speed_target
        #
        # print(f"[TriskarControlChannel][INIT] "
        #       f"- max_abs_RL_motor_val: '{self.max_abs_RL_motor_val}' "
        #       f"- max_abs_B_motor_val: '{self.max_abs_B_motor_val}'\n")
        #
        # print(f"LR_max_forward_target: {LR_max_forward_target} - LR_max_strafe_target: '{LR_max_strafe_target}' - "
        #       f"B_max_strafe_target: {B_max_strafe_target} - max_rot_target: {max_rot_target}")

    # HOWEVER, as you can see, given the same speed targets, the contribution given by ROTATION is MUCH HIGHER than
    # the the ones given by FORWARD and STRAFE. This is because '_mL_R' is much bigger than the other quantities.
    # the ratios are different for the BACK and RIGHE/LEFT wheels, so we can't rescale the setpoints precisely.
    # as a rule of thumb, it's sufficient to SCALE DOWN the ROTATION INPUT by M
    # (usually 20: on left and right wheels, the rot constribution is 27 times higher than forward one
    # and ~15 times higher than the strafe one; on the back wheel, it's ~13 times higher than the strafe.
    # 20 is more or less in the middle and it seems to work).
    #
    # this means that the overall max_abs_rot_speed_target is max_abs_speed_target * 1/M.
    # We call 1/M = N and redo the computations:

    # the MAX possible absolute value for RIGHT and LEFT wheels is
    # max_abs(dx12) + max_abs(dy12) + max_abs(dthz123)
    # - max_abs(dx12) = abs(C60_R) * max_abs_speed_target
    # - max_abs(dy12) = abs(C30_R) * max_abs_speed_target
    # - max_abs(dthz123) = abs(mL_R) * max_abs_speed_target * N
    #
    # all of the three rotation targets maximum absolute values
    # are the max value of the control signal - the center value (control values are "centered control values").
    # if we consider that in case the control signal range is not EVEN, the CENTER VALUE is half the range rounded DOWN,
    # the max abs value of the signal is "MAX_CONTROL_SIGNAL_VALUE - CONTROL_SIGNAL_CENTER_VALUE".

    # the MAX possible absolute value for BACK wheel is
    # (abs(m1_R) * max_abs_strafe_speed_target) + max_abs(dthz123)
    # all values we already talked about

    # FINALLY: if we have a setpoint only on one axis, we'd like the WHOLE power of the motors to be available.
    # so, the one that should be limited between -max_abs_speed and max_abs_speed should be the SPEED VECTOR.
    # so, the three are fighting for the use of the "motor resource", weighted by their current_value/max_abs_value.

    # so, the three contributions need to be mapped into a common range,
    # then they are weighted according to the current relative weights of the three speeds every time one changes.





        #    - compute the relative weights rw_i of all the CV_i: rw_i = CV_i/(sum_I(CV_i)) (NB this keeps the sign)
        #    - compute the new CV'_i = rw_i * M_CV
        #    - the CV'_i are the new setpoints of the front wheels.
        #    - compute the CV'_i_B of the BACK wheels using the structure coefficients:

        #    ---> for the rotation, it's the same;
        #    ---> for strafe:
        #         - FRONT WHEEL: fwss = C60_R * strafe_speed
        #         - BACK WHEEL: bwss = m1_R * strafe_speed
        #    since we have fwss, strafe_speed = fwss / C60_r ---> bwss = fwss * (m1_R / C60_R)
        #    (m1_R / C60_R) = (-1 / wheel radius) * (wheel radius / 0.5) = -2
        #    so the contribution of strafe on the back wheel is always DOUBLE that on the front wheels.

        #    what could happen is that if only strafe is present, the value computed using the front wheels
        #    may be CAPPED by the back wheel.

        #    in case all three DOFS are at the max,
        #    two out of the three motors will get the max setpoint = "max_control_value", one frontal and the back,
        #    since the back will have no "forward" component but twice the strafe.
        #    ---> it means that motors can have values from -max_control_value to max_control_value, which need to be
        #         remapped to [0, 255].

        #    if there are only rotation and strafe, it may be possible that the value on the BACK wheel is CAPPED,
        #    meaning that it could exceed an absolute value of max_control_value.
        #    In that case, the ratio between the sum of 2*strafe and angular must be scaled down

