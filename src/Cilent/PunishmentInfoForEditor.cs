using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace RandomPunisher
{
    public class PunishmentInfoForEditor : PunishmentInfo , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(object sender, NotifyCollectionChangedEventArgs e)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MediaList")); }

        public string STitle { get { return Title; } set { Title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")); } }

        public void Preview(object sender, RoutedEventArgs e) { (App.Current as App).NavToPage(this); }
    }
}
