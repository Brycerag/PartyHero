using UnityEngine;

/// <summary>
/// UI System for the player swap waiting screen.
/// Phase 3: Console-based stub implementation with development helper keys.
/// Phase 7: Replace with proper Unity Canvas UI with buttons and TextMeshPro.
/// </summary>
public class WaitingForSwapUISystem
{
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.5f; // Update display twice per second

    public void OnEnter()
    {
        Debug.Log("[WaitingForSwapUI] Player swap screen activated");
        DisplaySwapScreen();
    }

    public void Update()
    {
        // Handle development helper keys
        HandleDevelopmentInput();

        // Periodically refresh display
        if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }

    public void OnExit()
    {
        Debug.Log("[WaitingForSwapUI] Player swap screen deactivated");
    }

    private void DisplaySwapScreen()
    {
        Debug.Log("=============================================");
        Debug.Log("         PLAYER SWAP TIME");
        Debug.Log("=============================================");
        Debug.Log("");
        Debug.Log("  Please swap to the next player position");
        Debug.Log("");
        Debug.Log("  Press the player ready trigger when ready");
        Debug.Log("  (Development: Press R to simulate)");
        Debug.Log("");
        Debug.Log("=============================================");
    }

    private void UpdateDisplay()
    {
        string playerStatus = GetPlayerReadyStatus();
        Debug.Log($"[SwapUI Update] Player Status: {playerStatus}");
    }

    private string GetPlayerReadyStatus()
    {
        if (ShowFlowManager.Instance == null)
            return "○ Unknown (ShowFlowManager not found)";

        return ShowFlowManager.Instance.isPlayerReady ? "✓ Ready" : "○ Swapping...";
    }

    /// <summary>
    /// Development helper keys for testing without external MIDI/OSC triggers.
    /// Phase 6: These will be supplemented (not replaced) by actual MIDI input.
    /// </summary>
    private void HandleDevelopmentInput()
    {
        if (ShowFlowManager.Instance == null)
            return;

        // R key: Simulate player ready after swap
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[SwapUI Dev] R key pressed - simulating player ready");
            ShowFlowManager.Instance.TriggerPlayerReady();
        }
    }
}
