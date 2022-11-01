using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace imageSorter4
{
    public class Paging : INotifyPropertyChanged
    {

        private int size = 0;

        public int Size
        {
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
            get => Math.Min(page, size);
            private set
            {
                if (page != value)
                {
                    page = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Next()
        {
            if (Size <= 0) return false;

            if (page == Size)
                Page = 1;
            else
                Page++;

            return true;
        }

        public bool Prev()
        {
            if (Size <= 0) return false;

            if (page == 1)
                Page = Size;
            else
                Page--;

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
