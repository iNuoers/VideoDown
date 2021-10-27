using CsharpHttpHelper;
using CsharpHttpHelper.Enum;
using Newtonsoft.Json.Linq;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using VideoDown.Model;
using VideoDown.Utility;

namespace VideoDown.ViewModel
{
    public class MainViewModel : Screen
    {
        private readonly IWindowManager _windowManager;

        private HttpHelper http = new HttpHelper();
        private HttpItem item = new HttpItem();
        private HttpResult result = new HttpResult();
        private string xiguaCookie = string.Empty;

        private List<UrlDB> UrlList = new List<UrlDB>();
        public string RichText { get; set; }
        public string LogText { get; set; }

        public MainViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
        protected override void OnInitialActivate()
        {
            UpdateApp();
            //ProductDB();
        }

        public void Start()
        {
            if (string.IsNullOrWhiteSpace(RichText))
            {
                _windowManager.ShowMessageBox("链接不能为空");
                return;
            }

            Thread fThread = new Thread(new ThreadStart(StringToUrlList));
            fThread.Start();
        }

        #region 分析链接

        private void StringToUrlList()
        {
            Log("开始分析链接！");
            if (true)
            {
                xiguaCookie = GetXiguaCookie();

            }
            var richTextArr = RichText.Replace("/r", "").Split(new string[] { "\n" }, StringSplitOptions.None);
            UrlList.Clear();

            Regex reg = new Regex(@"[a-zA-z]+://[^\s]*", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(RichText);
            foreach (Match m in mc)
            {
                string url = m.Value;

                if (!string.IsNullOrWhiteSpace(url))
                {
                    string name = "未知";
                    string code = "other";

                    if (url.Contains("huoshan.com"))
                    {
                        name = "火山";
                        code = "huoshan";
                    }
                    if (url.Contains("douyin.com"))
                    {
                        name = "抖音";
                        code = "douyin";
                    }
                    if (url.Contains("kuaishou"))
                    {
                        name = "快手";
                        code = "kuaishou";
                    }
                    if (url.Contains("ixigua"))
                    {
                        name = "西瓜";
                        code = "xigua";
                    }
                    if (url.Contains("pipix"))
                    {
                        name = "皮皮虾";
                        code = "pipix";
                    }

                    UrlList.Add(new UrlDB { url = url.Trim(), code = code, name = name });
                }
            }

            Log("分析链接完毕！");
            Log($"分析链接总数{UrlList.Count}条！");

            Thread fThread = new Thread(new ThreadStart(AnalysisUrls));
            fThread.Start();
        }

        private void AnalysisUrls()
        {
            Log("开始处理链接！");
            var ll = UrlList.Where(x=>x.code=="xigua").ToList();
            for (int i = 0; i < UrlList.Count; i++)
            {
                Log($"开始处理第{i + 1}个,{UrlList[i].name}！进度{i + 1}/{UrlList.Count}");

                switch (UrlList[i].code)
                {
                    case "douyin":
                        Douyin(UrlList[i].url, i);
                        break;

                    case "huoshan":
                        Huoshan(UrlList[i].url, i);

                        break;

                    case "kuaishou":
                        Kuaishou(UrlList[i].url, i);

                        break;

                    case "xigua":
                        Xigua(UrlList[i].url, i);

                        break;

                    case "pipix":
                        Pipixia(UrlList[i].url, i);

                        break;


                        

                    default:
                        Log($"不支持此链接！");
                        break;
                }
            }
            Log($"全部操作完毕.....！");
        }

        #endregion 分析链接

        #region 进度条

        public bool Enable { get; set; } = true;
        public Visibility Visibility_ProgressBar { get; set; } = Visibility.Collapsed;

        public long ProgressBarMaximum { get; set; }
        public long ProgressBarMinimum { get; set; } = 0;
        public long ProgressBarValue { get; set; }
        public string ProgressBarText { get; set; }

        private void ProgressBarChange(long i)
        {
            Visibility_ProgressBar = Visibility.Visible;
            Enable = false;
            ProgressBarValue = i;
            ProgressBarText = $"当前进度:{i}/{ProgressBarMaximum}";
            if (ProgressBarValue >= ProgressBarMaximum)
            {
                Enable = true;
                Visibility_ProgressBar = Visibility.Collapsed;
            }
        }

        #endregion 进度条

        #region 快手

        private void Kuaishou(string url, int i)
        {
            try
            {
                var html = Http_Get_301Mobil(url).Html;
                html = html.Replace(" ", "").Replace("\r", "").Replace("\n", "");
                var json = HttpHelper.GetBetweenHtml(html, "type=\"text/javascript\">window.pageData=", "}}</script>") + "}}";
                var jo = JObject.Parse(json);
                var videoUrl = jo["video"]["srcNoMark"];
                var title = jo["video"]["caption"];
                var id = jo["video"]["id"];
                //5192650406202789169
                if (videoUrl == null)
                {
                    Log($"第{i + 1}个解析失败");

                    return;
                }

                Log($"开始处理第{i + 1}个,{UrlList[i].name}！进度{i + 1}/{UrlList.Count}");
                var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video/kuaishou");
                var path = SavePathProgress($"快手-{title.ToString()}-{id.ToString()}.mp4");
                var saveFile = Path.Combine(saveDir, path);
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                if (!File.Exists(saveFile))
                {
                    Log($"开始下载第{i + 1}个！");

                    Down(videoUrl.ToString(), saveFile);
                }
                else
                {
                    Log($"第{i + 1}个文件已存在，无需重复下载！");
                }
            }
            catch (Exception e)
            {
                Log($"第{i + 1}个处理失败,错误原因 {e.Message}");
            }
        }

        #endregion 快手

        #region 西瓜

        private void Xigua(string url, int i)
        {
            try
            {
                var result = Http_Get_301(url, xiguaCookie);
                    var html = result.Html;
                if (html.Contains("404页"))
                {
                    Log($"第{i + 1}个处理失败,视频不存在");
                    return;

                }
                url = result.ResponseUri;
                var id = "";

                Regex regex = new Regex(@"/[0-9]{8,}");
                Match match = regex.Match(url);
                if (match.Success)
                {
                    id = match.Value;
                }

                //var id = HttpHelper.GetBetweenHtml(url, "https://www.ixigua.com/", "/"); ;
                if (string.IsNullOrWhiteSpace(id))
                {
                    Log($"第{i + 1}个处理失败,链接不匹配");
                    return;
                }

                html = html.Replace(" ", "").Replace("\r", "").Replace("\n", "");
                var json = HttpHelper.GetBetweenHtml(html, "window._SSR_HYDRATED_DATA=", "}</script>") + "}";
                var jo = JObject.Parse(json);
                var videojson = jo["anyVideo"]["gidInformation"]["packerData"]["video"];
                if (videojson == null)
                {
                    videojson = jo["anyVideo"]["gidInformation"]["packerData"];
                }
                var videoUrlListJson = videojson["videoResource"]["normal"]["video_list"];
                var videoUrl = videoUrlListJson["video_4"];
                if (videoUrl == null)
                {
                    videoUrl = videoUrlListJson["video_3"];
                }
                if (videoUrl == null)
                {
                    videoUrl = videoUrlListJson["video_2"];
                }
                if (videoUrl == null)
                {
                    videoUrl = videoUrlListJson["video_1"];
                }
                videoUrl = videoUrl["main_url"];
                if (videoUrl == null)
                {
                    Log($"第{i + 1}个解析失败");
                    return;
                }

                var title = videojson["title"] != null ? videojson["title"] : jo["anyVideo"]["gidInformation"]["packerData"]["albumInfo"]["title"];

                if (videoUrl == null)
                {
                    Log($"第{i + 1}个解析失败");

                    return;
                }
                var videoUrlString = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(videoUrl.ToString(), x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
                videoUrlString = HttpHelper.Base64ToString(videoUrlString, Encoding.UTF8);

                Log($"开始处理第{i + 1}个,{UrlList[i].name}！进度{i + 1}/{UrlList.Count}");
                var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video/xigua");
                var path = SavePathProgress($"西瓜-{title.ToString()}-{id.ToString()}.mp4");
                var saveFile = Path.Combine(saveDir, path);

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                if (!File.Exists(saveFile))
                {
                    Log($"开始下载第{i + 1}个！");

                    Down(videoUrlString, saveFile);
                }
                else
                {
                    Log($"第{i + 1}个文件已存在，无需重复下载！");
                }
            }
            catch (Exception e)
            {
                Log($"第{i + 1}个处理失败,错误原因 {e.Message}");
            }
        }

        private string GetXiguaCookie()
        {
          var  result = Http_Get_Un301("https://i.snssdk.com/slardar/sdk.js?bid=xigua_video_web_pc");
            var setCookie = result.Header["Set-Cookie"];
            var MONITOR_WEB_ID = HttpHelper.GetBetweenHtml(setCookie, "MONITOR_WEB_ID=", ";");
            string post = "{\"region\":\"cn\",\"aid\":1768,\"needFid\":false,\"service\":\"www.ixigua.com\",\"migrate_info\":{\"ticket\":\"\",\"source\":\"node\"},\"cbUrlProtocol\":\"https\",\"union\":true}";
            result = Http_Post("https://ttwid.bytedance.com/ttwid/union/register/", post);
            setCookie = result.Header["Set-Cookie"];
            var ttwid = HttpHelper.GetBetweenHtml(setCookie, "ttwid=", ";");

            return $"MONITOR_WEB_ID={MONITOR_WEB_ID};ttwid={ttwid}";
        }

        #endregion 西瓜

        #region 抖音

        private void Douyin(string url, int i)
        {
            try
            {
                url = Http_Get_301(url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "https://www.douyin.com/video/", "previous_page").Replace("?","");

                if (string.IsNullOrEmpty(urlID))
                {
                    Log($"第{i + 1}个获取标识码失败,跳过");
                    return;
                }
                var jsonUrl = $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={urlID}&dytk=";
                var result = Http_Get_301(jsonUrl, null);
                var cookie = result.Cookie;
                var json = JObject.Parse(result.Html);
                var url_list = json["item_list"][0]["video"]["play_addr"]["url_list"][0];
                var titleJson = json["item_list"][0]["desc"];//desc
                var title = DateTime.Now.ToString("yyyyMMddhhmmss");
                if (titleJson != null)
                {
                    title = titleJson.ToString().Trim();
                }
                if (url_list != null)
                {
                    var videoUrl = url_list.ToString().Replace("playwm", "play");
                    var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video/douyin");
                    var path = SavePathProgress($"抖音-{title.ToString()}-{urlID}.mp4");
                    var saveFile = Path.Combine(saveDir, path);
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    if (!File.Exists(saveFile))
                    {
                        Log($"开始下载第{i + 1}个！");
                        Down(videoUrl, saveFile);
                    }
                    else
                    {
                        Log($"第{i + 1}个文件已存在，无需重复下载！");
                    }
                }
                Log($"第{i + 1}个操作完毕.....！");
            }
            catch (Exception e)
            {
                Log($"第{i + 1}个操作失败.....！");
                Log($"失败原因:{e.Message}！");
            }
        }

        #endregion 抖音


        #region 皮皮虾

        private void Pipixia(string url, int i)
        {
            try
            {
                url = Http_Get_301(url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "https://h5.pipix.com/item/", "app_id=").Replace("?","");

                if (string.IsNullOrEmpty(urlID))
                {
                    Log($"第{i + 1}个获取标识码失败,跳过");
                    return;
                }
                var jsonUrl = $"https://is.snssdk.com/bds/item/detail/?app_name=super&aid=1319&item_id={urlID}";
                var result = Http_Get_301(jsonUrl, null);
                var cookie = result.Cookie;
                var json = JObject.Parse(result.Html);
                var url_list = json["data"]["data"]["origin_video_download"]["url_list"][0]["url"];
                var titleJson = json["data"]["data"]["video"]["video_download"]["title"];
                var title = DateTime.Now.ToString("yyyyMMddhhmmss");
                if (titleJson != null)
                {
                    title = titleJson.ToString().Trim();
                }
                if (url_list != null)
                {
                    var videoUrl = url_list.ToString();
                    var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video/pipix");
                    var path = SavePathProgress($"皮皮虾-{title.ToString()}-{urlID}.mp4");
                    var saveFile = Path.Combine(saveDir, path);
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    if (!File.Exists(saveFile))
                    {
                        Log($"开始下载第{i + 1}个！");
                        Down(videoUrl, saveFile);
                    }
                    else
                    {
                        Log($"第{i + 1}个文件已存在，无需重复下载！");
                    }
                }
                Log($"第{i + 1}个操作完毕.....！");
            }
            catch (Exception e)
            {
                Log($"第{i + 1}个操作失败.....！");
                Log($"失败原因:{e.Message}！");
            }
        }

        #endregion 抖音




        #region 火山

        private void Huoshan(string url, int i)
        {
            try
            {
                var result = Http_Get_301(url);
                url = Http_Get_301(url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "item_id=", "&");
                if (string.IsNullOrEmpty(urlID))
                {
                    Log($"第{i + 1}个获取标识码失败,跳过");
                    return;
                }
                var jsonUrl = $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={urlID}&dytk=";
                var json = JObject.Parse(Http_Get_301(jsonUrl, null).Html);
                var url_list = json["item_list"][0]["video"]["play_addr"]["url_list"][0];
                var titleJson = json["item_list"][0]["desc"];//desc
                var title = DateTime.Now.ToString("yyyyMMddhhmmss");
                if (titleJson != null)
                {
                    title = titleJson.ToString().Trim();
                }
                if (url_list != null)
                {
                    var videoUrl = url_list.ToString().Replace("playwm", "play");
                    var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video/huoshan");
                    var path = SavePathProgress($"火山-{title.ToString()}-{urlID}.mp4");
                    var saveFile = Path.Combine(saveDir, path);

                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir);
                    }
                    if (!File.Exists(saveFile))
                    {
                        Log($"开始下载第{i + 1}个！");

                        Down(videoUrl, saveFile);
                    }
                    else
                    {
                        Log($"第{i + 1}个文件已存在，无需重复下载！");
                    }
                }
                Log($"第{i + 1}个操作完毕.....！");
            }
            catch (Exception e)
            {
                Log($"第{i + 1}个操作失败.....！");
                Log($"失败原因:{e.Message}！");
            }
        }

