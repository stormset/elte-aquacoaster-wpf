using System;
using System.Windows.Media;


namespace AquaCoaster.Resources
{
    class ResourceModel
    {
        public Boolean Visible { get; set; } = true;
        public Int32 Order { get; set; }
#nullable enable
        public String? CategoryName { get; set; }
#nullable disable
        public ImageSource Image { get; set; }
        public Type TypeReference { get; set; }
    }
}
