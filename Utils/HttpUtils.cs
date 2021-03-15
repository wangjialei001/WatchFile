using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FileWatchApi.Utils
{
    public class HttpUtils
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        public HttpUtils(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }
        public async Task UploadFile(Stream file, string fileName)
        {
            var host = _configuration["UploadHost"];
            string url = "http://" + host + "/api/Upload/PostFile";

            // 设置参数 
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            CookieContainer cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线 
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            //请求头部信息  
            StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());

            Stream postStream = request.GetRequestStream();
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            using (MemoryStream streamFile = new MemoryStream())
            {
                await file.CopyToAsync(streamFile);
                streamFile.Seek(0, SeekOrigin.Begin);
                byte[] bytes = new byte[streamFile.Length];
                streamFile.Read(bytes, 0, bytes.Length);
                postStream.Write(bytes, 0, bytes.Length);
            }
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();

            //发送请求并获取相应回应数据 
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求 
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            //返回结果网页（html）代码 
            string content = sr.ReadToEnd();
        }
        public async Task UploadFile1(byte[] bytes, string fileName)
        {
            var host = _configuration["UploadHost"];
            string url = "http://" + host + "/api/Upload/PostFile";

            var client = _clientFactory.CreateClient();
            using (var content = new MultipartFormDataContent())
            {
                var byteArrayContent = new ByteArrayContent(bytes);

                content.Add(byteArrayContent, "file", fileName);

                var result = await client.PostAsync(url, content);
            }
        }
    }
}
