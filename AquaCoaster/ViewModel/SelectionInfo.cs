using AquaCoaster.Model.Entities;
using System;
using System.Windows.Media;


namespace AquaCoaster.ViewModel
{
    public class SelectionInfo : ViewModelBase
    {
        private Boolean _anySelected;
        public Boolean AnySelected
        {
            get => _anySelected;
            set
            {
                _anySelected = value;
                OnPropertyChanged();
            }
        }

        private Boolean _selectionActive;
        public Boolean SelectionActive
        {
            get => _selectionActive;
            set
            {
                _selectionActive = value;
                OnPropertyChanged();
            }
        }

        private Boolean _hoverActive;
        public Boolean HoverActive
        {
            get => _hoverActive;
            set
            {
                _hoverActive = value;
                OnPropertyChanged();
            }
        }

        private Boolean _IsValid;
        public Boolean IsValid
        {
            get => _IsValid;
            set
            {
                _IsValid = value;
                OnPropertyChanged();
            }
        }

        private Infrastructure _selectedInfrastructure;
        public Infrastructure SelectedInfrastructure
        {
            get => _selectedInfrastructure;
            set
            {
                _selectedInfrastructure = value;
                OnPropertyChanged();
            }
        }

        public Type SelectedType { get; set; }

        public String SelectedName { get; set; }

        public ImageSource SelectedImage { get; set; }

        private Utilities.Point _selectedSize;
        public Utilities.Point SelectedSize
        {
            get => _selectedSize;
            set
            {
                _selectedSize = value;
                OnPropertyChanged();
            }
        }

        private Utilities.Point _selectedCoords;
        public Utilities.Point SelectedCoords
        {
            get => _selectedCoords;
            set
            {
                _selectedCoords = value;
                OnPropertyChanged();
            }
        }
    }
}
