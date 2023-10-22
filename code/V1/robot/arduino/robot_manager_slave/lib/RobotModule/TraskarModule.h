
#ifndef Included_TriskarModule_H
#define Included_TriskarModule_H


#include <Arduino.h>
#include <RobotModule.h>
#include <dof.h>
#include <sensor.h>


// PINS
#define _MR_A_PIN 3
#define _MR_B_PIN 5
#define _ML_A_PIN 7
#define _ML_B_PIN 4
#define _MB_A_PIN 8
#define _MB_B_PIN 6


class TriskarModule: public RobotModule {

    public:
        TriskarModule (): RobotModule(3, 0) {

            // DEFINE THE DOFS

            dofs[0] = new BasicDcDofBig(_MR_A_PIN, _MR_B_PIN);
            dofs[1] = new BasicDcDofBig(_ML_A_PIN, _ML_B_PIN);
            dofs[2] = new BasicDcDofBig(_MB_A_PIN, _MR_B_PIN);

            // DEFINE THE SENSORS
            // no sensors..
        }
};


#endif
