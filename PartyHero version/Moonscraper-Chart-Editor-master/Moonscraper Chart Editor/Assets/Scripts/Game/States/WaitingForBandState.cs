// Copyright (c) 2024 PartyHero
// Waiting for band ready state

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// State that waits for the band to be ready before starting the next song.
/// Shows "Waiting for Band" UI with ready indicators.
/// Player is already marked ready, waiting for band ready trigger.
/// </summary>
public class WaitingForBandState : SystemManagerState
{
    bool playerReady;

    public WaitingForBandState(bool playerReady)
    {
        this.playerReady = playerReady;

        // Add systems
        AddSystem(new WaitingForBandUISystem(playerReady));
    }

    public override void Enter()
    {
        base.Enter();

        if (ShowFlowManager.Instance)
        {
            ShowFlowManager.Instance.isPlayerReady = playerReady;
            ShowFlowManager.Instance.isBandReady = false;
            ShowFlowManager.Instance.BroadcastGameState("waiting_band");

            if (ShowFlowManager.Instance.debugShowFlow)
            {
                Debug.Log($"[WaitingForBandState] Entered. Player ready: {playerReady}");
            }
        }
    }

    public override void Update()
    {
        base.Update();

        // Check if band is ready (updated by ShowFlowManager via triggers)
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.isBandReady)
        {
            StartNextSong();
        }
    }

    void StartNextSong()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[WaitingForBandState] Band ready, starting next song");
        }

        // Check if demo mode should be enabled
        bool enableBot = (ShowFlowManager.Instance && ShowFlowManager.Instance.currentPlayerMode == ShowFlowManager.PlayerMode.NoPlayer);

        // Try to load next song from SongMappingManager
        if (MoonscraperChartEditor.Song.SongMappingManager.Instance != null)
        {
            bool loaded = MoonscraperChartEditor.Song.SongMappingManager.Instance.LoadNextSong();
            if (loaded)
            {
                // Song will be loaded, but we need to wait for it to load before playing
                // For now, just let the song editor load it
                // TODO: Hook into song loaded event and then call Play(enableBot)
                return;
            }
        }

        // Fallback: No SongMappingManager or no next song - restart current song
        Debug.LogWarning("[WaitingForBandState] No next song in setlist, restarting current song");
        ChartEditor.Instance.Play(enableBot: enableBot);
    }

    public override void Exit()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[WaitingForBandState] Exited");
        }

        base.Exit();
    }
}
