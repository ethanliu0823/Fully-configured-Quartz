using QuartzExtScheduler.Utility.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace QuartzExtScheduler.Utility
{
    public class WeixinService
    {
        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="openId">微信openid</param>
        /// <param name="templateId">模板id</param>
        /// <param name="textUrl">模板消息url</param>
        /// <param name="titleColor">标题文本颜色[为空默认未黑色]</param>
        /// <param name="dictionaryData">消息体</param>
        /// <param name="accessToken">accessToken</param>
        /// <returns></returns>
        public static string SendMessage(string openId, string templateId, string textUrl, string titleColor, Dictionary<string, object> dictionaryData, string accessToken)
        {
            string urlString = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", accessToken);
            var jsonObj =
                new
                {
                    touser = openId,
                    template_id = templateId,
                    url = textUrl,
                    topcolor = titleColor,
                    data = dictionaryData
                };
            return HttpUtil.PostHtml(urlString, JsonConvert.SerializeObject(jsonObj));
        }

        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="openId">微信openid</param>
        /// <param name="templateId">模板id</param>
        /// <param name="textUrl">模板消息url</param>
        /// <param name="titleColor">标题文本颜色[为空默认未黑色]</param>
        /// <param name="dictionaryData">消息体</param>
        /// <returns></returns>
        public static string SendMessage(string openId, string templateId, string textUrl, string titleColor, Dictionary<string, object> dictionaryData)
        {
            //微信被封后封闭入口，梳理好后再开放
            return "{推送端口暂时关闭，梳理清楚后再开放。}";

            var accessToken = WeixinToken.GetAccessToken();
            string urlString = string.Format("https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={0}", accessToken);
            var jsonObj =
                new
                {
                    touser = openId,
                    template_id = templateId,
                    url = textUrl,
                    topcolor = titleColor,
                    data = dictionaryData
                };
            return HttpUtil.PostHtml(urlString, JsonConvert.SerializeObject(jsonObj)) + "&access_token:" + accessToken;
        }

        /// <summary>
        /// 发送任务处理通知给管理员
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="textUrl"></param>
        /// <param name="accessToken"></param>
        /// <param name="first"></param>
        /// <param name="keyword1"></param>
        /// <param name="keyword2"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static string SendMessage2Admin(string openId, string templateId, string textUrl, string accessToken, string first, string keyword1, string keyword2, string remark)
        {
            Dictionary<string, object> dictionaryData = new Dictionary<string, object>();
            //任务处理通知
            var dicData = new Dictionary<string, object>();
            dicData.Add("first", new { value = first, color = "#ff0000" });
            dicData.Add("keyword1", new { value = keyword1, color = "#000000" });
            dicData.Add("keyword2", new { value = keyword2, color = "#000000" });
            dicData.Add("remark", new { value = remark, color = "#173177" });
            return SendMessage(openId, templateId, textUrl, "#000000", dicData, accessToken);
        }

        public static string SendMessage2Admin(string openId, string templateId, string accessToken, string first, string keyword1, string keyword2, string remark)
        {
            return SendMessage2Admin(openId, templateId, String.Empty, accessToken, first, keyword1, keyword2, remark);
        }
    }

    public class WeixinToken
    {
        #region 获取每次操作微信API的Token访问令牌
        private static string GetToken(string sToken, string appid, string secret)
        {
            var ourUrl = ConfigurationManager.AppSettings["WeChatTokenRequestURL"];
            return MemoryCacheHelper.GetCacheItem<string>("access_token" + sToken, delegate()
            {
                var token = String.Empty;
                string grant_type = "client_credential";
                var url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type={0}&appid={1}&secret={2}",
                                        grant_type, appid, secret);

                var result = HttpUtil.GetHtml(ourUrl);
                string ourRegex = "\"success\"\\s*?:\\s*?(?<success>.*?),.*?\"data\"\\s*?:\\s*?\"(?<token>.*?)\"";
                var success = Regex.Match(result, ourRegex).Groups["success"].Value;
                var isSucc = false;
                Boolean.TryParse(success, out isSucc);
                if (isSucc)
                {
                    token = Regex.Match(result, ourRegex).Groups["token"].Value;
                }
                else
                {
                    var myEx1 = MyException.GetMyExceptionWrapper(String.Format("获取access_token失败：【{0}】", result));
                    MyException.WriteLog(myEx1);

                    result = HttpUtil.GetHtml(url);
                    /*//如果返回内容不成功则一直获取，直到取得access_token
                    if (!result.Contains("access_token"))
                    {
                        //移除重新获取
                        MemoryCacheHelper.RemoveCacheItem("access_token");
                        LogHelper.Info("access_token获取失败："+result);
                        result = HttpHelper.GetHtml(url);
                    }*/
                    if (!result.Contains("access_token"))
                    {
                        var myEx = MyException.GetMyExceptionWrapper(String.Format("获取access_token失败：【{0}】", result));
                        MyException.WriteLog(myEx);
                    }
                    string regex = "\"access_token\":\"(?<token>.*?)\"";
                    token = Regex.Match(result, regex).Groups["token"].Value;
                }
                return token;
            }, null, DateTime.Now.AddSeconds(6000)

                 );
        }


        /// <summary>
        /// 获取每次操作微信API的Token访问令牌 (程序内部调用)
        /// </summary>
        /// <param name="appid">应用ID</param>
        /// <param name="secret">开发者凭据</param>
        /// <returns></returns>
        public static string GetAccessToken()
        {
            string appid = ConfigurationManager.AppSettings["AppId"].Trim();
            string secret = ConfigurationManager.AppSettings["AppSecret"].Trim();

            return GetAccessToken(appid, appid, secret);
        }

        /// <summary>
        /// 获取每次操作微信API的Token访问令牌（微信接口调用，防止修改token）
        /// </summary>
        /// <param name="appid">应用ID</param>
        /// <param name="secret">开发者凭据</param>
        /// <returns></returns>
        public static string GetAccessToken(string sToken, string appid, string secret)
        {
            //正常情况下access_token有效期为7200秒,这里使用缓存设置短于这个时间即可
            string access_token = GetToken(sToken, appid, secret);

            //验证Token是否过期
            if (TokenExpired(access_token))//参考：http://www.cnblogs.com/s0611163/p/4098270.html
            {
                //移除重新获取
                MemoryCacheHelper.RemoveCacheItem("access_token" + sToken);
                access_token = GetToken(sToken, appid, secret);
            }
            return access_token;
        }

        #endregion

        #region 验证Token是否过期
        /// <summary>
        /// 验证Token是否过期
        /// </summary>
        public static bool TokenExpired(string access_token)
        {
            string jsonStr = HttpUtil.GetHtml(string.Format("https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}", access_token));
            if (jsonStr.Contains("42001") && jsonStr.Contains("errcode"))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 生成随机字符串
        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <returns></returns>
        private static string GetStringCode()
        {
            int number; char code;
            string checkCode = String.Empty;
            Random random = new Random();
            for (int i = 0; i < 16; i++)
            {
                number = random.Next();
                if (number % 2 == 0)
                    code = (char)('0' + (char)(number % 10));
                else
                    code = (char)('A' + (char)(number % 26));
                checkCode += code.ToString();
            }
            return checkCode;
        }
        #endregion
    }

    /// <summary>
    /// 参数模型
    /// </summary>
    public class ParamEntity
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 显示颜色
        /// </summary>
        public string color { get; set; }
    }
}
