using UnityEngine;
using YARG.Core.Logging;
using YARG.Core.Input;

namespace YARG.PartyHero
{
    /// <summary>
    /// Handles MIDI input for PartyHero show flow triggers
    /// Integrates with YARG's existing MIDI system (Hidrogen/PlasticBand)
    /// Configuration loaded from partyhero_config.json
    /// </summary>
    public class MidiInputHandler : MonoBehaviour
    {
        [Header("MIDI Configuration (Loaded from config file)")]
        [Tooltip("MIDI note for band ready trigger (C4 = 60)")]
        public int bandReadyNote = 60;

        [Tooltip("MIDI note for force next state (C#4 = 61)")]
        public int forceNextStateNote = 61;

        [Tooltip("MIDI CC for player ready toggle")]
        public int playerReadyCC = 20;

        [Tooltip("Minimum velocity to trigger (0-127)")]
        public int minimumVelocity = 64;

        private ShowFlowStateMachine _stateMachine;
        private PartyHeroState _partyHeroState;
        private bool _enabled = true;

        public void Initialize(ShowFlowStateMachine stateMachine, PartyHeroState state)
        {
            _stateMachine = stateMachine;
            _partyHeroState = state;
            
            // Load configuration from file
            LoadFromConfig();
            
            // Subscribe to config reload events
            PartyHeroConfigManager.Instance.OnConfigReloaded += OnConfigReloaded;
            
            YargLogger.LogInfo("[PartyHero] MIDI Input Handler initialized");
            YargLogger.LogInfo($"[PartyHero] MIDI Enabled: {_enabled}");
            YargLogger.LogInfo($"[PartyHero] Band Ready Note: {bandReadyNote} (MIDI {GetNoteName(bandReadyNote)})");
            YargLogger.LogInfo($"[PartyHero] Force Next State Note: {forceNextStateNote} (MIDI {GetNoteName(forceNextStateNote)})");
            YargLogger.LogInfo($"[PartyHero] Player Ready CC: {playerReadyCC}");
        }

        private void LoadFromConfig()
        {
            var config = PartyHeroConfigManager.Instance.Config.midi;
            bandReadyNote = config.bandReadyNote;
            forceNextStateNote = config.forceNextStateNote;
            playerReadyCC = config.playerReadyCC;
            minimumVelocity = config.minimumVelocity;
            _enabled = config.enabled;
        }

        private void OnConfigReloaded(PartyHeroConfig config)
        {
            YargLogger.LogInfo("[PartyHero] MIDI: Reloading configuration...");
            LoadFromConfig();
        }

        private void OnDestroy()
        {
            // Unsubscribe from config events
            if (PartyHeroConfigManager.Instance != null)
            {
                PartyHeroConfigManager.Instance.OnConfigReloaded -= OnConfigReloaded;
            }
        }

        private void Update()
        {
            // Only process MIDI when not in development mode
            // (development mode uses keyboard shortcuts instead)
            if (_partyHeroState == null || _partyHeroState.developmentMode)
            {
                return;
            }

            // TODO: Hook into YARG's MIDI input system
            // This is a placeholder for MIDI integration
            // Need to access YARG's input manager to listen for MIDI events
            
            // Example of what we need:
            // if (MidiDevice.GetNoteDown(bandReadyNote, minimumVelocity))
            // {
            //     OnBandReady();
            // }
        }

        /// <summary>
        /// Called when band ready MIDI note is received
        /// </summary>
        public void OnBandReady()
        {
            YargLogger.LogInfo("[PartyHero] MIDI: Band ready received");

            if (_stateMachine != null)
            {
                _stateMachine.TriggerBandReady();
            }
        }

        /// <summary>
        /// Called when player ready MIDI CC is received
        /// </summary>
        public void OnPlayerReady()
        {
            YargLogger.LogInfo("[PartyHero] MIDI: Player ready received");

            if (_stateMachine != null)
            {
                _stateMachine.TriggerPlayerReady();
            }
        }
        }

        /// <summary>
        /// Called when force next state MIDI note is received
        /// </summary>
        public void OnForceNextState()
        {
            YargLogger.LogInfo("[PartyHero] MIDI: Force next state received");

            if (_stateMachine != null)
            {
                // Force load next song regardless of current state
                _stateMachine.LoadNextSong();
            }
        }

        /// <summary>
        /// Process a raw MIDI note on message
        /// Call this from YARG's MIDI input callback
        /// </summary>
        public void ProcessMidiNoteOn(int noteNumber, int velocity)
        {
            if (velocity < minimumVelocity)
            {
                return; // Ignore low velocity notes
            }

            YargLogger.LogInfo($"[PartyHero] MIDI Note On: {noteNumber} ({GetNoteName(noteNumber)}) Velocity: {velocity}");

            if (noteNumber == bandReadyNote)
            {
                OnBandReady();
            }
            else if (noteNumber == forceNextStateNote)
            {
                OnForceNextState();
            }
        }

        /// <summary>
        /// Process a raw MIDI control change message
        /// Call this from YARG's MIDI input callback
        /// </summary>
        public void ProcessMidiCC(int ccNumber, int value)
        {
            YargLogger.LogInfo($"[PartyHero] MIDI CC: {ccNumber} Value: {value}");

            if (ccNumber == playerReadyCC && value > 64)
            {
                OnPlayerReady();
            }
        }

        /// <summary>
        /// Get musical note name from MIDI note number
        /// </summary>
        private string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = (noteNumber / 12) - 1;
            int note = noteNumber % 12;
            return $"{noteNames[note]}{octave}";
        }
    }
}
