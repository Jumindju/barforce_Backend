#include <WebSocketClient.h>
#include <SPI.h>
#include <WiFi.h>
#include <ArduinoJson.h>
#include <Wire.h>
#include "SSD1306.h"

using namespace net;

int machineDBId = 1;

int outputPins[4] = {
  17,   // 1
  5,    // 2
  18,   // 3
  19    // 4
};
int OkBtnInputPin = 15;
int AbortBtnInputPin = 2;

SSD1306 display(0x3c, 21, 22); 

const char* ssid = "chayns®"; // fe_ge_ahaus
const char* password = ""; // 84688415471223421

WebSocketClient client;

const char* WebSocketServerUrl = "barforce.herokuapp.com";
const char* WebSocketServerPath = "/machine";
uint16_t WebSocketServerPort = 80;

void setup() {
  Serial.begin(115200);
  for(int i=0; i<4;i++){
    pinMode(outputPins[i], OUTPUT);
  }
  pinMode(OkBtnInputPin, INPUT);
  pinMode(AbortBtnInputPin, INPUT);

  display.init();
  display.flipScreenVertically();
  displayText("Warte ...", true);
  display.display();

  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());

  client.onOpen([](WebSocket & ws) {
    Serial.println("Client connected");
    char message[] = "{Action:'init',Data:1}";
    ws.send(TEXT, message, (uint16_t)strlen(message));
  });
  client.onClose([](WebSocket & ws, const WebSocketCloseCode code, const char *reason, uint16_t length) {
    Serial.println("Client disconnected");
    delay(1000);
    client.open(WebSocketServerUrl, WebSocketServerPort, WebSocketServerPath);
  });
  client.onMessage([](WebSocket & ws, WebSocketDataType dataType, const char *message, uint16_t length) {
    StaticJsonDocument<JSON_OBJECT_SIZE(2) + JSON_ARRAY_SIZE(JSON_OBJECT_SIZE(4))> userDrinkDoc;
    deserializeJson(userDrinkDoc, message);
    //JsonObject userDrink = userDrinkDoc.as<JsonObject>();
    JsonVariant userNameObj = userDrinkDoc["UserName"];
    String userName = userNameObj.as<String>();
    JsonArray drinkList = userDrinkDoc["DrinkList"];
    displayText(userName,true);
    int action = 0;
    while(action == 0){
      if(digitalRead(OkBtnInputPin) == HIGH){
        action = 1; // Ok
      }
      if(digitalRead(AbortBtnInputPin) == HIGH){
        action = 2; // Abbrechen
      }
    }
    char* retMessage;
    // erst wenn OK Button gedrückt ist
    if(action == 1){
      displayText("Wird zubereitet...", false);
      action = activatePins(drinkList); 
      retMessage ="{Action:'finished'}";
    }
    if(action == 2){
      retMessage = "{Action:'aborted'}";
      displayText("Abgebrochen.", false);
      delay(1000);  
    } 
    displayText("Warte ...",true);
    delay(1000);     
    ws.send(TEXT, retMessage, (uint16_t)strlen(retMessage));
    
  });
  client.open(WebSocketServerUrl, WebSocketServerPort, WebSocketServerPath);
}

void loop() {
  client.listen();
  if(!client.isAlive()){
    delay(1000);
    client.open(WebSocketServerUrl, WebSocketServerPort, WebSocketServerPath);
    delay(1000);
  }
}

int activatePins(JsonArray array){
  for(String value : array) {
    StaticJsonDocument<JSON_OBJECT_SIZE(4)> objDoc;
    deserializeJson(objDoc, value);
    //JsonObject obj = objDoc.as<JsonObject>();
    JsonVariant ammountMlObj = objDoc["AmmountMl"];
    int ammountMl = ammountMlObj.as<int>();
    JsonVariant idObj = objDoc["Id"];
    int id = idObj.as<int>();
        Serial.println();
        Serial.print("AmmountML: ");
        Serial.print(ammountMl);
        Serial.print(", Id: ");
        Serial.print(id);
        Serial.println();
    if(ammountMl > 0){
      bool result = openValve(ammountMl,outputPins[id-1]);
      if(!result){
        return 2;
      }
    }
  }
  return 1;
}

bool openValve(int ammountMl, int pin){
    digitalWrite(pin, HIGH);
    // Per Durchflusssensor Menge messen, bis Menge = ammountMl
    for(int i =0; i < ammountMl*8 ;i++){ // Abbrechen alle 10 ms scannen
      if(digitalRead(AbortBtnInputPin) == HIGH){
        digitalWrite(pin, LOW);
        return false;
      }
      delay(10);
    }
    //
    digitalWrite(pin, LOW);
    return true;
}
void displayText(String text, bool bigSize){
  if(bigSize){
    // Cut Name to DisplaySize
    String points = "...";
    uint16_t nameWidth = display.getStringWidth(text);
    uint16_t pointsWidth = display.getStringWidth(points);
    if(nameWidth > 87){
      while(nameWidth + pointsWidth > 87){
        text = text.substring(0,text.length()-1);
        nameWidth = display.getStringWidth(text);
      }
      text = text + points;
    }
  }
  
  display.clear();
  if(bigSize){
    display.setFont(ArialMT_Plain_24);
    display.drawString(0, 0,text);
  }else{
    display.setFont(ArialMT_Plain_16);
    display.drawString(0, 1,text);
  }
  

  //Abbrechen
  display.setFont(ArialMT_Plain_16);
  display.drawString(0, 47,"Abbrechen");
  //Ok
  display.setFont(ArialMT_Plain_16);
  display.drawString(108, 47,"Ok");

  display.display();
}
