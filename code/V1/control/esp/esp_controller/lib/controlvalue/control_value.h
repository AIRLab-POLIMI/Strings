
#ifndef control_value_h
#define control_value_h


#include <Arduino.h>


class ControlValue
{
private:

    uint8_t _current_value;

public:

    uint8_t CurrentValue() {
        return _current_value;
    }

    void UpdateValue() {

    }
};

#endif