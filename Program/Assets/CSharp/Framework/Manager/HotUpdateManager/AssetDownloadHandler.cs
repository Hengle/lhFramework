using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
namespace lhFramework.Infrastructure.Managers
{
    class AssetDownloadHandler : DownloadHandlerScript
    {
        private FileStream m_fileStream;
        private long m_localFileSize = 0;
        private long m_totalFileSize = 0;
        private long m_currentSize = 0;

        public AssetDownloadHandler(string filePath,long fileLoadedSize)
            : base()
        {
            Init(filePath, fileLoadedSize);
        }
        public AssetDownloadHandler( byte[] buff, string filePath, long fileLoadedSize)
            : base(buff)
        {
            Init(filePath, fileLoadedSize);
        }
        public void Release()
        {
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
            }
        }
        public long GetSize()
        {
            return m_totalFileSize+m_localFileSize;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="bundleName">BundleName</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="fileMode">文件读写模式</param>
        private void Init(string filePath,long fileLoadedSize)
        {
            m_fileStream = new FileStream(filePath, FileMode.Append);
            m_localFileSize = fileLoadedSize;
            //UnityEngine.Debug.Log(string.Format("BundleDownloader :: Init {0} - m_localFileSize {1}, fileMode {2}", m_fileStream.Name, m_localFileSize, fileMode));
        }

        /// <summary>
        /// 获取数据(未实现)
        /// </summary>
        /// <returns>null</returns>
        protected override byte[] GetData()
        {
            return null;
        }

        /// <summary>
        /// 获取文本(未实现)
        /// </summary>
        /// <returns>null</returns>
        protected override string GetText()
        {
            return null;
        }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        /// <returns>下载进度</returns>
        protected override float GetProgress()
        {
            if (m_totalFileSize != 0)
            {
                return (m_currentSize + m_localFileSize) / (float)(m_totalFileSize + m_localFileSize);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 根据数据长度设置文件大小
        /// </summary>
        /// <param name="contentLength">数据长度</param>
        protected override void ReceiveContentLength(int contentLength)
        {
            m_totalFileSize = contentLength;
            //UnityEngine.Debug.Log(string.Format("BundleDownloader :: ReceiveContentLength - name {0}, length {1}", m_fileStream.Name, contentLength));
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="dataLength">数据长度</param>
        /// <returns>true成功,false失败</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length < 1)
            {
                UnityEngine.Debug.Log("BundleDownloader :: ReceiveData - received a null/empty buffer");
                return false;
            }
            //UnityEngine.Debug.Log(string.Format("BundleDownloader :: ReceiveData - name {0}, length {1}", m_fileStream.Name, dataLength));
            m_fileStream.Write(data, 0, dataLength);
            m_fileStream.Flush();

            m_currentSize += dataLength;
            return true;
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        protected override void CompleteContent()
        {
            //UnityEngine.Debug.Log(string.Format("BundleDownloader :: CompleteContent {0} - DOWNLOAD COMPLETE!", m_fileStream.Name));
            m_fileStream.Close();
            m_fileStream.Dispose();
        }
    }
}