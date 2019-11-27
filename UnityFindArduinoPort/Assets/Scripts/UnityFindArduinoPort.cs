using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Management;
public class UnityFindArduinoPort : MonoBehaviour
{
    private CommunicateWithArduino unoFish, unoHorse, unoBow;
    private string[] ports;
    
    void Start()
	{
        string[] ports = SerialPort.GetPortNames();
        foreach(string port in ports) 
        {
            Debug.Log("Ping port name: " + port);
            StartCoroutine(PingArduinos(port));
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator PingArduinos(string portName)
    {
        string pingCmd = "ping";
        string receivedData = "";
        float waitSec = 0.5f;

        CommunicateWithArduino pingArduino = new CommunicateWithArduino(portName, baudRate:9600);
        new Thread(pingArduino.ConnectToArduino).Start();
        yield return new WaitForSeconds(waitSec);
        new Thread(pingArduino.SendData).Start(pingCmd);
        new Thread(() => 
        {   
            if (pingArduino.ReceiveData() != "") receivedData = pingArduino.ReceiveData();
        }).Start();
        yield return new WaitForSeconds(waitSec * 10);
        new Thread(pingArduino.CloseSerial).Start();
        yield return new WaitForSeconds(waitSec);
        if (receivedData == "fish")
        {
            unoFish = new CommunicateWithArduino(portName, baudRate:9600);
            new Thread(unoFish.ConnectToArduino).Start();
            Debug.Log("Find fish arduino on " + portName + ", connection starts.");
        }
        else if (receivedData == "horse")
        {
            unoHorse = new CommunicateWithArduino(portName, baudRate:9600);
            new Thread(unoHorse.ConnectToArduino).Start();
            Debug.Log("Find horse arduino on " + portName + ", connection starts.");
        }
        else if (receivedData == "bow")
        {
            unoBow = new CommunicateWithArduino(portName, baudRate:9600);
            new Thread(unoBow.ConnectToArduino).Start();
            Debug.Log("Find bow arduino on " + portName + ", connection starts.");
        }
        else if (receivedData == "") Debug.Log("The arduino on " + portName + " has no response.");
        else Debug.Log("The arduino on " + portName + " is not the correct one.");
        yield return 0;
    }

    class CommunicateWithArduino
	{
		public bool connected = true;
		public bool mac = false;
		public string choice = "COM6";

		private SerialPort arduinoController;
		private string portName;
	    private int baudRate;
	    private Parity parity;
	    private int dataBits;
	    private StopBits stopBits;
	    private Handshake handshake;
	    private bool RtsEnable;
	    private int ReadTimeout;
	    private bool isMac;
	    private bool isConnected;

		public CommunicateWithArduino(string portName, int baudRate = 9600, Parity parity = Parity.None,
                                      int dataBits = 8, StopBits stopBits = StopBits.One,
                                      Handshake handshake = Handshake.None, bool RtsEnable = true,
                                      int ReadTimeout = 1, bool isMac = false, bool isConnected = true) {
            this.portName = portName;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            this.handshake = handshake;
            this.RtsEnable = RtsEnable;
            this.ReadTimeout = ReadTimeout;
            this.isMac = isMac;
            this.isConnected = isConnected;
            ConnectToArduino();
        }

        
		public void ConnectToArduino() {
			if (connected) {
				string portChoice = portName;
				// if (mac) {
				// 	int p = (int) Environment.OSVersion.Platform;
				// 	// Are we on Unix?
				// 	if (p == 4 || p == 128 || p == 6) {
				// 		List<string> serial_ports = new List<string>();
				// 		string[] ttys = Directory.GetFiles("/dev/", "cu.*");
				// 		foreach (string dev in ttys) {
				// 			if (dev.StartsWith("/dev/tty.")) {
				// 				serial_ports.Add(dev);
				// 				Debug.Log(string.Format(dev));
				// 			}
				// 		}
				// 	}
				// 	portChoice = "/dev/" + choice;
				// }
				arduinoController = new SerialPort(portChoice, baudRate, Parity.None, 8, StopBits.One);
				arduinoController.Handshake = Handshake.None;
				arduinoController.RtsEnable = true;
				arduinoController.Open();
				Debug.LogWarning(arduinoController);
			}
		}
        
		public void SendData(object obj)
        {
			string data = obj as string;
			// Debug.Log(data);
			if (connected)
            {
				if (arduinoController != null)
                {
					arduinoController.Write(data);
					arduinoController.Write("\n");
				}
				else
                {
					Debug.Log(arduinoController);
					Debug.Log("nullport");
				}
			}
			else
            {
				Debug.Log("not connected");
			}
			Thread.Sleep(500);
		}

        public string ReceiveData()
        {
            return arduinoController.ReadLine();
        }

        public void CloseSerial()
        {
            arduinoController.Close();
        }
	}
}
