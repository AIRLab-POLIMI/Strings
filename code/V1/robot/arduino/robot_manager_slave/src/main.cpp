#include <Arduino.h>
#include <OdileArms.h>


OdileArms odile;


void setup() {
  odile.setup();
}

void loop() {
  odile.loop();
}
