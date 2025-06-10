using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using RandomPunisher.Pages;

namespace RandomPunisher
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        private ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;

        private Window CurWindow = Window.Current; Type[] Pages = new Type[] { typeof(Page1), typeof(Page2), typeof(Page3), typeof(Page4), typeof(Page5), typeof(Page6), typeof(Page7), typeof(Page8) };

        public string[] PicFileTypes = new string[] { ".jpeg", ".png", ".bmp", ".tiff", ".ico", ".gif", ".svg" };

        public FileOpenPicker PicPicker = new FileOpenPicker() { SuggestedStartLocation = PickerLocationId.ComputerFolder, CommitButtonText = "选择选定的 图片", ViewMode = PickerViewMode.Thumbnail, SettingsIdentifier = "Pic" };

        public string[] VidFileTypes = new string[] { ".3g2", ".3gp", ".3gp2", ".3gpp", ".asf", ".avi", ".divx", ".m1v", ".m2ts", ".m2t", ".m2v", ".m4v", ".mkv", ".mod", ".mov", ".mp2v", ".mp4", ".mp4v", ".mpe", ".mpeg", ".mpg", ".mpg4", ".mpv2", ".mts", ".ogm", ".ogv", ".ogx", ".tod", ".ts", ".tts", ".webm", ".wm", ".wmv", ".xvid" };

        public FileOpenPicker VidPicker = new FileOpenPicker() { SuggestedStartLocation = PickerLocationId.ComputerFolder, CommitButtonText = "选择选定的 视频", ViewMode = PickerViewMode.Thumbnail, SettingsIdentifier = "Vid" };

        public StorageFolder RootFolder = null; public StorageFile JsonFile; public string Punishments; public JsonArray JsonTemp = new JsonArray();

        public Frame rootFrame; public RootPage Root; public static bool ConfirmClose = false; public bool DialogOpened = false;

        public static Regex ValidPathMatcher = new Regex(@"^([a-zA-Z]:|([a-zA-Z]:)?\\[^\/\:\*\?\""\<\>\|\,]*)$");

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public async Task<ContentDialogResult> ShowDialog(object Title, object Content, string CloseButtonText, string PrimaryButtonText = "", string SecondaryButtonText = "", ContentDialogButton DefaultButton = ContentDialogButton.None, bool IsPrimaryButtonEnabled = true, bool IsSecondaryButtonEnabled = true)
        {
            DialogOpened = true;
            ContentDialog Dialog = new ContentDialog()
            {
                Title = Title, Content = Content, RequestedTheme = rootFrame.ActualTheme, DefaultButton = DefaultButton,
                CloseButtonText = CloseButtonText, PrimaryButtonText = PrimaryButtonText, SecondaryButtonText = SecondaryButtonText,
                IsPrimaryButtonEnabled = IsPrimaryButtonEnabled, IsSecondaryButtonEnabled = IsSecondaryButtonEnabled,
                Style = Resources["DefaultContentDialogStyle"] as Style
            };
            switch (DefaultButton)
            {
                case ContentDialogButton.Primary: Dialog.PrimaryButtonStyle = Resources["AccentButtonStyle"] as Style; break;
                case ContentDialogButton.Secondary: Dialog.SecondaryButtonStyle = Resources["AccentButtonStyle"] as Style; break;
                case ContentDialogButton.Close: Dialog.CloseButtonStyle = Resources["AccentButtonStyle"] as Style; break;
            } ContentDialogResult Result = await Dialog.ShowAsync();
            DialogOpened = false; Dialog = null; return Result;
        }

        public async Task<ContentDialogResult> ShowError(string Content)
        { return await ShowDialog(new DialogTitle { Icon = "\xE783", Title = "错误" }, new DialogContent { Content = "ErrorInf", value = Content }, "确定"); }

        public void NavToPage(PunishmentInfo Info)
        { Root.RootFrame.Navigate(Pages[Info.Style], Info, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromBottom }); }

        public async void Punish()
        {
            try
            {
                if (JsonTemp.Count == 0) { JsonTemp = JsonArray.Parse(Punishments); }
                int i = new Random().Next(0, JsonTemp.Count - 1); JsonArray Temp = JsonTemp[i].GetArray();
                NavToPage(new PunishmentInfo
                {
                    Id = Temp[0].GetString(),
                    Title = Temp[1].GetString(),
                    Content = Temp[2].GetString(),
                    MediaList = GetMediaList(Temp[3].GetArray()),
                    Style = Convert.ToInt32(Temp[4].GetNumber())
                }); Temp = null; JsonTemp.RemoveAt(i);
            } catch (Exception ex) { await ShowError(ex.ToString()); }
        }

        public static ObservableCollection<MediaInfo> GetMediaList(JsonArray JAMediaList, bool option = false)
        {
            ObservableCollection<MediaInfo> MediaList = new ObservableCollection<MediaInfo> { }; JsonArray Temp;
            try
            {
                for (int i = 0; i < JAMediaList.Count; i++)
                { Temp = JAMediaList[i].GetArray(); MediaList.Add(new MediaInfo { MediaPath = Temp[0].GetString(), MediaType = Temp[1].GetString() }); }
            } catch { } if (option) { MediaList.CollectionChanged += (s, e) => { ConfirmClose = true; }; } Temp = null; return MediaList;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        { CommonLaunch(e.Arguments); }

        private void CommonLaunch(object param)
        {
            rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                try
                {
                    switch (Convert.ToInt16(RoamingSettings.Values["AppTheme"]))
                    {
                        case 0: rootFrame.RequestedTheme = ElementTheme.Default; break;
                        case 1: rootFrame.RequestedTheme = ElementTheme.Dark; break;
                        case 2: rootFrame.RequestedTheme = ElementTheme.Light; break;
                    }
                } catch { } rootFrame.Navigate(typeof(RootPage), param);
            } CurWindow.Activate();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    Uri uri = (args as ProtocolActivatedEventArgs).Uri;
                    List<string> v = uri.ToString().Substring(uri.Scheme.Length + 1).Split('/').ToList();
                    for(int i = v.Count - 1; i >= 0; i--)
                    {
                        if(new string[] { "", "/" }.Contains(v[i]))
                        { v.RemoveAt(i); }
                    } v.Insert(0, uri.Scheme); CommonLaunch(v); break;
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
