using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using muxc = Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace RandomPunisher
{
    public sealed partial class DialogContent : UserControl
    {
        App CurApp = App.Current as App; public event EventHandler EHandler;

        ObservableCollection<DriveInfoEx> AllDrivesList = new ObservableCollection<DriveInfoEx> { };

        public DialogContent() { this.InitializeComponent(); }

        private void SetContent(object sender, RoutedEventArgs e)
        {
            switch (Content)
            {
                case "SelectDrive": ErrorReport.Visibility = SelectDrive.Visibility = Visibility.Visible; GetDrives(); break;
                case "Wait": Processing.Visibility = Visibility.Visible; EHandler?.Invoke(this, new EventArgs()); break;
                case "ShowLingText": LongTextPresenter.Visibility = Visibility.Visible; LongTextPresenter.Text = value as string; break;
                case "ErrorInf":
                    ErrorInf.Visibility = Visibility.Visible; ErrorInf.Text = value as string;
                    (base.Content as ScrollViewer).VerticalScrollMode = ScrollMode.Disabled;
                    break;
            }
        }

        public object value { private get; set; }

        public new string Content { private get; set; }

        private async void GetDrives(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                DrivesList.IsEnabled = false; AllDrivesList.Clear(); StorageFolder Dir; IDictionary<string, object> Prop;
                foreach (DriveInfo Drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        Dir = await StorageFolder.GetFolderFromPathAsync(Drive.RootDirectory.ToString());
                        if((await Dir.TryGetItemAsync("媒体资源")).IsOfType(StorageItemTypes.Folder) &&
                            (await(await Dir.GetFolderAsync("媒体资源")).TryGetItemAsync("Punishments.json")).IsOfType(StorageItemTypes.File))
                        {
                            Prop = await Dir.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace", "System.Capacity" });
                            AllDrivesList.Add(new DriveInfoEx
                            {
                                Path = Drive.RootDirectory.ToString(),
                                Type = Drive.DriveType,
                                UsedSpace = (UInt64)Prop["System.Capacity"] - (UInt64)Prop["System.FreeSpace"],
                                TotalSpace = (UInt64)Prop["System.Capacity"]
                            });
                        }
                    } catch (NullReferenceException) { }
                } DrivesList.IsEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                ErrorReport.Text = "错误：无法访问文件系统！！！\n请在设置允许本应用使用文件系统权限，\n然后重启本应用！";
                DrivesList.PlaceholderText = ""; DriveInfoEx.TryReqFSPrivilege();
            } catch (Exception ex) { ErrorReport.Text = "发生了未知错误，信息如下：\n" + ex.Message; }
        }

        private void EnableContinue(object sender, SelectionChangedEventArgs e)
        { (Parent as ContentDialog).IsPrimaryButtonEnabled = DrivesList.SelectedItem != null; }

        public DriveInfoEx GetDrive() { return DrivesList.SelectedItem as DriveInfoEx; }
    }
}
