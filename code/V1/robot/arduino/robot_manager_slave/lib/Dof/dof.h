
#ifndef dof_h
#define dof_h

#include <Arduino.h>
#include <Servo.h>


#define MAX_DC_MOTOR_SIGNAL 255



class Dof {
  protected:

    bool _isSpeed;
  
  public:
    
    virtual ~Dof() {}  // Declare a virtual destructor for the base class

    Dof(bool isSpeed){
      _isSpeed = isSpeed;
    }

    bool IsSpeed() {
      return _isSpeed;
    }

    virtual bool OnNewValueReceived(uint8_t value) = 0;

    virtual void Setup() = 0;

    virtual void SetToZero() = 0;
};


class BasicDof: public Dof {
  protected:

    int _min;
    int _max;
    float _normRange;

    uint8_t _prevValueRaw;
    int _currentValue;

  public: 
    BasicDof(int min, int max, bool isSpeed): Dof(isSpeed) {
      _min = min;
      _max = max;
      _normRange = (max - min)/255.0;
    }

    bool OnNewValueReceived(uint8_t value) {
      // IF the value is new, map the value from 0-255 to min-max
      if (value != _prevValueRaw){
        _currentValue =  static_cast<int>(value * _normRange) + _min;
        _prevValueRaw = value;
        return true;
      }
      return false;
    }

    void SetToZero() {
      // nothing happends, stay where it was, it's a POSITION dof
    }
};

// define basicservodof that extends basicdof
class BasicServoDof : public BasicDof {
  private:
    // pin
    int _pin;
    // servo
    Servo _servo;

  public:
    // implement constructor
    BasicServoDof(int pin, int min, int max) : BasicDof(min, max, false) {
      _pin = pin;
    }

    void Setup() {
      _servo.attach(_pin);
    }

    bool OnNewValueReceived(uint8_t value) {
      // call the parent method
      // write the value to the servo if the value has changed. 
      // NB check happens in the PARENT method using the raw value
      if (BasicDof::OnNewValueReceived(value)){
        _servo.write(_currentValue);
        return true;
      }
      return false;
    }
};



class BasicDcDof: public Dof {
  protected:

    int _pinA;
    int _pinB;

    uint8_t _prevValueRaw;
    int _currentValue;

  public: 

    BasicDcDof(int pinA, int pinB): Dof(true) {
      _pinA = pinA;
      _pinB = pinB;
    }

    void Setup() {
      pinMode(_pinA, OUTPUT);
      pinMode(_pinB, OUTPUT);
    }

    bool OnNewValueReceived(uint8_t value) {
      // IF the value is new, map the value from 0-255 to min-max
      if (value != _prevValueRaw){
        _currentValue =  value;
        _prevValueRaw = value;

        SetMotorSpeed(_currentValue);

        return true;
      }

      return false;
    }

    void SetToZero() {
      OnNewValueReceived(127);
    }

    virtual void SetMotorSpeed(int speed) = 0;
};

// class BasicDcDofMid inherits from BasicDcDof
class BasicDcDofMid : public BasicDcDof {

  public:

    BasicDcDofMid(int pinA, int pinB) : BasicDcDof(pinA, pinB){}

    void SetMotorSpeed(int speed) {
      // Make sure the speed is within the limit.
      if (speed > MAX_DC_MOTOR_SIGNAL) {
          speed = MAX_DC_MOTOR_SIGNAL;
      } else if (speed < -MAX_DC_MOTOR_SIGNAL) {
          speed = -MAX_DC_MOTOR_SIGNAL;
      }
      
      // Set the speed and direction.
      if (speed >= 0) {
          analogWrite(_pinA, speed);
          analogWrite(_pinB, 0);
      } else {
          analogWrite(_pinA, 0);
          analogWrite(_pinB, -speed);
      }
    }
};



// class BasicDcDofMid inherits from BasicDcDof
class BasicDcDofBig : public BasicDcDof {

  public:

    BasicDcDofBig(int pinA, int pinB) : BasicDcDof(pinA, pinB){}

    // add code to the Setup method of the Base class BasicDcDof
    void Setup() {
      pinMode(_pinA, OUTPUT);
      pinMode(_pinB, OUTPUT);
    
      TCCR2B = TCCR2B & B11111000 | B0000001; // for PWM frequency of 31372.55 Hz
      TCCR1B = TCCR1B & B11111000 | B0000001;
      TCCR0B = TCCR0B & B11111000 | B0000001;
    }


    void SetMotorSpeed(int speed) {
      // Make sure the speed is within the limit.
      if (speed > MAX_DC_MOTOR_SIGNAL) {
          speed = MAX_DC_MOTOR_SIGNAL;
      } else if (speed < -MAX_DC_MOTOR_SIGNAL) {
          speed = -MAX_DC_MOTOR_SIGNAL;
      }
      
      // Serial.print("[set_wheel_speed] - speed: ");
      // Serial.print(speed);
      // Serial.print(" - pinA: ");
      // Serial.print(pinA);
      // Serial.print(" - pinB: ");
      // Serial.println(pinB);
      
      // Set the speed and direction.
      if (speed >= 0) {
          analogWrite(_pinA, speed);
          digitalWrite(_pinB, true);
      } else {
          analogWrite(_pinA, -speed);
          digitalWrite(_pinB, false);
      }
    }
};


#endif
