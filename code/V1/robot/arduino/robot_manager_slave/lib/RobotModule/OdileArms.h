
#ifndef Included_OdileArms_H
#define Included_OdileArms_H


#include <Arduino.h>
#include <RobotModule.h>
#include <dof.h>
#include <sensor.h>


// PINS
#define HEAD_X_SERVO_PIN 2
#define HEAD_Y_SERVO_PIN 2
#define NECK_FORWARD_SERVO_PIN 2
#define ARM_HOR_SERVO_PIN 2
#define ARM_SHOULDER_SERVO_PIN 2
#define ARM_ELBOW_SERVO_PIN 2


class OdileArms : public RobotModule {
    
    public:
    
        OdileArms (): RobotModule(6, 0) {

        // DEFINE THE DOFS

        dofs[0] = new BasicServoDof(HEAD_X_SERVO_PIN, 0, 180);
        dofs[1] = new BasicServoDof(HEAD_Y_SERVO_PIN, 0, 180);
        dofs[2] = new BasicServoDof(NECK_FORWARD_SERVO_PIN, 0, 180);
        dofs[3] = new BasicServoDof(ARM_HOR_SERVO_PIN, 0, 180);
        dofs[4] = new BasicServoDof(ARM_SHOULDER_SERVO_PIN, 0, 180);
        dofs[5] = new BasicServoDof(ARM_ELBOW_SERVO_PIN, 0, 180);

        // DEFINE THE SENSORS
        // no sensors..
    }
};



#endif