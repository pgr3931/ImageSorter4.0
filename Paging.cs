using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace imageSorter4
{
    public class Paging : INotifyPropertyChanged
    {

        private int size;

        public int Size {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    if (page > value) Page = value;
                    OnPropertyChanged();
                }
            }
        }

        private int page = 1;

        public int Page
        {
            get => page;
            private set
            {
                if (page != value)
                {
                    page = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Next()
        {
            if (page == Size)
                Page = 1;
            else
                Page++;
        }

        public void Prev()
        {
            if (page == 1)
                Page = Size;
            else
                Page--;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
