#ifndef dof_h
#define dof_h

#include <Arduino.h>
#include <Servo.h>


// define abstract class dof
class dof {
  public:
    // constructor
    dof();
 
  private:
    // servo
    Servo _servo;
    // min value
    int _min;
    // max value
    int _max;
};


class BasicDof {
  protected:

    int _min;
    int _max;
    float _normRange;

    int _currentValue;

  public: 
    BasicDof(int min, int max) {
      _min = min;
      _max = max;
      _normRange = (max - min)/255.0;
    }

    void OnNewValueReceived(uint8_t value) {
      // map the value from 0-255 to min-max
      int _currentValue =  static_cast<int>(value * _normRange) + _min;
    }
};

// define basicservodof that extends basicdof
class BasicServoDof : public BasicDof {
  private:
    // pin
    int _pin;
    // servo
    Servo _servo;
    // prev value
    int _prevValue;

  public:
    // implement constructor
    BasicServoDof(int pin, int min, int max) : BasicDof(min, max) {
      _pin = pin;
    }

    void Setup() {
      _servo.attach(_pin);
      _prevValue = _min - 1; // initialise the prev value to a value that is different from any possible input value
    }

    void OnNewValueReceived(uint8_t value) {
      // call the parent method
      BasicDof::OnNewValueReceived(value);
      // write the value to the servo if the value has changed
      if (_currentValue != _prevValue) {
        _servo.write(_currentValue);
        _prevValue = _currentValue;
      }
    }
};

class ServoDof {

  public:
  
    void Setup(int pin) {
        _pin = pin;
        _servo.attach(_pin);
        _currentAngle = -1;
    }

    void OnNewValueReceived(int value){
        if (value != _currentAngle) {
          _currentAngle = value;
          _servo.write(_currentAngle);
        }
    }

  private:
    // servo
    int _pin;
    Servo _servo;
    int _currentAngle;
};


#endif
