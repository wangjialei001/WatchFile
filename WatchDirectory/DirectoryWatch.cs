using FileWatchApi.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatch.WatchDirectory
{
    public class DirectoryWatch
    {
        private readonly IConfiguration _configuration;
        private readonly HttpUtils _httpUtils;
        private readonly List<string> watchFiles;
        private readonly FileSystemWatcher watcher;
        public DirectoryWatch(IConfiguration configuration, HttpUtils httpUtils)
        {
            _configuration = configuration;
            _httpUtils = httpUtils;
            var watchFilesStr = _configuration["WatchFiles"];
            if (!string.IsNullOrEmpty(watchFilesStr))
            {
                watchFiles = watchFilesStr.Split(",").ToList();
            }
            watcher = new FileSystemWatcher();
        }
        public void Run()
        {
            var path = _configuration["WatchPath"];
            //FileSystemWatcher watcher = new FileSystemWatcher();
            //using (FileSystemWatcher watcher = new FileSystemWatcher())
            //{
            watcher.Path = path;
            watcher.IncludeSubdirectories = false;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            watcher.Deleted += Watcher_Deleted;
            watcher.Created += Watcher_Created;
            //}
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Watcher_Created...........");
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Watcher_Deleted...........");
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                string fileName = e.Name;
                //Console.WriteLine(fileName+"发生变化！");
                if (watchFiles != null && watchFiles.Count() > 0 && watchFiles.Any(t => fileName.StartsWith(t)))
                {
                    var watchFile = watchFiles.FirstOrDefault(t => fileName.StartsWith(t));
                    UploadQueue uploadQueue = QueueUtil.uploadQueueDic[watchFile];
                    uploadQueue.MsgQueue.Enqueue(new UploadQueueModel { FullPath = e.FullPath, FileName = e.Name, _httpUtils = _httpUtils });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await ReadFile(source, e);
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task ReadFile(object source, FileSystemEventArgs e)
        {
            try
            {
                using (var fileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    fileStream.Close();
                    using (Stream stream = new MemoryStream(bytes))
                    {
                        await _httpUtils.UploadFile(stream, e.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
