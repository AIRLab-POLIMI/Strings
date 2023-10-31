
#ifndef serialchannel_h
#define serialchannel_h


#include <Arduino.h>

// class serialchannel that handles all the serial communication
class SerialChannel {
  public:

    // current data
    String current_line;
    
    // setup
    void Setup(unsigned long baudRate) {
        Serial.begin(baudRate);
    }

    // ------------------ ------------------ ------------------ ------------------ READ

    bool read_line() {
        if (Serial.available() > 0) {
            current_line = Serial.readStringUntil('\n');
            return true;    
        }
        else
            return false;
    }

    uint8_t read_byte() {
        return Serial.read();
    }

    // ------------------ ------------------ ------------------ ------------------ WRITE

    void write_line(String line) {
        Serial.println(line);
    }

    void write_byte(uint8_t byte) {
        Serial.write(byte);
    }

    // ------------------ ------------------ ------------------ ------------------ FLAGS

    bool n_available(int num_bytes) {
        return Serial.available() >= num_bytes;
    }


};


#endif