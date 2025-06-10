using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace RandomPunisher.Pages
{
    public sealed partial class TextContainer : UserControl
    {
        public string Title { private get; set; } public new string Content { private get; set; }

        public TextContainer()
        {
            this.InitializeComponent();
        }

        private void GetFontFamilies(object sender, RoutedEventArgs e)
        {
            List<FontFamily> FontFamilies = new List<FontFamily> { }; int i0 = 0;
            string[] FontArray = CanvasTextFormat.GetSystemFontFamilies();
            for(int i = 0; i < FontArray.Length; i++)
            { FontFamilies.Add(new FontFamily(FontArray[i])); if(FontArray[i] == TitlePresenter.FontFamily.Source) { i0 = i; } }
            TextFont.ItemsSource = FontFamilies; TextFont.SelectedIndex = i0;
        }

        private void SetFont(object sender, SelectionChangedEventArgs e)
        { TitlePresenter.FontFamily = ContentPresenter.FontFamily = TextFont.SelectedItem as FontFamily; }
    }
}
