using CsharpHttpHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VideoDown.Model;

namespace VideoDown.Utility
{
    public class Jiexi
    {
        private readonly Http http = new Http();

        public HttpReturnData GetDownUrl(UrlDB db)
        {
            switch (db.TypeCode)
            {
                case "kuaishou":
                    return Kuaishou(db);

                case "douyin":
                    return Douyin(db);

                case "huoshan":
                    return Huoshan(db);

                case "pipix":
                    return Pipixia(db);

                case "xigua":
                    return Xigua(db);

                default:
                    return new HttpReturnData { Success = false, Message = "匹配失败" };
            }
        }

        #region 快手

        private HttpReturnData Kuaishou(UrlDB db)
        {
            try
            {
                var url = db.Url;
                var html = http.Http_Get_301Mobil(url).Html;
                html = html.Replace(" ", "").Replace("\r", "").Replace("\n", "");
                var json = HttpHelper.GetBetweenHtml(html, "type=\"text/javascript\">window.pageData=", "}}</script>") + "}}";
                var jo = JObject.Parse(json);
                var videoUrl = jo["video"]["srcNoMark"];
                var title = jo["video"]["caption"];
                var id = jo["video"]["id"];
                //var artist = jo["user"]["name"];

                //5192650406202789169
                if (videoUrl == null)
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }

                var b = PathHelper.Save("kuaishou", title.ToString(), id.ToString());
                if (b.Success)
                {
                    b.Data = new UrlDB
                    {
                        Url = db.Url,
                        TypeCode = db.TypeCode,
                        TypeName = db.TypeName,
                        SavePath = b.Message,
                        FileName = System.IO.Path.GetFileNameWithoutExtension(b.Message),
                        ID = id.ToString(),
                        Explain = title.ToString(),
                        DownUrl = videoUrl.ToString(),
                    };
                }
                return b;
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "解析出错" };
            }
        }

        #endregion 快手

        #region 西瓜

        private HttpReturnData Xigua(UrlDB db)
        {
            var url = db.Url;
            try
            {
                var xiguaCookie = GetXiguaCookie(url);
                var result = http.Http_Get_301(url, xiguaCookie);
                var html = result.Html;
                if (html.Contains("404页"))
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
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
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }

                html = html.Replace(" ", "").Replace("\r", "").Replace("\n", "");
                var json = HttpHelper.GetBetweenHtml(html, "window._SSR_HYDRATED_DATA=", "}</script>") + "}";
                var jo = JObject.Parse(json);
                var videojson = jo["anyVideo"]["gidInformation"]["packerData"]["video"];
                if (videojson == null)
                {
                    videojson = jo["anyVideo"]["gidInformation"]["packerData"];
                }
                var videoUrlListJson = videojson["videoResource"]["dash_120fps"]["video_list"];
                if (videoUrlListJson == null)
                {
                    videoUrlListJson = videojson["videoResource"]["dash_120fps"]["dynamic_video"];
                    if (videoUrlListJson != null)
                    {
                        var video = videoUrlListJson["dynamic_video_list"];
                        var audio = videoUrlListJson["dynamic_audio_list"];
                        if (audio != null && video != null)
                        {
                            var video1 = JArray.Parse(video.ToString()).Last()["main_url"];
                            var audio1 = JArray.Parse(audio.ToString()).Last()["main_url"];
                            var video1String = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(video1.ToString(), x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
                            video1String = HttpHelper.Base64ToString(video1String, Encoding.UTF8);
                            var audio1String = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(audio1.ToString(), x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
                            audio1String = HttpHelper.Base64ToString(audio1String, Encoding.UTF8);
                            var title1 = videojson["title"] ?? jo["anyVideo"]["gidInformation"]["packerData"]["albumInfo"]["title"];

                            var b1 = PathHelper.Save("xigua", title1.ToString(), id.ToString());
                            if (b1.Success)
                            {
                                var b2 = PathHelper.Save1("xigua", title1.ToString(), id.ToString());
                                b2.Data = new UrlDB
                                {
                                    Url = db.Url,
                                    TypeCode = db.TypeCode,
                                    TypeName = db.TypeName,
                                    SavePath = b1.Message,
                                    FileName = System.IO.Path.GetFileNameWithoutExtension(b2.Message),
                                    ID = id.ToString(),
                                    Explain = title1.ToString(),
                                    DownUrl = video1String.ToString(),
                                    DownUrl1 = audio1String.ToString(),
                                };
                                return b2;
                            }
                        }
                    }
                }
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
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }

                var title = videojson["title"] ?? jo["anyVideo"]["gidInformation"]["packerData"]["albumInfo"]["title"];

                if (videoUrl == null)
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
                var videoUrlString = new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(videoUrl.ToString(), x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
                videoUrlString = HttpHelper.Base64ToString(videoUrlString, Encoding.UTF8);

                var b = PathHelper.Save("xigua", title.ToString(), id.ToString());
                if (b.Success)
                {
                    b.Data = new UrlDB
                    {
                        Url = db.Url,
                        TypeCode = db.TypeCode,
                        TypeName = db.TypeName,
                        SavePath = b.Message,
                        FileName = System.IO.Path.GetFileNameWithoutExtension(b.Message),
                        ID = id.ToString(),
                        Explain = title.ToString(),
                        DownUrl = videoUrlString.ToString(),
                    };
                }
                return b;
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "解析出错" };
            }
        }

        private string GetXiguaCookie(string url)
        {
            var result = http.Http_Get_301(url);
            //https://i.snssdk.com/slardar/sdk_setting?bid=xigua_video_web_pc
            var setCookie = result.Header["Set-Cookie"];
            var __ac_nonce = HttpHelper.GetBetweenHtml(setCookie, "__ac_nonce=", ";");
            result = http.Http_Get_Un301("https://i.snssdk.com/slardar/sdk_setting?bid=xigua_video_web_pc");
            //https://i.snssdk.com/slardar/sdk_setting?bid=xigua_video_web_pc
            setCookie = result.Header["Set-Cookie"];
            var MONITOR_WEB_ID = HttpHelper.GetBetweenHtml(setCookie, "MONITOR_WEB_ID=", ";");
            string post = "{\"region\":\"cn\",\"aid\":1768,\"needFid\":false,\"service\":\"www.ixigua.com\",\"migrate_info\":{\"ticket\":\"\",\"source\":\"node\"},\"cbUrlProtocol\":\"https\",\"union\":true}";

            result = http.Http_Post("https://ttwid.bytedance.com/ttwid/union/register/", post);
            setCookie = result.Header["Set-Cookie"];
            var ttwid = HttpHelper.GetBetweenHtml(setCookie, "ttwid=", ";");

            return $"MONITOR_WEB_ID={MONITOR_WEB_ID};ttwid={ttwid};__ac_nonce={__ac_nonce}";
        }

        #endregion 西瓜

        #region 抖音

        private HttpReturnData Douyin(UrlDB db)
        {
            try
            {
                var url = db.Url;
                url = http.Http_Get_301(db.Url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "https://www.douyin.com/video/", "previous_page").Replace("?", "");

                if (string.IsNullOrEmpty(urlID))
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
                var jsonUrl = $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={urlID}&dytk=";
                var result = http.Http_Get_301(jsonUrl, null);
                var cookie = result.Cookie;
                var json = JObject.Parse(result.Html);
                var url_list = json["item_list"][0]["video"]["play_addr"]["url_list"][0];
                var titleJson = json["item_list"][0]["desc"];//desc
                var title = DateTime.Now.ToString("yyyyMMddhhmmss");
                if (titleJson != null)
                {
                    title = titleJson.ToString().Replace("\n", "").Replace("\r", "").Trim();
                }
                if (url_list != null)
                {
                    var videoUrl = url_list.ToString().Replace("playwm", "play");
                    var b = PathHelper.Save("douyin", title.ToString(), urlID);
                    if (b.Success)
                    {
                        b.Data = new UrlDB
                        {
                            Url = db.Url,
                            TypeCode = db.TypeCode,
                            TypeName = db.TypeName,
                            SavePath = b.Message,
                            FileName = System.IO.Path.GetFileNameWithoutExtension(b.Message),
                            ID = urlID,
                            Explain = title.ToString(),
                            DownUrl = videoUrl.ToString(),
                        };
                    }
                    return b;
                }
                else
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "解析出错" };
            }
        }

        #endregion 抖音

        #region 皮皮虾

        private HttpReturnData Pipixia(UrlDB db)
        {
            var url = db.Url;
            try
            {
                url = http.Http_Get_301(url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "https://h5.pipix.com/item/", "app_id=").Replace("?", "");

                if (string.IsNullOrEmpty(urlID))
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
                var jsonUrl = $"https://is.snssdk.com/bds/item/detail/?app_name=super&aid=1319&item_id={urlID}";
                var result = http.Http_Get_301(jsonUrl, null);
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
                    var b = PathHelper.Save("pipix", title.ToString(), urlID);
                    if (b.Success)
                    {
                        b.Data = new UrlDB
                        {
                            Url = db.Url,
                            TypeCode = db.TypeCode,
                            TypeName = db.TypeName,
                            SavePath = b.Message,
                            FileName = System.IO.Path.GetFileNameWithoutExtension(b.Message),
                            ID = urlID,
                            Explain = title.ToString(),
                            DownUrl = videoUrl.ToString(),
                        };
                    }
                    return b;
                }
                else
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "解析出错" };
            }
        }

        #endregion 皮皮虾

        #region 火山

        private HttpReturnData Huoshan(UrlDB db)
        {
            var url = db.Url;

            try
            {
                var result = http.Http_Get_301(url);
                url = http.Http_Get_301(url).ResponseUri;
                string urlID = HttpHelper.GetBetweenHtml(url, "item_id=", "&");
                if (string.IsNullOrEmpty(urlID))
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
                var jsonUrl = $"https://www.iesdouyin.com/web/api/v2/aweme/iteminfo/?item_ids={urlID}&dytk=";
                var json = JObject.Parse(http.Http_Get_301(jsonUrl, null).Html);
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
                    var b = PathHelper.Save("huoshan", title.ToString(), urlID);
                    if (b.Success)
                    {
                        b.Data = new UrlDB
                        {
                            Url = db.Url,
                            TypeCode = db.TypeCode,
                            TypeName = db.TypeName,
                            SavePath = b.Message,
                            FileName = System.IO.Path.GetFileNameWithoutExtension(b.Message),
                            ID = urlID,
                            Explain = title.ToString(),
                            DownUrl = videoUrl.ToString(),
                        };
                    }
                    return b;
                }
                else
                {
                    return new HttpReturnData { Success = false, Message = "解析失败" };
                }
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "解析出错" };
            }
        }

        #endregion 火山
    }
}