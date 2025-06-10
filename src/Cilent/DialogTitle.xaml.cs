using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace RandomPunisher
{
    public sealed partial class DialogTitle : UserControl
    {
        public DialogTitle() { this.InitializeComponent(); }

        public string Icon { set { MoreIcon.Glyph = value; } }

        public bool IsWaiting
        {
            set
            {
                MoreIcon.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                Waiting.Visibility = value ? Visibility.Visible : Visibility.Collapsed; Waiting.IsActive = value;
            }
        }

        public string Title { set { Text.Text = value; } }
    }
}
