#include <WebSocketClient.h>
#include <SPI.h>
#include <WiFi.h>
#include <ArduinoJson.h>
using namespace net;

int machineDBId = 1;

int outputPins[4] = {
  17,   // 1
  5,    // 2
  18,   // 3
  19    // 4
};
int btnInputPin = 15;

const char* ssid = "chayns®"; // fe_ge_ahaus // FRITZ!Box 6490 Cable";
const char* password = ""; // 84688415471223421 // 25740003065298191354

WebSocketClient client;

const char* WebSocketServerUrl = "barforce.herokuapp.com";
const char* WebSocketServerPath = "/machine";
uint16_t WebSocketServerPort = 80;

void setup() {
  Serial.begin(115200);
  for(int i=0; i<4;i++){
    pinMode(outputPins[i], OUTPUT);
  }
  pinMode(btnInputPin, INPUT);

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
  Serial.print("Message recieved: ");
    Serial.println(message);

    while(digitalRead(btnInputPin) != HIGH);
    // erst wenn Button gedrückt ist
    StaticJsonDocument<JSON_ARRAY_SIZE(4)> arrayDoc;
    deserializeJson(arrayDoc, message);
    serializeJsonPretty(arrayDoc, Serial);
    JsonArray array = arrayDoc.as<JsonArray>();
    activatePins(array);    
    char retMessage[] = "{Action:'finished'}";
    ws.send(TEXT, retMessage, (uint16_t)strlen(retMessage));
  });
  client.open(WebSocketServerUrl, WebSocketServerPort, WebSocketServerPath);
}

void loop() {
  client.listen();
}

void activatePins(JsonArray array){
  Serial.println("activatePins");
  for(String value : array) {
    Serial.println("Object: " + value);
    DynamicJsonDocument objDoc(1024);
    deserializeJson(objDoc, value);
    JsonObject obj = objDoc.as<JsonObject>();
    serializeJsonPretty(objDoc, Serial);
    JsonVariant ammountMlObj = objDoc["AmmountMl"];
    int ammountMl = ammountMlObj.as<int>();
    JsonVariant idObj = objDoc["Id"];
    int id = idObj.as<int>();
    if(ammountMl > 0){
      digitalWrite(outputPins[id - 1], HIGH);
      // Per Durchflusssensor Menge messen, bis Menge = obj[i].AmmountMl
       delay(ammountMl*10);
      //
      digitalWrite(outputPins[id - 1], LOW);
    }
  }
}
