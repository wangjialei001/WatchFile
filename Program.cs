using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileWatch.WatchDirectory;
using FileWatchApi.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileWatchApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            try
            {
                var configuration = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;
                #region ╤сап
                var watchFilesStr = configuration["WatchFiles"];
                if (!string.IsNullOrEmpty(watchFilesStr))
                {
                    var watchFiles = watchFilesStr.Split(",").ToList();
                    foreach(var watchFile in watchFiles)
                    {
                        var uploadQueue = new UploadQueue();
                        QueueUtil.uploadQueueDic.Add(watchFile, uploadQueue);
                        uploadQueue.Run();
                    }
                }
                #endregion
                var isWatch = configuration["IsWatch"];
                if (!string.IsNullOrEmpty(isWatch) && isWatch == "1")
                {
                    var watch = host.Services.GetService(typeof(DirectoryWatch)) as DirectoryWatch;
                    Thread thread1 = new Thread(() =>
                    {
                        watch.Run();
                    });
                    thread1.Start();
                    //var watch = host.Services.GetService(typeof(DirectoryWatch)) as DirectoryWatch;
                    //watch.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(options =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                    });
                });
    }
}