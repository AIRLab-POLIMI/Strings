#ifndef sensor_h
#define sensor_h

#include <Arduino.h>


class Sensor
{
private:

    uint8_t _currentValue;

public:

    uint8_t GetCurrentValue() {
        return _currentValue;
    }

    void UpdateValue() {
    }
};


#endif
