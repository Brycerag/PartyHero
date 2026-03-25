using System;
using UnityEngine;
using YARG.Core.Logging;

#if UNITY_EDITOR || OSCCORE_IMPORTED
// OscCore package required: https://github.com/stella3d/OscCore
// Install via: Add package from git URL: https://github.com/stella3d/OscCore.git
// using OscCore;
#endif

namespace YARG.PartyHero
{
    /// <summary>
    /// Handles OSC (Open Sound Control) communication for PartyHero
    /// Bidirectional communication with DAWs (Ableton, AbleSet, etc.)
    /// 
    /// REQUIRES: OscCore package (https://github.com/stella3d/OscCore)
    /// Install via Package Manager: Add package from git URL
    /// </summary>
    public class OscManager : MonoBehaviour
    {
        [Header("OSC Configuration")]
        [Tooltip("Port to receive OSC messages on")]
        public int receivePort = 8000;

        [Tooltip("IP address to send OSC messages to (usually localhost for DAW)")]
        public string sendAddress = "127.0.0.1";

        [Tooltip("Port to send OSC messages to")]
        public int sendPort = 9000;

        [Header("Message Addresses")]
        [Tooltip("OSC address for band ready trigger")]
        public string bandReadyAddress = "/partyhero/band_ready";

        [Tooltip("OSC address for force state change")]
        public string forceStateAddress = "/partyhero/force_state";

        [Tooltip("OSC address for sync time")]
        public string syncTimeAddress = "/partyhero/sync_time";

        private ShowFlowStateMachine _stateMachine;
        private PartyHeroState _partyHeroState;
        private bool _initialized = false;

// Uncomment when OscCore is installed
#if OSCCORE_IMPORTED
        // private OscServer _server;
        // private OscClient _client;
#endif

        public void Initialize(ShowFlowStateMachine stateMachine, PartyHeroState state)
        {
            _stateMachine = stateMachine;
            _partyHeroState = state;

            try
            {
                InitializeOsc();
                _initialized = true;
                YargLogger.LogInfo($"[PartyHero] OSC Manager initialized");
                YargLogger.LogInfo($"[PartyHero] Receiving on port {receivePort}");
                YargLogger.LogInfo($"[PartyHero] Sending to {sendAddress}:{sendPort}");
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to initialize OSC");
                _initialized = false;
            }
        }

        private void InitializeOsc()
        {
#if OSCCORE_IMPORTED
            // Create OSC server for receiving messages
            // _server = new OscServer(receivePort);
            // _server.TryAddMethod(bandReadyAddress, OnBandReady);
            // _server.TryAddMethod(forceStateAddress, OnForceState);
            // _server.TryAddMethod(syncTimeAddress, OnSyncTime);

            // Create OSC client for sending messages
            // _client = new OscClient(sendAddress, sendPort);

            YargLogger.LogInfo("[PartyHero] OSC initialized successfully");
#else
            YargLogger.LogWarning("[PartyHero] OSC not available - OscCore package not installed");
            YargLogger.LogWarning("[PartyHero] Install via: Package Manager > Add from git URL > https://github.com/stella3d/OscCore.git");
#endif
        }

        private void OnDestroy()
        {
#if OSCCORE_IMPORTED
            // Clean up OSC connections
            // _server?.Dispose();
            // _client?.Dispose();
#endif
        }

        #region Receive Messages (DAW → Game)

        /// <summary>
        /// Called when band ready OSC message is received
        /// Message: /partyhero/band_ready
        /// </summary>
        private void OnBandReady()
        {
            YargLogger.LogInfo("[PartyHero] OSC: Band ready received");

            if (_stateMachine != null && 
                _stateMachine.CurrentStateType == ShowFlowStateType.WaitingForBand)
            {
                // TODO: Trigger band ready in WaitingForBandState
                YargLogger.LogInfo("[PartyHero] Setting band ready via OSC");
            }
        }

        /// <summary>
        /// Called when force state OSC message is received
        /// Message: /partyhero/force_state [stateName]
        /// </summary>
        private void OnForceState(string stateName)
        {
            YargLogger.LogInfo($"[PartyHero] OSC: Force state to {stateName}");

            if (_stateMachine == null) return;

            // Parse state name and force transition
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

        /// <summary>
        /// Called when sync time OSC message is received
        /// Message: /partyhero/sync_time [seconds]
        /// </summary>
        private void OnSyncTime(float seconds)
        {
            YargLogger.LogInfo($"[PartyHero] OSC: Sync time to {seconds:F3}s");

            // TODO: Implement timeline synchronization with DAW
            // This would be used to sync game time with DAW playback position
        }

        #endregion

        #region Send Messages (Game → DAW)

        /// <summary>
        /// Send song start notification to DAW
        /// Message: /partyhero/song_start [songName]
        /// </summary>
        public void SendSongStart(string songName)
        {
            if (!_initialized) return;

            YargLogger.LogInfo($"[PartyHero] OSC: Sending song_start '{songName}'");

#if OSCCORE_IMPORTED
            // _client?.Send("/partyhero/song_start", songName);
#endif
        }

        /// <summary>
        /// Send song end notification to DAW
        /// Message: /partyhero/song_end [score]
        /// </summary>
        public void SendSongEnd(int score)
        {
            if (!_initialized) return;

            YargLogger.LogInfo($"[PartyHero] OSC: Sending song_end with score {score}");

#if OSCCORE_IMPORTED
            // _client?.Send("/partyhero/song_end", score);
#endif
        }

        /// <summary>
        /// Send state change notification to DAW
        /// Message: /partyhero/state_change [stateName]
        /// </summary>
        public void SendStateChange(string stateName)
        {
            if (!_initialized) return;

            YargLogger.LogInfo($"[PartyHero] OSC: Sending state_change '{stateName}'");

#if OSCCORE_IMPORTED
            // _client?.Send("/partyhero/state_change", stateName);
#endif
        }

        /// <summary>
        /// Send custom OSC message
        /// </summary>
        public void SendCustomMessage(string address, params object[] values)
        {
            if (!_initialized) return;

            YargLogger.LogInfo($"[PartyHero] OSC: Sending custom message to {address}");

#if OSCCORE_IMPORTED
            // _client?.Send(address, values);
#endif
        }

        #endregion

        #region Manual Testing

        /// <summary>
        /// Test OSC by simulating a band ready message (for debugging)
        /// </summary>
        [ContextMenu("Test: Simulate Band Ready")]
        public void TestBandReady()
        {
            YargLogger.LogInfo("[PartyHero] OSC: Testing band ready");
            OnBandReady();
        }

        /// <summary>
        /// Test OSC by sending a test message
        /// </summary>
        [ContextMenu("Test: Send Test Message")]
        public void TestSendMessage()
        {
            SendStateChange("test");
        }

        #endregion
    }
}
