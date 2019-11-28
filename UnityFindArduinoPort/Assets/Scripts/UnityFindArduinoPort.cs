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
    
    private CommunicateWithArduino fishUno;
    private CommunicateWithArduino[] unos;
    private string[] ports;
    private string[] readStrings;
    private bool completeCheckingPorts;
    private const string pingCmd = "ping";
    private string receivedData = "";
    private float waitSec = 1f;
    private int boardNum;
    
    void Start()
	{
        InitializeSerialPorts();
    }


    void Update()
    {
        ReadConnectedPorts();
        if (!completeCheckingPorts)
        {
            if ((horseUno != null && bowUno != null) || (fishUno != null)) completeCheckingPorts = true;
            SendPingToPorts();
        }
    }

    private void SendPingToPorts()
    {
        // Ping all connected serial ports
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i].isConnected && unos[i] != null)
            {
                new Thread(unos[i].SendData).Start(pingCmd);
            }
        }
        
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i].isConnected && unos[i] != null)
            {
                if (unos[i].readData == "horse") 
                {
                    horseUno = unos[i];
                    unos[i] = null;
                }
                else if (unos[i].readData == "bow")
                {
                    bowUno = unos[i];
                    unos[i] = null;
                }
                else if (unos[i].readData == "fish")
                {
                    fishUno = unos[i];
                    unos[i] = null;
                }
                else Debug.Log("unknown recevied data.");
            }
        }
    }

    private void ReadConnectedPorts()
    {
        // read all connected serial ports
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i] != null)
            {
                if (unos[i].isConnected)
                {
                    if (unos[i].readData != "" && unos[i].readData != readStrings[i])
                    {
                        readStrings[i] = unos[i].readData;
                        Debug.Log(unos[i].portName + ": " + readStrings[i]);
                    }
                }
            }
        }
    }

    private void InitializeSerialPorts()
    {
        ports = SerialPort.GetPortNames();
        foreach (string s in ports) Debug.Log("Find connections on: " + s);

        boardNum = ports.Length;
        unos = new CommunicateWithArduino[boardNum];
        readStrings = new string[boardNum];

        // initialize communications
        for (int i = 0; i < boardNum; i++)
        {
            unos[i] = new CommunicateWithArduino(ports[i], baudRate:9600);
            readStrings[i] = "";
        }

        // if connection initialized successfully, start reading serial port
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i].isConnected)
            {
                new Thread(unos[i].ReceiveData).Start();
            }
        }
    }

    class CommunicateWithArduino
	{
		public bool connected = true;
		public bool mac = false;

	    public bool isConnected;
        public string portName;
        public string readData = "";
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
                                      int ReadTimeout = 50, bool isMac = false, bool isConnected = true) {
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
			}
		}
        
		public void SendData(object obj)
        {
			string data = obj as string;
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

        public void ReceiveData()
        {
            while (true)
            {
                if (arduinoController != null && arduinoController.IsOpen)
                {
                    try
                    {
                        readData = arduinoController.ReadLine();
                        // Debug.Log(readData);
                    }
                    catch (TimeoutException) { }
                }
            }
        }

        public void CloseSerial()
        {
            arduinoController.Close();
        }
	}
}
