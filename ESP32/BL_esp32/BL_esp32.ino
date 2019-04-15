#include "BluetoothSerial.h" //Header File for Serial Bluetooth, will be added by default into Arduino

#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial ESP_BT; //Object for Bluetooth

int incoming;
String bluetooth_rx_buffer = "";
// Delimiter used to separate messages
char DELIMITER = '|';

void LEDsPlay(String msg){
  if (msg == "1")
        {
        digitalWrite(LED_BUILTIN, HIGH);
        ESP_BT.println("LED turned ON");
        }
        
    if (msg == "0")
        {
        digitalWrite(LED_BUILTIN, LOW);
        ESP_BT.println("LED turned OFF");
        }
}

/**
 * Called when a complete message is received.
 */
void gotMessage(String message) {
  
  Serial.println("[RECV] " + message);
  
  // Do something!
  LEDsPlay(message);
}

/**
 * Finds complete messages from the rx buffer.
 */
void parseReadBuffer() {
  
  // Find the first delimiter in the buffer
  int inx = bluetooth_rx_buffer.indexOf(DELIMITER);

  // If there is none, exit
  if (inx == -1) return;
  
  // Get the complete message, minus the delimiter
  String s = bluetooth_rx_buffer.substring(0, inx);
  
  // Remove the message from the buffer
  bluetooth_rx_buffer = bluetooth_rx_buffer.substring(inx + 1);
  
  // Process the message
  gotMessage(s);
  
  // Look for more complete messages
  parseReadBuffer();
}

void setup() {
  Serial.begin(9600); //Start Serial monitor in 9600
  ESP_BT.begin("Gemma"); //Name of your Bluetooth Signal
  Serial.println("Bluetooth Device is Ready to Pair");

  pinMode (LED_BUILTIN, OUTPUT);//Specify that LED pin is output
}

void loop() {

  while(ESP_BT.available()){
    bluetooth_rx_buffer += (char)ESP_BT.read();
  }

  //Look for complete messages
  parseReadBuffer();
  delay(20);
}
