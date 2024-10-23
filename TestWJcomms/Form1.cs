using System.IO.Ports;
using System.IO;

namespace TestWJcomms
{
    public partial class Form1 : Form
    {

        SerialPort serialPort1 = new System.IO.Ports.SerialPort();
        public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
        private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
        bool updateloop = false;
        string[] sigstr = {"010", "011", "013", "015", "017", "016", "012", "010", "013", "012", "028",
                            "026", "020", "017", "018", "012", "010", "009", "005", "010", "007", "011",
                            "015", "004", "011", "004", "014"};
        int sigstrindex;
        int step = 0;

        public Form1()
        {
            InitializeComponent();   
            serialPort1.ReadTimeout = 10000;
            serialPort1.WriteTimeout = 250;
            serialPort1.BaudRate = 9600;
            serialPort1.PortName = "COM1";
            serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            serialPort1.Open();

        }

        public void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // If the com port has been closed, do nothing
            if (!serialPort1.IsOpen)
            {
                MessageBox.Show("Port is no longer open.");
                return;

            }
            Read();
            //Thread readThread = new Thread(Read);
            //readThread.Start();
            //readThread.Join();

        }
        public void Read()
        {
            string message = "";
            string command = "";
            int msglength;

            if (command == "SVG?" || step == sigstr.Length)
            {
                step = 0;
            }

            //while (_continue)
            //{
                try
                {
                 msglength = serialPort1.BytesToRead;
                 message = serialPort1.ReadLine();
                 Log(LogMsgType.Incoming, message + "\n");

                //Thread.Sleep(50);

                 command = message.Substring(0, 4);

                 if (command == "SVG?")
                 {
                    SendData("SVG " + sigstr[step] + ",1");
                    step = step + 1;
                 }
                    

                 }
                catch (TimeoutException)
                {
                    MessageBox.Show("From TestWJcomms. Problem reading port. Timeout exception.");
                }
                catch
                {
                    MessageBox.Show("From TestWJcomms. Problem reading port. Some other exception.");
                }  

        }

        private void SendData(string msg)
        {
            // Send the user's text straight out the port
            try
            {
                serialPort1.Write(msg + "\n");
            }
            catch (TimeoutException)
            {
                MessageBox.Show("In TestWJcomms SendData. Problem writing to port. Timeout exception.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("error in writeout to serialport1. Original error: " + ex.Message);
                updateloop = false;
            };

            // Show in the terminal window the user's text
            Log(LogMsgType.Outgoing, msg + "\n");

            

        }

        /// <summary> Log data to the terminal window. </summary>
        /// <param name="msgtype"> The type of message to be written. </param>
        /// <param name="msg"> The string containing the message to be shown. </param>
        private void Log(LogMsgType msgtype, string msg)
        {
            rtfTerminal.Invoke(new EventHandler(delegate
            {
                rtfTerminal.SelectedText = string.Empty;
                rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
                rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
                rtfTerminal.AppendText(msg);
                rtfTerminal.ScrollToCaret();
            }));
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
