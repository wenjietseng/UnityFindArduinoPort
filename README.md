# UnityFindArduinoPort
Detect the connected port of Arduino boards on Windows

## Arduino side
Change `BOARD_NAME`, arduino sends `BOARD_NAME` to serial port. After receiving a string `"ping"` from Unity, it will stop sending `BOARD_NAME`.

## Unity side
After connecting available arduino boards on the serial ports, unity will read the `BOARD_NAME` at first place. Then it checks the predefine `BOARD_NAME`s and index in the uno boards array. Finally, it records the index for further usage and sends a string `"ping"` to Arduino.

## Note
- .NET 2.0 is necessary from serial port communication in Unity
- If using unity to read serial port, however, nothing is presenting there, the unity crashes. (need to find the reason)

## Future
- more testing
- fix the problems I found in Note 2
- make it usable on different platform (Linux, Mac OS)