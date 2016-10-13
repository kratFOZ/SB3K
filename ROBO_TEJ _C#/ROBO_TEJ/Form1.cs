using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.IO.Ports;

namespace ROBO_TEJ
{


    public partial class frmMain : Form
    {
        //Console construct
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        int y,yMax,xMax;

        //public controllers
        int ctrl_powerLevel;
        int ctrl_powerLeft, ctrl_powerRight;
        Int16 ctrl_engineOn = 0;
        Int16 ctrl_dirMod = 1;

        bool serialEnable = true;


        bool[,] envArr = new bool[20, 20];


        private bool f, b, l, r;

        int ca = 0;
        int[] carr = new int[20]; 



       
        public frmMain()
        {
        
            InitializeComponent();

            spMain.PortName = "COM34";
            spMain.BaudRate = 38400;

            if (serialEnable)
            {
                try
                {
                    spMain.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                spMain.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceived);
            }
        }




        private void DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
              
                SerialPort spl = (SerialPort)sender;        
                string s = spl.ReadLine();

                Console.WriteLine(s);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            xMax = envArr.GetLength(1) - 1 / 2;
            yMax = envArr.GetLength(0) - 1 / 2;
            AllocConsole();
            stopEngine();
            this.Location = new Point(100, 200);
            this.KeyPreview = true;
            this.KeyDown += frmMain_KeyDown;
            this.KeyUp += frmMain_KeyUp;
            //RecSig();
        }



       
        //powerLevel modification done here
        void frmMain_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.W:
                {
                    if (!b)
                    {
                        btnW.BackColor = Color.Yellow;
                        f = true;
                    }
                    break;
                }
                case Keys.A:
                {
                    if (!r)
                    {
                        btnA.BackColor = Color.Yellow;
                        l = true;
                    }
                    break;
                        
                }
                case Keys.S:
                {
                    if (!f)
                    {
                        btnS.BackColor = Color.Yellow;
                        b = true;
                            
                    }
                    break;
                }
                case Keys.D:
                {
                    if (!l)
                    {
                        btnD.BackColor = Color.Yellow;
                        r = true;
                    }
                    break;
                }
            }

        }

        void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    {
                        btnW.BackColor = Color.White;
                        f = false;
                        break;
                    }
                case Keys.A:
                    {
                        btnA.BackColor = Color.White;
                        l = false;
                        break;

                    }
                case Keys.S:
                    {
                        btnS.BackColor = Color.White;
                        b = false;
                        break;
                    }
                case Keys.D:
                    {
                        btnD.BackColor = Color.White;
                        r = false;
                        break;
                    }
            }


            ctrl_powerLeft = 0;ctrl_powerRight = 0;

        }


        private void RecSig()
        {

        }

        private void SendSig()
        {
            
            /*string format:
             *0 - direction modifier (0 = forward, 1 = backward, 2 = left, 3 = right)
             *1 - left power (0-9)
             *2 - right power (0-9)
             */
            string s = "" + ctrl_dirMod + ctrl_powerLeft.ToString("0") + ctrl_powerRight.ToString("0");
            if (serialEnable)
            {      
                spMain.WriteLine(s);
                Console.WriteLine(s);
            }
        }




        //controls mouse power
        private void tmrMain_Tick(object sender, EventArgs e)
        {
            y = Screen.PrimaryScreen.Bounds.Height/2-Cursor.Position.Y;

            if (y > panel1.Height/2)
                y = panel1.Height/2-5;
            else if (y < -panel1.Height/2)
                y = -panel1.Height/2+5;

            ctrl_powerLevel = (int)Math.Round((((double)y/(double)panel1.Height*2)*100.0) / 10.0) * 10;
            if (ctrl_powerLevel >= 100)
                ctrl_powerLevel = 90;

            if (ctrl_powerLevel > 0)
                lblSpeed.Text = "Current speed:" + Math.Abs(ctrl_powerLevel) + '%';
            else
            {
                ctrl_powerLevel = 0;
                lblSpeed.Text = "Stopped";
            }


            label1.Top = (panel1.Height/2-y);
            label1.Text = ctrl_powerLevel.ToString();
            ctrl_powerLevel = ctrl_powerLevel / 10;
            ctrl_powerLevel = Math.Abs(ctrl_powerLevel);

            //forward or back
            if ((f || b) && !l && !r)
            {
                if (b)
                    ctrl_dirMod = 2;
                else
                    ctrl_dirMod = 1;
                ctrl_powerLeft = ctrl_powerLevel;
                ctrl_powerRight = ctrl_powerLeft;
            }
            //just left
            else if ((l || r) && !f && !b)
            {
                if (l)
                    ctrl_dirMod = 3;
                else
                    ctrl_dirMod = 4;
                ctrl_powerLeft = ctrl_powerLevel;
                ctrl_powerRight = ctrl_powerLeft;
            }
            else if ((f || b) && (l || r))
            {
                if ((f || b) && l)
                {
                    if (f)
                        ctrl_dirMod = 1;
                    else
                        ctrl_dirMod = 2;

                    ctrl_powerRight = ctrl_powerLevel;
                    ctrl_powerLeft = (int)Math.Round((double)ctrl_powerRight / 2);
                }
                else if ((f || b) && r)
                {
                    if (f)
                        ctrl_dirMod = 1;
                    else
                        ctrl_dirMod = 2;
                    ctrl_powerLeft = ctrl_powerLevel;
                    ctrl_powerRight = (int)Math.Round((double)ctrl_powerLeft / 2);
                }

            }

            SendSig();
        }

        void startEngine()
        {    
            Console.Clear();
            tmrMain.Enabled = true;
           // tmrControl.Enabled =true;
            ctrl_engineOn = 1;
            btnEngine.Text = "Engine (is:ON)";
            panel1.Enabled = true;
            panel1.BackColor = Color.Yellow;
            label1.Visible = true;
            ctrl_powerLevel = 0;            
        }

        void stopEngine()
        {
            ctrl_engineOn = 0;
            SendSig();
            tmrMain.Enabled = false;
           // tmrControl.Enabled = false;
            ctrl_engineOn = 0;
            btnEngine.Text = "Engine (is:OFF)";
            panel1.Enabled = false;            
            panel1.BackColor = Color.DimGray;
            label1.Visible = false;
            ctrl_powerLevel = 0;
            lblSpeed.Text = "ENGINE OFF";
        }

        //engine starter toggle
        private void button1_Click(object sender, EventArgs e)
        {
            if (ctrl_engineOn == 1)
                stopEngine();
            else
                startEngine();
        }

        //controls keyboard direction
        private void tmrKeyb_Tick(object sender, EventArgs e)
        {
            //forward or back
            if ((f || b) && !l && !r)
            {
                if(b)
                    ctrl_dirMod = 2;
                else
                    ctrl_dirMod = 1;
                ctrl_powerLeft = ctrl_powerLevel;
                ctrl_powerRight = ctrl_powerLeft ;
            }
            //just left
            else if ((l || r) && !f && !b)
            {
                if (l)
                    ctrl_dirMod = 3;
                else
                    ctrl_dirMod = 4;
                ctrl_powerLeft = ctrl_powerLevel;
                ctrl_powerRight = ctrl_powerLeft;
            }
            else if ((f || b) && (l || r))
            {
                if ((f || b) && l)
                {
                    if (f)
                        ctrl_dirMod = 1;
                    else
                        ctrl_dirMod = 2;

                    ctrl_powerRight = ctrl_powerLevel;
                    ctrl_powerLeft = (int)Math.Round((double)ctrl_powerRight/2);
                }
                else if ((f || b) && r)
                {
                    if (f)
                        ctrl_dirMod = 1;
                    else
                        ctrl_dirMod = 2;
                    ctrl_powerLeft = ctrl_powerLevel;
                    ctrl_powerRight = (int)Math.Round((double)ctrl_powerLeft / 2);
                }

            }

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

        }

    }
}