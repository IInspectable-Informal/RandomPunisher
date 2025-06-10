using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RandomPunisher
{
    public sealed partial class RootPage : Page
    {
        private ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;

        private App CurApp = Application.Current as App; private Window CurWindow = Window.Current;

        private ApplicationView AppView = ApplicationView.GetForCurrentView();

        private CoreApplicationViewTitleBar CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        private string WindowTitle { get { return Package.Current.DisplayName; } }

        public RootPage()
        {
            this.InitializeComponent();
            CoreTitleBar.ExtendViewIntoTitleBar = true; CurWindow.SetTitleBar(AppTitleBarMain);
            AppView.TitleBar.ButtonInactiveBackgroundColor = AppView.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            SetHideTitleBarOption(); ChangeTitleBarButtonTheme(this); TitleBarFullScreenMode();
        }

        private void Animate(object sender, PointerRoutedEventArgs e) { muxc.AnimatedIcon.SetState(AniIcon, "PointerOver"); }

        private void Recovery(object sender, PointerRoutedEventArgs e) { muxc.AnimatedIcon.SetState(AniIcon, "Normal"); }

        private void TitleBarVisibilityChange(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            VisualStateManager.GoToState(TBBgMaterial, AppView.IsFullScreenMode ? "Fullscreen" : "Normal", false);
        }

        private void TitleBarFullScreenMode(object sender = null, WindowSizeChangedEventArgs e = null)
        {
            RootFrame.Margin = new Thickness(0, AppView.IsFullScreenMode ? 0 : 32, 0, 0);
            VisualStateManager.GoToState(TBBgMaterial, "Normal", false);
        }

        private void ChangeTitleBarButtonTheme(FrameworkElement sender, object args = null)
        {
            if (sender.ActualTheme == ElementTheme.Light)
            { AppView.TitleBar.ButtonForegroundColor = Colors.Black; AppView.TitleBar.ButtonInactiveForegroundColor = Colors.DarkGray; }
            else { AppView.TitleBar.ButtonForegroundColor = Colors.White; AppView.TitleBar.ButtonInactiveForegroundColor = Colors.LightGray; }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                DialogContent Content = new DialogContent() { Content = "SelectDrive" }; StorageFolder DriveRoot = null;
                if(e.Parameter is List<string> Param1 && Param1.Count > 1)
                {
                    if (Param1[1].Length == 1 && "CDEFGHIJKLMNOPQRSTUVWXYZAB".Contains(Param1[1].ToUpper()))
                    { DriveRoot = await StorageFolder.GetFolderFromPathAsync(Param1[1] + ":"); }
                    else { throw new FormatException("通过协议快速初始化本应用时第一个参数应为盘符。\n例如：" + Param1[0] + "://C"); }
                }
                else if (await CurApp.ShowDialog(new DialogTitle { Icon = "\xEDA2", Title = "选择驱动器" }, Content,
                    "退出应用", "继续", "", ContentDialogButton.None, false) == ContentDialogResult.Primary)
                { DriveRoot = await StorageFolder.GetFolderFromPathAsync(Content.GetDrive().Path); }
                else { await AppView.TryConsolidateAsync(); return; }
                CurApp.RootFolder = await DriveRoot.CreateFolderAsync("媒体资源", CreationCollisionOption.OpenIfExists);
                CurApp.JsonFile = await CurApp.RootFolder.CreateFileAsync("Punishments.json", CreationCollisionOption.OpenIfExists);
                CurApp.Punishments = await FileIO.ReadTextAsync(CurApp.JsonFile);
                if (CurApp.Punishments == "") { await FileIO.WriteTextAsync(CurApp.JsonFile, CurApp.Punishments = "[]"); }
                foreach (string PicFileType in CurApp.PicFileTypes) { CurApp.PicPicker.FileTypeFilter.Add(PicFileType); }
                foreach (string VidFileType in CurApp.VidFileTypes) { CurApp.VidPicker.FileTypeFilter.Add(VidFileType); }
                CurApp.VidFileTypes = null; CurApp.PicFileTypes = null; DriveRoot = null; CurApp.Root = this;
                RootFrame.Navigate(typeof(MainPage), null, new DrillInNavigationTransitionInfo());
                SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += ConfirmClose;
                if (e.Parameter is List<string> Param2 && Param2.Count > 2)
                {
                    switch (Param2[2])
                    {
                        case "编辑": goto case "修改";
                        case "修改": OpenEditor(); break;
                        case "惩罚": CurApp.Punish(); break;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            { _ = CurApp.ShowError("无法访问文件系统！！！\n请在设置允许本应用使用文件系统权限，\n然后重启本应用！"); DriveInfoEx.TryReqFSPrivilege(); }
            catch (Exception ex) { await CurApp.ShowError(ex.ToString()); await AppView.TryConsolidateAsync(); }
        }

        private void ConfirmClose(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            Deferral deferral = e.GetDeferral(); e.Handled = true;
            if (CurApp.DialogOpened == false) { CloseApp(0); }
            deferral.Complete();
        }

        private void NavigateBack(object sender, RoutedEventArgs e)
        { if (RootFrame.Content.GetType() == typeof(Editor)) { CloseApp(); } else { RootFrame.GoBack(); } }

        private void OnNavigated(object sender, NavigationEventArgs e)
        { BackButton.Visibility = RootFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed; }

        private void IsFullScreenMode(object sender, RoutedEventArgs e)
        {
            MenuIcon1.Glyph = AppView.IsFullScreenMode ? "\xE73F" : "\xE740";
            (sender as MenuFlyoutItem).Text = AppView.IsFullScreenMode ? "退出全屏" : "全屏";
        }

        private void FullScreen(object sender, RoutedEventArgs e)
        { if (AppView.IsFullScreenMode) { AppView.ExitFullScreenMode(); } else { AppView.TryEnterFullScreenMode(); } }

        private async void CloseApp(object sender = null, RoutedEventArgs e = null)
        {
            Type CurPage = RootFrame.Content.GetType();
            if (CurPage == typeof(Editor))
            {
                if (App.ConfirmClose)
                {
                    switch (await CurApp.ShowDialog(new DialogTitle { Icon = "\xE7BA", Title = "你的更改尚未保存" }, "如果继续离开，你所做的更改将丢失！\n是否保存？", "取消", "保存", "不保存", ContentDialogButton.Primary))
                    {
                        case ContentDialogResult.Primary: (RootFrame.Content as Editor).Save(sender == null ? "0" : null); break;
                        case ContentDialogResult.Secondary: App.ConfirmClose = false; if (sender != null) { await AppView.TryConsolidateAsync(); } else { RootFrame.GoBack(); } break;
                    }
                } else { if (sender != null) { await AppView.TryConsolidateAsync(); } else { RootFrame.GoBack(); } }
            }
            else if (CurPage == typeof(MainPage)) { await AppView.TryConsolidateAsync(); }
            else { await CurApp.ShowDialog(new DialogTitle { Icon = "\xE7BA", Title = "别跑！！！你被惩罚了！" }, "不想要接受惩罚就老老实实别趴台睡觉,\n这个程序有一双火眼金睛，\n台下的一切都被看得清清楚楚！！！", "关闭对话框"); }
        }

        private void IsMainPage(object sender, RoutedEventArgs e)
        { if (RootFrame.Content != null) { (sender as MenuFlyoutItem).IsEnabled = RootFrame.Content.GetType() == typeof(MainPage); } else { (sender as MenuFlyoutItem).IsEnabled = false; } }

        public void OpenEditor(object sender = null, RoutedEventArgs e = null)
        { RootFrame.Navigate(typeof(Editor), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft }); }

        private void GetHideTitleBarOption(object sender, RoutedEventArgs e)
        {
            (sender as MenuFlyoutItem).IsEnabled = !AppView.IsFullScreenMode;
            MenuIcon2.Glyph = (bool)RoamingSettings.Values["HideTitleBar"] ? "\xE8FB" : "";
        }

        private void SetHideTitleBarOption(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                bool HideTitleBar = !(bool)RoamingSettings.Values["HideTitleBar"]; if(sender == null) { HideTitleBar = !HideTitleBar; }
                if (HideTitleBar) { CurWindow.SizeChanged += TitleBarFullScreenMode; CoreTitleBar.IsVisibleChanged += TitleBarVisibilityChange; }
                else { try { CurWindow.SizeChanged -= TitleBarFullScreenMode; CoreTitleBar.IsVisibleChanged -= TitleBarVisibilityChange; } catch { } }
                RoamingSettings.Values["HideTitleBar"] = HideTitleBar;
            }
            catch
            {
                CurWindow.SizeChanged += TitleBarFullScreenMode; CoreTitleBar.IsVisibleChanged += TitleBarVisibilityChange;
                RoamingSettings.Values["HideTitleBar"] = true;
            }
        }

        private void GetAppTheme(object sender, RoutedEventArgs e)
        {
            muxc.RadioMenuFlyoutItem Item = sender as muxc.RadioMenuFlyoutItem;
            try { Item.IsChecked = Convert.ToInt16(RoamingSettings.Values["AppTheme"]) == Convert.ToInt16(Item.Tag); }
            catch { Item.IsChecked = Convert.ToInt16(Item.Tag) == 0; }
        }

        private void ChangeAppTheme(object sender, RoutedEventArgs e)
        {
            short Tag = Convert.ToInt16((sender as muxc.RadioMenuFlyoutItem).Tag);
            CurApp.rootFrame.RequestedTheme = (new ElementTheme[] { ElementTheme.Default, ElementTheme.Dark, ElementTheme.Light })[Tag];
            RoamingSettings.Values["AppTheme"] = Tag;
        }

        private void DoubleTapCloseApp(object sender, DoubleTappedRoutedEventArgs e) { CloseApp(0); }

        private void ShowFlyout(object sender, TappedRoutedEventArgs e) { (sender as UIElement).ContextFlyout.ShowAt(sender as FrameworkElement); }
    }
}
