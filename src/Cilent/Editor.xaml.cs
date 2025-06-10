using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RandomPunisher
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Editor : Page , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<MediaInfo> LMediaList = null; Grid ListMgr;

        App CurApp = App.Current as App; PunishmentInfoForEditor SelItem = null;

        ObservableCollection<PunishmentInfoForEditor> PunishmentsList { get; set; }

        static ObservableCollection<PunishmentInfoForEditor> Item0 = null;

        public Editor()
        {
            this.InitializeComponent(); AdaptWindowWidth(); ListMgr = Resources["ListMgr"] as Grid;
            Loaded += (s, e) => { VisualStateManager.GoToState(StyleTextTip, "Disabled", false); }; Resources.Remove("ListMgr");
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                PunishmentsList = new ObservableCollection<PunishmentInfoForEditor> { };
                App.ConfirmClose = false; JsonArray Temp1 = JsonArray.Parse(CurApp.Punishments); JsonArray Temp2; PStyle.SelectionChanged += ConfirmClose1;
                try
                {
                    for (int i = 0; i < Temp1.Count; i++)
                    {
                        Temp2 = Temp1[i].GetArray();
                        PunishmentsList.Add(new PunishmentInfoForEditor
                        {
                            Id = Temp2[0].GetString(),
                            Title = Temp2[1].GetString(),
                            Content = Temp2[2].GetString(),
                            MediaList = App.GetMediaList(Temp2[3].GetArray(), true),
                            Style = (int)Temp2[4].GetNumber()
                        });
                    }
                }catch (Exception ex) { await CurApp.ShowError(ex.ToString()); }
                Temp1 = Temp2 = null;  PunishmentsList.CollectionChanged += (s, e1) => { ConfirmClose(); };
                if(e.Parameter != null) { Loaded += (s, e1) => { AddData(); }; }
            } else if(e.NavigationMode == NavigationMode.Back) { bool bConfirmClose = !App.ConfirmClose; PunishmentsList = Item0; }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            switch (e.NavigationMode)
            {
                case NavigationMode.New: Item0 = PunishmentsList; break;
                case NavigationMode.Back: Item0 = null; break;
            }
        }

        private void Animate(object sender, PointerRoutedEventArgs e) { muxc.AnimatedIcon.SetState(AniIcon, "PointerOver"); }

        private void Recovery(object sender, PointerRoutedEventArgs e) { muxc.AnimatedIcon.SetState(AniIcon, "Normal"); }

        private void AdaptWindowWidth(object sender = null, SizeChangedEventArgs e = null)
        {
            double Width = ActualWidth; WidePres.Margin = new Thickness(Width >= 772 ? 5 : 0, 0, 0, 0);
            OpenList.Visibility = Width >= 765 ? Visibility.Collapsed : Visibility.Visible;
            if (Width >= 765) { FlyoutPres.Content = null; WidePres.Content = ListMgr; }
            else { WidePres.Content = null; FlyoutPres.Content = ListMgr; }
        }

        private void SetTipStyle(object sender, DependencyPropertyChangedEventArgs e)
        { VisualStateManager.GoToState(StyleTextTip, ConfigPanel.IsEnabled ? "Normal" : "Disabled", false); }

        void AddData(object sender = null, RoutedEventArgs e = null)
        {
            PunishmentsList.Add(new PunishmentInfoForEditor { Id = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff"), Title = "示例标题", Content = "示例内容", Style = 7 });
            PunishmentsListView.SelectedIndex = PunishmentsList.Count - 1;
        }

        async void AddMedia(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuFlyoutItem).Text)
            {
                case "本地图片":
                    foreach (StorageFile Photo in await CurApp.PicPicker.PickMultipleFilesAsync()) { await AddFile(Photo, "\xE91B"); } break;
                case "本地视频":
                    foreach (StorageFile Video in await CurApp.VidPicker.PickMultipleFilesAsync()) { await AddFile(Video, "\xE8B2"); } break;
                case "流媒体":
                    bool? Loop = true; Regex ValidURLs = new Regex(@"(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&:/~\+#]*[\w\-\@?^=%&/~\+#])?");
                    DialogContent Content = new DialogContent() { Content = "GetURL" }; string[] URLs; List<MediaInfo> Results = new List<MediaInfo> { };
                    ContentDialog Dialog = new ContentDialog()
                    {
                        Content = Content, PrimaryButtonText = "继续", CloseButtonText = "取消",
                        RequestedTheme = ActualTheme, Style = CurApp.Resources["DefaultContentDialogStyle"] as Style
                    };
                    while (Loop == true)
                    {
                        CurApp.DialogOpened = true;
                        if (await Dialog.ShowAsync() == ContentDialogResult.Primary)
                        {
                            URLs = Content.URLs.Text.Split('\r'); Results = new List<MediaInfo>() { }; Content.ErrorReport.Text = "";
                            for (int i = 0; i < URLs.Length; i++)
                            {
                                if(URLs[i] != "")
                                {
                                    if (ValidURLs.IsMatch(URLs[i]))
                                    {
                                        bool ContainMedia = false;
                                        foreach(string Exts in CurApp.PicPicker.FileTypeFilter)
                                        { if (ContainMedia = URLs[i].Contains(Exts)) { Results.Add(new MediaInfo { MediaPath = URLs[i], MediaType = "\xE91B" }); break; } }
                                        if (!ContainMedia)
                                        {
                                            foreach(string Exts in CurApp.VidPicker.FileTypeFilter)
                                            { if (ContainMedia = URLs[i].Contains(Exts)) { Results.Add(new MediaInfo { MediaPath = URLs[i], MediaType = "\xE8B2" }); break; } }
                                        } if (!ContainMedia) { Content.ErrorReport.Text += "第 " + (i + 1) + " 行的 URL 不含支持的媒体\n"; }
                                    } else { Content.ErrorReport.Text += "第 " + (i + 1) + " 行不是有效的 URL 地址\n"; }
                                }
                            } if(Content.ErrorReport.Text == "") { Loop = false; }
                        } else { Loop = null; } CurApp.DialogOpened = false;
                    }
                    if(Loop == false)
                    {
                        List<string> OURLs = new List<string> { };
                        foreach (MediaInfo Media in LMediaList)
                        { if (Media.MediaPath.Contains("://")) { OURLs.Add(Media.MediaPath); } }
                        foreach (MediaInfo URL in Results)
                        {
                            if (!OURLs.Contains(URL.MediaPath))
                            { OURLs.Add(URL.MediaPath); LMediaList.Add(URL); }
                        }
                    } break;
            }
        }

        async void IdentifyObject(object sender, DragEventArgs e)
        {
            e.Handled = true; e.AcceptedOperation = DataPackageOperation.Link;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                foreach (IStorageItem Item in await e.DataView.GetStorageItemsAsync())
                { if (Item.IsOfType(StorageItemTypes.File)) { e.DragUIOverride.Caption = "释放以添加文件"; return; } }
                e.DragUIOverride.IsGlyphVisible = false; e.DragUIOverride.Caption = "请将文件拖到此处！";
            } else { e.DragUIOverride.IsGlyphVisible = false; e.DragUIOverride.Caption = "请将文件拖到此处！"; }
        }

        async void AddFiles(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                foreach (IStorageItem Item in await e.DataView.GetStorageItemsAsync())
                { if (Item.IsOfType(StorageItemTypes.File)) { await AddFile((StorageFile)Item); } }
            }
        }

        async Task AddFile(IStorageFile File, string Type = null)
        {
            for(int i = LMediaList.Count - 1; i >= 0; i--)
            {
                if(File.Name == LMediaList[i].MediaPath.Substring(LMediaList[i].MediaPath.LastIndexOf("\\") + 1))
                {
                    if(await CurApp.ShowDialog(new DialogTitle { Icon = "\xE9CE", Title = "真的要替换吗？" }, new DialogContent
                    {
                        Content = "ShowLingText",
                        value = "你真的要将原有资源：" + LMediaList[i].MediaPath + "\n替换为：" + File.Path + "\n吗?\n替换后，将无法撤回！"
                    }, "取消", "确定") == ContentDialogResult.Primary)
                    { LMediaList.RemoveAt(i); break; } else { return; }
                }
            }
            if (Type == null)
            {
                if (CurApp.PicPicker.FileTypeFilter.Contains(File.FileType.ToLower()))
                { LMediaList.Add(new MediaInfo { MediaPath = File.Path, MediaType = "\xE91B" }); }
                else if (CurApp.VidPicker.FileTypeFilter.Contains(File.FileType.ToLower()))
                { LMediaList.Add(new MediaInfo { MediaPath = File.Path, MediaType = "\xE8B2" }); }
            } else { LMediaList.Add(new MediaInfo { MediaPath = File.Path, MediaType = Type }); }
        }

        private void ConfirmClose(object sender = null, TextChangedEventArgs e = null) { App.ConfirmClose = true; }

        private void ConfirmClose1(object sender, SelectionChangedEventArgs e) { ConfirmClose(); }

        void Option(object sender = null, SelectionChangedEventArgs e = null)
        {
            switch (PStyle.SelectedIndex)
            {
                case -1:
                    if(PunishmentsListView.SelectedItems.Count == 1)
                    { _ = CurApp.ShowDialog(new DialogTitle { Icon = "\xE783", Title = "错误" }, "必须选择一个样式！", "确定"); PStyle.SelectedIndex = 7; }
                    break;
                case 6: ItemContent.IsEnabled = !(MediaListView.IsEnabled = true); break;
                case 7: ItemContent.IsEnabled = !(MediaListView.IsEnabled = false); break;
                default: ItemContent.IsEnabled = MediaListView.IsEnabled = true; break;
            }
        }

        void CanDelete(object sender, SelectionChangedEventArgs e)
        {
            bool bConfirmClose = !App.ConfirmClose;
            if (bConfirmClose) { ItemTitle.TextChanged -= ConfirmClose; ItemContent.TextChanged -= ConfirmClose; PStyle.SelectionChanged -= ConfirmClose1; }
            int i = PunishmentsListView.SelectedItems.Count; DelButton.IsEnabled = i != 0;
            if (ConfigPanel.IsEnabled = i == 1)
            { SelItem = PunishmentsList[PunishmentsListView.SelectedIndex]; LMediaList = SelItem.MediaList; }
            else { SelItem = null; LMediaList = null; }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelItem"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LMediaList"));
            if (bConfirmClose) { ItemTitle.TextChanged += ConfirmClose; ItemContent.TextChanged += ConfirmClose; PStyle.SelectionChanged += ConfirmClose1; }
        }

        void CanDelMedia(object sender, SelectionChangedEventArgs e)
        { DelMediaButton.IsEnabled = MediaListView.SelectedItems.Count != 0; }

        private void AllowMultipleManagement(object sender, RoutedEventArgs e)
        { PunishmentsListView.SelectionMode = (bool)(sender as CheckBox).IsChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single; }

        async void DelItem(object sender, RoutedEventArgs e)
        {
            if (await CurApp.ShowDialog(new DialogTitle { Icon = "\xE7BA", Title = "你真的要删除吗？" }, "所选惩罚项删除后，\n将永久丢失，且无法找回！！！", "取消", "确定") == ContentDialogResult.Primary)
            {
                IList<object> SelectedItems = PunishmentsListView.SelectedItems;
                for(int i = PunishmentsList.Count - 1; i >= 0; i--)
                { if (SelectedItems.Contains(PunishmentsList[i])) { PunishmentsList.RemoveAt(i); } }
            }
        }

        async void DelMedia(object sender, RoutedEventArgs e)
        {
            if (await CurApp.ShowDialog(new DialogTitle { Icon = "\xE7BA", Title = "你真的要删除吗？" }, "所选媒体删除后，\n将永久丢失，且无法找回！！！", "取消", "确定") == ContentDialogResult.Primary)
            {
                IList<object> SelectedMedia = MediaListView.SelectedItems;
                for (int i = LMediaList.Count - 1; i >= 0; i--)
                { if (SelectedMedia.Contains(LMediaList[i])) { LMediaList.RemoveAt(i); } }
            }
        }

        public async void Save(object sender = null, RoutedEventArgs e = null)
        {
            DialogContent Content = new DialogContent() { Content = "Wait" }; Content.EHandler += SaveData;
            if (await CurApp.ShowDialog(new DialogTitle { IsWaiting = true, Title = "正在保存" }, Content, "") == ContentDialogResult.Primary)
            { if (sender != null) { CurApp.Root.RootFrame.GoBack(); } else { await Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryConsolidateAsync(); } }
        }

        async void SaveData(object sender, EventArgs e)
        {
            DialogContent Content = sender as DialogContent; ContentDialog Dialog = Content.Parent as ContentDialog;
            try
            {
                //全局变量
                List<string> MediaSourcesRoots = new List<string> { }; Content.StatusText.Text = "正在保存数据，\n请勿最小化（以免被系统挂起）、关闭应用！！！";
                Dictionary<string, List<string>> NewMediaSources = new Dictionary<string, List<string>> { }; List<string> SNewMediaSources;
                Dictionary<string, List<string>> OldMediaSources = new Dictionary<string, List<string>> { }; List<string> SOldMediaSources;
                //数据处理：
                JsonArray Punishments = new JsonArray();
                JsonArray Punishment; JsonArray MediaList; JsonArray Media;
                foreach (PunishmentInfoForEditor Item in PunishmentsList)
                {
                    Punishment = new JsonArray();
                    Punishment.Add(JsonValue.CreateStringValue(Item.Id));
                    Punishment.Add(JsonValue.CreateStringValue(Item.Title));
                    Punishment.Add(JsonValue.CreateStringValue(Item.Style == 6 ? "" : Item.Content));
                    MediaList = new JsonArray();
                    if (Item.Style != 7 && Item.MediaList.Count != 0)
                    {
                        SNewMediaSources = new List<string> { }; SOldMediaSources = new List<string> { };
                        foreach (MediaInfo mediaInfo in Item.MediaList)
                        {
                            Media = new JsonArray();
                            if (App.ValidPathMatcher.IsMatch(mediaInfo.MediaPath))
                            {
                                Media.Add(JsonValue.CreateStringValue(mediaInfo.MediaPath.Substring(mediaInfo.MediaPath.LastIndexOf('\\') + 1)));
                                SNewMediaSources.Add(mediaInfo.MediaPath);
                            }
                            else
                            {
                                Media.Add(JsonValue.CreateStringValue(mediaInfo.MediaPath));
                                if (!mediaInfo.MediaPath.Contains("://")) { SOldMediaSources.Add(mediaInfo.MediaPath); }
                            }
                            Media.Add(JsonValue.CreateStringValue(mediaInfo.MediaType)); MediaList.Add(Media);
                        }
                        MediaSourcesRoots.Add(Item.Id); NewMediaSources.Add(Item.Id, SNewMediaSources); OldMediaSources.Add(Item.Id, SOldMediaSources);
                    }
                    Punishment.Add(MediaList);
                    Punishment.Add(JsonValue.CreateNumberValue(Item.Style));
                    Punishments.Add(Punishment);
                } string JsonText = Punishments.Stringify();
                //获取总大小
                double TotalSize = System.Text.Encoding.UTF8.GetByteCount(JsonText); double TotalNewSourceSize = TotalSize; double Items = 0;
                foreach (StorageFolder MediaSourcesRoot in await CurApp.RootFolder.GetFoldersAsync())
                {
                    if (MediaSourcesRoots.Contains(MediaSourcesRoot.Name))
                    {
                        SOldMediaSources = OldMediaSources[MediaSourcesRoot.Name];
                        foreach (StorageFile MediaSource in await MediaSourcesRoot.GetFilesAsync())
                        { if (!SOldMediaSources.Contains(MediaSource.Name)) { TotalSize -= (await MediaSource.GetBasicPropertiesAsync()).Size; Items += 1; } }
                    } else { TotalSize -= (await MediaSourcesRoot.GetBasicPropertiesAsync()).Size; }
                }
                foreach (string SourcesRoot in MediaSourcesRoots)
                {
                    foreach (string NewMediaSource in NewMediaSources[SourcesRoot])
                    { TotalNewSourceSize += (await (await StorageFile.GetFileFromPathAsync(NewMediaSource)).GetBasicPropertiesAsync()).Size; Items += 1; }
                } TotalSize += TotalNewSourceSize;
                string DrivePath = CurApp.RootFolder.Path.Substring(0, 2);
                ulong FreeSpace = (ulong)(await CurApp.RootFolder.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace" }))["System.FreeSpace"];
                if (TotalSize > FreeSpace)
                {
                    Dialog.Title = new DialogTitle { Icon = "\xE783", Title = "存储空间不足" };
                    Content.StatusText.Text = "无法保存！目标驱动器 " + DrivePath + " 的存储空间不足！" +
                        "\n所需总空间大小：" + DriveInfoEx.GetFriendlySpace(TotalSize) +
                        "\n剩余空间： " + DriveInfoEx.GetFriendlySpace(FreeSpace) +
                        "\n还需要：" + DriveInfoEx.GetFriendlySpace(TotalSize - FreeSpace);
                    Dialog.CloseButtonText = "关闭对话框";
                }
                else
                {
                    //文件系统操作
                    foreach (StorageFolder Folder in await CurApp.RootFolder.GetFoldersAsync())
                    { if (!MediaSourcesRoots.Contains(Folder.Name)) { await Folder.DeleteAsync(StorageDeleteOption.PermanentDelete); } }
                    StorageFolder SourcesRoot; StorageFile MediaSource;
                    Content.Status.IsIndeterminate = false; Content.Progress.Visibility = Visibility.Visible;
                    Content.Progress.Text = "0 / " + (Content.Status.Maximum = Items + 1);
                    foreach (string SourcesRootName in MediaSourcesRoots)
                    {
                        SourcesRoot = await CurApp.RootFolder.CreateFolderAsync(SourcesRootName, CreationCollisionOption.OpenIfExists);
                        SOldMediaSources = OldMediaSources[SourcesRootName];
                        foreach (StorageFile OldMediaSource in await SourcesRoot.GetFilesAsync())
                        {
                            if (!SOldMediaSources.Contains(OldMediaSource.Name))
                            {
                                Content.StatusText.Text = "正在删除：" + OldMediaSource.Name + "\n请勿最小化（以免被系统挂起）、关闭应用！！！";
                                await OldMediaSource.DeleteAsync(StorageDeleteOption.PermanentDelete);
                                Content.Progress.Text = (Content.Status.Value += 1) + " / " + (Items + 1);
                            }
                        }
                        foreach (string Source in NewMediaSources[SourcesRootName])
                        {
                            Content.StatusText.Text = "正在复制：" + Source + "\n到：" + DrivePath + "\n请勿最小化（以免被系统挂起）、关闭应用！！！";
                            MediaSource = await StorageFile.GetFileFromPathAsync(Source);
                            await MediaSource.CopyAsync(SourcesRoot, Source.Substring(Source.LastIndexOf('\\') + 1), NameCollisionOption.ReplaceExisting);
                            Content.Progress.Text = (Content.Status.Value += 1) + " / " + (Items + 1);
                        }
                    } CurApp.Punishments = JsonText; await FileIO.WriteTextAsync(CurApp.JsonFile, JsonText);
                    Content.Progress.Text = (Content.Status.Value += 1) + " / " + (Items + 1); CurApp.JsonTemp = new JsonArray();
                    Content.Status.Visibility = Content.Progress.Visibility = Visibility.Collapsed;
                    Dialog.Title = new DialogTitle { Icon = "\xE930", Title = "保存成功" };
                    Content.StatusText.Text = "惩罚列表及其媒体资源已全部保存！";
                    Dialog.PrimaryButtonText = "关闭对话框";
                }
                Punishments = Punishment = MediaList = Media = null; //内存回收
                NewMediaSources = OldMediaSources = null; MediaSourcesRoots = SNewMediaSources = SOldMediaSources = null; //内存回收
                Content.Status.IsIndeterminate = false; Content.Status.Visibility = Visibility.Collapsed; //内存回收
                Dialog.Focus(FocusState.Programmatic);
            }
            catch(Exception ex)
            {
                Dialog.Title = new DialogTitle { Icon = "\xE783", Title = "错误" };
                Dialog.Content = new DialogContent { Content = "ErrorInf", value = ex.ToString() };
                Dialog.CloseButtonText = "确定";
            }
        }
    }
}
