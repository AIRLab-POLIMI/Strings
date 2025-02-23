
#ifndef pot_control_h
#define pot_control_h

#include <Arduino.h>
#include <control_value.h>


class PotentiometerControlValue : public ControlValue
{
private:

    uint8_t _pin;
    float _normRange;

    // esp32 reads values in [0, 4095] and we need to map them to [0, 255]
    // the ranges are known in advance, so it's only a rescaling operation

    void Map(float read_value) {
        _current_value = static_cast<uint8_t> (read_value * _normRange);
    }

public:

    PotentiometerControlValue(uint8_t pin) {
        _pin = pin;
        _normRange = 255.0/4095.0;
    }

    void UpdateValue() override {
        // Serial.print("   [potentiometer control value][UpdateValue] Reading from pin ");
        // Serial.println(_pin);
        Map(analogRead(_pin));
        Serial.println(_current_value);
    }
};

#endif