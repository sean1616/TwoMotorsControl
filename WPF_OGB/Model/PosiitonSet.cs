using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPF_OGB.ViewModels;

namespace WPF_OGB.Model
{
    public class PosiitonSet : NotifyBase
    {
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
    }
}
