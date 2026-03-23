// Copyright (c) 2024 PartyHero
// Set end state - displayed between sets

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// State that displays between sets in the show.
/// Shows "Set End" UI and waits for manual resume trigger.
/// Can also transition to Show End state from here.
/// </summary>
public class SetEndState : SystemManagerState
{
    private SetEndUISystem uiSystem;
    private bool resumeTriggered = false;

    public SetEndState()
    {
        uiSystem = new SetEndUISystem();
        AddSystem(uiSystem);
    }

    public override void Enter()
    {
        base.Enter();

        if (ShowFlowManager.Instance)
        {
            ShowFlowManager.Instance.BroadcastGameState("set_end");

            if (ShowFlowManager.Instance.debugShowFlow)
            {
                Debug.Log("[SetEndState] Entered set end screen");
            }
        }

        resumeTriggered = false;
    }

    public override void Update()
    {
        base.Update();

        // Check for resume trigger (set via ShowFlowManager from OSC/MIDI)
        // TODO: Add resume trigger mechanism
        // For now, allow manual resume via R key (handled in UI system)
    }

    /// <summary>
    /// Resume show after set break
    /// </summary>
    public void ResumeShow()
    {
        if (resumeTriggered)
            return;

        resumeTriggered = true;

        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[SetEndState] Resuming show");
        }

        // Try to load next song from SongMappingManager (first song of next set)
        if (MoonscraperChartEditor.Song.SongMappingManager.Instance != null)
        {
            bool loaded = MoonscraperChartEditor.Song.SongMappingManager.Instance.LoadNextSong();
            if (loaded)
            {
                // Song will be loaded automatically
                return;
            }
        }

        // Fallback: Return to editor if no next song
        Debug.LogWarning("[SetEndState] No next song in setlist, returning to editor");
        ChartEditor.Instance.ChangeState(ChartEditor.State.Editor);
    }

    /// <summary>
    /// End entire show
    /// </summary>
    public void EndShow()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[SetEndState] Ending show");
        }

        // Transition to Show End state
        ChartEditor.Instance.ChangeState(ChartEditor.State.ShowEnd, new ShowEndState());
    }

    public override void Exit()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[SetEndState] Exiting set end screen");
        }

        base.Exit();
    }
}
