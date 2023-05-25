using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.ComponentModel;

namespace SelfStabilizingDroneController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string[] ports;
        SerialCommunication _client;

        int PitchAngle = 0;
        int RollAngle = 0;

        int XMax = 15;
        int YMax = 15;

        /// <summary>
        /// This array has 16 bytes for motors, 4 bytes for each motor
        /// Motor 1: 0-3
        /// Motor 2: 4-7
        /// Motor 3: 8-11
        /// Motor 4: 12-15
        /// </summary>
        byte[] motorData;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            _client = new SerialCommunication();
            _client.NewSerialLineReceived += OnNewSerialLineReceived;
            _client.NewDataReceived += OnNewDataReceived;
            _client.Init();
            
            ports = SerialPort.GetPortNames();
            motorData = new byte[16];

            foreach (var port in ports)
            {
                if (!portBox.Items.Contains(port))
                {
                    portBox.Items.Add(port);
                }
            }

            ForwardButton.IsEnabled = false;
            BackwardButton.IsEnabled = false;
            LeftwardButton.IsEnabled = false;
            RightwardButton.IsEnabled = false;
        }
        private void OnNewSerialLineReceived(object sender, NewSerialLineEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DebugBox.AppendText((args.Data + '\n'));
                DebugBox.ScrollToEnd();
            });
        }

        private void OnNewDataReceived(object sender, NewDataEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GyroAngularVelocity_X.Text = args.Data[0];
                GyroAngularVelocity_Y.Text = args.Data[1];
                GyroAngularVelocity_Z.Text = args.Data[2];
                GForce_X.Text = args.Data[3];
                GForce_Y.Text = args.Data[4];
                GForce_Z.Text = args.Data[5];
                GyroAngles_X.Text = args.Data[6];
                GyroAngles_Y.Text = args.Data[7];
                GyroAngles_Z.Text = args.Data[8];
                ComplementedAngles_X.Text = args.Data[9];
                ComplementedAngles_Y.Text = args.Data[10];
                ComplementedAngles_Z.Text = args.Data[11];
                Motor0.Text = args.Data[12];
                Motor1.Text = args.Data[13];
                Motor2.Text = args.Data[14];
                Motor3.Text = args.Data[15];
            });
        }

        private void PortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (portBox.SelectedItem != null)
                {
                    _client.SetPort((string)portBox.SelectedItem);
                }
            }
            catch (Exception)
            {

            }
        }

        private void MasterMotorValueUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] value = ConvertValueToBytes(e.NewValue);
            masterValue.Content = ((int)e.NewValue).ToString();
            for (int i = 0; i < value.Length; i++)
            {
                motorData[i] = value[i];
                motorData[i + 4] = value[i];
                motorData[i + 8] = value[i];
                motorData[i + 12] = value[i];
            }

            _client.UpdateMotorValues(motorData);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!portBox.IsEnabled)
            {
                portBox.IsEnabled = true;
                button.Content = "Open";
                _client.Close();
            }
            else
            {
                if (_client.IsOpen)
                {
                    try
                    {
                        _client.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
                try
                {
                    _client.Open();
                    portBox.IsEnabled = false;
                    button.Content = "Close";

                    ForwardButton.IsEnabled = true;
                    BackwardButton.IsEnabled = true;
                    LeftwardButton.IsEnabled = true;
                    RightwardButton.IsEnabled = true;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        private void Motor1Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] value = ConvertValueToBytes(e.NewValue);
            motor1Label.Content = ((int)e.NewValue).ToString();
            for (int i = 0; i < value.Length; i++)
            {
                motorData[i] = value[i];
            }

            _client.UpdateMotorValues(motorData);
        }

        private void Motor2Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] value = ConvertValueToBytes(e.NewValue);
            motor2Label.Content = ((int)e.NewValue).ToString();
            for (int i = 0; i < value.Length; i++)
            {
                motorData[i + 4] = value[i];
            }

            _client.UpdateMotorValues(motorData);
        }

        private void Motor3Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] value = ConvertValueToBytes(e.NewValue);
            motor3Label.Content = ((int)e.NewValue).ToString();
            for (int i = 0; i < value.Length; i++)
            {
                motorData[i + 8] = value[i];
            }

            _client.UpdateMotorValues(motorData);
        }

        private void Motor4Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte[] value = ConvertValueToBytes(e.NewValue);
            motor4Label.Content = ((int)e.NewValue).ToString();
            for (int i = 0; i < value.Length; i++)
            {
                motorData[i + 12] = value[i];
            }

            _client.UpdateMotorValues(motorData);
        }

        private byte[] ConvertValueToBytes(double input)
        {
            byte[] data = new byte[4];
            int value = (int)input;

            for (int i = 0; i < 4; i++)
            {
                data[i] = (byte)(value % 10);
                value /= 10;
            }
            byte[] temp = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                temp[i] = data[3 - i];
            }
            data = temp;

            return data;
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (PitchAngle < XMax)
            {
                PitchAngle++;
                PitchValue.Text = PitchAngle.ToString();
                OnPropertyChanged(nameof(PitchValue));
                UpdateSetpoints();
            }
        }

        private void BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (PitchAngle > -XMax)
            {
                PitchAngle--;
                PitchValue.Text = PitchAngle.ToString();
                OnPropertyChanged(nameof(PitchValue));
                UpdateSetpoints();
            }
        }

        private void LeftwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (RollAngle > -YMax)
            {
                RollAngle--;
                RollValue.Text = RollAngle.ToString();
                OnPropertyChanged(nameof(RollValue));
                UpdateSetpoints();
            }
        }

        private void RightwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (RollAngle < YMax)
            {
                RollAngle++;
                RollValue.Text = RollAngle.ToString();
                OnPropertyChanged(nameof(RollValue));
                UpdateSetpoints();
            }
        }

        private void UpdateSetpoints()
        {
            _client.UpdateSetpoints(RollAngle, PitchAngle);
        }

        private void UpdateSerial(byte data, int index)
        {

        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
