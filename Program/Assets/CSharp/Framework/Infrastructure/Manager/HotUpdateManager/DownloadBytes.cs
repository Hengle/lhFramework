using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace lhFramework.Infrastructure.Managers
{
    public class DownloadBytes
    {
        private class ThreadInfo
        {
            public string url { get; set; }
            public long startPosition { get; set; }
            public long size { get; set; }
            public byte[] bytes { get; set; }
            public bool isDone { get; set; }
        }
        private long m_receiveSize;
        private long m_fileSizeAll;
        private int m_bufferSize = 512;
        private string m_tempPath;
        private int m_threadNum;
        private string m_url;
        private List<ThreadInfo> m_infos = new List<ThreadInfo>();
        /// <summary>
        /// 下载进度
        /// </summary>
        /// <param name="rowIndex">任务索引</param>
        /// <param name="percent">进度</param>
        public delegate void DownloadingHandler(double percent,bool completed,byte[] result);
        public event DownloadingHandler downloadingHandler;
        public DownloadBytes(string url,long fileSize,int threadNum,string tempPath=null)
        {
            m_fileSizeAll = fileSize;
            m_url = url;
            m_threadNum = threadNum;
            m_tempPath = tempPath;
            if (m_fileSizeAll <= 0)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveLength), m_url);
            }
            else
            {
                Download();
            }
        }
        void Download()
        {
            long splitSize = m_fileSizeAll / m_threadNum;
            long remainingSize = m_fileSizeAll % m_threadNum;
            if (m_threadNum>1)
            {
                for (int i = 0; i < m_threadNum; i++)
                {
                    if (i < m_threadNum - 1)
                    {
                        var info = new ThreadInfo()
                        {
                            url = m_url,
                            startPosition = splitSize * i,
                            size = splitSize
                        };
                        m_infos.Add(info);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveHttp), info);
                    }
                    else
                    {
                        var info = new ThreadInfo()
                        {
                            url = m_url,
                            startPosition = m_fileSizeAll - remainingSize,
                            size = remainingSize
                        };
                        m_infos.Add(info);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveHttp), info);
                    }
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(Merge));
            }
            else
            {
                var info = new ThreadInfo()
                {
                    url = m_url,
                    startPosition = 0,
                    size = m_fileSizeAll
                };
                ThreadPool.QueueUserWorkItem(new WaitCallback(SingleReceive),info);
            }
        }
        void ReceiveLength(object o)
        {
            string url = (string)o;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            m_fileSizeAll = request.GetResponse().ContentLength;
            request.Abort();
            Download();
        }
        void ReceiveHttp(object o)
        {
            ThreadInfo info = (ThreadInfo)o;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(info.url);
            request.AddRange(info.startPosition, info.startPosition + info.size-1);
            var res = request.GetResponse();
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    var response= res as HttpWebResponse;
                    var ns = response.GetResponseStream();
                    byte[] bytes = new byte[m_bufferSize];
                    int readSize = 0;
                    while ((readSize = ns.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        stream.Write(bytes, 0, readSize);
                        m_receiveSize += readSize;
                        double percent = (double)m_receiveSize / (double)m_fileSizeAll * 100;
                        downloadingHandler(percent, false, null);//触发下载进度事件
                    }
                    info.bytes = stream.ToArray();
                    info.isDone = true;
                    ns.Close();
                }
                catch(Exception exc)
                {
                    UnityEngine.Debug.LogError("Error ReceiveHttp:"+exc);
                }
                finally
                {
                    res.Close();
                    request.Abort();
                }
            }
        }
        void SingleReceive(object o)
        {
            try
            {
                ThreadInfo info = (ThreadInfo)o;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(info.url);
                var res = request.GetResponse();
                using (MemoryStream stream = new MemoryStream())
                {
                    try
                    {
                        var response = res as HttpWebResponse;
                        var ns = response.GetResponseStream();
                        byte[] bytes = new byte[m_bufferSize];
                        int readSize = 0;
                        while ((readSize = ns.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            stream.Write(bytes, 0, readSize);
                            m_receiveSize += readSize;
                            double percent = (double)m_receiveSize / (double)m_fileSizeAll * 100;
                            downloadingHandler(percent, false, null);//触发下载进度事件
                        }
                        info.bytes = stream.ToArray();
                        info.isDone = true;
                        ns.Close();
                    }
                    catch (Exception exc)
                    {
                        UnityEngine.Debug.LogError("Error ReceiveHttp:" + exc);
                    }
                    finally
                    {
                        byte[] bytes = stream.ToArray();
                        if (!string.IsNullOrEmpty(m_tempPath))
                        {
                            using (FileStream s = new FileStream(m_tempPath, FileMode.Create, FileAccess.Write))
                            {
                                s.Write(bytes, 0, bytes.Length);
                            }
                        }
                        downloadingHandler(100, true, bytes);
                        downloadingHandler = null;
                        res.Close();
                        request.Abort();
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error ReceiveHttp:" + e);
            }
        }
        void Merge(object o)
        {
            while (true)
            {
                int length = 0;
                bool isDone = true;
                for (int i = 0; i < m_infos.Count; i++)
                {
                    if (!m_infos[i].isDone)
                    {
                        isDone = false;
                        break;
                    }
                    length = m_infos[i].bytes.Length;
                }
                if (isDone)
                {
                    byte[] bytes = new byte[length];
                    int nextLength = 0;
                    for (int i = 0; i < m_infos.Count; i++)
                    {
                        var info = m_infos[i];
                        Array.Copy(info.bytes, 0, bytes, nextLength, info.bytes.Length);
                        nextLength += info.bytes.Length;
                    }
                    if (!string.IsNullOrEmpty(m_tempPath))
                    {
                        using (FileStream stream=new FileStream(m_tempPath,FileMode.Create,FileAccess.Write))
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    downloadingHandler(100, true, bytes);
                    downloadingHandler = null;
                    break;
                }
                Thread.Sleep(30);
            }
        }
    }
}
