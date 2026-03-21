using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MoonscraperChartEditor.Song
{
    /// <summary>
    /// Receives external DAW synchronization via OSC (Open Sound Control).
    /// Compatible with AbleSet, Ableton Link, and other OSC-enabled DAWs.
    /// 
    /// Expected OSC messages from AbleSet:
    /// - /playback/playing [0|1] - Transport state
    /// - /playback/time [float] - Current time in seconds
    /// - /playback/beat [float] - Current beat position 
    /// - /tempo [float] - Current tempo (BPM)
    /// - /track/name [string] - Current track/song name
    /// </summary>
    public class ExternalSyncManager : MonoBehaviour
    {
        public static ExternalSyncManager Instance { get; private set; }

        [Header("OSC Connection")]
        [Tooltip("UDP port to listen for OSC messages (AbleSet default: 39043)")]
        public int oscPort = 39043;

        [Tooltip("Enable external sync from DAW")]
        public bool syncEnabled = false;

        [Header("Sync State")]
        [Tooltip("Is DAW currently playing?")]
        public bool isPlaying = false;

        [Tooltip("Current DAW time in seconds")]
        public float currentTime = 0f;

        [Tooltip("Current beat position from DAW")]
        public float currentBeat = 0f;

        [Tooltip("Current tempo (BPM) from DAW")]
        public float currentTempo = 120f;

        [Tooltip("Current track/song name from DAW")]
        public string currentTrackName = "";

        [Header("Debug")]
        [Tooltip("Log received OSC messages to console")]
        public bool debugOscMessages = false;

        [Tooltip("Last OSC message received time")]
        public float lastMessageTime = 0f;

        // Private fields
        private UdpClient udpClient;
        private Thread receiveThread;
        private bool isReceiving = false;
        private object stateLock = new object();

        // Temporary state for thread-safe updates
        private bool _isPlaying = false;
        private float _currentTime = 0f;
        private float _currentBeat = 0f;
        private float _currentTempo = 120f;
        private string _currentTrackName = "";
        private bool _hasNewData = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            if (syncEnabled)
            {
                StartOscReceiver();
            }
        }

        void Update()
        {
            // Copy thread-safe data to public fields in main thread
            if (_hasNewData)
            {
                lock (stateLock)
                {
                    isPlaying = _isPlaying;
                    currentTime = _currentTime;
                    currentBeat = _currentBeat;
                    currentTempo = _currentTempo;
                    currentTrackName = _currentTrackName;
                    _hasNewData = false;
                }

                lastMessageTime = Time.unscaledTime;
            }
        }

        void OnDestroy()
        {
            StopOscReceiver();
        }

        void OnApplicationQuit()
        {
            StopOscReceiver();
        }

        public void StartOscReceiver()
        {
            if (isReceiving)
                return;

            try
            {
                udpClient = new UdpClient(oscPort);
                isReceiving = true;

                receiveThread = new Thread(ReceiveOscMessages);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                Debug.Log($"[ExternalSyncManager] OSC receiver started on port {oscPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalSyncManager] Failed to start OSC receiver: {e.Message}");
                isReceiving = false;
            }
        }

        public void StopOscReceiver()
        {
            if (!isReceiving)
                return;

            isReceiving = false;

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
                receiveThread = null;
            }

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            Debug.Log("[ExternalSyncManager] OSC receiver stopped");
        }

        private void ReceiveOscMessages()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isReceiving)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    if (data != null && data.Length > 0)
                    {
                        ProcessOscMessage(data, data.Length);
                    }
                }
                catch (SocketException)
                {
                    // Socket closed, expected on shutdown
                    break;
                }
                catch (ThreadAbortException)
                {
                    // Thread aborted, expected on shutdown
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ExternalSyncManager] Error receiving OSC: {e.Message}");
                }
            }
        }

        private void ProcessOscMessage(byte[] data, int length)
        {
            OscMessage msg = OscMessage.Parse(data, length);
            if (msg == null)
                return;

            if (debugOscMessages)
            {
                Debug.Log($"[ExternalSyncManager] OSC: {msg}");
            }

            lock (stateLock)
            {
                // Handle different OSC address patterns
                switch (msg.Address)
                {
                    case "/playback/playing":
                    case "/playing":
                        _isPlaying = msg.GetInt(0) != 0 || msg.GetBool(0);
                        _hasNewData = true;
                        break;

                    case "/playback/time":
                    case "/time":
                        _currentTime = msg.GetFloat(0);
                        _hasNewData = true;
                        break;

                    case "/playback/beat":
                    case "/beat":
                        _currentBeat = msg.GetFloat(0);
                        _hasNewData = true;
                        break;

                    case "/tempo":
                    case "/playback/tempo":
                        _currentTempo = msg.GetFloat(0, 120f);
                        if (_currentTempo < 20f || _currentTempo > 300f)
                            _currentTempo = 120f; // Sanity check
                        _hasNewData = true;
                        break;

                    case "/track/name":
                    case "/song/name":
                        _currentTrackName = msg.GetString(0);
                        _hasNewData = true;
                        break;

                    // Combined state message (custom format)
                    case "/sync/state":
                        if (msg.Arguments.Count >= 3)
                        {
                            _isPlaying = msg.GetBool(0);
                            _currentTime = msg.GetFloat(1);
                            _currentTempo = msg.GetFloat(2, 120f);
                            _hasNewData = true;
                        }
                        break;

                    default:
                        // Unknown OSC address, ignore
                        break;
                }
            }
        }

        /// <summary>
        /// Get current sync time. Returns DAW time if syncing, otherwise returns fallback.
        /// </summary>
        public float GetSyncTime(float fallbackTime)
        {
            if (!syncEnabled || Time.unscaledTime - lastMessageTime > 2f)
                return fallbackTime; // Not syncing or timed out

            return currentTime;
        }

        /// <summary>
        /// Is external sync currently active and receiving data?
        /// </summary>
        public bool IsSyncActive()
        {
            return syncEnabled && isReceiving && (Time.unscaledTime - lastMessageTime < 2f);
        }

        /// <summary>
        /// Check if external sync changed playing state
        /// </summary>
        public bool JustStartedPlaying()
        {
            // This should be enhanced with proper state tracking
            return IsSyncActive() && isPlaying;
        }

        /// <summary>
        /// Check if external sync stopped
        /// </summary>
        public bool JustStopped()
        {
            // This should be enhanced with proper state tracking
            return IsSyncActive() && !isPlaying;
        }

        public void SetSyncEnabled(bool enabled)
        {
            if (syncEnabled == enabled)
                return;

            syncEnabled = enabled;

            if (enabled)
            {
                StartOscReceiver();
            }
            else
            {
                StopOscReceiver();
            }
        }

        public void SetOscPort(int port)
        {
            if (port == oscPort)
                return;

            bool wasEnabled = syncEnabled && isReceiving;

            if (wasEnabled)
                StopOscReceiver();

            oscPort = port;

            if (wasEnabled)
                StartOscReceiver();
        }

        public string GetConnectionStatus()
        {
            if (!syncEnabled)
                return "External sync disabled";

            if (!isReceiving)
                return "Not listening for OSC";

            float timeSinceMessage = Time.unscaledTime - lastMessageTime;

            if (lastMessageTime == 0f)
                return $"Listening on port {oscPort}, waiting for messages...";

            if (timeSinceMessage < 2f)
                return $"Connected (last message {timeSinceMessage:F1}s ago)";

            return $"Connection timeout (last message {timeSinceMessage:F1}s ago)";
        }
    }
}
