// Copyright (c) 2024 PartyHero
// Show flow and state management for live performances

using UnityEngine;
using MoonscraperChartEditor.Song;

/// <summary>
/// Manages show flow, state transitions, player modes, and external triggers (MIDI/OSC).
/// Central coordinator for song transitions, player swaps, and show states.
/// </summary>
public class ShowFlowManager : MonoBehaviour
{
    public static ShowFlowManager Instance { get; private set; }

    [Header("Show Settings")]
    [Tooltip("Global toggle: Show 'Next Song' name on results screen")]
    public bool showNextSongName = true;

    [Tooltip("Show player name on results screen (future feature)")]
    public bool showPlayerName = false;

    [Tooltip("Enable show flow system (results screens, waiting states)")]
    public bool showFlowEnabled = true;

    [Tooltip("Require band ready signal before starting next song")]
    public bool requireBandReady = true;

    [Header("State Tracking")]
    [Tooltip("Current player mode for this song transition")]
    public PlayerMode currentPlayerMode = PlayerMode.Continuing;

    [Tooltip("Is the current player ready?")]
    public bool isPlayerReady = false;

    [Tooltip("Is the band ready to start?")]
    public bool isBandReady = false;

    [Header("MIDI Trigger Configuration")]
    [Tooltip("MIDI note number for player swap trigger")]
    public int playerSwapNoteNumber = 124;

    [Tooltip("MIDI note number for player ready signal")]
    public int playerReadyNoteNumber = 125;

    [Tooltip("MIDI note number for band ready signal")]
    public int bandReadyNoteNumber = 126;

    [Tooltip("MIDI note number for song complete trigger")]
    public int songCompleteNoteNumber = 127;

    [Tooltip("MIDI note number for set end trigger")]
    public int setEndNoteNumber = 122;

    [Tooltip("MIDI note number for show end trigger")]
    public int showEndNoteNumber = 121;

    [Tooltip("MIDI note number for no player/demo mode trigger")]
    public int noPlayerModeNoteNumber = 120;

    [Header("Debug")]
    [Tooltip("Log state transitions and triggers to console")]
    public bool debugShowFlow = true;

    /// <summary>
    /// Player mode for song transitions
    /// </summary>
    public enum PlayerMode
    {
        Continuing,    // Same player continues to next song
        Swapping,      // New player is taking over
        NoPlayer       // Demo mode - no player, auto-hit all notes
    }

    // Private state
    private BaseGameplayRulestate.NoteStats lastSongStats;
    private bool hasLastSongStats = false;

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
        RegisterOscTriggers();
        
