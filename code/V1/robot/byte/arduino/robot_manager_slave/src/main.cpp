#include <Arduino.h>
#include <OdileArms.h>
#include <TraskarModule.h>


OdileArms robotModule;
// TriskarModule robotModule;


void setup() {
  robotModule.setup();
}

void loop() {
  robotModule.loop();
}
