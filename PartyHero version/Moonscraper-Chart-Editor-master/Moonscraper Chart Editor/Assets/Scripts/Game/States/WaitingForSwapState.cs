using UnityEngine;

public class WaitingForSwapState : SystemManagerState
{
    private WaitingForSwapUISystem uiSystem;
    private bool swapComplete = false;

    public WaitingForSwapState()
    {
        uiSystem = new WaitingForSwapUISystem();
    }

    public override void Enter()
    {
        Debug.Log("[ShowFlow] Entering WaitingForSwapState");
        uiSystem.OnEnter();
        swapComplete = false;

        // Reset player ready flag (done in ShowFlowManager.TransitionToWaitingForSwap)
        // Broadcast player swap state via OSC
        ShowFlowManager.Instance.BroadcastPlayerState("swapping");
    }

    public override void Update()
    {
        uiSystem.Update();

        // Check if player is ready after swap
        if (!swapComplete && ShowFlowManager.Instance.isPlayerReady)
        {
            Debug.Log("[ShowFlow] Player swap complete, player is ready");
            swapComplete = true;
            ProceedAfterSwap();
        }
    }

    public override void Exit()
    {
        Debug.Log("[ShowFlow] Exiting WaitingForSwapState");
        uiSystem.OnExit();
    }

    private void ProceedAfterSwap()
    {
        // After swap, check if we need to wait for band or go directly to next song
        if (ShowFlowManager.Instance.requireBandReady)
        {
            ShowFlowManager.Instance.TransitionToWaitingForBand();
        }
        else
        {
            StartNextSong();
        }
    }

    private void StartNextSong()
    {
        // Phase 7: Load next song from SongMappingManager
        Debug.Log("[ShowFlow] Starting next song");
        ShowFlowManager.Instance.BroadcastPlayerState("ready");

        // Check if demo mode should be enabled
        bool enableBot = (ShowFlowManager.Instance && ShowFlowManager.Instance.currentPlayerMode == ShowFlowManager.PlayerMode.NoPlayer);

        // Try to load next song from SongMappingManager
        if (MoonscraperChartEditor.Song.SongMappingManager.Instance != null)
        {
            bool loaded = MoonscraperChartEditor.Song.SongMappingManager.Instance.LoadNextSong();
            if (loaded)
            {
                // Song will be loaded, wait for load completion before playing
                // TODO: Hook into song loaded event and then call Play(enableBot)
                return;
            }
        }

        // Fallback: No next song - restart current song
        Debug.LogWarning("[WaitingForSwapState] No next song in setlist, restarting current song");
        ChartEditor.Instance.Play(enableBot: enableBot);
    }
}