        if (debugShowFlow)
        {
            Debug.Log("[ShowFlowManager] Initialized. Show flow enabled: " + showFlowEnabled);
        }
    }

    void OnDestroy()
    {
        UnregisterOscTriggers();
    }

    /// <summary>
    /// Register OSC message handlers with ExternalSyncManager
    /// </summary>
    void RegisterOscTriggers()
    {
        if (ExternalSyncManager.Instance != null)
        {
            // Hook into OSC message processing
            // Note: ExternalSyncManager needs to expose an event for this
            // For now, we'll check messages in ProcessOscMessage method
            if (debugShowFlow)
            {
                Debug.Log("[ShowFlowManager] OSC triggers registered with ExternalSyncManager");
            }
        }
    }

    void UnregisterOscTriggers()
    {
        // Cleanup OSC event handlers
    }

    /// <summary>
    /// Process incoming OSC message (called from ExternalSyncManager)
    /// </summary>
    public void ProcessOscMessage(string address, object[] args)
    {
        if (!showFlowEnabled)
            return;

        switch (address)
        {
            case "/player/swap":
                TriggerPlayerSwap();
                break;

            case "/player/ready":
                TriggerPlayerReady();
                break;

            case "/band/ready":
                TriggerBandReady();
                break;

            case "/song/complete":
                TriggerSongComplete();
                break;

            case "/set/end":
                TriggerSetEnd();
                break;

            case "/show/end":
                TriggerShowEnd();
                break;

            case "/game/mode/demo":
                TriggerNoPlayerMode();
                break;
        }
    }

    /// <summary>
    /// Process incoming MIDI Note On message (called from MIDI input system)
    /// </summary>
    public void ProcessMidiNote(int noteNumber)
    {
        if (!showFlowEnabled)
            return;

        if (noteNumber == playerSwapNoteNumber)
            TriggerPlayerSwap();
        else if (noteNumber == playerReadyNoteNumber)
            TriggerPlayerReady();
        else if (noteNumber == bandReadyNoteNumber)
            TriggerBandReady();
        else if (noteNumber == songCompleteNoteNumber)
            TriggerSongComplete();
        else if (noteNumber == setEndNoteNumber)
            TriggerSetEnd();
        else if (noteNumber == showEndNoteNumber)
            TriggerShowEnd();
        else if (noteNumber == noPlayerModeNoteNumber)
            TriggerNoPlayerMode();
    }

    // ===== TRIGGER METHODS =====

    /// <summary>
    /// Trigger player swap mode for next transition
    /// </summary>
    public void TriggerPlayerSwap()
    {
        currentPlayerMode = PlayerMode.Swapping;
        isPlayerReady = false; // New player not ready yet

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Player swap triggered");

        BroadcastPlayerState("swapping");
    }

    /// <summary>
    /// Signal that player is ready
    /// </summary>
    public void TriggerPlayerReady()
    {
        isPlayerReady = true;

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Player ready");

        BroadcastPlayerState("ready");
    }

    /// <summary>
    /// Signal that band is ready to start next song
    /// </summary>
    public void TriggerBandReady()
    {
        isBandReady = true;

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Band ready");

        BroadcastBandState("ready");
    }

    /// <summary>
    /// Force player ready (band override)
    /// </summary>
    public void ForcePlayerReady()
    {
        isPlayerReady = true;

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Player ready forced by band");

        BroadcastPlayerState("ready_forced");
    }

    /// <summary>
    /// Trigger no player mode (demo mode)
    /// </summary>
    public void TriggerNoPlayerMode()
    {
        currentPlayerMode = PlayerMode.NoPlayer;

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] No player mode (demo) triggered");

        BroadcastPlayerState("no_player");
    }

    /// <summary>
    /// Trigger song complete, transition to results
    /// </summary>
    public void TriggerSongComplete()
    {
        if (!showFlowEnabled)
        {
            // Show flow disabled, just stop playback
            ChartEditor.Instance.Stop();
            return;
        }

        ChartEditor editor = ChartEditor.Instance;

        // Only trigger if currently playing
        if (editor.currentState != ChartEditor.State.Playing)
        {
            if (debugShowFlow)
                Debug.LogWarning("[ShowFlowManager] Song complete triggered but not in Playing state");
            return;
        }

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Song complete triggered");

        // Get stats from GameplayStateSystem
        var gameplaySystem = GetCurrentGameplaySystem();
        if (gameplaySystem != null && gameplaySystem.currentRulestate != null)
        {
            lastSongStats = gameplaySystem.currentRulestate.stats;
            hasLastSongStats = true;

            // Transition to Results state
            TransitionToResults();
        }
        else
        {
            Debug.LogError("[ShowFlowManager] Cannot get gameplay stats for results screen");
            editor.Stop(); // Fallback to normal stop
        }
    }

    /// <summary>
    /// Trigger set end screen
    /// </summary>
    public void TriggerSetEnd()
    {
        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Set end triggered");

        ChartEditor.Instance.ChangeState(ChartEditor.State.SetEnd, new SetEndState());
        BroadcastGameState("set_end");
    }

    /// <summary>
    /// Trigger show end screen
    /// </summary>
    public void TriggerShowEnd()
    {
        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Show end triggered");

        ChartEditor.Instance.ChangeState(ChartEditor.State.ShowEnd, new ShowEndState());
        BroadcastGameState("show_end");
    }

    // ===== TRANSITION METHODS =====

    /// <summary>
    /// Transition to Results state
    /// </summary>
    public void TransitionToResults()
    {
        if (!hasLastSongStats)
        {
            Debug.LogError("[ShowFlowManager] No stats available for results screen");
            return;
        }

        ChartEditor.Instance.ChangeState(ChartEditor.State.Results, new ResultsState(lastSongStats));

        BroadcastGameState("results");

        if (debugShowFlow)
            Debug.Log($"[ShowFlowManager] Transitioned to Results. Stats: {lastSongStats.notesHit}/{lastSongStats.totalNotes}, Streak: {lastSongStats.noteStreak}");
    }

    /// <summary>
    /// Transition to Waiting For Band state
    /// </summary>
    public void TransitionToWaitingForBand(bool playerReady)
    {
        isPlayerReady = playerReady;
        isBandReady = false;

        ChartEditor.Instance.ChangeState(ChartEditor.State.WaitingForBand, new WaitingForBandState(playerReady));

        BroadcastGameState("waiting_band");

        if (debugShowFlow)
            Debug.Log($"[ShowFlowManager] Transitioned to Waiting For Band. Player ready: {playerReady}");
    }

    /// <summary>
    /// Transition to Waiting For Swap state
    /// </summary>
    public void TransitionToWaitingForSwap()
    {
        isPlayerReady = false;
        isBandReady = false;

        ChartEditor.Instance.ChangeState(ChartEditor.State.WaitingForSwap, new WaitingForSwapState());

        BroadcastGameState("waiting_swap");

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Transitioned to Waiting For Swap");
    }

    // ===== QUERY METHODS =====

    /// <summary>
    /// Get the name of the next song in the setlist
    /// </summary>
    public string GetNextSongName()
    {
        if (MoonscraperChartEditor.Song.SongMappingManager.Instance == null)
            return null;

        return MoonscraperChartEditor.Song.SongMappingManager.Instance.GetNextSongName();
    }

    /// <summary>
    /// Should we show the next song name on results screen?
    /// Checks global setting and per-song override
    /// </summary>
    public bool ShouldShowNextSong()
    {
        // Global master kill switch
        if (!showNextSongName)
            return false;

        // TODO: Check per-song showNextSong setting from SongMappingManager (Phase 6)
        // For now, respect global setting only
        return true;
    }

    /// <summary>
    /// Get last song stats
    /// </summary>
    public BaseGameplayRulestate.NoteStats GetLastSongStats()
    {
        return lastSongStats;
    }

    /// <summary>
    /// Check if we have stats from last song
    /// </summary>
    public bool HasLastSongStats()
    {
        return hasLastSongStats;
    }

    /// <summary>
    /// Reset player/band ready states (called when starting new song)
    /// </summary>
    public void ResetReadyStates()
    {
        isPlayerReady = false;
        isBandReady = false;
        currentPlayerMode = PlayerMode.Continuing; // Reset to default

        if (debugShowFlow)
            Debug.Log("[ShowFlowManager] Ready states reset");
    }

    // ===== OSC BROADCAST METHODS =====

    /// <summary>
    /// Broadcast current game state via OSC
    /// </summary>
    public void BroadcastGameState(string stateName)
    {
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.SendOscMessage("/game/state", stateName);

            if (debugShowFlow)
                Debug.Log($"[ShowFlowManager] Broadcasted game state: {stateName}");
        }
    }

    /// <summary>
    /// Broadcast player state via OSC
    /// </summary>
    public void BroadcastPlayerState(string stateName)
    {
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.SendOscMessage("/player/state", stateName);

            if (debugShowFlow)
                Debug.Log($"[ShowFlowManager] Broadcasted player state: {stateName}");
        }
    }

    /// <summary>
    /// Broadcast band state via OSC
    /// </summary>
    public void BroadcastBandState(string stateName)
    {
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.SendOscMessage("/band/state", stateName);

            if (debugShowFlow)
                Debug.Log($"[ShowFlowManager] Broadcasted band state: {stateName}");
        }
    }

    /// <summary>
    /// Broadcast song stats via OSC
    /// </summary>
    public void BroadcastSongStats(BaseGameplayRulestate.NoteStats stats)
    {
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.SendOscMessage("/song/stats", 
                (int)stats.notesHit, 
                (int)stats.totalNotes, 
                (int)stats.noteStreak);

            if (debugShowFlow)
                Debug.Log($"[ShowFlowManager] Broadcasted song stats: {stats.notesHit}/{stats.totalNotes}, Streak: {stats.noteStreak}");
        }
    }

    // ===== HELPER METHODS =====

    /// <summary>
    /// Get current GameplayStateSystem instance
    /// </summary>
    GameplayStateSystem GetCurrentGameplaySystem()
    {
        return GameplayStateSystem.Instance;
    }
}
