
import time
import keyboard

from utils.constants import default_rasp_port
from classes.network_channel import NetworkChannel
from classes.input_values import InputValues, InputValue
from utils.byte_helper import int_list_to_bytes


# Define the IP address and port your program is listening on
ip_address = "127.0.0.1"
port = 12345  # Replace with the actual port your program uses

destination_ip = ""
destination_port = default_rasp_port


network_channel = NetworkChannel(ip_address, port)
network_channel.setup_udp()

input_values = InputValues()
input_values.add_input_value(InputValue('r', 't'))
input_values.add_input_value(InputValue('a', 's'))
input_values.add_input_value(InputValue('n', 'm'))

while True:
    time.sleep(0.1)

    if keyboard.is_pressed('q'):
        break

    message = input_values.get_message()
    network_channel.write_udp(int_list_to_bytes(message), destination_ip, destination_port)
