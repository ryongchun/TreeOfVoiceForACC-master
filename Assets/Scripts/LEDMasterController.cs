﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using UnityEngine.UI;

using System.IO.Ports;


public class LEDMasterController : MonoBehaviour
{

    //////////////////////////////////
    //
    // Private Variable
    //
    //////////////////////////////////
    /// <summary>
    public string m_portName = "COM0"; // should be specified in the inspector
    SerialPort m_serialPort;


    /// </summary>
    //SerialToArduinoMgr m_SerialToArduinoMgr; 
    Thread m_Thread;

    Action m_updateArduino;


   LEDColorGenController m_LEDColorGenController;

    public byte[] m_LEDArray; // 200  LEDs

    float m_Delay;
    public int m_LEDCount; // from LEDColorGenController component

    int m_index;
    //////////////////////////////////
    //
    // Function
    //
    //////////////////////////////////

    byte[] m_LEDArray1;

    private void Awake()
    { // init me


        // Serial Communication between C# and Arduino
        //http://www.hobbytronics.co.uk/arduino-serial-buffer-size = how to change the buffer size of arduino
        //https://www.codeproject.com/Articles/473828/Arduino-Csharp-and-Serial-Interface
        //Don't forget that the Arduino reboots every time you open the serial port from the computer - 
        //and the bootloader runs for a second or two gobbling up any serial data you send its way..
        //This takes a second or so, and during that time any data that you send is lost.
        //https://forum.arduino.cc/index.php?topic=459847.0

        //https://docs.microsoft.com/en-us/dotnet/api/system.io.ports.serialport.writetimeout?view=netframework-4.8

        // Set up the serial Port

        m_serialPort = new SerialPort(m_portName, 115200); // bit rate= 567000 bps = 


        //m_SerialPort.ReadTimeout = 50;
        // m_serialPort.ReadTimeout = 1000;  // sets the timeout value: 1000 ms  = sufficient for our purpose?
        // InfiniteTimeout is the default.
        //  m_SerialPort1.WriteTimeout = 5000??

        try
        {
            m_serialPort.Open();
        }
        catch (Exception ex)
        {
            Debug.Log("Error:" + ex.ToString());
//#if UNITY_EDITOR
//            // Application.Quit() does not work in the editor so
//            // UnityEditor.EditorApplication.isPlaying = false;
//            UnityEditor.EditorApplication.Exit(0);
//#else
//                   Application.Quit();
//#endif
        }


        //m_SerialToArduinoMgr = new SerialToArduinoMgr();

        //m_SerialToArduinoMgr.Setup();

        //m_port = m_SerialToArduinoMgr.port;
    }

    //https://www.codeproject.com/Questions/1178661/How-do-I-receive-a-byte-array-over-the-serial-port
    //That is most likely because you only read the first byte. 
    //Serial ports are extremely slow and you need to keep checking whether there is any data ready to read,
    //you cannot assume that you will get the complete message on your first attempt.

    //int MyInt = 1;

    //byte[] b = BitConverter.GetBytes(MyInt);
    //serialPort1.Write(b,0,4);
    void Start()
    {


        //m_ledColorGenController = gameObject.GetComponent<LEDColorGenController>();
        ////It is assumed that all the necessary components are already attached to CommHub gameObject, which  is referred to by
        //// gameObject field variable. gameObject.GetComponent<LEDColorGenController>() == this.gameObject.GetComponent<LEDColorGenController>();
        //if (m_ledColorGenController == null)
        //{
        //    Debug.LogError("The global Variable  m_ledColorGenController is not  defined");
        //    Application.Quit();
        //}

        //m_ledColorGenController.m_ledSenderHandler += UpdateLEDArray; // THis is moved to CommHub.cs

        // public delegate LEDSenderHandler (byte[] LEDArray); defined in LEDColorGenController
        // public event LEDSenderHandler m_ledSenderHandler;

        m_LEDColorGenController = this.gameObject.GetComponent<LEDColorGenController>();

        m_LEDCount = m_LEDColorGenController.m_totalNumOfLEDs;

        m_LEDArray = new byte[m_LEDCount * 3]; // 280*3 = 840 < 1024


        // prepare test led array
        
      
        int m_index = 0;

        // define an action
        m_updateArduino = () => {

            //Debug.Log("Thread Run Test");
            //Write(byte[] buffer, int offset, int count);
            // for debugging, comment out:

            try
            {

                // for (int i =0; i < m_LEDArray.Length; i++)
                // {
                //Debug.Log(i + "th byte:" +m_LEDArray[i]);
                // }
                //https://social.msdn.microsoft.com/Forums/vstudio/en-US/93583332-d307-4552-bd61-9a2adfcf2480/serial-port-write-method-is-blocking-execution?forum=vbgeneral


                //Yes, the Write methods do block , until all data have been passed from the serial port driver to the UART FIFO.
                //Usually, this is not a problem.It will not block "forever," just for as long as it takes.
                //For example, if you were to send a 2K byte string, at 9600 bps, the write method would take about 2 seconds to return.
                //Are you seeing some other operation? Naturally, you must make sure that something else isn't affecting operation,
                //such as the .Handshake property.  If .Handshake is set to HandshakeRequestToSend or HandshakeRequestToSendXonXoff, 
                //then CTS must be True in order to send data.  If CTS is not connected, or not asserted, then the Write method may not return.



                m_serialPort.Write(m_LEDArray, 0, m_LEDArray.Length);
            }
            catch (Exception ex)
            {
                Debug.Log("Error:" + ex.ToString());
//#if UNITY_EDITOR
//                // Application.Quit() does not work in the editor so
//                // UnityEditor.EditorApplication.isPlaying = false;
//                UnityEditor.EditorApplication.Exit(0);
//#else
//                   Application.Quit();
//#endif

            }

            // The WriteBufferSize of the Serial Port is 1024, whereas that of Arduino is 64; m_LEDArray is 200 * 3 = 600 bytes less than
            // the Serial Port size.
            //https://stackoverflow.com/questions/22768668/c-sharp-cant-read-full-buffer-from-serial-port-arduino

        };
        
       
       m_Thread = new Thread(new ThreadStart(m_updateArduino)); // ThreadStart() is a delegate (pointer type)
                                                                // Thread state = unstarted

    }// void Start()


