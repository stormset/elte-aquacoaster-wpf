using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.ViewModel
{
    public class PlaceableField : ViewModelBase
    {
        public Type ElementType { get; set; }

        public String Name { get; set; }

        public Int32 Price { get; set; }

        public String Size { get; set; }

        public String DetailedInfo { get; set; }


        private Boolean _selected;
        public Boolean Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public ImageSource Image { get; set; }

        public DelegateCommand ClickCommand { get; set; }
    }
}
