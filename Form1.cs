using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScottPlot.WinForms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using KnobControl;
using ScottPlot.Renderable;
using ScottPlot.Plottable;
using static OpenTK.Graphics.OpenGL.GL;
using ScottPlot;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using OpenTK.Input;


namespace WJ8712A
{
    public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
    public partial class Form1 : Form
    {

        double freq = 7200000;
        double scanfreq;
        double scanstart, scanstop, scanincrement;
        int sigstrengthindex = 0;
        int oldIFBW = 0;
        int newIFBW = 0;
        string freqstr = "        ";
        string pad0freq;
        bool updateloop = false;
        double freqstep;
        string controlstring = "";
        string[] filters = {"0.056", "0.063", "0.069", "0.075", "0.081", "0.088", "0.094", "0.100", "0.113", "0.125", "0.138",
                            "0.150", "0.163", "0.175", "0.188", "0.200", "0.225", "0.250", "0.275", "0.300", "0.325", "0'350",
                            "0.375", "0.400", "0.450", "0.500", "0.550", "0.600", "0.650", "0.700", "0.750", "0.800", "0.900",
                            "1.000", "1.100", "1.200", "1.300", "1.400", "1.500", "1.600", "1.800", "2.000", "2.200", "2.400",
                            "2.600", "2.800", "3.000", "3.200", "3.600", "4.000", "4.400", "4.800", "5.200", "5.600", "6.000",
                            "6.400", "7.200", "8.000" };

        // Various colors for logging info
        private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        private Color[] ScanColor = { Color.Blue, Color.Black, Color.Red };
        int scannumber;
        double oldscanfreq = 0;
        double oldss = 0;
        int scansteps = 0;
        int currentscanstep = 0;
        List<double> scanxarray = new List<double>();
        List<double> scanyarray = new List<double>();
        delegate void SetPlotCallback(double[] x, double[] y);
        double[] dataX = new double[0];
        double[] dataY = new double[0];
        int k = 0;

        public Form1()
        {
            InitializeComponent();
            freqstr = freq.ToString();
            freqstr = freqstr.PadLeft(8);
            Setfreqx(freq);
            string[] ports = SerialPort.GetPortNames();
            foreach (string s in ports)
            {
                comboBox1.Items.Add(s);
            }
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeChart();
        }

        private void InitializeChart()
        {
            formsPlot1.Plot.XLabel("Frequency");
            formsPlot1.Plot.YLabel("Signal Strength");
            formsPlot1.Plot.Title("Frequency Scan");
            //formsPlot1.Plot.AddDataLogger();
            formsPlot1.Configuration.LockHorizontalAxis = true;
            formsPlot1.Configuration.LockVerticalAxis = true;
            formsPlot1.Refresh();
        }

