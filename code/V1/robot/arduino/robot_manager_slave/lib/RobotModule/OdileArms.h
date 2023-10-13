
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
    OdileArms () {

        // int dofsSize = 3;
        // int sensorsSize = 2;

        // dofs = new A[dofsSize];
        // sensors = new A[sensorsSize];

        // DEFINE THE DOFS
        BasicServoDof head_x = BasicServoDof(HEAD_X_SERVO_PIN, 0, 180);
        BasicServoDof head_y = BasicServoDof(HEAD_Y_SERVO_PIN, 0, 180);
        BasicServoDof neck_forward = BasicServoDof(NECK_FORWARD_SERVO_PIN, 0, 180);
        BasicServoDof arm_horizontal = BasicServoDof(ARM_HOR_SERVO_PIN, 0, 180);
        BasicServoDof arm_shoulder = BasicServoDof(ARM_SHOULDER_SERVO_PIN, 0, 180);
        BasicServoDof arm_elbow = BasicServoDof(ARM_ELBOW_SERVO_PIN, 0, 180);
        
        BasicServoDof tempDofs[] = {head_x, head_y, neck_forward, arm_horizontal, arm_shoulder, arm_elbow};
        dofs = tempDofs;
    
        // DEFINE THE SENSORS
        Sensor sensor;

        Sensor tempSensors[] = {sensor};
        sensors = tempSensors;
    }
};



#endif