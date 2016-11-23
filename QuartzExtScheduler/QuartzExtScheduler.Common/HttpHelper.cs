/// <summary>  
/// 类说明：HttpHelper类，用来实现Http访问，Post或者Get方式的，直接访问，带Cookie的，带证书的等方式，可以设置代理  
/// </summary>  
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace QuartzExtScheduler.Common
{
        /// <summary>  
     /// Http连接操作帮助类  
     /// </summary>  
     public class HttpHelper  
     {
          /// <summary>  
         /// Post 提交调用抓取  
         /// </summary>  
         /// <param name="url">提交地址</param>  
         /// <param name="param">参数</param>  
         /// <returns>string</returns>  
         public static string GetHtml(string url)
         {
            return  handleRequest("Get",url, string.Empty);
         }
         /// <summary>  
         /// Post 提交调用抓取  
         /// </summary>  
         /// <param name="url">提交地址</param>  
         /// <param name="param">参数</param>  
         /// <returns>string</returns>  
         public static string GetHtml(string url, string param)
         {
             return handleRequest("Post", url, param);
         }

         /// <summary>
         /// 下载网络图片
         /// </summary>
         /// <param name="Url">图片url</param>
         public static void DownloadImage(string Url,string filename) {
             try
             {
                 using (System.Net.WebClient wc = new System.Net.WebClient())
                 {
                     string path = string.Format("{0}images\\news\\{1}", System.Web.HttpContext.Current.Request.PhysicalApplicationPath,filename);
                     wc.Headers.Add("User-Agent", "Chrome");
                     wc.DownloadFile(Url, path);//保存到本地的文件名和路径，请自行更改
                 }
             }
             catch (Exception ex)
             {

             }
         }

         /// <summary>  
         /// Post 提交调用抓取  
         /// </summary>  
         /// <param name="url">提交地址</param>  
         /// <param name="param">参数</param>  
         /// <returns>string</returns>  
         private static string handleRequest(string mothod, string url, string param)
         {             
             HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
             req.Method = mothod;
             req.Timeout = 120 * 1000;
             req.ContentType = "application/x-www-form-urlencoded;";

             if (mothod == "Post")
             {
                 byte[] bs = System.Text.Encoding.UTF8.GetBytes(param);
                 req.ContentLength = bs.Length;

                 using (Stream reqStream = req.GetRequestStream())
                 {
                     reqStream.Write(bs, 0, bs.Length);
                     reqStream.Flush();
                 }
             }

             try
             {
                 using (WebResponse wr = req.GetResponse())
                 {
                     //在这里对接收到的页面内容进行处理  
                     Stream strm = wr.GetResponseStream();
                     StreamReader sr = new StreamReader(strm, System.Text.Encoding.UTF8);

                     string line;
                     System.Text.StringBuilder sb = new System.Text.StringBuilder();
                     while ((line = sr.ReadLine()) != null)
                     {
                         sb.Append(line + System.Environment.NewLine);
                     }
                     sr.Close();
                     //strm.Close();
                     return sb.ToString();
                 }
             }
             catch (Exception ex)
             {
                 //_log.Error("请求" + url + "时出错:" + ex.Message);
                 return string.Empty;
             }
         }  
     }     
 }  

