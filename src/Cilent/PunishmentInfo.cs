using System.Collections.ObjectModel;

namespace RandomPunisher
{
    public class PunishmentInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public string Content { get; set; }

        public ObservableCollection<MediaInfo> MediaList = new ObservableCollection<MediaInfo> { };

        public int Style { get; set; }
    }
}
