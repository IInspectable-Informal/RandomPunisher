using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Windows.Foundation;
using Windows.Storage;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using muxc = Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace RandomPunisher.Pages
{
    public sealed partial class MediaContainer : UserControl
    {
        double PaneWidth { get; set; } = 320; // MinWidth = 166

        public int Style2PW
        {
            set
            {
                switch (value)
                {
                    case 0: goto case 5;
                    case 1: goto case 4;
                    case 4: PaneWidth = 250; break;
                    case 5: PaneWidth = 160; break;
                }
            }
        }

        public ObservableCollection<MediaInfo> MediaList { get; set; }

        public string Id; MediaInfo Info;PunishmentPage CurPage;

        public MediaPlayer Player = new MediaPlayer() { TimelineController = new MediaTimelineController() };

        public MediaContainer(PunishmentPage Page)
        {
            CurPage = Page; this.InitializeComponent();
            Player.CommandManager.PauseReceived += Pause;
            Player.CommandManager.PlayReceived += Resume;
            Player.MediaOpened += async(s, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PlayButton.IsEnabled = true; Pos.Visibility = Visibility.Visible; Opened();
                    Pause(); Pos.Maximum = (Player.Source as MediaSource).Duration.Value.TotalSeconds * 10;
                });
            };
            Player.MediaEnded += async(s, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Player.TimelineController.Pause(); Pause(); Pos.Value = 0;
                    ItemsNavView.IsPaneToggleButtonVisible = true; TransportControl.Visibility = Visibility.Visible;
                });
            };
            Player.MediaFailed += (s, e) => { Failed(e.ErrorMessage); };
            ImgSource.ImageFailed += (s, e) => { Failed(e.ErrorMessage); };
            Loaded += (s, e) => { SetSize(); };
        }

        void Opened(object sender = null, RoutedEventArgs e = null) { Waiting.Visibility = Visibility.Collapsed; Waiting.IsIndeterminate = false; }

        void Failed(string ex) { Waiting.ShowError = true; _ = (App.Current as App).ShowError(ex); }

        public void SetSize(object sender = null, SizeChangedEventArgs e = null)
        {
            Size ActualSize = new Size(VidMonitor.ActualWidth, VidMonitor.ActualHeight);
            Player.SetSurfaceSize(ActualSize);
            var compositor = ElementCompositionPreview.GetElementVisual(VidMonitor).Compositor;
            var surface = Player.GetSurface(compositor);
            var spriteVisual = compositor.CreateSpriteVisual();
            spriteVisual.Size = ActualSize.ToVector2();
            spriteVisual.Brush = compositor.CreateSurfaceBrush(surface.CompositionSurface);
            var container = compositor.CreateContainerVisual();
            container.Children.InsertAtTop(spriteVisual);
            ElementCompositionPreview.SetElementChildVisual(VidMonitor, container);
        }

        private void SetTCVis(object sender, TappedRoutedEventArgs e)
        { TransportControl.Visibility = (ItemsNavView.IsPaneToggleButtonVisible = !ItemsNavView.IsPaneToggleButtonVisible) ? Visibility.Visible : Visibility.Collapsed; }

        private string FormatPath(string Path)
        {
            if(Path.Contains("://") || App.ValidPathMatcher.IsMatch(Path)) { return Path; }
            return (App.Current as App).RootFolder.Path + "\\" + Id + "\\" + Path;
        }

        private async void SelectMedia(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                Player.TimelineController.Pause(); Pause(); Pos.Value = 0; Info = sender.SelectedItem as MediaInfo;
                Waiting.ShowError = false; Waiting.Visibility = Visibility.Visible; Pos.Visibility = Visibility.Collapsed;
                if (Waiting.IsIndeterminate = Pos.IsEnabled = PlayButton.IsEnabled = VolControl.IsEnabled = Info.MediaType == "\xE8B2")
                {
                    VidMonitor.Visibility = Visibility.Visible; ImgMonitor.Visibility = Visibility.Collapsed;
                    if (Info.MediaPath.Contains("://")) { Player.Source = MediaSource.CreateFromUri(new Uri(Info.MediaPath)); }
                    else
                    {
                        StorageFile File = await StorageFile.GetFileFromPathAsync(FormatPath(Info.MediaPath));
                        Player.Source = MediaSource.CreateFromStream(await File.OpenReadAsync(), File.ContentType);
                    }
                }
                else
                {
                    ImgMonitor.Visibility = Visibility.Visible; VidMonitor.Visibility = Visibility.Collapsed; Player.Source = null;
                    if (Info.MediaPath.Contains("://")) { ImgSource.UriSource = new Uri(Info.MediaPath); }
                    else { ImgSource.SetSource(await (await StorageFile.GetFileFromPathAsync(FormatPath(Info.MediaPath))).OpenReadAsync()); }
                }
            }
            catch (Exception ex)
            {
                await (App.Current as App).ShowError(ex.ToString());
            }
        }

        private async void ChangePos(MediaTimelineController s, object e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            { Pos.Value = Player.TimelineController.Position.TotalSeconds * 10; });
        }

        private void Pause(MediaPlaybackCommandManager s = null, MediaPlaybackCommandManagerPauseReceivedEventArgs e = null)
        {
            PlayIcon.Glyph = "\xE768"; PlayTip.Content = "播放";
            try { Player.TimelineController.PositionChanged -= ChangePos; } catch { }
            Pos.ValueChanged += SetPos; Pos.IsEnabled = true;
        }

        private void Resume(MediaPlaybackCommandManager s = null, MediaPlaybackCommandManagerPlayReceivedEventArgs e = null)
        {
            PlayIcon.Glyph = "\xE769"; PlayTip.Content = "暂停";
            Player.TimelineController.PositionChanged += ChangePos;
            try { Pos.ValueChanged -= SetPos; } catch { } Pos.IsEnabled = false;
        }

        private void SetPos(object sender, RangeBaseValueChangedEventArgs e)
        {
            Player.TimelineController.Position = TimeSpan.FromSeconds(Pos.Value * 0.1);
        }

        private void PlayerControl(object sender, RoutedEventArgs e)
        {
            if(Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            { Player.TimelineController.Pause(); Pause(); }
            else { Player.TimelineController.Resume(); Resume(); }
        }
    }

    class Converter1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        { return TimeSpan.FromSeconds(System.Convert.ToDouble(value) * 0.1).ToString(@"hh\:mm\:ss").Replace(":", " : "); }

        public object ConvertBack(object value, Type targetType, object parameter, string language) { return null; }
    }

    class Converter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        { return System.Convert.ToDouble(value) * 100; }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        { return System.Convert.ToDouble(value) / 100; }
    }
}
