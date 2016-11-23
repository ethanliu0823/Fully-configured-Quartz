using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace QuartzExtScheduler.Common
{
    public class JsonRequest
    {
        public string Get(string url, Dictionary<string, string> headers = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.KeepAlive = true;

            request.Timeout = 60 * 1000 * 60;
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            return _get(request);
        }


        public string Post<T>(string url, T data, Dictionary<string, string> headers = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 60 * 1000 * 60;
            request.ContentType = "application/json";
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                var json = JsonConvert.SerializeObject(data);
                sw.Write(json);
                sw.Close();
            }


            return _get(request);
        }

        public string Put<T>(string url, T data, ContentType ct)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";// CommonBase.GetDescription(ct);

            var json = JsonConvert.SerializeObject(data);

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(json);
                sw.Close();
            }

            return _get(request);
        }

        public string Delete(string url, ContentType ct, Dictionary<string, string> headers = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            request.ContentType = "application/json";// CommonBase.GetDescription(ct);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            //var json = CommonBase.ToJson(data);

            //using(var sw = new StreamWriter(request.GetRequestStream()))
            //{
            //    sw.Write(json);
            //    sw.Close();
            //}
            return _get(request);
        }

        private static string _get(HttpWebRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var respStream = response.GetResponseStream();
                    if (respStream != null)
                        using (var reader = new StreamReader(respStream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                }
            }
            return string.Empty;
        }
    }
}