        private void freqx1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx1.Location.Y + .5 * freqx1.Size.Height))
            { freqstep = 1; }
            else { freqstep = -1; }

            if (!(freqx1.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }
        private async void Updatefreqx(double oldfreq, double freqstep)
        {
            double newfreq = oldfreq;
            bool firstloop = true;

            while (updateloop == true)
            {
                newfreq = newfreq + freqstep;
                if (newfreq > 0)
                {
                    freqstr = newfreq.ToString().PadLeft(8);
                    pad0freq = newfreq.ToString().PadLeft(8, '0');
                    pad0freq = pad0freq.Substring(0, 2) + "." + pad0freq.Substring(2, 6);
                    freqx1.Text = freqstr.Substring(7, 1);
                    freqx10.Text = freqstr.Substring(6, 1);
                    freqx100.Text = freqstr.Substring(5, 1);
                    freqx1000.Text = freqstr.Substring(4, 1);
                    freqx10000.Text = freqstr.Substring(3, 1);
                    freqx100000.Text = freqstr.Substring(2, 1);
                    freqx1000000.Text = freqstr.Substring(1, 1);
                    freqx10000000.Text = freqstr.Substring(0, 1);
                    freqx1.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx10.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx100.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx1000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx10000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx100000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx1000000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freqx10000000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
                    freq = newfreq;
                    controlstring = "FRQ " + pad0freq.ToString();
                    SendData(controlstring);

                }
                if (firstloop)
                {
                    firstloop = false;
                    await Task.Delay(500);
                }
                else { await Task.Delay(50); }

            }
        }

        private void Setfreqx(double setfreq)
        {
            freqstr = setfreq.ToString();
            freqstr = freqstr.PadLeft(8);
            freqx1.Text = freqstr.Substring(7, 1);
            freqx10.Text = freqstr.Substring(6, 1);
            freqx100.Text = freqstr.Substring(5, 1);
            freqx1000.Text = freqstr.Substring(4, 1);
            freqx10000.Text = freqstr.Substring(3, 1);
            freqx100000.Text = freqstr.Substring(2, 1);
            freqx1000000.Text = freqstr.Substring(1, 1);
            freqx10000000.Text = freqstr.Substring(0, 1);
            freqx1.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx10.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx100.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx1000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx10000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx100000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx1000000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
            freqx10000000.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
        }

        private void freqx1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx10_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx10.Location.Y + .5 * freqx10.Size.Height))
            { freqstep = 10; }
            else { freqstep = -10; }

            if (!(freqx1.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx10_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx100_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx100.Location.Y + .5 * freqx100.Size.Height))
            { freqstep = 100; }
            else { freqstep = -100; }

            if (!(freqx100.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx100_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx1000_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx1000.Location.Y + .5 * freqx1000.Size.Height))
            { freqstep = 1000; }
            else { freqstep = -1000; }

            if (!(freqx1000.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx1000_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx10000_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx10000.Location.Y + .5 * freqx10000.Size.Height))
            { freqstep = 10000; }
            else { freqstep = -10000; }

            if (!(freqx10000.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx10000_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx100000_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx100000.Location.Y + .5 * freqx100000.Size.Height))
            { freqstep = 100000; }
            else { freqstep = -100000; }

            if (!(freqx100000.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx100000_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx1000000_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx1000000.Location.Y + .5 * freqx1000000.Size.Height))
            { freqstep = 1000000; }
            else { freqstep = -1000000; }


            if (!(freqx1000000.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx1000000_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx10000000_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Y < (freqx10000000.Location.Y + .5 * freqx10000000.Size.Height))
            { freqstep = 10000000; }
            else { freqstep = -10000000; }

            if (!(freqx10000000.Text == ""))
            {
                updateloop = true;
                Updatefreqx(freq, freqstep);
            }
        }

        private void freqx10000000_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            updateloop = false;
        }

        private void freqx1_TextChanged(object sender, EventArgs e)
        {

        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            string data = Read();

        }

        public string Read()
        {

            string message = "";

            try
            {
                message = serialPort1.ReadLine();


            }
            catch (TimeoutException)
            {
                MessageBox.Show("In WJ8712A Read. Problem reading port. Timeout exception.");
            }

            return message;

        }

        private void UpdateChart(double[] dataX, double[] dataY)
        {
            if (formsPlot1.InvokeRequired)
            {
                SetPlotCallback d = new SetPlotCallback(UpdateChart);
                formsPlot1.Invoke(d, new object[] { dataX, dataY });
            }
            else
            {

                if (currentscanstep == 0)
                {
                    int divisor = 3;
                    scannumber = scannumber + 1;
                    scannumber = (scannumber % divisor);
                }

                formsPlot1.Plot.AddScatter(dataX, dataY, color: ScanColor[scannumber]);
                formsPlot1.Plot.AxisAuto();

                if (currentscanstep != 0)
                {
                   // formsPlot1.Plot.AddLine(oldscanfreq, oldss, scanfreq, newss, color: ScanColor[scannumber]);
                }

                formsPlot1.Refresh();
            }

        }

        private void SendScanData(string msg, int step, int stepstoscan)
        {
            // Send the user's text straight out the port
            try
            {
                // Show in the terminal window the user's text
                Log(LogMsgType.Outgoing, msg + "\n");
                txtSendData.SelectAll();

                //string response = await WriteScanQuery(msg + "\n");
                // Write message to the serial port
                serialPort1.Write(msg + "\n");
                serialPort1.Write("SVG?" + "\n");
            }
            catch (TimeoutException)
            {
                MessageBox.Show("In WJ8712 SendScanData. Problem writing to port. Timeout exception.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in writeout to serialport1. Original error: " + ex.Message);
            };

        }

        private void SendData(string msg)
        {
            // Send the user's text straight out the port
            try
            {
                Log(LogMsgType.Outgoing, msg + "\n");
                txtSendData.SelectAll();
                serialPort1.Write(msg + "\n");
            }
            catch (TimeoutException)
            {
                MessageBox.Show("In WJ8712 SendData. Problem writing to port. Timeout exception.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in writeout to serialport1. Original error: " + ex.Message);
                updateloop = false;
            };

        }
        private void Log(LogMsgType msgtype, string msg)
        {
                rtfTerminal.SelectedText = string.Empty;
                rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
                rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
                rtfTerminal.AppendText(msg);
                rtfTerminal.ScrollToCaret();
            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendData(txtSendData.Text);
        }

        private void openPort_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (this.comboBox1.Text)
            {
                case "":
                    MessageBox.Show("You must select a comm port.");
                    break;
                default:
                    if (serialPort1.IsOpen)
                    {
                        serialPort1.Close();
                        lblCommConnected.BackColor = Color.Red;
                        btnopenPort.Text = "Open Port";

                    }
                    else
                    {
                        serialPort1.PortName = this.comboBox1.Text;
                        serialPort1.BaudRate = 9600;
                        serialPort1.DataBits = 8;
                        serialPort1.Parity = System.IO.Ports.Parity.None;
                        serialPort1.StopBits = System.IO.Ports.StopBits.One;
                        try
                        {
                            serialPort1.Open();
                            lblCommConnected.BackColor = Color.LimeGreen;
                            btnopenPort.Text = "Close Port";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: Could not open port. Original error: " + ex.Message);
                        }
                    }
                    break;
            }
        }

        private void comboBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string s in ports)
            {
                comboBox1.Items.Add(s);
            }
        }


        private void knobControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PWFvalue.Text = knobControl1.Value.ToString();
        }

        private void knobControl1_ValueChanged(object Sender)
        {
            PWFvalue.Text = knobControl1.Value.ToString();

        }

        private void knobControl1_ValueChanged_1(object Sender)
        {
            newIFBW = knobControl1.Value;
            if (newIFBW != oldIFBW)
            {
                PWFvalue.Text = filters[knobControl1.Value - 1];
                controlstring = "BWN " + knobControl1.Value.ToString();
                SendData(controlstring);
                oldIFBW = newIFBW;
            };

        }

        private void BFOslider_Scroll(object sender, ScrollEventArgs e)
        {
            //  if (e.Type == ScrollEventType.EndScroll)
            {
                BFOslider.Value = BFOslider.Value - (BFOslider.Value % 10);
                BFOvalue.Text = BFOslider.Value.ToString();
                controlstring = "BFO " + BFOslider.Value.ToString();
                SendData(controlstring);
            }
        }

        private void PBTslider_Scroll(object sender, ScrollEventArgs e)
        {
            //  if (e.Type == ScrollEventType.EndScroll)
            {
                PBTslider.Value = PBTslider.Value - (PBTslider.Value % 10);
                PBTvalue.Text = PBTslider.Value.ToString();
                controlstring = "PBT " + PBTslider.Value.ToString();
                SendData(controlstring);
            }
        }

        private void AGCbtn_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (AGCbtn.Text)
            {
                case "Slow":
                    AGCbtn.Text = "Fast";
                    controlstring = "AGC 2 ";
                    SendData(controlstring);
                    AGCbtn.BackColor = Color.Aquamarine;
                    break;
                case "Manual":
                    AGCbtn.Text = "Slow";
                    controlstring = "AGC 1 ";
                    SendData(controlstring);
                    AGCbtn.BackColor = Color.Khaki;
                    break;
                case "Fast":
                    AGCbtn.Text = "Manual";
                    controlstring = "AGC 0 ";
                    SendData(controlstring);
                    AGCbtn.BackColor = Color.LightGoldenrodYellow;
                    break;
            }
        }

        private void NFslider_Scroll(object sender, ScrollEventArgs e)
        {
            //    if (e.Type == ScrollEventType.EndScroll)
            {
                NFslider.Value = NFslider.Value;
                NFvalue.Text = NFslider.Value.ToString();
                controlstring = "NFR " + NFslider.Value.ToString();
                SendData(controlstring);
            }
        }

        private void NFreset_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            NFslider.Value = 0;
            NFvalue.Text = "0";
            controlstring = "NFR 0";
            SendData(controlstring);
        }

        private void BFOreset_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            BFOslider.Value = 0;
            BFOvalue.Text = "0";
            controlstring = "BFO 0";
            SendData(controlstring);

        }

        private void PBTreset_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PBTslider.Value = 0;
            PBTvalue.Text = "0";
            controlstring = "PBT 0";
            SendData(controlstring);

        }

        private void NFonoffbtn_Click(object sender, EventArgs e)
        {
            if (NFonoffbtn.Text == "Off")
            {
                NFonoffbtn.Text = "On";
                NFonoffbtn.BackColor = Color.LightGreen;
                controlstring = "NFM 1";
                SendData(controlstring);
            }
            else
            {
                NFonoffbtn.Text = "Off";
                NFonoffbtn.BackColor = Color.LightGray;
                controlstring = "NFM 0";
                SendData(controlstring);
            }
        }

        private void USBbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.LightGreen;
            LSBbtn.BackColor = Color.White;
            CWbtn.BackColor = Color.White;
            AMbtn.BackColor = Color.White;
            FMbtn.BackColor = Color.White;
            ISBbtn.BackColor = Color.White;
            knobControl1.Value = 47;
            controlstring = "DET 4";
            SendData(controlstring);
        }

        private void LSBbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.White;
            LSBbtn.BackColor = Color.LightGreen;
            CWbtn.BackColor = Color.White;
            AMbtn.BackColor = Color.White;
            FMbtn.BackColor = Color.White;
            ISBbtn.BackColor = Color.White;
            knobControl1.Value = 47;
            controlstring = "DET 5";
            SendData(controlstring);
        }

        private void CWbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.White;
            LSBbtn.BackColor = Color.White;
            CWbtn.BackColor = Color.LightGreen;
            AMbtn.BackColor = Color.White;
            FMbtn.BackColor = Color.White;
            ISBbtn.BackColor = Color.White;
            knobControl1.Value = 36;
            controlstring = "DET 3";
            SendData(controlstring);
        }

        private void AMbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.White;
            LSBbtn.BackColor = Color.White;
            CWbtn.BackColor = Color.White;
            AMbtn.BackColor = Color.LightGreen;
            FMbtn.BackColor = Color.White;
            ISBbtn.BackColor = Color.White;
            knobControl1.Value = 55;
            controlstring = "DET 1";
            SendData(controlstring);
        }

        private void FMbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.White;
            LSBbtn.BackColor = Color.White;
            CWbtn.BackColor = Color.White;
            AMbtn.BackColor = Color.White;
            FMbtn.BackColor = Color.LightGreen;
            ISBbtn.BackColor = Color.White;
            knobControl1.Value = 55;
            controlstring = "DET 2";
            SendData(controlstring);
        }

        private void formsPlot1_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void ISBbtn_Click(object sender, EventArgs e)
        {
            USBbtn.BackColor = Color.White;
            LSBbtn.BackColor = Color.White;
            CWbtn.BackColor = Color.White;
            AMbtn.BackColor = Color.White;
            FMbtn.BackColor = Color.White;
            ISBbtn.BackColor = Color.LightGreen;
            knobControl1.Value = 55;
            controlstring = "DET 6";
            SendData(controlstring);

         }

        private void ClearScanbtn_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Refresh();
        }

        private void ClearScanbtn_Click(object sender, EventArgs e)
        {

        }

        private void formsPlot1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Handle mouse click events

           if (e.Button == MouseButtons.Left)
            {
                double mousex = formsPlot1.GetMouseCoordinates().x;
                double mousey = formsPlot1.GetMouseCoordinates().y;
                double xValue = formsPlot1.Plot.XAxis.Dims.GetUnit(((float)mousex));
                formsPlot1.Plot.AddTooltip(label: "Special Point " + mousex.ToString() + " " + mousey.ToString(), x: mousex, y: mousey);

           /*     double mouseX = formsPlot1.GetMouseCoordinates().x;
                double frequency = formsPlot1.Plot.XAxis.Dims.GetUnit((float)mouseX);

                MessageBox.Show($"Frequency selected: {frequency}");
           */
                    // Add your radio frequency setting code here
                    // SetRadioFrequency(frequency);
            }
            
        }

        private void Scanbtn_Click(object sender, EventArgs e)
        {

            if (!serialPort1.IsOpen)
            { 
                MessageBox.Show("Serial port is not open.");
                return; 
            }

            if (!Check_Scan_Parameters())
                return;

            if (panel1.Enabled == true)
            {
                panel1.Enabled = false;
                panel1.BackColor = Color.LightGray;
                Scanbtn.BackColor = Color.LightGreen;
                Scanbtn.Text = "Scanning";
            }

            ScanLoop();
            
        }

        private void formsPlot1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*
            double mousex = formsPlot1.GetMouseCoordinates().x;
            double mousey = formsPlot1.GetMouseCoordinates().y;
            double xValue = formsPlot1.Plot.XAxis.Dims.GetUnit(((float)mousex));
            formsPlot1.Plot.AddTooltip(label: "Special Point " + mousex.ToString() + " " + mousey.ToString(), x: mousex, y: mousey);
            \*/
        }


        private void ScanLoop()
        {
            string scancontrolmesssage = "";
            string data = ""; 
            double[] dataX = new double[0];
            double[] dataY = new double[0];


            // set the Chart parameters
            formsPlot1.Plot.SetAxisLimitsX(scanstart, scanstop);
            formsPlot1.Refresh();

            scansteps = (int)((scanstop - scanstart) * 1000 / scanincrement);

            serialPort1.DataReceived -= new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            for (currentscanstep = 0; currentscanstep <= scansteps; ++currentscanstep )
            {
                if (Scanbtn.Text == "Scanning")
                {
                    scanfreq = scanstart + currentscanstep * scanincrement / 1000;
                    scancontrolmesssage = "FRQ " + scanfreq.ToString();
                    //tcs1 = new TaskCompletionSource<string>();
                    SendScanData(scancontrolmesssage, currentscanstep, scansteps);
                    //threadread = new Thread(Read() );
                    //string data = Read();

                    try
                    {
                        data = serialPort1.ReadLine();
                        data = data.Substring(4, 3);
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show("In WJ8712 SendScanData. Problem writing to port. Timeout exception.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error in writeout to serialport1. Original error: " + ex.Message);
                    };

                    if (!double.TryParse(data, out double newss))
                    {
                        MessageBox.Show("Invalid value incomming from receiver");
                        return;
                    }

                    //formsPlot1.Plot.AddPoint(scanfreq, newss, color: ScanColor[scannumber]);

                    dataX = dataX.Append(scanfreq).ToArray();
                    dataY = dataY.Append(newss).ToArray();

                    Log(LogMsgType.Incoming, data + "\n");
                    UpdateChart(dataX, dataY);
                }

                if (currentscanstep == scansteps)
                {
                    panel1.Enabled = true;
                    panel1.BackColor = Color.White;
                    Scanbtn.BackColor = Color.LightGray;
                    Scanbtn.Text = "Scan";
                }
            }

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

        }

        /*    private async Task<string> ReadFromSerialPortAsync(SerialPort serialPort)
            {
                return await Task.Run(() =>
                {
                    // return serialPort.ReadLine();
                    return Read();
                });
            } */

        private bool Check_Scan_Parameters()
        {
            if (double.TryParse(StartFreqtxt.Text, out scanstart))
            {
            }
            else
            {
                MessageBox.Show("Start Frequency is not a valid number");
                return false;
            }

            if (double.TryParse(StopFreqtxt.Text, out scanstop))
            {
            }
            else
            {
                MessageBox.Show("Stop Frequency is not a valid number");
                return false;
            }

            if (double.TryParse(ScanInctxt.Text, out scanincrement))
            {
            }
            else
            {
                MessageBox.Show("Scan Increment is not a valid number");
                return false;
            }

            if (scanstart < 0 || scanstart > 30)
            {
                MessageBox.Show("Start Frequency must be between 0 and 30");
                return false;
            }

            if (scanstop < 0 || scanstop > 30)
            {
                MessageBox.Show("Stop Frequency must be between 0 and 30");
                return false;
            }
            if (scanstop < scanstart)
            {
                MessageBox.Show("Stop Frequency must be greater than Start Frequency");
                return false;
            }

            if (scanincrement < 0.1)
            {
                MessageBox.Show("Scan Increment must be greater than 0.1 Kiloherz.");
                return false;
            }

            if (scanincrement/1000 > (scanstop - scanstart))
            {
                MessageBox.Show("Scan Increment must be less than the difference between Start and Stop Frequencies.");
                return false;
            }
     
            return true;
        }
    }
}
