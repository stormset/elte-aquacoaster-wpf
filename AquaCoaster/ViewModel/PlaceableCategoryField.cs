using System;
using System.Collections.ObjectModel;
using System.Windows.Media;


namespace AquaCoaster.ViewModel
{
    public class PlaceableCategoryField : ViewModelBase
    {
        public String CategoryName { get; set; }

        public WPFUI.Common.Icon CategoryIcon { get; set; }

        public ObservableCollection<PlaceableField> PlaceableFields { get; set; }
    }
}
