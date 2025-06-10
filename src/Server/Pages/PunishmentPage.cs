using System;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RandomPunisher.Pages
{
    public class PunishmentPage : Page , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ApplicationView AppView = ApplicationView.GetForCurrentView(); Window CurWindow = Window.Current;

        public TextContainer TextControl = new TextContainer();

        public MediaContainer MediaControl; bool IsFullAppView = false;
        
        public MediaContainer NormalSize = null; public MediaContainer FullAppView = null;

        void SetNS(MediaContainer Control) { NormalSize = Control; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NormalSize")); }

        void SetFAV(MediaContainer Control) { FullAppView = Control; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FullAppView")); }

        public PunishmentPage() { NormalSize = MediaControl = new MediaContainer(this); }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CurWindow.SizeChanged += SetFsButtonState;
            try
            {
                PunishmentInfo Info = e.Parameter as PunishmentInfo;
                MediaControl.Id = Info.Id; MediaControl.MediaList = Info.MediaList; TextControl.Title = Info.Title;
                if((MediaControl.Style2PW = Info.Style) == 6) { TextControl.ContentFontSize.Visibility = Visibility.Collapsed; }
                else { TextControl.Content = Info.Content; }
            } catch { }
        }

        private void SetFsButtonState(object sender, WindowSizeChangedEventArgs e)
        {
            if (AppView.IsFullScreenMode) { if (FullAppView == null) { SetFillAppView(); IsFullAppView = true; } }
            else { if (IsFullAppView && NormalSize == null) { SetFillAppView(); } }
            MediaControl.FullscreenIcon.Glyph = AppView.IsFullScreenMode ? "\xE73F" : "\xE740";
            MediaControl.FullscreenTip.Content = AppView.IsFullScreenMode ? "退出全屏" : "全屏";
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        { CurWindow.SizeChanged -= SetFsButtonState; MediaControl.Player.TimelineController.Pause(); MediaControl.Player.Dispose(); }

        public void SetFillAppView(object sender = null, RoutedEventArgs e = null)
        {
            if (FullAppView == null) { SetNS(null); SetFAV(MediaControl); TextControl.Visibility = Visibility.Collapsed; }
            else { SetFAV(null); SetNS(MediaControl); TextControl.Visibility = Visibility.Visible; } IsFullAppView = false;
            MediaControl.FillAppViewIcon.Glyph = FullAppView == null ? "\xE8A7" : "\xE944";
            MediaControl.FillAppViewTip.Content = FullAppView == null ? "填满应用视图" : "还原";
        }

        public void Fullscreen(object sender, RoutedEventArgs e)
        { if (AppView.IsFullScreenMode) { AppView.ExitFullScreenMode(); } else { AppView.TryEnterFullScreenMode(); } }
    }
}
