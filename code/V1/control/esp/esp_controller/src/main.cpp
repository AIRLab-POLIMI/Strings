#include <Arduino.h>
#include <esp_network_channel.h>
#include <potentiometer_control_value.h>


// ----------------------------------------------------------------------- VARIABLES

#define SEND_ELAPSED_MILLIS 30
ulong last_sent_time;


// ----------------------------------------------------------------------- WIFI

// setup with STATIC IP
IPAddress m_staticIP(192, 168, 0, 105); // ESP32 static ip
IPAddress robotIP(192, 168, 0, 102);       
int robotPort = 44444;

EspNetworkChannel esp_network_channel(m_staticIP, robotIP, robotPort);

// ----------------------------------------------------------------------- CONTROL VALUES

PotentiometerControlValue potentiometer(34);

ControlValue* control_values[] = {
    &potentiometer
};
int num_control_values = sizeof(control_values)/sizeof(control_values[0]);

// ----------------------------------------------------------------------- SETUP

void setup() {
    Serial.begin(115200);
    Serial.println("STARTING SETUP");
    delay(1000);

    // ----------------------------------------------------------------------- WIFI

    // connect to wifi
    esp_network_channel.setup();
}

// ----------------------------------------------------------------------- LOOP

void loop() {

    // ----------------------------------------------------------------------- CONTROL VALUES

    // if time has elapsed, get values and send them
    ulong now = millis();
    if (now - last_sent_time > SEND_ELAPSED_MILLIS) {

        // update control values
        for (int i = 0; i < num_control_values; i++) {
            // Serial.print("Updating control value ");
            // Serial.println(i);
            control_values[i]->UpdateValue();
        }

        esp_network_channel.write_control_values_udp(control_values, num_control_values);
        last_sent_time = now;
    }
}