        #endregion 火山

        #region 下载

        private void Down(string videoUrl, string saveFile)
        {
            var downLoader = new WebClient();
            downLoader.Headers.Add("User-Agent:Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1");
            downLoader.DownloadFileAsync(new Uri(videoUrl), saveFile);
            //downLoader.DownloadFileCompleted += DownLoader_DownloadFileCompleted;
            downLoader.DownloadProgressChanged += DownLoader_DownloadProgressChanged;
        }

        private void DownLoader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                ProgressBarText = $"下载中 {e.TotalBytesToReceive / 1024}kb - {e.BytesReceived / 1024}kb";
                ProgressBarMaximum = e.TotalBytesToReceive;
                ProgressBarChange(e.BytesReceived);
            }));
        }

        #endregion 下载

        #region 打开目录

        public void Open()
        {
            var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video");

            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            System.Diagnostics.Process.Start(saveDir);
        }

        #endregion 打开目录

        #region 日志

        private void Log(string msg)
        {
            LogText = $"{DateTime.Now}-{msg}{System.Environment.NewLine}" + LogText;
        }

        #endregion 日志

        #region Http请求

        private HttpResult Http_Get_Un301(string url, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                Method = "GET",
                Cookie = cookie,

                IsToLower = false,
                Allowautoredirect = false,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            result = http.GetHtml(item);
            return result;
        }

        private HttpResult Http_Get_301(string url, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                UserAgent= "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36",
                Accept= "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                Method = "GET",
                Cookie = cookie,
                IsToLower = false,
                Allowautoredirect = true,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            result = http.GetHtml(item);
            return result;
        }

        private HttpResult Http_Get_301Mobil(string url, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                Method = "GET",
                IsToLower = false,
                Allowautoredirect = true,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            result = http.GetHtml(item);
            return result;
        }

        private HttpResult Http_Post(string url,string postdata, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                Method = "Post",
                Accept= "application/json, text/plain, */*",
                UserAgent= "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36",
                ContentType= "application/x-www-form-urlencoded",
                Cookie = cookie,
                Postdata= postdata,
                IsToLower = false,
                Allowautoredirect = false,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            result = http.GetHtml(item);
            return result;
        }


        #endregion Http请求

        #region SavaPath

        private string SavePathProgress(string saveFile)
        {
            return saveFile.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace("*", "").Trim().Replace(" ", "");
        }

        #endregion SavaPath

        #region 更新

        private void UpdateApp()
        {
            var result = Update.CheckUpdate();
            if (result.success)
            {
                try
                {
                    var updateInfo =JObject.Parse(result.data.ToString());
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


        #endregion

        #region 信息框
        private void Message(string msg)
        {
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                _windowManager.ShowMessageBox(msg);
            }));
        }
        #endregion

    }
}