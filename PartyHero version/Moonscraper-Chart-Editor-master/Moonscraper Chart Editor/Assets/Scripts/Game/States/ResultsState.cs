// Copyright (c) 2024 PartyHero
// Results screen state for post-song statistics display

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// State that displays results after a song completes.
/// Shows stats (hit %, streak, notes hit), optionally shows next song name,
/// and provides Continue button to advance to next state.
/// </summary>
public class ResultsState : SystemManagerState
{
    BaseGameplayRulestate.NoteStats stats;
    string nextSongName;
    bool showNextSong;

    public ResultsState(BaseGameplayRulestate.NoteStats stats)
    {
        this.stats = stats;
        DetermineNextSongDisplay();

        // Add systems
        AddSystem(new ResultsUISystem(stats, nextSongName, showNextSong));
        // TODO Phase 1: Add NextSongPreloader system when implementing preloading
    }

    void DetermineNextSongDisplay()
    {
        if (!ShowFlowManager.Instance || !ShowFlowManager.Instance.showFlowEnabled)
        {
            showNextSong = false;
            nextSongName = null;
            return;
        }

        // Check global setting + per-song AbleSet data
        showNextSong = ShowFlowManager.Instance.ShouldShowNextSong();
        
        if (showNextSong)
        {
            nextSongName = ShowFlowManager.Instance.GetNextSongName();
            
            // If we couldn't get a next song name, don't show it
            if (string.IsNullOrEmpty(nextSongName))
            {
                showNextSong = false;
            }
        }
        else
        {
            nextSongName = null;
        }
    }

    public override void Enter()
    {
        base.Enter();

        if (ShowFlowManager.Instance)
        {
            ShowFlowManager.Instance.BroadcastGameState("results");
            ShowFlowManager.Instance.BroadcastSongStats(stats);

            if (ShowFlowManager.Instance.debugShowFlow)
            {
                Debug.Log($"[ResultsState] Entered. Stats: {stats.notesHit}/{stats.totalNotes}, Streak: {stats.noteStreak}");
            }
        }
    }

    public override void Update()
    {
        base.Update();

        // Check for "Continue" button click (handled in ResultsUISystem)
        // State transitions are triggered via ShowFlowManager methods
    }

    public override void Exit()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[ResultsState] Exited");
        }

        base.Exit();
    }
}
