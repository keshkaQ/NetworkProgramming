using System.Collections.ObjectModel;

namespace Producer.Models
{
    public class iPhoneModel
    {
        public string? ModelName { get; set; }
        public ObservableCollection<Iphone> ColorVariants { get; set; } = new ObservableCollection<Iphone>();
    }
}
