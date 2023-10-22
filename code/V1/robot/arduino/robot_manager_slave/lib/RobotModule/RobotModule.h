
#ifndef Included_RobotModule_H
#define Included_RobotModule_H


#include <Arduino.h>
#include <serial_channel.h>
#include <dof.h>
#include <sensor.h>


// ------------------ ------------------ ------------------ ------------------ TO USE

// the actual DOFS and SENSORS are defined in the constructor of the specific class that defines the robot. 
// IMPORTANT: the dofs/sensors are added to the correpsonidng arrays. They need to be in the SAME ORDER as they are in the PYTHON program. 
// Assignment of values depends only on ORDER. 


// ------------------ ------------------ ------------------ ------------------ SETUP 

#define BAUD_RATE 500000
String PYTHON_PRESENTATION = "READY";
#define PRESENTATION_RESPONSE "READY"

// if after 'MAX_WATCHDOG_ELAPSED_TIME_MILLIS' time in milliseconds there is no input signal from serial, 
// assume central control has died, and stop everything
#define MAX_WATCHDOG_ELAPSED_TIME_MILLIS 2000



// generate a RobotModule class
class RobotModule
{
    protected:
        // ------------------ ------------------ ------------------ ------------------ VARIABLES

        // dofs
        Dof **dofs;
        // compute the number of elements in the dofs array
        uint8_t dofs_count;

        // sensors
        Sensor **sensors;
        uint8_t sensors_count;


    private:
        // ------------------ ------------------ ------------------ ------------------ VARIABLES

        // serial
        SerialChannel serialChannel;

        // watchdog
        long current_time;
        long last_command_time;


        // ------------------ ------------------ ------------------ ------------------ PRESENTATION

        void presentation() {
            // present to the python controller: 
            // 1. await presentation message via serial
            // 2. respond with presentation message via serial

            while (true)
            {
                if (serialChannel.read_line() && serialChannel.current_line == PYTHON_PRESENTATION) {
                    // if the message is the presentation message, respond with the presentation response
                    serialChannel.write_line(PRESENTATION_RESPONSE);
                    break;
                }
                else {
                    // if the message is not the presentation message, await the presentation message
                    delay(100);
                    // serialChannel.write_line("WRONG message: ");
                    // serialChannel.write_line(serialChannel.current_line);
                    // serialChannel.write_line(" - right message is: ");
                    // serialChannel.write_line(PYTHON_PRESENTATION);
                }
            }
        }

        // ------------------ ------------------ ------------------ ------------------ GET DOF COMMANDS

        bool get_dof_commands() {
            // get the commands for each dof
            if (!serialChannel.n_available(dofs_count))
                return false;

            for (size_t i = 0; i < dofs_count; i++)
            {
                // read the line
                uint8_t new_val = serialChannel.read_byte();
                // send the value to the dof
                dofs[i]->OnNewValueReceived(new_val);
            }
            return true;
        }

        // ------------------ ------------------ ------------------ ------------------ SENSOR DATA

        void update_sensor_data() {
            // update the sensor data if present
            for (size_t i = 0; i < sensors_count; i++)
            {
                // update the sensor value
                sensors[i]->UpdateValue();
            }
        }

        void send_sensor_data() {
            // send the sensor data if present
            for (size_t i = 0; i < sensors_count; i++)
            {
                // send the sensor value
                serialChannel.write_byte(sensors[i]->GetCurrentValue());
            }
        }

        // ------------------ ------------------ ------------------ ------------------ WATCHDOG

        void stop_everything() {
            // - set all speeds to 0 for DOFs of type "Speed"
            // - update also 'last_command_time' so that the watchdog_tick method does not keep triggering 
            // (but only will every 'MAX_WATCHDOG_ELAPSED_TIME' ms if nothing happens)
            
            for (size_t i = 0; i < dofs_count; i++)
            {
                if (dofs[i]->IsSpeed())
                    dofs[i]->SetToZero();
            }
            
            last_command_time = millis();
        }

        void watchdog_tick() {
            current_time = millis();
            
            if (current_time - last_command_time > MAX_WATCHDOG_ELAPSED_TIME_MILLIS){
                stop_everything();
            }
        }


    public: 

        // ------------------ ------------------ ------------------ ------------------ MAIN 

        RobotModule(int dofcount, int sensorcount) {
            dofs_count = dofcount;
            sensors_count = sensorcount;

            dofs = new Dof*[dofs_count];

            sensors = new Sensor*[sensors_count];
        }

        ~RobotModule() {
            delete[] dofs;
            delete[] sensors;
        }

        void setup() {

            // serial setup
            serialChannel.Setup(BAUD_RATE);

            // setup DOFS
            for (size_t i = 0; i < dofs_count; i++)
            {
                // Serial.println(dofs[i]->IsSpeed());
                dofs[i]->Setup();
            }
            
            // present to the python controller
            presentation();

            // await cooldown time
            delay(1000);
        }

        void loop() {
            // A. update sensors
            update_sensor_data();

            // B. try to read from serial. 
            // - if there is no data: make a watchdog tick
            // - if there is data: READ it and send it to the dofs, then update the last_command_time, then await 2 ms and SEND the sensor data if present
            if (!get_dof_commands()){
                watchdog_tick();

            } else {
                // update the last command time
                last_command_time = millis();
                // await 2 ms
                delay(2);
                // send the sensor data if present
                send_sensor_data();
            }
        }
};


#endif