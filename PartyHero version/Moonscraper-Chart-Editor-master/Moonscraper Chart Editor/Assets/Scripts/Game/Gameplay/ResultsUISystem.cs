// Copyright (c) 2024 PartyHero
// Results UI system for displaying post-song statistics

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// System that displays the results UI after a song ends.
/// Shows stats (hit %, streak, notes hit/total) and optionally next song name.
/// Handles Continue button input to advance to next state.
/// 
/// TODO Phase 1: This is a stub implementation using debug console.
/// TODO Phase 2: Create proper Unity UI with Canvas, Text elements, and Button.
/// </summary>
public class ResultsUISystem : SystemManagerState.System
{
    BaseGameplayRulestate.NoteStats stats;
    string nextSongName;
    bool showNextSong;

    // UI state
    float displayStartTime;
    bool continuePressed = false;

    public ResultsUISystem(BaseGameplayRulestate.NoteStats stats, string nextSongName, bool showNextSong)
    {
        this.stats = stats;
        this.nextSongName = nextSongName;
        this.showNextSong = showNextSong;
    }

    public override void SystemEnter()
    {
        displayStartTime = Time.time;
        continuePressed = false;

        DisplayStats();
        
        // TODO Phase 2: Load and show Unity UI canvas with results
    }

    void DisplayStats()
    {
        float hitPercent = (stats.totalNotes > 0) ? ((float)stats.notesHit / stats.totalNotes) * 100f : 0f;

        Debug.Log("============================");
        Debug.Log("       SONG COMPLETE");
        Debug.Log("============================");
        Debug.Log($"Hit: {hitPercent:F1}%");
        Debug.Log($"Best Streak: {stats.noteStreak}");
        Debug.Log($"Notes Hit: {stats.notesHit} / {stats.totalNotes}");
        
        if (showNextSong && !string.IsNullOrEmpty(nextSongName))
        {
            Debug.Log($"Next: {nextSongName}");
        }
        
        Debug.Log("============================");
        Debug.Log("Press SPACE to continue...");
        Debug.Log("============================");
    }

    public override void SystemUpdate()
    {
        // Check for continue input (Space key for now)
        // TODO Phase 2: Replace with actual UI button
        if (!continuePressed && Input.GetKeyDown(KeyCode.Space))
        {
            continuePressed = true;
            OnContinuePressed();
        }
    }

    void OnContinuePressed()
    {
        if (ShowFlowManager.Instance == null || !ShowFlowManager.Instance.showFlowEnabled)
        {
            // Show flow disabled, return to editor
            ChartEditor.Instance.Stop();
            return;
        }

        if (ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[ResultsUISystem] Continue pressed");
        }

        // Determine next state based on PlayerMode
        var flowManager = ShowFlowManager.Instance;

        switch (flowManager.currentPlayerMode)
        {
            case ShowFlowManager.PlayerMode.Swapping:
                // Go to player swap screen
                flowManager.TransitionToWaitingForSwap();
                break;

            case ShowFlowManager.PlayerMode.NoPlayer:
                // Start next song in demo mode (bot-enabled)
                StartNextSongDemoMode();
                break;

            case ShowFlowManager.PlayerMode.Continuing:
            default:
                // Player continues, go to waiting for band screen
                flowManager.TransitionToWaitingForBand(playerReady: true);
                break;
        }
    }

    void StartNextSongDemoMode()
    {
        if (ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[ResultsUISystem] Starting next song in demo mode (bot-enabled)");
        }

        // Show brief demo mode transition
        Debug.Log("============================");
        Debug.Log("  ENTERING DEMO MODE");
        Debug.Log("============================");

        // Try to load next song from SongMappingManager
        if (MoonscraperChartEditor.Song.SongMappingManager.Instance != null)
        {
            bool loaded = MoonscraperChartEditor.Song.SongMappingManager.Instance.LoadNextSong();
            if (loaded)
            {
                // Song will be loaded, wait for load completion before playing
                // TODO: Hook into song loaded event and then call Play(enableBot: true)
                ShowFlowManager.Instance.BroadcastGameState("demo_mode");
                return;
            }
        }

        // Fallback: No next song - restart current song in bot mode
        Debug.LogWarning("[ResultsUISystem] No next song in setlist, restarting current song in demo mode");
        ChartEditor editor = ChartEditor.Instance;
        editor.Play(enableBot: true);

        ShowFlowManager.Instance.BroadcastGameState("demo_mode");
    }

    public override void SystemExit()
    {
        // TODO Phase 2: Hide and cleanup Unity UI
    }
}
