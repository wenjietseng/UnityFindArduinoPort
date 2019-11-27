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
    private CommunicateWithArduino horseUno;
    private CommunicateWithArduino bowUno;
    private CommunicateWithArduino[] unos;
    private Thread[] readThreads;
    private string[] ports;
    private string[] readStrings;

    private const string pingCmd = "ping";
    private string receivedData = "";
    private float waitSec = 1f;
    private bool findingPorts = true;
    private int boardNum;
    
    void Start()
	{
        ports = SerialPort.GetPortNames();
        foreach (string s in ports) Debug.Log(s);
        boardNum = ports.Length;
        unos = new CommunicateWithArduino[boardNum];
        readStrings = new string[boardNum];
        for (int i = 0; i < boardNum; i++)
        {
            unos[i] = new CommunicateWithArduino(ports[i], baudRate:9600);
            readStrings[i] = "";
        }
        // for (int i = 0; i < boardNum; i++)
        // {
        //     if (unos[i].isConnected)
        //     {
                new Thread(() => {readStrings[1] = unos[1].ReceiveData(); });
            // }
        // }
     }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i].isConnected)
            {
                if (readStrings[i] != "") Debug.Log(unos[i].portName + ": " + readStrings[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.P) && findingPorts)
        {
            for (int i = 0; i < boardNum; i++)
            {
                if (unos[i].isConnected)
                {
                    new Thread(unos[i].SendData).Start(pingCmd);
                }
            }
            findingPorts = false;
        }
    }

    public IEnumerator PingArduinos(string portName)
    {


        CommunicateWithArduino pingArduino = new CommunicateWithArduino(portName, baudRate:9600);
        Debug.Log("open " + portName);
        new Thread(pingArduino.ConnectToArduino).Start();
        Debug.Log("ping " + portName);
        new Thread(pingArduino.SendData).Start(pingCmd);
        yield return new WaitForSeconds(waitSec);
        new Thread(() => 
        {   
            if (pingArduino.ReceiveData() != "") receivedData = pingArduino.ReceiveData();
        }).Start();
        yield return new WaitForSeconds(waitSec * 10);
        // new Thread(pingArduino.CloseSerial).Start();
        yield return new WaitForSeconds(waitSec);
        if (receivedData == "fish")
        {
            // unoFish = new CommunicateWithArduino(portName, baudRate:9600);
            // new Thread(unoFish.ConnectToArduino).Start();
            Debug.Log("Find fish arduino on " + portName + ", connection starts.");
        }
        else if (receivedData == "horse")
        {
            // unoHorse = new CommunicateWithArduino(portName, baudRate:9600);
            // new Thread(unoHorse.ConnectToArduino).Start();
            Debug.Log("Find horse arduino on " + portName + ", connection starts.");
        }
        else if (receivedData == "bow")
        {
            // unoBow = new CommunicateWithArduino(portName, baudRate:9600);
            // new Thread(unoBow.ConnectToArduino).Start();
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

	    public bool isConnected;
        public string portName;
		private SerialPort arduinoController;

	    private int baudRate;
	    private Parity parity;
	    private int dataBits;
	    private StopBits stopBits;
	    private Handshake handshake;
	    private bool RtsEnable;
	    private int ReadTimeout;
	    private bool isMac;


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
				try
                {
                    arduinoController.Open();
                }
                catch (IOException)
                {
                    this.isConnected = false;
                    Debug.Log("Port " + this.portName + " not exists");
                }
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
                    Debug.Log(this.portName + ": send " + data);
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
            string message = "";
            while (this.isConnected) {
                print("keep running?");
                try
                {
                    message = arduinoController.ReadLine();
                    Console.WriteLine(message);
                }
                catch (TimeoutException) { }
            }
            return message;
        }

        public void CloseSerial()
        {
            arduinoController.Close();
        }
	}
}
