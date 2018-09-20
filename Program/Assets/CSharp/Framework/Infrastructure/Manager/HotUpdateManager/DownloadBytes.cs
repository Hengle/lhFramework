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
        private int m_infoCompleteCount;
        private List<ThreadInfo> m_infos = new List<ThreadInfo>();
        /// <summary>
        /// 下载进度
        /// </summary>
        /// <param name="rowIndex">任务索引</param>
        /// <param name="percent">进度</param>
        public delegate void DownloadingHandler(double percent,long receivedSize,bool completed,byte[] result);
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
            if (m_threadNum>1)
            { 
                long splitSize = m_fileSizeAll / m_threadNum;
                long remainingSize = m_fileSizeAll % m_threadNum;
                for (int i = 0; i < m_threadNum; i++)
                {
                    var info = new ThreadInfo()
                    {
                        url = m_url,
                        startPosition = splitSize * i,
                        size = splitSize
                    };
                    m_infoCompleteCount++;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveHttp), info);
                }
                if (remainingSize>0)
                {
                    var info = new ThreadInfo()
                    {
                        url = m_url,
                        startPosition = m_fileSizeAll - remainingSize,
                        size = remainingSize
                    };
                    m_infoCompleteCount++;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveHttp), info);
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
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(url);
                using (var res = request.GetResponse())
                {
                    m_fileSizeAll = res.ContentLength;
                    res.Close();
                }
                request.Abort();
                //m_fileSizeAll = 100;
            }
            catch(Exception exc)
            {
                UnityEngine.Debug.LogError("Error ReceiveHttp:" + exc+"   "+ url);
            }
            Download();
        }
        public void ReceiveHttp(object o)
        {
            try
            {
                ThreadInfo info = (ThreadInfo)o;
                HttpWebRequest request = WebRequest.CreateHttp(info.url);
                try
                {
                    request.AddRange(info.startPosition, info.startPosition + info.size - 1);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e + "    " + info.startPosition + "   " + info.size + "   " + info.url);
                }
                using (var res = request.GetResponse())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        try
                        {
                            var response = res as HttpWebResponse;
                            var ns = response.GetResponseStream();
                            byte[] bytes = new byte[512];
                            int readSize = 0;
                            while ((readSize = ns.Read(bytes, 0, bytes.Length)) > 0)
                            {
                                stream.Write(bytes, 0, readSize);
                            }
                            info.bytes = stream.ToArray();
                            info.isDone = true;
                            ns.Close();
                        }
                        catch (Exception exc)
                        {
                            UnityEngine.Debug.LogError("Error ReceiveHttp:" + exc + "   " + info.url + "   " + info.startPosition);
                        }
                        finally
                        {
                            res.Close();
                            request.Abort();
                            lock (m_infos)
                            {
                                m_infos.Add(info);
                            }
                            m_infoCompleteCount--;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error ReceiveHttp:" + e);
            }
        }
        void SingleReceive(object o)
        {
            try
            {
                ThreadInfo info = (ThreadInfo)o;
                HttpWebRequest request = WebRequest.CreateHttp(info.url);
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
                            downloadingHandler(percent, readSize, false, null);//触发下载进度事件
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
                            FileInfo temFileInfo = new FileInfo(m_tempPath);
                            if (!temFileInfo.Directory.Exists)
                            {
                                temFileInfo.Directory.Create();
                            }
                            using (FileStream s = new FileStream(m_tempPath, FileMode.Create, FileAccess.Write))
                            {
                                s.Write(bytes, 0, bytes.Length);
                            }
                        }
                        downloadingHandler(100, 0, true, bytes);
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
            if (!string.IsNullOrEmpty(m_tempPath))
            {
                FileInfo fileInfo = new FileInfo(m_tempPath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
            }
            while (true)
            {
                try
                { 
                    if (m_infoCompleteCount <= 0)
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
                            length += m_infos[i].bytes.Length;
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
                                using (FileStream stream = new FileStream(m_tempPath, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    stream.Write(bytes, 0, bytes.Length);
                                }
                            }
                            downloadingHandler(100, m_receiveSize, true, bytes);
                            downloadingHandler = null;
                            break;
                        }
                    }
                }
                catch(Exception exc)
                {
                    UnityEngine.Debug.LogError(exc);
                }
                Thread.Sleep(30);
            }
        }
    }
}
