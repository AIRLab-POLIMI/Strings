#include <Arduino.h>
#include <esp_network_channel.h>
#include <potentiometer_control_value.h>

// ----------------------------------------------------------------------- WIFI

// setup with STATIC IP
IPAddress m_staticIP(192, 168, 1, 60); // ESP32 static ip
IPAddress robotIP(192, 168, 1, 2);       
int robotPort = 44444;

EspNetworkChannel esp_network_channel(m_staticIP, robotIP, robotPort);

// ----------------------------------------------------------------------- CONTROL VALUES

PotentiometerControlValue potentiometer(34);

ControlValue* control_values[] = {
    &potentiometer
};
int control_values_size = sizeof(control_values);

// ----------------------------------------------------------------------- SETUP

void setup() {
    Serial.begin(115200);
    Serial.println("STARTING SETUP");

    // ----------------------------------------------------------------------- WIFI

    // connect to wifi

    esp_network_channel.setup();

    // ----------------------------------------------------------------------- CONTROL VALUES

    // update control values
    for (int i = 0; i < sizeof(control_values)/sizeof(control_values[0]); i++) {
        control_values[i]->UpdateValue();
    }
}

// ----------------------------------------------------------------------- LOOP

void loop() {

    // ----------------------------------------------------------------------- CONTROL VALUES

    // update control values
    for (int i = 0; i < sizeof(control_values)/sizeof(control_values[0]); i++) {
        control_values[i]->UpdateValue();
    }

    // ----------------------------------------------------------------------- WIFI

    // send messages
    esp_network_channel.write_control_values_udp(control_values, control_values_size);
}