using System;
using System.Net.Http;
using System.Text;

namespace GeekDesk.Util
{
    public class HttpUtil
    {
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        #region Get请求
        public static string Get(string url)
        {
            try
            {
                var response = HttpClient.GetAsync(url).GetAwaiter().GetResult();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "HttpUtil Get 失败: " + url);
                return "";
            }
        }
        #endregion
    }
}
