using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace LZY.Lidar
{
    public class EthernetPAVODevice : MonoBehaviour, ILidarDevice
    {
        public const string DEFAULT_IP_ADDRESS = "10.10.10.101";
        public const int DEFAULT_PORT = 2368;
        
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
        private List<long> m_strengths;
        
        public bool isConnected 
        {
            get => m_isConnected;
            private set
            {
                if (m_isConnected == value) return;
                m_isConnected = value;
                m_onConnectionChanged?.Invoke(m_isConnected);
            }
        }
        
        private bool m_isInitialized = false;
        private bool m_isConnected = false;

        public float degreeShift => m_degreeShift * 0.01f;
        [Range(10, 35999), SerializeField] private int m_degreeShift = 10;

        public float minDegreeScope => m_minDegreeScope * 0.01f;
        [Range(0, 35999), SerializeField] private int m_minDegreeScope = 4500;
        
        public float maxDegreeScope => m_maxDegreeScope * 0.01f;
        [Range(0, 35999), SerializeField] private int m_maxDegreeScope = 31500;
        
        public float motorSpeed => m_motorSpeed;
        [Range(10, 30), SerializeField] private int m_motorSpeed = 10;
        
        public float mergeCoef => m_mergeCoef;
        [Range(1, 8), SerializeField] private int m_mergeCoef = 1;

        public UnityEvent<bool> onConnectionChanged => m_onConnectionChanged;
        [SerializeField] private UnityEvent<bool> m_onConnectionChanged;

        private const int MAX_SCAN_POINTS = 4096;
        private Thread _scanThread;
        
        public void Initialize()
        {
            if (m_isInitialized) return;
            
            if (Pavo.pavo_init())
                m_isInitialized = true;
            else
                Debug.Log("[PAVO] Failed to initialize PAVO driver.");
        }

        public void Deinitialize()
        {
            if (!m_isInitialized) return;
            
            Pavo.pavo_deinit();
            m_isInitialized = false;
        }

        public void Connect(string ipAddress = DEFAULT_IP_ADDRESS, ushort portNumber = DEFAULT_PORT)
        {
            if (isConnected) return;

            m_distances = new List<long>();
            m_strengths = new List<long>();

            Initialize();

            if (!Pavo.pavo_open(ipAddress, portNumber))
            {
                Debug.Log("[PAVO] Failed to connect to PAVO device. Make sure sensor is properly connected and configured. Also make sure, there is no firewall restriction for this app.");
                Deinitialize();
                return;
            }

            isConnected = true;
            Debug.Log("[PAVO] Connected to lidar.");
            Pavo.pavo_set_degree_shift(ref m_degreeShift);
            Pavo.pavo_set_degree_scope(ref m_minDegreeScope, ref m_maxDegreeScope);
            Pavo.pavo_set_motor_speed(ref m_motorSpeed);
            Pavo.pavo_set_merge_coef(ref m_mergeCoef);
            DebugLogPavoDeviceInformation();

            _scanThread = new Thread(ScanLoop);
            _scanThread.Start();
        }

        public void Disconnect()
        {
            if (!isConnected) return;

            Pavo.pavo_close();
            isConnected = false;
            _scanThread?.Join();
            
            Debug.Log("[PAVO] Disconnected from Siminics Pavo Sensor.");
        }

        private void ScanLoop(object obj)
        {
            try
            {
                var buffer = new PavoScanPoint[MAX_SCAN_POINTS];
                int count;

                while (m_isInitialized && isConnected)
                {
                    count = MAX_SCAN_POINTS;
                    if (Pavo.pavo_get_scan(buffer, ref count, 100))
                    {
                        lock (m_distances)
                        {
                            m_distances.Clear();
                            for (int i = 0; i < count; i++)
                                m_distances.Add((long)(buffer[i].distance * 2f));
                        }
                    }
                    
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[PAVO] " + ex.Message);
            }
        }

        private void OnDisable()
        {
            Disconnect();
            Deinitialize();
        }
        
        private void OnApplicationQuit()
        {
            Disconnect();
            Deinitialize();
        }
        
        private void DebugLogPavoDeviceInformation()
        {
            var sb = new StringBuilder(128);

            string firmware;
            if (!Pavo.pavo_get_fw_ver(sb, sb.Capacity))
                Debug.LogError("[PAVO] Failed to get firmware version");
            firmware = sb.ToString();
            sb.Clear();
            
            string lidarIp;
            if (!Pavo.pavo_get_lidar_ip(sb, sb.Capacity))
                Debug.LogError($"[PAVO] Lidar IP: {sb}");
            lidarIp = sb.ToString();
            sb.Clear();
            
            string destIp;
            if (!Pavo.pavo_get_dest_ip(sb, sb.Capacity))
                Debug.Log("[PAVO] Failed to get destination IP");
            destIp = sb.ToString();
            sb.Clear();
            
            if (!Pavo.pavo_get_dest_port(out var destPort))
                Debug.Log("[PAVO] Failed to get destination port");

            if (!Pavo.pavo_get_sn(out var sn))
                Debug.Log("[PAVO] Failed to get serial number");

            if (!Pavo.pavo_get_pn(out var pn))
                Debug.Log("[PAVO] Failed to get part number");

            if (!Pavo.pavo_get_motor_speed(out m_motorSpeed))
                Debug.Log("[PAVO] Failed to get motor speed");

            if (!Pavo.pavo_get_merge_coef(out m_mergeCoef))
                Debug.Log("[PAVO] Failed to get merge coefficient");

            if (!Pavo.pavo_get_degree_scope(out m_minDegreeScope, out m_maxDegreeScope))
                Debug.Log("[PAVO] Failed to get degree scope");

            if (!Pavo.pavo_get_degree_shift(out m_degreeShift))
                Debug.Log("[PAVO] Failed to get degree shift");

            Debug.Log("[PAVO] Device Information:" +
                      $"\nFirmware : {firmware}" +
                      $"\nLidar IP : {lidarIp}" +
                      $"\nDestination IP : {destIp}" +
                      $"\nDestination Port : {destPort}" +
                      $"\nSerial Number : {sn}" +
                      $"\nPart Number : {pn}" +
                      $"\nMotor Speed : {motorSpeed}" +
                      $"\nMerge Coefficient : {mergeCoef}" +
                      $"\nDegree Scope : Min = {minDegreeScope}°, Max = {maxDegreeScope}°" +
                      $"\nDegree Shift : {degreeShift}°");
        }
    }
}