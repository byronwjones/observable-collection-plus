using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Collections.Tests
{
    /// <summary>
    /// A class that implements INotifyPropertyChanged
    /// </summary>
    internal class InpcObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int A
        {
            get { return _A; }
            set
            {
                _A = value;
                OnPropertyChanged("A");
            }
        }

        public int B
        {
            get { return _B; }
            set
            {
                _B = value;
                OnPropertyChanged("B");
            }
        }

        public int C
        {
            get { return _C; }
            set
            {
                _C = value;
                OnPropertyChanged("C");
            }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private int _A;
        private int _B;
        private int _C;
    }
}
