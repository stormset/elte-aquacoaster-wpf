using System;
using System.Windows.Media;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.ViewModel
{
    public class InfrastructureField : ViewModelBase
    {
        private Infrastructure _infrastructure;
        public Infrastructure Infrastructure
        {
            get => _infrastructure;
            set
            {
                if (_infrastructure != value)
                {
                    _infrastructure = value;
                    OnPropertyChanged();
                }
            }
        }

        private Point _coordinates;
        public Point Coordinates
        {
            get => _coordinates;
            set
            {
                if (_coordinates != value)
                {
                    _coordinates = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }

        public DelegateCommand ClickCommand { get; set; }
    }
}
