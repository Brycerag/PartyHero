using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using YARG.Core.Logging;

namespace YARG.PartyHero
{
    /// <summary>
    /// Handles TCP socket communication for PartyHero
    /// Bidirectional communication with custom protocols
    /// Lower latency than UDP-based OSC
    /// </summary>
    public class TcpManager : MonoBehaviour
    {
        [Header("TCP Configuration (Loaded from config file)")]
        [Tooltip("Port to listen for incoming connections")]
        public int listenPort = 9001;

        [Tooltip("Remote IP to connect to (if acting as client)")]
        public string remoteAddress = "127.0.0.1";

        [Tooltip("Remote port to connect to (if acting as client)")]
        public int remotePort = 9002;

        [Header("Mode")]
        [Tooltip("Server mode listens for connections, Client mode connects to remote")]
        public bool serverMode = true;

        private ShowFlowStateMachine _stateMachine;
        private PartyHeroState _partyHeroState;
        private bool _enabled = true;
        
        private TcpListener _server;
        private TcpClient _client;
        private Thread _listenerThread;
        private NetworkStream _stream;
        private bool _running = false;
        private bool _connected = false;

        public bool IsConnected => _connected;

        public void Initialize(ShowFlowStateMachine stateMachine, PartyHeroState state)
        {
            _stateMachine = stateMachine;
            _partyHeroState = state;

            // Load configuration from file
            LoadFromConfig();
            
            // Subscribe to config reload events
            PartyHeroConfigManager.Instance.OnConfigReloaded += OnConfigReloaded;

            try
            {
                if (_enabled)
                {
                    if (serverMode)
                    {
                        StartServer();
                    }
                    else
                    {
                        StartClient();
                    }
                }
                else
                {
                    YargLogger.LogInfo("[PartyHero] TCP Manager disabled in config");
                }
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to initialize TCP");
            }
        }

        private void LoadFromConfig()
        {
            var config = PartyHeroConfigManager.Instance.Config.tcp;
            serverMode = config.serverMode;
            listenPort = config.listenPort;
            remoteAddress = config.remoteAddress;
            remotePort = config.remotePort;
            _enabled = config.enabled;
        }

        private void OnConfigReloaded(PartyHeroConfig config)
        {
            YargLogger.LogInfo("[PartyHero] TCP: Reloading configuration...");
            
            // Stop current connections
            _running = false;
            OnDestroy();
            
            // Reload and restart
            LoadFromConfig();
            
            if (_enabled)
            {
                if (serverMode)
                {
                    StartServer();
                }
                else
                {
                    StartClient();
                }
            }
        }

        #region Server Mode

        private void StartServer()
        {
            _running = true;
            _listenerThread = new Thread(ServerLoop)
            {
                IsBackground = true
            };
            _listenerThread.Start();

            YargLogger.LogInfo($"[PartyHero] TCP Server started on port {listenPort}");
        }

        private void ServerLoop()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, listenPort);
                _server.Start();

                YargLogger.LogInfo($"[PartyHero] TCP Server listening on port {listenPort}");

