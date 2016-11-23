using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Common.Logging;

namespace QuartzExtScheduler.Utility.Utility
{
    /// <summary>
    /// HTTP请求工具类
    /// </summary>
    public class HttpUtil
    {
        // 定义浏览器
        public static string BrowserAccept;
        public static string BrowserUserAgent;
        public static Dictionary<string, string> BrowserHeaders;
        public static CookieContainer BrowserCookie;
        private static ILog _log = LogManager.GetCurrentClassLogger();

        static HttpUtil()
        {
            BrowserAccept = "*/*";
            BrowserUserAgent = "Mozilla/5.0 (Windows NT 6.1)";
            BrowserHeaders = new Dictionary<string, string> { { "Accept-Language", "zh-cn" }, { "Accept-Encoding", "gzip, deflate" } };
            BrowserCookie = new CookieContainer();
        }

        public static string GetHtml(string url)
        {
            return GetHtml(url, Encoding.UTF8);
        }

        public static string GetHtml(string url, string referer)
        {
            return GetHtml(url, Encoding.UTF8, referer);
        }

        public static string GetHtml(string url, Encoding encoding)
        {
            return GetHtml(url, encoding, "");
        }

        public static string GetHtml(string url, Encoding encoding, string referer)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);

            //头信息
            req.Accept = BrowserAccept;
            foreach (var kvp in BrowserHeaders)
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }
            req.UserAgent = BrowserUserAgent;

            req.CookieContainer = BrowserCookie;

            req.Referer = referer;

            string strReturn = "";

            try
            {
                //接收返回参数
                var res = (HttpWebResponse)req.GetResponse();

                StreamReader sr;
                switch (res.ContentEncoding)
                {
                    case "gzip":
                        var gzs = new GZipStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(gzs, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();
                        break;

                    case "deflate":
                        var ds = new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(ds, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();
                        break;

                    default:
                        sr = new StreamReader(res.GetResponseStream(), encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                _log.ErrorFormat("HTTP GET请求错误(url:{0})，原因：{1}", url, ex.Message);
            }

            return strReturn;
        }

        public static string PostHtml(String url, String data, Dictionary<string, string> headers)
        {
            return PostHtml(url, data, headers, "");
        }

        public static string PostHtml(String url, String data, Dictionary<string, string> headers, string referer)
        {
            var encoding = Encoding.UTF8;

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";

            //头信息
            req.Accept = BrowserAccept;
            req.Referer = referer;
            req.ContentType = "application/x-www-form-urlencoded";
            foreach (KeyValuePair<string, string> kvp in BrowserHeaders)
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }
            foreach (var header in headers)
            {
                req.Headers.Add(header.Key, header.Value);
            }
            req.UserAgent = BrowserUserAgent;

            req.ServicePoint.Expect100Continue = false;

            req.CookieContainer = BrowserCookie;

            byte[] bytes = encoding.GetBytes(data);

            req.ContentLength = bytes.Length;

            string strReturn = "";

            try
            {
                Stream requestStream = req.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                //接收返回参数
                var res = (HttpWebResponse)req.GetResponse();

                StreamReader sr;
                switch (res.ContentEncoding)
                {
                    case "gzip":
                        var gzs = new GZipStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(gzs, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                    case "deflate":
                        var ds = new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(ds, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                    default:
                        sr = new StreamReader(res.GetResponseStream(), encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                }
            }
            catch (System.Exception ex)
            {
                _log.ErrorFormat("HTTP POST请求错误\nurl:{0}\ndata:{1})，原因：{2}", url, data, ex.Message);
            }

            return strReturn;
        }

        public static string PostHtml(String url, String data)
        {
            return PostHtml(url, data, Encoding.UTF8);
        }

        public static string PostHtml(String url, String data, String referer)
        {
            return PostHtml(url, data, Encoding.UTF8, referer);
        }

        public static string PostHtml(String url, String data, Encoding encoding)
        {
            return PostHtml(url, data, encoding, "");
        }

        public static string PostHtml(String url, String data, Encoding encoding, String referer)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";

            //头信息
            req.Accept = BrowserAccept;
            req.Referer = referer;
            req.ContentType = "application/x-www-form-urlencoded";
            foreach (KeyValuePair<string, string> kvp in BrowserHeaders)
            {
                req.Headers.Add(kvp.Key, kvp.Value);
            }
            req.UserAgent = BrowserUserAgent;

            req.ServicePoint.Expect100Continue = false;

            req.CookieContainer = BrowserCookie;

            byte[] bytes = encoding.GetBytes(data);

            req.ContentLength = bytes.Length;

            string strReturn = "";

            try
            {
                Stream requestStream = req.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                //接收返回参数
                var res = (HttpWebResponse)req.GetResponse();

                StreamReader sr;
                switch (res.ContentEncoding)
                {
                    case "gzip":
                        var gzs = new GZipStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(gzs, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                    case "deflate":
                        var ds = new DeflateStream(res.GetResponseStream(), CompressionMode.Decompress);

                        sr = new StreamReader(ds, encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                    default:
                        sr = new StreamReader(res.GetResponseStream(), encoding);
                        strReturn = sr.ReadToEnd();
                        sr.Close();

                        res.Close();

                        break;

                }
            }
            catch (System.Exception ex)
            {
                _log.ErrorFormat("HTTP POST请求错误\nurl:{0}\ndata:{1})，原因：{2}", url, data, ex.Message);
            }

            return strReturn;
        }

    }
}
