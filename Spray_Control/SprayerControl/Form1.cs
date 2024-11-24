using SprayerControl;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ArduinoControlApp
{
    public partial class Form1 : Form
    {
        private readonly UdpClient udpClient;
        private IPEndPoint arduinoEndPoint;

        public Form1()
        {
            InitializeComponent();
            udpClient = new UdpClient(8888);
            arduinoEndPoint = new IPEndPoint(IPAddress.Parse("192.168.5.50"), 8888);

            StartListening();
            PopulateDebugLevelComboBox();
        }

        private void PopulateDebugLevelComboBox()
        {
            // Populate ComboBox with debug level options (0 to 10)
            for (int i = 0; i <= 10; i++)
            {
                cmbDebugPwmLevel.Items.Add($"{i} - {GetDebugLevelDescription(i)}");
            }
            cmbDebugPwmLevel.SelectedIndex = 0; // Default selection
        }

        private string GetDebugLevelDescription(int level)
        {
            return level switch
            {
                0 => "debug - Turns OFF all reporting",
                1 => "dutycycleTurncomp - Individual nozzle reporting",
                2 => "setPWMTiming - Sets timing of nozzle on cycle",
                3 => "ControlNozzle - Controls nozzle (not used in a turn)",
                4 => "Pressure - System pressure",
                5 => "EvenOdd - Toggle firing of even/odd nozzles",
                6 => "Flow - Flow rate control",
                7 => "NozzleSpeed - Individual nozzle speed in a turn",
                8 => "PrintDebug - Overall system reporting",
                9 => "PrintAOG - Reports variables passed from AOG",
                10 => "Calibrate_PSI_Flow - Calibration function",
                _ => "Unknown"
            };
        }


        private void StartListening()
        {
            udpClient.BeginReceive(OnDataReceived, null);
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            var data = udpClient.EndReceive(ar, ref arduinoEndPoint);
            string receivedData = Encoding.UTF8.GetString(data);
            UpdateUI(receivedData);
            StartListening();
        }

        private void UpdateUI(string data)
        {
            string[] variables = data.Split(',');
            foreach (var variable in variables)
            {
                var parts = variable.Split(':');
                if (parts.Length != 2) continue;

                string name = parts[0].Trim();
                string value = parts[1].Trim();

                Invoke((MethodInvoker)delegate {
                    switch (name)
                    {
                        case "pressure":
                            txtpressure.Text = value;
                            break;
                        case "onTime":
                            txtonTime.Text = value;
                            break;
                        case "actualGPAave":
                            txtactualGPAave.Text = value;
                            break;
                        case "gpsSpeed":
                            txtgpsSpeed.Text = value;
                            break;
                    }
                });
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = GenerateSettingsMessage();
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            udpClient.Send(bytes, bytes.Length, arduinoEndPoint);
        }

        private string GenerateSettingsMessage()
        {
            string settingsMessage = $"UPDATE_SETTINGS:GPATarget:{txtGPATarget.Text}" +
                                     $"SprayWidth:{txtSprayWidth.Text},FlowCalibration:{txtFlowCalibration.Text}," +
                                     $"PSICalibration:{txtPSICalibration.Text},DutyCycleAdjustment:{txtDutyCycleAdjustment.Text}," +
                                     $"PressureTarget:{txtPressureTarget.Text},numberNozzles:{txtnumberNozzles.Text}," +
                                     $"currentDutyCycle:{txtcurrentDutyCycle.Text},Hz:{txtHz.Text},LowBallValve:{txtLowBallValve.Text}," +
                                     $"Ball_Hyd:{txtBall_Hyd.Text},WheelAngle:{txtWheelAngle.Text},Kp:{txtKp.Text}," +
                                     $"Ki:{txtKi.Text},Kd:{txtKd.Text}";

            string switchesMessage = $"SET_SWITCHES:main:{(chkOn_Off.Checked ? 1 : 0)},pwm:{(chkPWM_Conventional.Checked ? 1 : 0)}," +
                                     $"stagger:{(chkStagger.Checked ? 1 : 0)}";

            int debugLevel = cmbDebugPwmLevel.SelectedIndex;
            string debugMessage = $"SET_DEBUG:debug:{debugLevel}";

            return $"{settingsMessage}\n{switchesMessage}\n{debugMessage}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            // Create an instance of HelpForm
            HelpForm helpForm = new HelpForm();
            // Show the HelpForm as a modal dialog
            helpForm.ShowDialog();
        }

    }
}


