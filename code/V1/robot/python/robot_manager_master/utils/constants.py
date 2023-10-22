
# --------------------------------------------------------- SERIAL
# DEFAULT serial parameters: SERIAL_ARDUINO class will be initialized with these values if not
serial_default_port = "/dev/ttyACM0"
serial_default_baud = 500000  # 115200
serial_default_timeout = 1  # in seconds
serial_default_delay_after_setup = 1  # in seconds
ARDUINO_READY_FLAG = "READY"
DEFAULT_SERIAL_ELAPSED = 0.0002  # in seconds

# --------------------------------------------------------- NETWORKING

DEFAULT_ESP_PORT = 4210

# Listen on Port: DEFAULT PORT of the socket connection of the raspberry
default_robot_port = 25666
# Size of receive buffer
default_buffer_size = 1024

DEFAULT_NETWORK_SEND_ELAPSED_SEC = 0.02  # in seconds
DEFAULT_MAX_CONSECUTIVE_MSG_READS = 3

DELIMITER = ":"
MSG_DELIMITER = '_'

# --------------------------------------------------------- CONTROL

min_control_value = 0
max_control_value = 255

# control value relative to value ZERO (center)
center_control_value = 127

# control targets with magnitude below this threshold are floored to zero
zero_threshold_control_value = 0.01



# --------------------------------------------------------- ROBOT SIZES

# --- Triskar Base
# MID
ROBOT_RADIUS_MID = 12.5
WHEEL_RADIUS_MID = 3.5
# BIG
ROBOT_RADIUS_BIG = 13.5
WHEEL_RADIUS_BIG = 5.0

