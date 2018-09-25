using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace lhFramework.Infrastructure.Managers
{
    public class TcpManager:Singleton<TcpManager>
    {
        private TcpConnection m_tcpConnection;
        public static void Connect(IPEndPoint point)
        {
            IPackage package = new ProtobufPackage();
            p_instance.m_tcpConnection = new TcpConnection(package);
            p_instance.m_tcpConnection.OnSend += p_instance.OnSend;
            p_instance.m_tcpConnection.OnReceive += p_instance.OnReceive;
            p_instance.m_tcpConnection.OnConnected += OnConnected;
            p_instance.m_tcpConnection.OnConnecting += OnConnecting;
            p_instance.m_tcpConnection.OnDisconnected += OnDisconnected;
            p_instance.m_tcpConnection.Connect(point);
            
        }
        public static void Send(byte[] bytes)
        {
            p_instance.m_tcpConnection.Send(bytes);
        }
        private static void OnDisconnected()
        {

        }
        private static void OnConnecting()
        {

        }
        private static void OnConnected()
        {

        }
        private void OnReceive(byte[] obj)
        {

        }
        private void OnSend(byte[] obj)
        {

        }
    }
}
