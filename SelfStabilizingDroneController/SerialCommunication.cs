using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfStabilizingDroneController
{
    public class SerialCommunication : ISerialCommunication
    {
        SerialPort _port;


        /// <summary>
        /// 0-15 motor bytes
        /// 16 mode byte
        /// 17-18 angle bytes (roll, pitch)
        /// 19 level byte
        /// </summary>
        byte[] _data;

        public delegate void NewSerialLineReceivedEventHandler(object sender, NewSerialLineEventArgs args);
        public event NewSerialLineReceivedEventHandler NewSerialLineReceived;

        public delegate void NewDataReceivedEventHandler(object sender, NewDataEventArgs args);
        public event NewDataReceivedEventHandler NewDataReceived;

        public bool IsOpen { get { return _port.IsOpen; } }
        public SerialCommunication()
        {
            _port = new SerialPort();
            _port.ReadTimeout = 5000;
            _data = new byte[20];
        }

        public void Init()
        {
            _port.DataReceived += OnDataReceived;

            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = 0;
            }
        }

        public void Open()
        {
            _port.Open();
        }

        public void Close()
        {
            _port.Close();
        }

        public void SetPort(string port)
        {
            _port.PortName = port;
        }

        public string Receive()
        {
            return null;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            string data = ((SerialPort)sender).ReadExisting().TrimStart('\r');
            //NewSerialLineReceived.Invoke(this, new NewSerialLineEventArgs { Data = data });
            if (data.StartsWith("[LOG](DATA)"))
            {
                GetValuesFromString(data.Replace("[LOG](DATA)", ""));
            }
        }

        void GetValuesFromString(string data)
        {
            string[] values = data.Split();
            NewDataReceived?.Invoke(this, new NewDataEventArgs { Data = values });
        }

        public void UpdateMotorValues(byte[] motorData)
        {
            if (motorData != null)
            {
                Array.Copy(motorData, _data, 16);
                UpdateData();
            }
        }

        public void UpdateData()
        {
            try
            {
                if (_port != null)
                {
                    _port.Write(_data, 0, _data.Length);
                }
            }
            catch (Exception)
            {

            }
        }

        public void UpdateSetpoints(int roll, int pitch)
        {
            if (roll < 0)
            {
                _data[17] = (byte)(15 - roll);
            }
            else
            {
                _data[17] = (byte)roll;
            }

            if (pitch < 0)
            {
                _data[18] = (byte)(15 - pitch);
            }
            else
            {
                _data[18] = (byte)pitch;
            }
            UpdateData();
        }

        public void CalibrateSensors()
        {
            _data[19] = 1;
            UpdateData();
        }
    }
}