                while (_running)
                {
                    // Wait for client connection (blocking)
                    if (_server.Pending())
                    {
                        _client = _server.AcceptTcpClient();
                        _stream = _client.GetStream();
                        _connected = true;

                        YargLogger.LogInfo("[PartyHero] TCP Client connected");

                        // Handle messages from this client
                        HandleClientMessages();

                        _connected = false;
                        YargLogger.LogInfo("[PartyHero] TCP Client disconnected");
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "TCP Server error");
            }
            finally
            {
                _server?.Stop();
            }
        }

        #endregion

        #region Client Mode

        private void StartClient()
        {
            _running = true;
            _listenerThread = new Thread(ClientLoop)
            {
                IsBackground = true
            };
            _listenerThread.Start();

            YargLogger.LogInfo($"[PartyHero] TCP Client connecting to {remoteAddress}:{remotePort}");
        }

        private void ClientLoop()
        {
            try
            {
                _client = new TcpClient(remoteAddress, remotePort);
                _stream = _client.GetStream();
                _connected = true;

                YargLogger.LogInfo($"[PartyHero] TCP Connected to {remoteAddress}:{remotePort}");

                // Handle messages from server
                HandleClientMessages();

                _connected = false;
                YargLogger.LogInfo("[PartyHero] TCP Disconnected from server");
            }
            catch (ThreadAbortException)
            {
                // Expected on shutdown
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "TCP Client error");
            }
        }

        #endregion

        #region Message Handling

        private void HandleClientMessages()
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (_running && _connected)
                {
                    if (_stream.DataAvailable)
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            ProcessMessage(message);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "TCP message handling error");
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                YargLogger.LogInfo($"[PartyHero] TCP Received: {message}");

                // Parse message format: COMMAND:ARGS
                string[] parts = message.Split(':');
                if (parts.Length < 1) return;

                string command = parts[0].Trim();
                string args = parts.Length > 1 ? parts[1].Trim() : "";

                // Handle commands on main thread
                UnityMainThreadDispatcher.Enqueue(() => HandleCommand(command, args));
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to process TCP message");
            }
        }

        private void HandleCommand(string command, string args)
        {
            switch (command.ToUpper())
            {
                case "BAND_READY":
                    OnBandReady();
                    break;

                case "PLAYER_READY":
                    OnPlayerReady();
                    break;

                case "FORCE_STATE":
                    OnForceState(args);
                    break;

                case "SYNC_TIME":
                    if (float.TryParse(args, out float time))
                    {
                        OnSyncTime(time);
                    }
                    break;

                case "PING":
                    SendMessage("PONG");
                    break;

                default:
                    YargLogger.LogWarning($"[PartyHero] Unknown TCP command: {command}");
                    break;
            }
        }

        #endregion

        #region Command Handlers

        private void OnBandReady()
        {
            YargLogger.LogInfo("[PartyHero] TCP: Band ready received");

            if (_stateMachine != null)
            {
                _stateMachine.TriggerBandReady();
            }
        }

        private void OnPlayerReady()
        {
            YargLogger.LogInfo("[PartyHero] TCP: Player ready received");

            if (_stateMachine != null)
            {
                _stateMachine.TriggerPlayerReady();
            }
        }

        private void OnForceState(string stateName)
        {
            YargLogger.LogInfo($"[PartyHero] TCP: Force state to {stateName}");

            if (_stateMachine == null) return;

            ShowFlowStateType stateType = stateName.ToLower() switch
            {
                "band" or "waitingforband" => ShowFlowStateType.WaitingForBand,
                "swap" or "waitingforswap" => ShowFlowStateType.WaitingForSwap,
                "break" or "setend" => ShowFlowStateType.SetEnd,
                "end" or "showend" => ShowFlowStateType.ShowEnd,
                _ => ShowFlowStateType.None
            };

            if (stateType != ShowFlowStateType.None)
            {
                _stateMachine.ChangeState(stateType);
            }
        }

        private void OnSyncTime(float seconds)
        {
            YargLogger.LogInfo($"[PartyHero] TCP: Sync time to {seconds:F3}s");
            // TODO: Implement timeline synchronization
        }

        #endregion

        #region Send Messages

        /// <summary>
        /// Send a message over TCP
        /// </summary>
        public void SendMessage(string message)
        {
            if (!_connected || _stream == null) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                _stream.Write(data, 0, data.Length);
                _stream.Flush();

                YargLogger.LogInfo($"[PartyHero] TCP Sent: {message}");
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to send TCP message");
            }
        }

        /// <summary>
        /// Send song start notification
        /// </summary>
        public void SendSongStart(string songName)
        {
            SendMessage($"SONG_START:{songName}");
        }

        /// <summary>
        /// Send song end notification
        /// </summary>
        public void SendSongEnd(int score)
        {
            SendMessage($"SONG_END:{score}");
        }

        /// <summary>
        /// Send state change notification
        /// </summary>
        public void SendStateChange(string stateName)
        {
            SendMessage($"STATE_CHANGE:{stateName}");
        }

        #endregion

        #region Lifecycle

        private void OnDestroy()
        {
            _running = false;

            try
            {
                _stream?.Close();
                _client?.Close();
                _server?.Stop();

                if (_listenerThread != null && _listenerThread.IsAlive)
                {
                    _listenerThread.Abort();
                }
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Error shutting down TCP");
            }
            
            // Unsubscribe from config events
            if (PartyHeroConfigManager.Instance != null)
            {
                PartyHeroConfigManager.Instance.OnConfigReloaded -= OnConfigReloaded;
            }
        }

        #endregion

        #region Testing

        [ContextMenu("Test: Send Ping")]
        public void TestPing()
        {
            SendMessage("PING");
        }

        [ContextMenu("Test: Send Band Ready")]
        public void TestBandReady()
        {
            SendMessage("BAND_READY");
        }

        #endregion
    }

    /// <summary>
    /// Helper to dispatch actions to Unity's main thread
    /// TCP listener runs on background thread, but Unity API requires main thread
    /// </summary>
    public static class UnityMainThreadDispatcher
    {
        private static readonly System.Collections.Generic.Queue<Action> _actions = new();
        private static readonly object _lock = new();

        public static void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Enqueue(action);
            }
        }

        public static void ExecuteQueue()
        {
            lock (_lock)
            {
                while (_actions.Count > 0)
                {
                    _actions.Dequeue()?.Invoke();
                }
            }
        }
    }
}
