using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using WPF_OGB.ViewModels;
using WPF_OGB.Model;
using System.Windows.Controls.Primitives;
using AForge.Video;
using AForge.Video.DirectShow;

namespace WPF_OGB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComViewModel vm;
        public enum JC_Mode
        {
            Jog = 0,
            Continue = 1
        }

        public JC_Mode M1_JC_Mode = JC_Mode.Jog;

        public MainWindow()
        {
            InitializeComponent();

            vm = new ComViewModel();
            this.DataContext = vm;
            M1.DataContext = vm.model_1;
            M2.DataContext = vm.model_2;

            vm.model_1.MotorSet_Name = "CCD";
            vm.model_2.MotorSet_Name = "Fiber";

            vm.port.ReadTimeout = 30;

            //ReSet axis name for diff. setup
            btn_A_m.Name = "btn_Z_m";
            btn_A_p.Name = "btn_Z_p";
            btn_B_m.Name = "btn_Y_m";
            btn_B_p.Name = "btn_Y_p";
            btn_C_m.Name = "btn_X_m";
            btn_C_p.Name = "btn_X_p";

            btn_A_m.ToolTip = "Z-";
            btn_A_p.ToolTip = "Z+";
            btn_B_m.ToolTip = "Y-";
            btn_B_p.ToolTip = "Y+";
            btn_C_m.ToolTip = "X-";
            btn_C_p.ToolTip = "X+";

            try
            {
                string[] myPorts = SerialPort.GetPortNames();

                Thread.Sleep(10);

                foreach (string s in myPorts)
                {
                    string comport_and_ID = s;

                    cbox_M1_Comport.Items.Add(comport_and_ID);  //寫入所有取得的com
                    cbox_M2_Comport.Items.Add(comport_and_ID);  //寫入所有取得的com
                }

                //cbox_M1_Comport.SelectedIndex = cbox_M1_Comport.Items.Count - 1;
                //vm.model_1.port.PortName = cbox_M1_Comport.SelectionBoxItem.ToString();
                //cbox_M2_Comport.SelectedIndex = 0;

                string port_M1 = vm.Ini_Read("Setting", "PortName_M1");
                string port_M2 = vm.Ini_Read("Setting", "PortName_M2");

                string M1_Title = vm.Ini_Read("Setting", "M1_Title");
                string M2_Title = vm.Ini_Read("Setting", "M2_Title");

                if (!string.IsNullOrEmpty(M1_Title))
                    vm.model_1.MotorSet_Name = M1_Title;

                if (!string.IsNullOrEmpty(M2_Title))
                    vm.model_2.MotorSet_Name = M2_Title;


                if (!string.IsNullOrEmpty(port_M1))
                {
                    cbox_M1_Comport.SelectedItem = port_M1;
                    vm.model_1.port.PortName = cbox_M1_Comport.SelectionBoxItem.ToString();

                    //Auto Connect
                    if (vm.model_1.port != null)
                        vm.model_1.Connected = true;
                }
                else
                    cbox_M1_Comport.SelectedIndex = 0;

                if (!string.IsNullOrEmpty(port_M2))
                {
                    cbox_M2_Comport.SelectedItem = vm.Ini_Read("Setting", "PortName_M2");
                    vm.model_2.port.PortName = cbox_M2_Comport.SelectionBoxItem.ToString();

                    //Auto Connect
                    if (vm.model_2.port != null)
                        vm.model_2.Connected = true;
                }
                else
                    cbox_M2_Comport.SelectedIndex = 0;
            }
            catch { }


        }

        private void btn_Cam_Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!LocalWebCam.IsRunning)
            {
                LocalWebCam.Start();
                btn_Cam_Connect.IsEnabled = false;
                btn_Cam_Stop.IsEnabled = true;
            }
        }

        private void btn_Cam_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (LocalWebCam.IsRunning)
            {
                LocalWebCam.Stop();
                btn_Cam_Connect.IsEnabled = true;
                btn_Cam_Stop.IsEnabled = false;
            }
        }

        public VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;

        private void btn_Camera_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                for (int i = 0; i < LoaclWebCamsCollection.Count; i++)
                {
                    cbox_Cam.Items.Add(LoaclWebCamsCollection[i].Name);
                }

                if (int.TryParse(vm.Ini_Read("Setting", "Camera_Index"), out int index))
                {
                    if (index >= 0)
                        cbox_Cam.SelectedIndex = index;
                    else
                        cbox_Cam.SelectedIndex = 0;
                }
                else
                    cbox_Cam.SelectedIndex = 0;

                LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
                LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

                LocalWebCam.Start();

                btn_Cam_Connect.IsEnabled = false;
                btn_Cam_Stop.IsEnabled = true;
            }
            catch
            {
                txt_msg.Text += "Load Camera Fail\r";
            }
        }

        System.Drawing.Image img;
        System.Drawing.Bitmap bitImag;
        void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                img = (System.Drawing.Bitmap)eventArgs.Frame.Clone();
                bitImag = (System.Drawing.Bitmap)eventArgs.Frame.Clone();

                if (vm.CAM_angle == 90)
                    img.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                else if (vm.CAM_angle == 180)
                    img.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                else if (vm.CAM_angle == 270)
                    img.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);

                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new System.Threading.ThreadStart(delegate
                {
                    frameHolder.Source = bi;
                }));
            }
            catch { }
        }


        private void cbox_Cam_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (cbox_Cam.SelectedIndex >= 0)
                {
                    LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[cbox_Cam.SelectedIndex].MonikerString);
                    LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
                }
                else
                    cbox_Cam.SelectedIndex = 0;

                if (LocalWebCam != null)
                    if (!LocalWebCam.IsRunning)
                        LocalWebCam.Start();

                vm.Ini_Write("Setting", "Camera_Index", cbox_Cam.SelectedIndex.ToString());
            }
            catch { };
        }

        private void cbox_Cam_DropDownOpened(object sender, EventArgs e)
        {

            try
            {
                if (LocalWebCam != null)
                    if (LocalWebCam.IsRunning)
                        LocalWebCam.Stop();

                LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                cbox_Cam.Items.Clear();
                for (int i = 0; i < LoaclWebCamsCollection.Count; i++)
                {
                    cbox_Cam.Items.Add(LoaclWebCamsCollection[i].Name);
                }
            }
            catch { };
        }

        private void btn_Motor_Dir_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;

            if (btn.IsChecked == false)
            {
                btn.Content = "Cont. Mode";
                M1_JC_Mode = JC_Mode.Continue;
            }
            else
            {
                btn.Content = "Jog Mode";
                M1_JC_Mode = JC_Mode.Jog;
            }
        }



        private void cbox_M1_Comport_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                var cbox = (ComboBox)sender;

                cbox.Items.Clear();
                string[] myPorts = SerialPort.GetPortNames();

                Thread.Sleep(10);

                foreach (string s in myPorts)
                {
                    string comport_and_ID = s;

                    cbox.Items.Add(comport_and_ID);  //寫入所有取得的com
                }
            }
            catch { }
        }

        private void cbox_M1_Comport_DropDownClosed(object sender, EventArgs e)
        {
            var cbox = (ComboBox)sender;

            if (cbox.SelectedItem != null)
            {
                try
                {
                    string com = cbox.SelectedItem.ToString().Split(' ')[0];
                    int i = cbox.Items.IndexOf(cbox.SelectedItem);
                    cbox.Items[i] = com;
                    cbox.SelectedItem = com;

                    if (vm.Selected_MotorSet == "M1")
                    {
                        vm.model_1.port.PortName = cbox.SelectionBoxItem.ToString();
                        vm.Ini_Write("Setting", "PortName_M1", vm.model_1.port.PortName);
                    }
                    else if (vm.Selected_MotorSet == "M2")
                    {
                        vm.model_2.port.PortName = cbox.SelectionBoxItem.ToString();
                        vm.Ini_Write("Setting", "PortName_M2", vm.model_2.port.PortName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace.ToString());
                }
            }
        }

        private void btn_Port_connect_Checked(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;

            MotorSetModel model = (MotorSetModel)btn.DataContext;

            int index = model.MotorSet_Index;

            if (vm.Port_Connect(index))
                btn.Content = "Disconnect";
            else
                btn.IsChecked = false;
        }

        private void btn_Port_connect_Unchecked(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;

            MotorSetModel model = (MotorSetModel)btn.DataContext;

            int index = model.MotorSet_Index;

            if (vm.Port_DisConnect(index))
            {
                btn.Content = "Connect";
            }
        }

        public void SendCMD(string cmd)
        {
            if (vm.port != null)
            {
                if (vm.port.IsOpen)
                {
                    vm.port.Write(cmd + "\r\n");
                    //Thread.Sleep(50);
                    //vm.port.WriteLine(" ");
                }
            }
        }

        public void SendCMD(int pIndex, string cmd)
        {
            if (pIndex == 0)
            {
                SendCMD(cmd);
            }
            else if (pIndex == 1)
            {
                if (vm.model_1.port != null)
                {
                    if (vm.model_1.port.IsOpen)
                    {
                        vm.model_1.port.Write(cmd + "\r");
                    }
                }
            }
            else if (pIndex == 2)
            {
                if (vm.model_2.port != null)
                {
                    if (vm.model_2.port.IsOpen)
                    {
                        vm.model_2.port.Write(cmd + "\r");
                    }
                    else
                    {
                        vm.Port_Connect(pIndex);

                        Thread.Sleep(120);

                        vm.model_2.port.Write(cmd + "\r");
                    }
                }
            }
        }

        private void cbox_port_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                var cbox = (ComboBox)sender;

                cbox.Items.Clear();
                string[] myPorts = SerialPort.GetPortNames();

                Thread.Sleep(10);

                foreach (string s in myPorts)
                {
                    string comport_and_ID = s;

                    cbox.Items.Add(comport_and_ID);  //寫入所有取得的com
                }
            }
            catch { }
        }

        private void cbox_port_DropDownClosed(object sender, EventArgs e)
        {
            var cbox = (ComboBox)sender;
            if (cbox.SelectedItem != null)
            {
                try
                {
                    string com = cbox.SelectedItem.ToString().Split(' ')[0];
                    int i = cbox.Items.IndexOf(cbox.SelectedItem);
                    cbox.Items[i] = com;
                    cbox.SelectedItem = com;

                    if (!vm.port.IsOpen)
                        vm.port.PortName = com;
                    else
                    {
                        vm.port.Close();
                        vm.port.PortName = com;
                    }

                    //vm.Ini_Write("Connection_Setting", "PortName", com);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace.ToString());
                }
            }
        }

        private void txt_stepDelay_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (TextBox)sender;

                MotorSetModel model = (MotorSetModel)txt.DataContext;

                string cmd = "";
                if (!model.CMDType)
                    cmd = string.Format("SPD {0}", txt.Text);
                else
                    cmd = string.Format("D{0}", txt.Text);

                if (string.IsNullOrEmpty(cmd)) return;

                SendCMD(model.MotorSet_Index, cmd);

                Read_to_TextBox(model.MotorSet_Index, 120, false);
            }
        }

        private void btn_Disable_Focus_Change_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = true;
            }
        }

        

        private void btn_MotorMove_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var btn = (Button)sender;

            char xyz = ' ';
            char dir = ' ';

            if (btn.Name[4] == 'X')
                xyz = 'X';
            else if (btn.Name[4] == 'Y')
                xyz = 'Y';
            else if (btn.Name[4] == 'Z')
                xyz = 'Z';

            if (btn.Name[6] == 'p')
                dir = 'p';
            else if (btn.Name[6] == 'm')
                dir = 'm';

            string cmd = "";

            MotorSetModel model = new MotorSetModel();

            if (vm.Selected_MotorSet == "M1")
            {
                model = vm.model_1;
            }
            else if (vm.Selected_MotorSet == "M2")
            {
                model = vm.model_2;
            }

            if (btn.Name.Contains("M2"))
            {
                model = vm.model_2;
            }

            char a = 'm', b = 'p';

            if (dir == 'p')
            { a = 'p'; b = 'm'; };

            if (!model.Continue_Jog)  //Continue
            {
                if (!model.CMDType)
                {
                    switch (xyz)
                    {
                        case 'X':
                            cmd = model.Motor_X_Reverse ? string.Format("{0}{1}1", xyz, a) : string.Format("{0}{1}1", xyz, b);
                            break;

                        case 'Y':
                            cmd = model.Motor_Y_Reverse ? string.Format("{0}{1}1", xyz, a) : string.Format("{0}{1}1", xyz, b);
                            break;

                        case 'Z':
                            cmd = model.Motor_Z_Reverse ? string.Format("{0}{1}1", xyz, a) : string.Format("{0}{1}1", xyz, b);
                            break;
                    }
                }
                else
                {
                    cmd = dir == 'p' ? string.Format("{0}_{1}", xyz, "CW") : string.Format("{0}_{1}", xyz, "CCW");
                }
            }
            else  //Jog
            {
                if (!model.CMDType)
                {
                    switch (xyz)
                    {
                        case 'X':
                            cmd = model.Motor_X_Reverse ? string.Format("Jog_{0}{1}_{2}", xyz, a, model.Jop_Steps.ToString()) : string.Format("Jog_{0}{1}_{2}", xyz, b, model.Jop_Steps.ToString());
                            break;

                        case 'Y':
                            cmd = model.Motor_Y_Reverse ? string.Format("Jog_{0}{1}_{2}", xyz, a, model.Jop_Steps.ToString()) : string.Format("Jog_{0}{1}_{2}", xyz, b, model.Jop_Steps.ToString());
                            break;

                        case 'Z':
                            cmd = model.Motor_Z_Reverse ? string.Format("Jog_{0}{1}_{2}", xyz, b, model.Jop_Steps.ToString()) : string.Format("Jog_{0}{1}_{2}", xyz, a, model.Jop_Steps.ToString());
                            break;
                    }
                }
                else
                {
                    cmd = dir == 'p' ? string.Format("{0}{1}{2}", xyz, '+', model.Jop_Steps.ToString()) : string.Format("{0}{1}{2}", xyz, '-', model.Jop_Steps.ToString());
                }
            }

            SendCMD(model.MotorSet_Index, cmd);
        }

        private void btn_MotorMove_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var obj = (Button)sender;
            MotorSetModel model = (MotorSetModel)obj.DataContext;

            if (!model.CMDType)
            {
                if (!model.Continue_Jog)
                {
                    var btn = (Button)sender;

                    char xyz = ' ';
                    char dir = ' ';

                    if (btn.Name[4] == 'X')
                        xyz = 'X';
                    else if (btn.Name[4] == 'Y')
                        xyz = 'Y';
                    else if (btn.Name[4] == 'Z')
                        xyz = 'Z';

                    if (btn.Name[6] == 'p')
                        dir = 'p';
                    else if (btn.Name[6] == 'm')
                        dir = 'm';

                    string cmd = "";

                    if (model != null)
                    {
                        char a = 'm', b = 'p';

                        if (dir == 'p')
                        { a = 'p'; b = 'm'; };

                        switch (xyz)
                        {
                            case 'X':
                                cmd = model.Motor_X_Reverse ? string.Format("{0}{1}0", xyz, a) : string.Format("{0}{1}0", xyz, b);
                                break;

                            case 'Y':
                                cmd = model.Motor_Y_Reverse ? string.Format("{0}{1}0", xyz, a) : string.Format("{0}{1}0", xyz, b);
                                break;

                            case 'Z':
                                cmd = model.Motor_Z_Reverse ? string.Format("{0}{1}0", xyz, a) : string.Format("{0}{1}0", xyz, b);
                                break;
                        }

                        SendCMD(model.MotorSet_Index, cmd);
                    }
                }
            }
            else
            {
                if (!model.Continue_Jog)
                    SendCMD(model.MotorSet_Index, "STOP");
            }

            string str = Read_to_TextBox(model.MotorSet_Index, 120, false);

            if (str.Contains("Position:"))
            {
                str = str.Replace("Position:", "");
                str.Trim();

                string[] listPos = str.Split(',');
                if (listPos.Length == 3)
                {
                    model.Pos_X = listPos[0];
                    model.Pos_Y = listPos[1];
                    model.Pos_Z = listPos[2];
                }
            }
        }


        private void txt_cmd_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var txt = (TextBox)sender;
                vm.SendCMD(txt.Text);

                Read_to_TextBox(120, true);
            }
        }

        private void btn_SendCMD_Click(object sender, RoutedEventArgs e)
        {
            vm.SendCMD(txt_cmd.Text);

            Read_to_TextBox(120, true);
        }

        StringBuilder sb = new StringBuilder();
        private void Read_to_TextBox(int delay, bool isClose)
        {
            try
            {
                Thread.Sleep(delay);

                string msg = "";
                int size = 0;
                size = vm.port.BytesToRead;
                if (size > 0)
                {
                    byte[] dataBuffer = new byte[size];
                    vm.port.Read(dataBuffer, 0, size);
                    string Data = Encoding.ASCII.GetString(dataBuffer);

                    foreach (char c in Data)
                    {
                        sb.Append(c);

                        char LF = (char)10;

                        if (c == LF)
                        {
                            msg = sb.ToString();
                            sb = new StringBuilder();

                            txt_msg.Text += msg;
                        }
                    }
                }

                if (isClose) vm.port.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Read MSG Failed");
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }

        private string Read_to_TextBox(int port_index, int delay, bool isClose)
        {
            string result = "";
            try
            {
                Thread.Sleep(delay);

                string msg = "";
                int size = 0;

                if (port_index == 0)
                    size = vm.port.BytesToRead;
                else if (port_index == 1)
                    size = vm.model_1.port.BytesToRead;
                else if (port_index == 2)
                    size = vm.model_2.port.BytesToRead;
                else
                    return "";

                if (size > 0)
                {
                    byte[] dataBuffer = new byte[size];

                    if (port_index == 0)
                        vm.port.Read(dataBuffer, 0, size);
                    else if (port_index == 1)
                        vm.model_1.port.Read(dataBuffer, 0, size);
                    else if (port_index == 2)
                        vm.model_2.port.Read(dataBuffer, 0, size);

                    string Data = Encoding.ASCII.GetString(dataBuffer);

                    foreach (char c in Data)
                    {
                        sb.Append(c);

                        char LF = (char)10;

                        if (c == LF)
                        {
                            msg = sb.ToString();
                            sb = new StringBuilder();

                            txt_msg.Text += msg;
                        }
                    }
                }

                txt_msg.ScrollToEnd();

                if (isClose)
                {
                    if (port_index == 0)
                        vm.port.Close();
                    else if (port_index == 1)
                        vm.model_1.port.Close();
                    else if (port_index == 2)
                        vm.model_2.port.Close();
                }

                return msg.Replace("\r\n", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Read MSG Failed");
                Console.WriteLine(ex.StackTrace.ToString());
            }

            return "";
        }

        private void StackPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            var obg = (Grid)sender;
            //MessageBox.Show(obg.Name);
            vm.Selected_MotorSet = obg.Name;
        }

        private void tbtn_direction_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Selected_MotorSet == "M1")
            {
                string str = cbox_direction.SelectionBoxItem.ToString();
                if (str == "X")
                {
                    vm.model_1.Motor_X_Reverse = !vm.model_1.Motor_X_Reverse;
                }
                else if (str == "Y")
                {
                    vm.model_1.Motor_Y_Reverse = !vm.model_1.Motor_Y_Reverse;
                }
                else if (str == "Z")
                {
                    vm.model_1.Motor_Z_Reverse = !vm.model_1.Motor_Z_Reverse;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (LocalWebCam != null && LocalWebCam.IsRunning)
                LocalWebCam.Stop();

        }

        private void btn_Cam_Mirror_Click(object sender, RoutedEventArgs e)
        {
            vm.CAM_flip = -1 * vm.CAM_flip;
        }

        private void btn_Cam_Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (vm.CAM_angle < 270)
                vm.CAM_angle += 90;
            else
                vm.CAM_angle = 0;
        }

        private void btn_Cam_Window_Click(object sender, RoutedEventArgs e)
        {

        }

        bool TextCmd = false;
        private void btn_TextOff_Click(object sender, RoutedEventArgs e)
        {
            if (TextCmd)
                textCmd_width.Width = new GridLength(0, GridUnitType.Pixel);
            else
                textCmd_width.Width = new GridLength(1, GridUnitType.Star);

            TextCmd = !TextCmd;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txt_for_test.Text = this.Width.ToString();
        }

        private void TextBox_MotorSet_Name_KeyDown(object sender, KeyEventArgs e)
        {
            var obj = (TextBox)sender;
            //MotorSetModel model = obj.DataContext as MotorSetModel;

            if (vm.Selected_MotorSet == "M1")
                vm.Ini_Write("Setting", "M1_Title" , obj.Text);
            else if (vm.Selected_MotorSet =="M2")
                vm.Ini_Write("Setting", "M2_Title", obj.Text);
        }

        private void btn_Show_SavePosition_Box_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)sender;
            MotorSetModel model = obj.DataContext as MotorSetModel;

            if (model.IsSavePosShowClose == Visibility.Collapsed)
            {
                grid_SavePosBox.Visibility = Visibility.Collapsed;
                model.IsSavePosShowClose = Visibility.Visible;
            }
            else
            {
                model.IsSavePosShowClose = Visibility.Collapsed;
                grid_SavePosBox.Visibility = Visibility.Visible;
            }
        }

        private void btn_SavePosition_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)sender;
            MotorSetModel model = obj.DataContext as MotorSetModel;         

            model.P_Selected.Pos_X = model.Pos_X;
            model.P_Selected.Pos_Y = model.Pos_Y;
            model.P_Selected.Pos_Z = model.Pos_Z;

            model.sPos_X = model.P_Selected.Pos_X;
            model.sPos_Y = model.P_Selected.Pos_Y;
            model.sPos_Z = model.P_Selected.Pos_Z;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var obj = (RadioButton)sender;
            MotorSetModel model = obj.DataContext as MotorSetModel;

            string strTag = obj.Tag.ToString();

            switch (strTag)
            {
                case "P1":
                    model.P_Selected = model.P1;
                    break;
                case "P2":
                    model.P_Selected = model.P2;
                    break;
                case "P3":
                    model.P_Selected = model.P3;
                    break;
                case "P4":
                    model.P_Selected = model.P4;
                    break;
            }

            model.sPos_X = model.P_Selected.Pos_X;
            model.sPos_Y = model.P_Selected.Pos_Y;
            model.sPos_Z = model.P_Selected.Pos_Z;
        }
    }
}
