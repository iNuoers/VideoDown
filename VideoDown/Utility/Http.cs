using CsharpHttpHelper;
using CsharpHttpHelper.Enum;

namespace VideoDown.Utility
{
    public class Http
    {
        #region Http请求

        private readonly HttpHelper http = new HttpHelper();
        private HttpItem item = new HttpItem();

        public HttpResult Http_Get_Un301(string url, string cookie = null)
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
            return http.GetHtml(item);
        }

        public HttpResult Http_Get_301(string url, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                Method = "GET",
                Cookie = cookie,
                IsToLower = false,
                Allowautoredirect = true,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            return http.GetHtml(item);
        }

        public HttpResult Http_Get_301Mobil(string url)
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
            return http.GetHtml(item);
        }

        public HttpResult Http_Post(string url, string postdata, string cookie = null)
        {
            item = new HttpItem()
            {
                URL = url,
                Method = "Post",
                Accept = "application/json, text/plain, */*",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded",
                Cookie = cookie,
                Postdata = postdata,
                IsToLower = false,
                Allowautoredirect = false,
                AutoRedirectCookie = true,
                ResultType = ResultType.String,
            };
            return http.GetHtml(item);
        }

        #endregion Http请求
    }
}