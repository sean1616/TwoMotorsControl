using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO.Ports;
using WPF_OGB.ViewModels;

namespace WPF_OGB.Model
{
    public class MotorSetModel : NotifyBase
    {
        public PosiitonSet P_Selected = new PosiitonSet();
        public PosiitonSet P1 = new PosiitonSet();
        public PosiitonSet P2 = new PosiitonSet();
        public PosiitonSet P3 = new PosiitonSet();
        public PosiitonSet P4 = new PosiitonSet();

        public MotorSetModel()
        {

        }

        public MotorSetModel(bool CMDType, int MotorSet_Index, int StepDelay, int Jop_Steps, bool Continue_Jog)
        {
            this.CMDType = CMDType;
            this.MotorSet_Index = MotorSet_Index;
            this.StepDelay = StepDelay;
            this.Jop_Steps = Jop_Steps;
            this.Continue_Jog = Continue_Jog;
        }

        private string _MotorSet_Name = "MotorSet";
        public string MotorSet_Name
        {
            get { return _MotorSet_Name; }
            set
            {
                _MotorSet_Name = value;
                OnPropertyChanged("MotorSet_Name");
            }
        }

        private bool _CMDType = false;  //false is Jog_Xp_1000, true is X+1000
        public bool CMDType
        {
            get { return _CMDType; }
            set { _CMDType = value; }
        }

        private int _MotorSet_Index = 1;
        public int MotorSet_Index
        {
            get { return _MotorSet_Index; }
            set
            {
                _MotorSet_Index = value;
            }
        }

        public SerialPort port = new SerialPort("COM24", 115200);

        private string _Comport = "COM3";
        public string Comport
        {
            get { return _Comport; }
            set
            {
                _Comport = value;
                OnPropertyChanged("Comport");
            }
        }

        private bool _Connected = false;
        public bool Connected
        {
            get { return _Connected; }
            set
            {
                _Connected = value;
                OnPropertyChanged("Connected");
            }
        }

        private bool _Continue_Jog = true;
        public bool Continue_Jog
        {
            get { return _Continue_Jog; }
            set
            {
                _Continue_Jog = value;
                OnPropertyChanged("Continue_Jog");
            }
        }

        private int _Jop_Steps = 300;
        public int Jop_Steps
        {
            get { return _Jop_Steps; }
            set
            {
                _Jop_Steps = value;
                OnPropertyChanged("Jop_Steps");
            }
        }

        private int _StepDelay = 10;
        public int StepDelay
        {
            get { return _StepDelay; }
            set
            {
                _StepDelay = value;
                OnPropertyChanged("StepDelay");
            }
        }

        private bool _view_vis = true;
        public bool view_vis
        {
            get { return _view_vis; }
            set
            {
                _view_vis = value;
                OnPropertyChanged("view_vis");
            }
        }

        private Visibility _IsSavePosShowClose = Visibility.Visible;
        public Visibility IsSavePosShowClose
        {
            get { return _IsSavePosShowClose; }
            set
            {
                _IsSavePosShowClose = value;
                OnPropertyChanged("IsSavePosShowClose");
            }
        }

        private string _Pos_X = "0";
        public string Pos_X
        {
            get { return _Pos_X; }
            set
            {
                _Pos_X = value;
                OnPropertyChanged("Pos_X");
            }
        }

        private string _Pos_Y = "0";
        public string Pos_Y
        {
            get { return _Pos_Y; }
            set
            {
                _Pos_Y = value;
                OnPropertyChanged("Pos_Y");
            }
        }

        private string _Pos_Z = "0";
        public string Pos_Z
        {
            get { return _Pos_Z; }
            set
            {
                _Pos_Z = value;
                OnPropertyChanged("Pos_Z");
            }
        }

        private string _sPos_X = "0";
        public string sPos_X
        {
            get { return _sPos_X; }
            set
            {
                _sPos_X = value;
                OnPropertyChanged("sPos_X");
            }
        }

        private string _sPos_Y = "0";
        public string sPos_Y
        {
            get { return _sPos_Y; }
            set
            {
                _sPos_Y = value;
                OnPropertyChanged("sPos_Y");
            }
        }

        private string _sPos_Z = "0";
        public string sPos_Z
        {
            get { return _sPos_Z; }
            set
            {
                _sPos_Z = value;
                OnPropertyChanged("sPos_Z");
            }
        }

        private string _Station_ID = "Station ID";
        public string Station_ID
        {
            get { return _Station_ID; }
            set
            {
                _Station_ID = value;
                OnPropertyChanged("Station_ID");
            }
        }


        private string _Station_Status = "IDLE";
        public string Station_Status
        {
            get { return _Station_Status; }
            set
            {
                _Station_Status = value;
                OnPropertyChanged("Station_Status");
            }
        }

        private bool _Motor_X_Reverse = false;
        public bool Motor_X_Reverse
        {
            get { return _Motor_X_Reverse; }
            set
            {
                _Motor_X_Reverse = value;
                OnPropertyChanged("Motor_X_Reverse");

                //Ini_Write("Motor_Setting", "Motor_X_Reverse", value.ToString());

                if (value)
                    SendCMD(string.Format("DIR X {0}", 0));
                else
                    SendCMD(string.Format("DIR X {0}", 1));
            }
        }

        private bool _Motor_Y_Reverse = false;
        public bool Motor_Y_Reverse
        {
            get { return _Motor_Y_Reverse; }
            set
            {
                _Motor_Y_Reverse = value;
                OnPropertyChanged("Motor_Y_Reverse");

                //Ini_Write("Motor_Setting", "Motor_Y_Reverse", value.ToString());

                if (value)
                    SendCMD(string.Format("DIR Y {0}", 0));
                else
                    SendCMD(string.Format("DIR Y {0}", 1));
            }
        }

        private bool _Motor_Z_Reverse = false;
        public bool Motor_Z_Reverse
        {
            get { return _Motor_Z_Reverse; }
            set
            {
                _Motor_Z_Reverse = value;
                OnPropertyChanged("Motor_Z_Reverse");

                //Ini_Write("Motor_Setting", "Motor_Z_Reverse", value.ToString());

                if (value)
                    SendCMD(string.Format("DIR Z {0}", 0));
                else
                    SendCMD(string.Format("DIR Z {0}", 1));
            }
        }

        private void SendCMD(string cmd)
        {
            if (port != null)
            {
                if (port.IsOpen)
                {
                    port.Write(cmd + "\r\n");
                    port.WriteLine(" ");
                }
            }
        }
    }
}
