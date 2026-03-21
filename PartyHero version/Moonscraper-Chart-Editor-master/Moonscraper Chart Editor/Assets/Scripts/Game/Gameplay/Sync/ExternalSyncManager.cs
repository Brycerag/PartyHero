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

        [Tooltip("Current DAW time in seconds (absolute timeline position)")]
        public float currentTime = 0f;

        [Tooltip("Current beat position from DAW")]
        public float currentBeat = 0f;

        [Tooltip("Current tempo (BPM) from DAW")]
        public float currentTempo = 120f;

        [Tooltip("Current track/song name from DAW")]
        public string currentTrackName = "";

        [Header("Timeline Offset (for continuous timelines)")]
        [Tooltip("Where current song starts in DAW timeline (set by SongMappingManager)")]
        public float currentSongTimelineStart = 0f;

        [Tooltip("Visual pre-roll for current song (seconds before song start)")]
        public float currentSongPreRoll = 3.0f;

        [Header("OSC Output (Sending to DAW)")]
        [Tooltip("IP address of DAW/AbleSet to send commands to")]
        public string dawIpAddress = "127.0.0.1";

        [Tooltip("Port to send OSC commands to DAW/AbleSet (AbleSet default: 39045)")]
        public int oscOutputPort = 39045;

        [Header("Debug")]
        [Tooltip("Log received OSC messages to console")]
        public bool debugOscMessages = false;

        [Tooltip("Last OSC message received time")]
        public float lastMessageTime = 0f;

        // Private fields
        private UdpClient udpClient;          // For receiving OSC
        private UdpClient sendClient;         // For sending OSC
        private IPEndPoint sendEndPoint;      // Target for sending
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

            // Initialize OSC sending client
            InitializeSendClient();
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
            DisposeSendClient();
        }

        void OnApplicationQuit()
        {
            StopOscReceiver();
            DisposeSendClient();
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
        /// Get song-relative time (for continuous timelines).
        /// Converts absolute DAW timeline position to time within current song.
        /// </summary>
        public float GetSongRelativeTime()
        {
            return currentTime - currentSongTimelineStart;
        }

        /// <summary>
        /// Get visual display time for chart scrolling.
        /// 
        /// READY STATE (DAW paused):
        ///   - Chart frozen at -visualPreRoll (e.g., -3.0s)
        ///   - Band can see upcoming notes while getting ready
        /// 
        /// PLAYING STATE (DAW playing):
        ///   - Chart immediately syncs to song-relative time
        ///   - If Ableton timeline has count-in bars BEFORE song start, they scroll naturally
        ///   - Example: DAW at -3s (count-in) → chart shows -3s and scrolls to 0s
        /// </summary>
        public float GetDisplayTime()
        {
            float songRelativeTime = GetSongRelativeTime();
            
            // If DAW is paused/stopped, hold at pre-roll position (ready state)
            // This lets the band see the chart and get ready before drummer starts playback
            if (!isPlaying)
            {
                // Hold chart at negative pre-roll position (e.g., -3.0s)
                // Band can see upcoming notes but chart doesn't scroll yet
                return -currentSongPreRoll;
            }
            
            // DAW is playing - immediately sync to actual song-relative position
            // No additional offset needed - Ableton timeline already has count-in built in
            // Example: If DAW is at 222s and song starts at 225s, show -3s (count-in)
            return songRelativeTime;
        }

        /// <summary>
        /// Set timeline offset for currently loaded song.
        /// Called by SongMappingManager when a song is loaded.
        /// </summary>
        public void SetCurrentSongOffset(float timelineStart, float preRoll)
        {
            currentSongTimelineStart = timelineStart;
            currentSongPreRoll = preRoll;
            
            Debug.Log($"[ExternalSyncManager] Song offset set - Timeline start: {timelineStart}s, Pre-roll: {preRoll}s");
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

        // ==================== OSC SENDING METHODS ====================

        private void InitializeSendClient()
        {
            try
            {
                sendClient = new UdpClient();
                UpdateSendEndpoint();
                Debug.Log($"[ExternalSyncManager] OSC send client initialized ({dawIpAddress}:{oscOutputPort})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalSyncManager] Failed to initialize OSC send client: {e.Message}");
            }
        }

        private void UpdateSendEndpoint()
        {
            try
            {
                IPAddress address = IPAddress.Parse(dawIpAddress);
                sendEndPoint = new IPEndPoint(address, oscOutputPort);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalSyncManager] Invalid DAW IP address '{dawIpAddress}': {e.Message}");
                // Fallback to localhost
                sendEndPoint = new IPEndPoint(IPAddress.Loopback, oscOutputPort);
            }
        }

        private void DisposeSendClient()
        {
            if (sendClient != null)
            {
                sendClient.Close();
                sendClient = null;
            }
        }

        /// <summary>
        /// Build an OSC message as a byte array.
        /// OSC format: [address][,types][args...]
        /// All elements are null-terminated and 4-byte aligned.
        /// </summary>
        private byte[] BuildOscMessage(string address, params object[] args)
        {
            System.Collections.Generic.List<byte> data = new System.Collections.Generic.List<byte>();

            // Add address (null-terminated, 4-byte aligned)
            byte[] addressBytes = System.Text.Encoding.ASCII.GetBytes(address);
            data.AddRange(addressBytes);
            data.Add(0); // null terminator
            while (data.Count % 4 != 0) data.Add(0); // padding

            // Build type tag string
            System.Text.StringBuilder typeTag = new System.Text.StringBuilder(",");
            foreach (var arg in args)
            {
                if (arg is int || arg is bool) typeTag.Append('i');
                else if (arg is float) typeTag.Append('f');
                else if (arg is string) typeTag.Append('s');
                else typeTag.Append('i'); // default to int
            }

            // Add type tag (null-terminated, 4-byte aligned)
            byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(typeTag.ToString());
            data.AddRange(typeBytes);
            data.Add(0); // null terminator
            while (data.Count % 4 != 0) data.Add(0); // padding

            // Add arguments
            foreach (var arg in args)
            {
                if (arg is int)
                {
                    int val = (int)arg;
                    data.Add((byte)((val >> 24) & 0xFF));
                    data.Add((byte)((val >> 16) & 0xFF));
                    data.Add((byte)((val >> 8) & 0xFF));
                    data.Add((byte)(val & 0xFF));
                }
                else if (arg is bool)
                {
                    int val = (bool)arg ? 1 : 0;
                    data.Add((byte)((val >> 24) & 0xFF));
                    data.Add((byte)((val >> 16) & 0xFF));
                    data.Add((byte)((val >> 8) & 0xFF));
                    data.Add((byte)(val & 0xFF));
                }
                else if (arg is float)
                {
                    byte[] floatBytes = System.BitConverter.GetBytes((float)arg);
                    if (System.BitConverter.IsLittleEndian)
                        System.Array.Reverse(floatBytes); // Convert to big-endian
                    data.AddRange(floatBytes);
                }
                else if (arg is string)
                {
                    byte[] strBytes = System.Text.Encoding.ASCII.GetBytes((string)arg);
                    data.AddRange(strBytes);
                    data.Add(0); // null terminator
                    while (data.Count % 4 != 0) data.Add(0); // padding
                }
            }

            return data.ToArray();
        }

        /// <summary>
        /// Send an OSC message to the configured DAW/AbleSet.
        /// </summary>
        public bool SendOscMessage(string address, params object[] args)
        {
            if (sendClient == null || sendEndPoint == null)
            {
                Debug.LogWarning("[ExternalSyncManager] OSC send client not initialized");
                return false;
            }

            try
            {
                byte[] message = BuildOscMessage(address, args);
                sendClient.Send(message, message.Length, sendEndPoint);

                if (debugOscMessages)
                {
                    string argsStr = string.Join(", ", args);
                    Debug.Log($"[ExternalSyncManager] Sent OSC: {address} [{argsStr}]");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalSyncManager] Failed to send OSC message '{address}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tell AbleSet to jump to a specific song/track by name.
        /// Uses AbleSet's OSC API: /ableset/jump/project [string]
        /// </summary>
        public bool CueSong(string trackName)
        {
            return SendOscMessage("/ableset/jump/project", trackName);
        }

        /// <summary>
        /// Tell AbleSet to jump to a specific timeline position.
        /// Uses AbleSet's OSC API: /ableset/jump/time [float]
        /// </summary>
        public bool JumpToTime(float seconds)
        {
            return SendOscMessage("/ableset/jump/time", seconds);
        }

        /// <summary>
        /// Update DAW IP address and output port (reinitializes send endpoint).
        /// </summary>
        public void SetDawAddress(string ipAddress, int port)
        {
            dawIpAddress = ipAddress;
            oscOutputPort = port;
            UpdateSendEndpoint();
            Debug.Log($"[ExternalSyncManager] DAW address updated to {dawIpAddress}:{oscOutputPort}");
        }

        // ==================== CONNECTION STATUS ====================

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
