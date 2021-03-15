using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatchApi.Utils
{
    public class QueueUtil
    {
        public static Dictionary<string, UploadQueue> uploadQueueDic = new Dictionary<string, UploadQueue>();
    }

    public class UploadQueueModel
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public HttpUtils _httpUtils { get; set; }
    }
    public class UploadQueue
    {
        public readonly Queue<UploadQueueModel> MsgQueue = new Queue<UploadQueueModel>();
        public void Run()
        {
            Thread thread = new Thread(async () => {
                while (true)
                {
                    UploadQueueModel msg;
                    MsgQueue.TryDequeue(out msg);
                    if (msg != null)
                    {
                        try
                        {
                            //Console.WriteLine(msg.FileName + "开始推送！");
                            using (var fileStream = new FileStream(msg.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                byte[] bytes = new byte[fileStream.Length];
                                fileStream.Read(bytes, 0, bytes.Length);
                                fileStream.Close();
                                //using (Stream stream = new MemoryStream(bytes))
                                //{
                                //    await msg._httpUtils.UploadFile(stream, msg.FileName);
                                //}
                                await msg._httpUtils.UploadFile1(bytes, msg.FileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("空队列，休息2秒");
                        await Task.Delay(5000);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }
}
