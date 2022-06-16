using System;
using System.Windows.Media;
using AquaCoaster.Model.Entities;
using AquaCoaster.Utilities;


namespace AquaCoaster.ViewModel
{
    public class PersonField : ViewModelBase
    {
        private Person _person;
        public Person Person
        {
            get => _person;
            set
            {
                if (_person != value)
                {
                    _person = value;
                    OnPropertyChanged();
                }
            }
        }

        private Boolean _isVisible;
        public Boolean IsVisible
        {
            get => Person.Status != Model.Enums.PersonStatus.PLAYING && Person.Status != Model.Enums.PersonStatus.EATING && Person.Status != Model.Enums.PersonStatus.REPAIRING;
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
