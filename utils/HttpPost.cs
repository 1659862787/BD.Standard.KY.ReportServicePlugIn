using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;

namespace BD.Standard.KY.ReportServicePlugIn
{
    public class HttpPost
    {


        public static K3CloudApiClient client { get; set; }



        /// <summary>
        /// 星空WebApi登录
        /// </summary>
        /// <param name="url">站点地址</param>
        /// <param name="accountid">账套id</param>
        /// <param name="username">用户名</param>
        /// <param name="password">用户密码</param>
        /// <returns>
        /// 返回K3CloudApiClient数据，返回值!=null成功;==null失败
        /// </returns>
        public static K3CloudApiClient WebApiClent(string url, string accountid, string username, string password)
        {
            client = new K3CloudApiClient(url);
            var loginResult = client.ValidateLogin(accountid, username, password, 2052);
            var result = JObject.Parse(loginResult)["LoginResultType"].Value<int>();
            if (result == 1)
            {
                return client;
            }
            return null;
        }
    }
}
