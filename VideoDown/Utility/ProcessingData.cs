using CsharpHttpHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VideoDown.Model;

namespace VideoDown.Utility
{
    public static class ProcessingData
    {
        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source"> 经过Unicode编码的字符串 </param>
        /// <returns> 正常字符串 </returns>
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source"> 源字符串 </param>
        /// <returns> Unicode编码后的字符串 </returns>
        public static string String2Unicode(string source)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(source);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        public static HttpReturnData JsonToHttpReturnData(HttpResult restResponse)
        {
            try
            {
                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new HttpReturnData { Success = true, Data = restResponse.Html };
                }
                else if (restResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var error = JObject.Parse(restResponse.Html)["Message"][0].ToString();
                        return new HttpReturnData { Success = false, Message = error };
                    }
                    catch (Exception)
                    {
                        return new HttpReturnData { Success = false, Message = "未知错误" };
                    }
                }
                else
                {
                    return new HttpReturnData { Success = false, Message = restResponse.Html };
                }
            }
            catch (Exception)
            {
                return new HttpReturnData { Success = false, Message = "转换失败" };
            }
        }

        public static List<UrlDB> StringToUrlList(string text)
        {
            List<UrlDB> UrlList = new List<UrlDB>();
            Regex reg = new Regex(@"[a-zA-z]+://[^\s]*", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(text);
            foreach (Match m in mc)
            {
                string url = m.Value;

                if (!string.IsNullOrWhiteSpace(url))
                {
                    string typename = "未知";
                    string typecode = "other";

                    if (url.Contains("huoshan.com"))
                    {
                        typename = "火山";
                        typecode = "huoshan";
                    }
                    if (url.Contains("douyin.com"))
                    {
                        typename = "抖音";
                        typecode = "douyin";
                    }
                    if (url.Contains("kuaishou"))
                    {
                        typename = "快手";
                        typecode = "kuaishou";
                    }
                    if (url.Contains("ixigua"))
                    {
                        typename = "西瓜";
                        typecode = "xigua";
                    }
                    if (url.Contains("pipix"))
                    {
                        typename = "皮皮虾";
                        typecode = "pipix";
                    }

                    UrlList.Add(new UrlDB { Url = url.Trim(), TypeCode = typecode, TypeName = typename });
                }
            }
            return UrlList;
        }
    }
}