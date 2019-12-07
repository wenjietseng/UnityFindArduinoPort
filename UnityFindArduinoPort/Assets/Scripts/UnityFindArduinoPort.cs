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
    public int fishNum = -1;
    public int horseNum = -1;
    public int bowNum = -1;
    public string[] readStrings;

    private CommunicateWithArduino[] unos;
    private string[] ports;
    private const string pingCmd = "ping";
    private bool finishPing = true;
    private int boardNum;
    
    void Start()
	{
        InitializeSerialPorts();
    }


    void Update()
    {
        if (finishPing)
        {
            ReadConnectedPorts();
            CheckReadData();
        }

        if (Input.GetKeyDown(KeyCode.H) && horseNum != -1) new Thread(unos[horseNum].SendData).Start("horse");
        if (Input.GetKeyDown(KeyCode.B) && bowNum != -1) new Thread(unos[bowNum].SendData).Start("bow");
        if (Input.GetKeyDown(KeyCode.F) && fishNum != -1) new Thread(unos[fishNum].SendData).Start("bow");
    }

    private void SendPingToPorts()
    {
        // Ping all connected serial ports
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i].isConnected && unos[i] != null) new Thread(unos[i].SendData).Start(pingCmd);
        }
        finishPing = false;
    }

    private void ReadConnectedPorts()
    {
        for (int i = 0; i < boardNum; i++)
            if (unos[i] != null && unos[i].isConnected) unos[i].readData = unos[i].ReceiveData();
    }
    private void CheckReadData()
    {
        for (int i = 0; i < boardNum; i++)
        {
            if (unos[i] != null && unos[i].isConnected)
            {
                if (unos[i].readData != "" && unos[i].readData != readStrings[i])
                {
                    readStrings[i] = unos[i].readData;
                    Debug.Log(unos[i].portName + ": " + readStrings[i]);                        
                    if (unos[i].readData == "horse") horseNum = i;
                    else if (unos[i].readData == "bow") bowNum = i;
                    else if (unos[i].readData == "fish") fishNum = i;
                    else Debug.Log("unknown recevied data on " + i + " uno");
                }                
            }
        }
        if ((horseNum != -1 && bowNum != -1) || fishNum != -1) SendPingToPorts();
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

        public string ReceiveData()
        {
            string s;
            try
            {
                s = arduinoController.ReadLine();
            }
            catch (TimeoutException)
            {
                s = "";
            }
            return s;
        }

        public void CloseSerial()
        {
            arduinoController.Close();
        }
	}
}
