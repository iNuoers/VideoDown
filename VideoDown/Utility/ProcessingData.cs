using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Text;
using System.Text.RegularExpressions;
using VideoDown.Model;

namespace VideoDown.Utility
{
    public class ProcessingData
    {
        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
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

        public static HttpReturnData JsonToHttpReturnData(IRestResponse restResponse)
        {
            try
            {
                if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new HttpReturnData { success = true, data = restResponse.Content };

                }
                else if (restResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var error = JObject.Parse(restResponse.Content)["Message"][0].ToString();
                        return new HttpReturnData { success = false, message = error };


                    }
                    catch (Exception)
                    {

                        return new HttpReturnData { success = false, message = "未知错误" };
                    }
                }
                else
                {
                    return new HttpReturnData { success = false, message = restResponse.Content };

                }

            }
            catch (Exception)
            {
                return new HttpReturnData { success = false, message = "转换失败" };
            }
        }

    }
}
