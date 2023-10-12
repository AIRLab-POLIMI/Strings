
#include <Arduino.h>


#ifdef ESP32
    #include <WiFi.h>
#else 
    #ifdef ESP8266
        #include <ESP8266WiFi.h>
        #include <string>
    #endif
#endif

#include <WiFiUdp.h>
#include <control_value.h>


using namespace std;


// ******** IP - DOF MAPPINGS
//
// ***TENDONS
// IP = 192.168.1.60 -> TENDON
// IP = 192.168.1.61 -> GLOVE ACCELEROMETER

// ----------------------------------------------------------------------------------------------DEFAULTS
// STATIC IP
IPAddress default_gateway(192, 168, 1, 1);   // IP Address of your WiFi Router (Gateway)
IPAddress default_subnet(255, 255, 255, 0);  // Subnet mask
IPAddress default_primaryDNS(192, 168, 1, 1);  // DNS 1
IPAddress default_secondaryDNS(8, 8, 8, 8);  // DNS 2
// WIFI
#define WIFI_SSID "Physical Metaverse 2.4GHz"
#define WIFI_PSW "earthbound"
// UDP
#define MY_UDP_PORT 4210
#define IN_SIZE 255
#define OUT_SIZE 255


class EspNetworkChannel {

    private:

    // ----------------------------------------------------------------------- WIFI 

        void connect_to_wifi(){

            Serial.println("[CONNECT TO WIFI] - begin");

            // Prevent connecting to wifi based on previous configuration
            WiFi.disconnect();  

            // setup with STATIC IP
            bool wifi_configured = false;
            while (!wifi_configured)
            {
                if (!WiFi.config(m_staticIP, m_gateway, m_subnet, m_primaryDNS, m_secondaryDNS)) {
                Serial.println("[CONNECT TO WIFI] - failed to configure STATIC IP");
                delay(1000);
                } else {
                wifi_configured = true;
                Serial.println("[CONNECT TO WIFI] - configured STATIC IP");
                }
            }

            // set the ESP32 to be a WiFi-client
            WiFi.mode(WIFI_STA);
            WiFi.begin(WIFI_SSID, WIFI_PSW);

            // Attempting connection to WiFi
            Serial.println("Trying to connect ...");
            while (WiFi.status() != WL_CONNECTED) {               
                Serial.print(" .. not yet connected - current wifi status/connected status: ");
                Serial.print(WiFi.status());
                Serial.println("/");
                Serial.println(WL_CONNECTED);
                delay(500);
            }

            // notify being connected to WiFi;
            Serial.print("Connected to Local Network - ESP IP: ");
            Serial.println(WiFi.localIP());

            WiFi.setAutoReconnect(true);
            WiFi.persistent(true);

            // Begin listening to UDP port
            UDP.begin(MY_UDP_PORT);
            Serial.print("UDP on:");
            Serial.println(MY_UDP_PORT);

            // turn WIFI led ON: WIFI connection successful
            digitalWrite(m_ledPinWiFi, HIGH);
            Serial.println("[CONNECT TO WIFI] - complete\n");
        }


    public: 

        // STATIC IP
        IPAddress m_staticIP; 
        IPAddress m_gateway;
        IPAddress m_subnet;
        IPAddress m_primaryDNS;
        IPAddress m_secondaryDNS;

        // WIFI
        char * m_wifi_ssid;  // should contain CHAR ARRAY
        char * m_wifi_psw;  // should contain CHAR ARRAY

        // UDP
        WiFiUDP UDP;
        char in_packet[IN_SIZE];
        char out_packet[OUT_SIZE];
        IPAddress m_defaultDestinationIP;  // should contain CHAR ARRAY
        int m_defaultDestinationPort;
        int m_in_size;
        int m_out_size;

        // LEDS
        uint8_t m_ledPinOn;    // LED ON when ESP ON
        uint8_t m_ledPinWiFi;  // LED ON when ESP connected to WIFI 
        uint8_t m_ledPinFunction; // additional LED for class-specific purposes (i.e. while button pressed)
        uint8_t leds[3];
        int numLeds;

        // VARS
        bool received;


        EspNetworkChannel(
            IPAddress staticIP, 
            IPAddress defaultDestinationIP, 
            int defaultDestinationPort = MY_UDP_PORT,  // the default is that all ESPs use the same port!

            char wifi_ssid[] = WIFI_SSID,
            char wifi_psw[] = WIFI_PSW,

            int in_size = IN_SIZE,
            int out_size = OUT_SIZE,

            IPAddress gateway = default_gateway, 
            IPAddress subnet = default_subnet, 
            IPAddress primaryDNS = default_primaryDNS, 
            IPAddress secondaryDNS = default_secondaryDNS) {

            // STATIC IP
            m_staticIP  = staticIP; 
            m_gateway = gateway;
            m_subnet = subnet;
            m_primaryDNS = primaryDNS;
            m_secondaryDNS = secondaryDNS;

            // WIFI
            m_wifi_ssid = wifi_ssid;
            m_wifi_psw = wifi_psw;

            // UDP
            m_defaultDestinationIP = defaultDestinationIP;
            m_defaultDestinationPort = defaultDestinationPort;
            m_in_size = in_size;
            m_out_size = out_size;

            // VARS
            received = false;
        }

