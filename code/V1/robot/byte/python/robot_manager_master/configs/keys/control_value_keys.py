

# --------------------------------------------------------- DOF KEYS
# control channel -> dof is assigned manually in the config file of each robot.
# string keys are required only for:

# - controllers: every signal of every controller needs to have an UNIQUE ID
# - control values: the values that are used to compute the DOF values. These are hard coded in each robot config.

# string keys are required for BOTH, because at runtime their MAPPING can be changed
# (so, which control signals influences which local "ControlValue" object)
# NB for controlChannles that are passthrough, the ControlValue Key is the same as the "dof" it controls direclty.


# rototranslation (triskar base
forward_dof_key = 'f'
strafe_dof_key = 's'
rotation_dof_key = 'r'

# head/eye (siid eye, odile head)
eye_x_dof_key = 'ex'
eye_y_dof_key = 'ey'

# siid
petals_dof_key = 'p'
led_dof_key = 'l'

# blackwing
wing_dof_key = 'w'
busto_dof_key = 'b'

# odile
neck_forward_dof_key = 'nf'
arm_horizontal_dof_key = 'ah'
arm_shoulder_dof_key = 'as'
arm_elbow_dof_key = 'ae'