    public void UpdateLEDArray(byte[] ledArray)
    {
        // use prepared ledArray rather than given for debugging



        // Send the new LED array only when the sending thread has finished sending the previous LEDArray
        // THat is, only when m_Thread.IsAlive is false. Tit happends when the method of the thread returns;
        // That is when the sending thread has sent all the LED array.

        //Debug.Log("1) Thread State == " + m_Thread.ThreadState);

        //Debug.Log("2) Thread.IsAlive " + m_Thread.IsAlive);

        //https://stackoverflow.com/questions/6578001/how-to-start-a-stopped-thread
        //This would create a new instance of the thread and start it. The ThreadStateException error is because,
        //simply, you can't re-start a thread that's in a stopped state.
        // m_MyThread.Start() is only valid for threads in the Unstarted state.
        //  What needs done in cases like this is to create a new thread instance and invoke Start() on the new instance.

        // send prepared byte arrays for debugging

        if ( !m_Thread.IsAlive )
        {
           
            try
            {
                // use the new LED array for the new invocation of the sending thread
              
                
                Debug.Log(" send LED array to arduino");

                for (int i = 0; i < m_LEDCount; i++)
                {
                    ledArray[3 * i] = 0;
                    ledArray[3 * i + 1] = 0;
                    ledArray[3 * i + 2] = 0;

                }

                ledArray[3 * m_index + 0] = 255;
                ledArray[3 * m_index + 1] = 0;
                ledArray[3 * m_index + 2] = 0;

                m_index++;
                if (m_index == m_LEDCount)
                {
                    m_index = 0; // wrap the index
                }

                m_LEDArray = ledArray;

                m_Thread = new Thread(new ThreadStart(m_updateArduino));
                //m_Thread.IsBackground = true;

                // Starting The thread sends m_LEDArray to the arduino master
                m_Thread.Start();


            }

            catch (Exception ex)
            {
                Debug.Log(" Exception =" + ex.ToString());
//#if UNITY_EDITOR
//                // Application.Quit() does not work in the editor so
//                // UnityEditor.EditorApplication.isPlaying = false;
//                UnityEditor.EditorApplication.Exit(0);
//#else
//                   Application.Quit();
//#endif

            }

            // ** yooJin, print m_LEDArray here and compare it with the data received by
            // the arduino master. Because the default serial port is used by the communication
            // between the unity script and the arduino master, you need to use another serial port
            // to send data from the master arduino to the serial monitor

            //for (int i=0; i < m_LEDCount; i++)
            // {
            //     Vector3 color = new Vector3();

            //     color[0] = m_LEDArray[3 * i + 0];
            //     color[1] = m_LEDArray[3 * i + 1];
            //     color[2] = m_LEDArray[3 * i + 2];

            //     // yooJin: Uncomment the following for debugging
            //     //Debug.Log(" In UpdateLEDArray: Send: " + i +  "th LED color:" + color);

            // }


        }
        else
        {
            Debug.Log("Thread is alive; Wait until it finishes and the arrived array of led bytes is discarded");

            // The sending thread is still busy sending  the previous LED array =>: The arrived LED array is discarded
        }

    }
    //  UpdateLEDArray()

    void Update()
    {
       
    }

}//public class LEDMasterController 

