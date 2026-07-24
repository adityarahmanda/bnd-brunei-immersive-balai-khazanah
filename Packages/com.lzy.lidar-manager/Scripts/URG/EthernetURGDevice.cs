/*!
 * \file
 * \brief Get distance data from Ethernet type URG
 * \author Jun Fujimoto
 * $Id: get_distance_ethernet.cs 403 2013-07-11 05:24:12Z fujimoto $
 */
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using UnityEngine.Events;
using LZY.PimDeWitte.UnityMainThreadDispatcher;

namespace LZY.Lidar
{
    public class EthernetURGDevice : URGDevice, ILidarDevice
    {
        public const string DEFAULT_IP_ADDRESS = "192.168.0.10";
        public const int DEFAULT_PORT = 10940;
        
        private Thread m_tcpClientThread;
        private TcpClient m_tcpClient;

        public List<long> distances
        {
            get => m_distances;
            set => m_distances = value;
        }
        
        private List<long> m_distances;
        
        public List<long> strengths 
        {
            get => m_strengths;
            set => m_strengths = value;
        }

        public float minDegreeScope => 45f;
        public float maxDegreeScope => 315f;
        
        private List<long> m_strengths;

        public bool isConnected
        {
            get => m_isConnected;
            set
            {
                if (m_isConnected == value) return;
                
                m_isConnected = value;
                UnityMainThreadDispatcher.Instance().Enqueue(() => m_onConnectionChanged?.Invoke(m_isConnected));
            }
        }
        private bool m_isConnected;
        private bool m_isConnecting;
        private bool m_isDisconnecting;

        public UnityEvent<bool> onConnectionChanged => m_onConnectionChanged;
        [SerializeField] private UnityEvent<bool> m_onConnectionChanged;

        public void Connect(string ip = DEFAULT_IP_ADDRESS, int port = DEFAULT_PORT)
        {
            if (isConnected) return;
            if (m_isConnecting) return;

            m_isConnecting = true;
            m_distances = new List<long>();
            m_strengths = new List<long>();

            try
            {
                m_tcpClient = new TcpClient();

                if (!StartTCPConnect(m_tcpClient, ip, port))
                {
                    Debug.LogError("[Hokuyo] Failed to connect to Hokuyo TCP server.");
                    m_isConnecting = false;
                    isConnected = false;
                    return;
                }

                Debug.Log("[Hokuyo] Connected to Hokuyo TCP Server with IP Address: " + ip + " and Port number: " + port.ToString());
                m_tcpClientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                m_tcpClientThread.Start(m_tcpClient);

                // start measure distance
                Write(SCIP_Writer.MD(0, 1080, 1, 0, 0));

                isConnected = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[Hokuyo] " + ex.Message);
            }
            finally
            {
                m_isConnecting = false;
            }
        }

        private bool StartTCPConnect(TcpClient client, string ip, int port)
        {
            var result = client.BeginConnect(ip, port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            client.EndConnect(result);
            return success;
        }

        private void OnDisable()
        {
            Disconnect();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            if (!isConnected) return;
            if (m_isDisconnecting) return;

            m_isDisconnecting = true;
            
            if (m_tcpClient != null)
            {
                try
                {
                    if (m_tcpClient.Connected)
                    {
                        Write(SCIP_Writer.QT());
                        NetworkStream stream = m_tcpClient.GetStream();
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError("[Hokuyo] " + ex.Message));
                }

                try
                {
                    m_tcpClient.Close();
                }
                catch (Exception ex)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError("[Hokuyo] " + ex.Message));
                }
            }

            if (m_tcpClientThread != null)
            {
                try
                {
                    m_tcpClientThread.Abort();
                }
                catch
                {
                    // ignored
                }
            }

            m_isDisconnecting = false;
            isConnected = false;
            UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.Log("[Hokuyo] Disconnected"));
        }

        public void Write(string scip)
        {
            NetworkStream stream = m_tcpClient.GetStream();
            write(stream, scip);
        }

        private void HandleClientComm(object obj)
        {
            try
            {
                using (TcpClient client = (TcpClient)obj)
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        while (true)
                        {
                            long time_stamp = 0;
                            string receive_data = read_line(stream);

                            string cmd = GetCommand(receive_data);
                            lock (m_distances)
                            {
                                if (cmd == GetCMDString(CMD.MD))
                                {
                                    // measure distance only
                                    m_distances.Clear();
                                    SCIP_Reader.MD(receive_data, ref time_stamp, ref m_distances);
                                }
                                else if (cmd == GetCMDString(CMD.ME))
                                {
                                    // measure distance and strength
                                    m_distances.Clear();
                                    m_strengths.Clear();
                                    SCIP_Reader.ME(receive_data, ref time_stamp, ref m_distances, ref m_strengths);
                                }
                                else
                                {
                                    UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.Log("[Hokuyo] " + receive_data));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => Debug.LogError("[Hokuyo] " + ex.Message));
                Disconnect();
            }
        }

        private string GetCommand(string get_command)
        {
            string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return split_command[0].Substring(0, 2);
        }

        private bool CheckCommand(string get_command, string cmd)
        {
            string[] split_command = get_command.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return split_command[0].StartsWith(cmd);
        }
        
        /// <summary>
        /// Read to "\n\n" from NetworkStream
        /// </summary>
        /// <returns>receive data</returns>
        static string read_line(NetworkStream stream)
        {
            if (stream.CanRead)
            {
                StringBuilder sb = new StringBuilder();
                bool is_NL2 = false;
                bool is_NL = false;
                do
                {
                    char buf = (char)stream.ReadByte();
                    if (buf == '\n')
                    {
                        if (is_NL)
                        {
                            is_NL2 = true;
                        }
                        else
                        {
                            is_NL = true;
                        }
                    }
                    else
                    {
                        is_NL = false;
                    }

                    sb.Append(buf);
                } while (!is_NL2);

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// write data
        /// </summary>
        static bool write(NetworkStream stream, string data)
        {
            if (stream.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                stream.Write(buffer, 0, buffer.Length);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
