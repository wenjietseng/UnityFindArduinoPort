#define BOARD_NAME "horse" // "horse", "bow", "fish"
#define DEBUG 0
String readStringInput = "";
bool readComplete = false;
bool boardNameSending = true;
int cmd_value = 0;

void setup() {
  Serial.begin(9600);
}

void loop() {
  // debug message
#if DEBUG == 1
#else DEBUG == 0
  delay(10);
#endif
  if (boardNameSending) Serial.println(BOARD_NAME);
  while (Serial.available())
  {
      char c = Serial.read();
      if (c == ' ' || c == '\n')
      {
        if (readStringInput == "ping")
        {
            boardNameSending = false;
            readStringInput = "";
        }
        else
        {
            // update input cmd from unity here
            delay(10);
            cmd_value = readStringInput.toInt();
            readStringInput = "";
            if (c == '\n') readComplete = true;
        }
    }
    else
    {
      readStringInput += c;
    }

    if (readComplete)
    {
        // do arduino control here
    }
    Serial.flush();
  }
}
