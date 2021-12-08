using Newtonsoft.Json.Linq;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VideoDown.Model;
using VideoDown.Utility;

namespace VideoDown.ViewModel
{
    public class MainViewModel : Screen
    {
        private readonly IWindowManager _windowManager;

        public List<UrlDBShow> UrlList { get; set; } = new List<UrlDBShow>();
        public string RichText { get; set; }
        public string LogText { get; set; }
        private readonly Jiexi jiexi = new Jiexi();

        public MainViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        protected override void OnInitialActivate()
        {
            UpdateApp();
        }

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public void Start()
        {
            if (string.IsNullOrWhiteSpace(RichText))
            {
                Message("链接不能为空");
                return;
            }

            var list = ProcessingData.StringToUrlList(RichText);
            if (list.Count == 0)
            {
                Message("链接不能为空");
                return;
            }
            tokenSource = new CancellationTokenSource();
            UrlList.Clear();
            IsChecked = false;
            List<UrlDBShow> showlist = new List<UrlDBShow>();
            Task.Factory.StartNew(() =>
            {
                var text = $"当前操作：批量解析\t";
                ProgressBarMaximum = list.Count;
                ProgressBarChange(0, text);
                for (int i = 0; i < list.Count; i++)
                {
                    if (tokenSource.IsCancellationRequested)
                        break;
                    var result = jiexi.GetDownUrl(list[i]);
                    if (result.Success)
                    {
                        Log($"第{i + 1}条解析成功");
                        var data = result.Data as UrlDB;
                        showlist.Add(new UrlDBShow { TypeCode = data.TypeCode, TypeName = data.TypeName, Url = data.Url, Num = i + 1, DownUrl = data.DownUrl, SavePath = data.SavePath, Explain = data.Explain, FileName = data.FileName, ID = data.ID, DownUrl1 = data.DownUrl1 }); ;
                    }
                    else
                    {
                        Log($"第{i + 1}条解析失败,{result.Message}");
                    }
                    ProgressBarChange(i + 1, text);
                }
                ProgressBarChange(ProgressBarMaximum, "");
                UrlList = showlist;

                Message($"链接分析完毕");
            });
        }

        #region 日志

        private void Log(string msg)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                LogText = $"{DateTime.Now}-{msg}{System.Environment.NewLine}" + LogText;
            }));
        }

        #endregion 日志

        #region 进度条

        public bool ProgressBarEnable { get; set; } = true;
        public Visibility Visibility_ProgressBar { get; set; } = Visibility.Collapsed;

        public long ProgressBarMaximum { get; set; }
        public long ProgressBarMinimum { get; set; } = 0;
        public long ProgressBarValue { get; set; }
        public string ProgressBarText { get; set; }

        private void ProgressBarChange(long i, string text)
        {
            Visibility_ProgressBar = Visibility.Visible;
            ProgressBarEnable = false;
            ProgressBarValue = i;
            ProgressBarText = $"{text}";
            if (ProgressBarValue >= ProgressBarMaximum)
            {
                ProgressBarEnable = true;
                Visibility_ProgressBar = Visibility.Collapsed;
            }
        }

        #endregion 进度条

        #region 打开目录

        public void Open()
        {
            PathHelper.Open();
        }

        #endregion 打开目录

        #region 更新

        private void UpdateApp()
        {
            var result = Update.CheckUpdate();
            if (result.Success)
            {
                try
                {
                    var updateInfo = JObject.Parse(result.Data.ToString());
                    var newVersion = new System.Version(updateInfo["Version"].ToString());
                    var oldVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    string UpdateTime = updateInfo["SoftUpdateTime"].ToString();
                    string UpdateInfo = updateInfo["UpdateInfo"].ToString();
                    string FullName = updateInfo["FullName"].ToString();
                    string UpdateUrl = updateInfo["UpdateUrl"].ToString();
                    if (newVersion > oldVersion)
                    {
                        Message($"检测到新版{newVersion},点击确定下载新版");
                        UrlHelper.OpenBrowser(UpdateUrl);
                        Process.GetCurrentProcess().Kill();
                    }
                }
                catch (Exception)
                {
                    Message("检测更新失败,软件退出");
                    Process.GetCurrentProcess().Kill();
                }
            }
            else
            {
                Message("检测更新失败,软件退出");
                Process.GetCurrentProcess().Kill();
            }
        }

        #endregion 更新

        #region 信息框

        private void Message(string msg)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                _windowManager.ShowMessageBox(msg);
            }));
        }

        #endregion 信息框

        #region 下载

        private void Down(string videoUrl, string saveFile)
        {
            var downLoader = new WebClient();
            downLoader.Headers.Add("User-Agent:Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1");
            downLoader.DownloadFileAsync(new Uri(videoUrl), saveFile);
            //downLoader.DownloadFileCompleted += DownLoader_DownloadFileCompleted;
            downLoader.DownloadProgressChanged += DownLoader_DownloadProgressChanged;
        }

        private void DownLoader_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                var text = $"正在下载，{string.Format("{0:F}", e.BytesReceived / 1024 / 1024) }M/{string.Format("{0:F}", e.TotalBytesToReceive / 1024 / 1024) }M ({string.Format("{0:F}", 100 * e.BytesReceived / e.TotalBytesToReceive) }%)";
                ProgressBarText = $"下载中 {e.TotalBytesToReceive / 1024}kb - {e.BytesReceived / 1024}kb";
                ProgressBarMaximum = e.TotalBytesToReceive;
                ProgressBarChange(e.BytesReceived, text);
            }));
        }

        #endregion 下载

        #region 下载勾选

        public void ToDwon()
        {
            tokenSource = new CancellationTokenSource();
            var list = UrlList.Where(x => x.Check).ToList();
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (tokenSource.IsCancellationRequested)
                        break;
                    if (string.IsNullOrWhiteSpace(list[i].DownUrl1))
                    {
                        Down(list[i].DownUrl, list[i].SavePath);
                    }
                    else
                    {
                        Down(list[i].DownUrl, list[i].SavePath);
                        Down(list[i].DownUrl1, list[i].SavePath.Replace("mp4", "mp3"));
                    }
                }
            });
        }

        #endregion 下载勾选

        #region 勾选方法

        public bool IsChecked { get; set; } = false;

        public void CheckedClick()
        {
            IsChecked = !IsChecked;
            List<UrlDBShow> list = new List<UrlDBShow>();
            foreach (var item in UrlList)
            {
                item.Check = IsChecked;
                list.Add(item);
            }
            UrlList.Clear();
            UrlList = list;
        }

        #endregion 勾选方法

        #region 菜单

        public UrlDBShow DataSelect { get; set; }

        public void CopyID()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.ID;

            Clipboard.SetDataObject(num);
        }

        public void CopyShuoMing()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.Explain;

            Clipboard.SetDataObject(num);
        }

        public void CopyUrl()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.Url;

            Clipboard.SetDataObject(num);
        }

        public void CopyDownUrl()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.DownUrl;

            Clipboard.SetDataObject(num);
        }

        public void ToUrl()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.Url;

            UrlHelper.OpenBrowser(num);
        }

        public void ToDownUrl()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.DownUrl;

            UrlHelper.OpenBrowser(num);
        }

        public void ToDownUrl1()
        {
            if (DataSelect == null)
            {
                Message("未选择任何数据行");
                return;
            }
            var num = DataSelect.DownUrl1;
            if (!string.IsNullOrWhiteSpace(num))
            {
                UrlHelper.OpenBrowser(num);
            }
        }

        #endregion 菜单
    }
}