    // ----------------------------------------------------------------------- GETTERS

        IPAddress remoteIP() {
            return UDP.remoteIP();
        }

        int remotePort() {
            return UDP.remotePort();
        }

    // ----------------------------------------------------------------------- UDP

    // ------------------------------------------------------ WRITE
       
        #ifdef ESP32
                void add_char_msg_to_packet(char msg[]) {
                    UDP.print(msg);
                }
        #else 
            #ifdef ESP8266
                void add_char_msg_to_packet(char msg[]) {
                    UDP.write(msg);
                }
            #endif
        #endif

        void write_char_udp(char msg[], IPAddress ip, int port){
            UDP.beginPacket(ip, port);
            add_char_msg_to_packet(msg);  // 
            UDP.endPacket();
        }

        void write_char_udp(char msg[], bool remote = false){
            try {
                IPAddress ip = remote ? UDP.remoteIP() : m_defaultDestinationIP;    
                int port = remote ? UDP.remotePort() : m_defaultDestinationPort;

                write_char_udp(msg, ip, port);
            }
            catch(const std::exception & e)
            {
                Serial.print("[WRITE CHAR UDP] - ERROR: '");
                Serial.print(e.what());
                Serial.println("'");
            }
        }

        void write_String_udp(String msg, IPAddress ip, int port){
            UDP.beginPacket(ip, port);
            UDP.print(msg);
            UDP.endPacket();
        }

        void write_string_udp(std::string msg, IPAddress ip, int port) {
            UDP.beginPacket(ip, port);
            UDP.print(msg.c_str());
            UDP.endPacket();
        }

        void write_int_udp(int value) {
            write_int_udp(value, m_defaultDestinationIP, m_defaultDestinationPort);
        }

        void write_int_udp(int value, IPAddress ip, int port){
            itoa(value, out_packet, 10);
        
            write_char_udp(out_packet, ip, port);
        }

        void write_bytes_int_udp(uint8_t int_bytes[], int byte_size, IPAddress ip, int port) {
            UDP.beginPacket(ip, port);
            UDP.write(int_bytes, byte_size);
            UDP.endPacket();
        }
    
        void write_control_values_udp(ControlValue* control_values[], int num_control_values, IPAddress ip, int port) {
            UDP.beginPacket(ip, port);
            for (int i = 0; i < num_control_values; i++) {
                Serial.print(" [write_control_values_udp] Writing control value ");
                Serial.print(i);
                Serial.print(" - value: ");
                Serial.println(control_values[i]->CurrentValue());
                UDP.write(control_values[i]->CurrentValue());
            }
            UDP.endPacket();
        }

        void write_control_values_udp(ControlValue* control_values[], int num_control_values) {
            write_control_values_udp(control_values, num_control_values, m_defaultDestinationIP, m_defaultDestinationPort);
        }

    // ------------------------------------------------------ READ
       
        bool read_udp_non_blocking(){
            
            int packetSize = UDP.parsePacket();

            received = false;
            
            if (packetSize) {
                Serial.print("Received packet! Size: ");
                Serial.println(packetSize); 
                int len = UDP.read(in_packet, IN_SIZE);  // the value is written in the BUFFER specificed as the first argument ("in_packet" in our case)
                if (len > 0)
                {
                    in_packet[len] = '\0';
                    received = true;
                }
                Serial.print("Packet received: ");
                Serial.print(in_packet);
                Serial.print(" - with size: ");
                Serial.print(len);
                Serial.print(" - current size: ");
                Serial.println(sizeof(in_packet));
            }

            return received;
        }

        /* check if the current IN PACKET from UDP is the same as the 
        msg specified as INPUT to the method */
        bool udp_msg_equals_to(char compare_msg[]) {
            return strcmp(in_packet, compare_msg) == 0;
        }

        /* check if the two input CHAR[] are equal */
        bool char_msgs_are_equal(char msg1[], char msg2[]) {
            return strcmp(msg1, msg2) == 0;
        }

    // -----------------------------------------------------------------------SETUP

        void setup() {

            Serial.println("[ESPUDP][SETUP]  ---------------------- START");

            connect_to_wifi();

            Serial.println("[ESPUDP][SETUP]  ---------------------- COMPLETE");
        }
};
