using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO.Ports;
using WPF_OGB.ViewModels;
using WPF_OGB.Model;
using WPF_OGB.Functions;

namespace WPF_OGB.ViewModels 
{
    public class ComViewModel : NotifyBase
    {
        public SerialPort port = new SerialPort("COM12", 115200);
        //public SerialPort port_M1 = new SerialPort("COM24", 115200);

        private MotorSetModel _model_1 = new MotorSetModel(false,1, 30, 5000, false);
        public MotorSetModel model_1
        {
            get { return _model_1; }
            set
            {
                _model_1 = value;
                OnPropertyChanged("model_1");
            }
        }

        private MotorSetModel _model_2 = new MotorSetModel(true,2, 30, 5000, true);
        public MotorSetModel model_2
        {
            get { return _model_2; }
            set
            {
                _model_2 = value;
                OnPropertyChanged("model_2");
            }
        }

        public void SendCMD(string cmd)
        {
            if (port != null)
            {
                if (!port.IsOpen)
                {
                    Port_Connect(0);
                }

                if (port.IsOpen)
                {
                    port.Write(cmd + "\r");
                    //port.WriteLine(" ");
                }
                
            }
        }

        static string CurrentDirectory = Directory.GetCurrentDirectory();
        string ini_path = Path.Combine(CurrentDirectory, "Instrument.ini");

        public static SetupIniIP ini = new SetupIniIP();

        public string Ini_Read(string Section, string key)
        {
            string _ini_read;
            if (File.Exists(ini_path))
            {
                _ini_read = ini.IniReadValue(Section, key, ini_path);
            }
            else
                _ini_read = "";

            return _ini_read;
        }

        public void Ini_Write(string Section, string key, string value)
        {
            if (!File.Exists(ini_path))
                Directory.CreateDirectory(Directory.GetParent(ini_path).ToString());  //建立資料夾
            ini.IniWriteValue(Section, key, value, ini_path);  //創建ini file並寫入基本設定
        }

        public bool Port_Connect(int port_index)
        {
            try
            {
                if (port_index == 0)
                {
                    if (string.IsNullOrEmpty(Selected_Comport))
                    {
                        MessageBox.Show("Comport name is empty");
                        return false;
                    }

                    if (port != null)
                        if (!port.IsOpen)
                        {
                            port.Open();
                            port.DiscardInBuffer();
                            port.DiscardOutBuffer();
                        }
                }
                else if (port_index == 1)
                {
                    if(string.IsNullOrEmpty(model_1.Comport))
                    {
                        MessageBox.Show("Comport name is empty");
                        return false;
                    }

                    if (model_1.port != null)
                        if (!model_1.port.IsOpen)
                        {
                            model_1.port.Open();
                            model_1.port.DiscardInBuffer();
                            model_1.port.DiscardOutBuffer();
                        }
                }
                else if (port_index == 2)
                {
                    if (string.IsNullOrEmpty(model_2.Comport))
                    {
                        MessageBox.Show("Comport name is empty");
                        return false;
                    }

                    if (model_2.port != null)
                        if (!model_2.port.IsOpen)
                        {
                            model_2.port.Open();
                            model_2.port.DiscardInBuffer();
                            model_2.port.DiscardOutBuffer();
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect Failed");
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public bool Port_DisConnect(int port_index)
        {
            try
            {
                if (port_index == 0)
                {
                    if (port.IsOpen)
                    {
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        port.Close();
                    }
                }
                else if (port_index == 1)
                {
                    if (model_1.port.IsOpen)
                    {
                        model_1.port.DiscardInBuffer();
                        model_1.port.DiscardOutBuffer();
                        model_1.port.Close();
                    }
                }
                else if (port_index == 2)
                {
                    if (model_2.port.IsOpen)
                    {
                        model_2.port.DiscardInBuffer();
                        model_2.port.DiscardOutBuffer();
                        model_2.port.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Disconnect Failed");
                Console.WriteLine(ex.StackTrace.ToString());
                return false;
            }

            return true;
        }

        //private string _Selected_Comport_M1;
        //public string Selected_Comport_M1
        //{
        //    get { return _Selected_Comport_M1; }
        //    set
        //    {
        //        _Selected_Comport_M1 = value;
        //        OnPropertyChanged("Selected_Comport_M1");
        //    }
        //}

        private string _Selected_MotorSet;
        public string Selected_MotorSet
        {
            get { return _Selected_MotorSet; }
            set
            {
                _Selected_MotorSet = value;
                OnPropertyChanged("Selected_MotorSet");
            }
        }

        private string _Selected_Comport;
        public string Selected_Comport
        {
            get { return _Selected_Comport; }
            set
            {
                _Selected_Comport = value;
                OnPropertyChanged("Selected_Comport");

                port.PortName = value;
            }
        }

        private bool _Continue_Jog = false;
        public bool Continue_Jog
        {
            get { return _Continue_Jog; }
            set
            {
                _Continue_Jog = value;
                OnPropertyChanged("Continue_Jog");
            }
        }

        private int _spd_dist = 300;
        public int spd_dist
        {
            get { return _spd_dist; }
            set
            {
                _spd_dist = value;
                OnPropertyChanged("spd_dist");
            }
        }

        private double _CAM_angle = 0;
        public double CAM_angle
        {
            get { return _CAM_angle; }
            set
            {
                _CAM_angle = value;
                OnPropertyChanged("CAM_angle");
            }
        }

        private double _CAM_flip = 1;
        public double CAM_flip
        {
            get { return _CAM_flip; }
            set
            {
                _CAM_flip = value;
                OnPropertyChanged("CAM_flip");
            }
        }

       

        //private bool _Motor_X_Reverse = false;
        //public bool Motor_X_Reverse
        //{
        //    get { return _Motor_X_Reverse; }
        //    set
        //    {
        //        _Motor_X_Reverse = value;
        //        OnPropertyChanged("Motor_X_Reverse");

        //        //Ini_Write("Motor_Setting", "Motor_X_Reverse", value.ToString());

        //        if (value)
        //            SendCMD(string.Format("DIR X {0}", 0));
        //        else
        //            SendCMD(string.Format("DIR X {0}", 1));
        //    }
        //}

        //private bool _Motor_Y_Reverse = false;
        //public bool Motor_Y_Reverse
        //{
        //    get { return _Motor_Y_Reverse; }
        //    set
        //    {
        //        _Motor_Y_Reverse = value;
        //        OnPropertyChanged("Motor_Y_Reverse");

        //        //Ini_Write("Motor_Setting", "Motor_Y_Reverse", value.ToString());

        //        if (value)
        //            SendCMD(string.Format("DIR Y {0}", 0));
        //        else
        //            SendCMD(string.Format("DIR Y {0}", 1));
        //    }
        //}

        //private bool _Motor_Z_Reverse = false;
        //public bool Motor_Z_Reverse
        //{
        //    get { return _Motor_Z_Reverse; }
        //    set
        //    {
        //        _Motor_Z_Reverse = value;
        //        OnPropertyChanged("Motor_Z_Reverse");

        //        //Ini_Write("Motor_Setting", "Motor_Z_Reverse", value.ToString());

        //        if (value)
        //            SendCMD(string.Format("DIR Z {0}", 0));
        //        else
        //            SendCMD(string.Format("DIR Z {0}", 1));
        //    }
        //}
    }
}
