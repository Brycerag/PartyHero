// Copyright (c) 2024 PartyHero
// Waiting for band UI system

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// System that displays the "Waiting for Band" UI.
/// Shows ready status for player and band, updates in real-time.
/// 
/// TODO Phase 2: This is a stub implementation using debug console.
/// TODO: Create proper Unity UI with Canvas, Text elements, and ready indicators.
/// </summary>
public class WaitingForBandUISystem : SystemManagerState.System
{
    bool playerReady;
    float lastUpdateTime;
    const float UPDATE_INTERVAL = 0.5f; // Update display every 0.5 seconds

    public WaitingForBandUISystem(bool playerReady)
    {
        this.playerReady = playerReady;
    }

    public override void SystemEnter()
    {
        lastUpdateTime = Time.time;
        DisplayWaitingScreen();
        
        // TODO: Load and show Unity UI canvas
    }

    void DisplayWaitingScreen()
    {
        Debug.Log("============================");
        Debug.Log("   WAITING FOR BAND");
        Debug.Log("============================");
        Debug.Log(GetPlayerReadyStatus());
        Debug.Log(GetBandReadyStatus());
        Debug.Log("============================");
    }

    string GetPlayerReadyStatus()
    {
        if (ShowFlowManager.Instance == null)
            return "Player: ?";

        return ShowFlowManager.Instance.isPlayerReady ? "✓ Player Ready" : "○ Player Not Ready";
    }

    string GetBandReadyStatus()
    {
        if (ShowFlowManager.Instance == null)
            return "Band: ?";

        return ShowFlowManager.Instance.isBandReady ? "✓ Band Ready" : "○ Band Not Ready";
    }

    public override void SystemUpdate()
    {
        // Update display periodically
        if (Time.time - lastUpdateTime > UPDATE_INTERVAL)
        {
            lastUpdateTime = Time.time;
            UpdateDisplay();
        }

        // Check for force player ready input (development helper)
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (ShowFlowManager.Instance != null)
            {
                ShowFlowManager.Instance.ForcePlayerReady();
                Debug.Log("[WaitingForBandUI] Player ready forced (P key)");
            }
        }

        // Check for band ready input (development helper)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (ShowFlowManager.Instance != null)
            {
                ShowFlowManager.Instance.TriggerBandReady();
                Debug.Log("[WaitingForBandUI] Band ready triggered (B key)");
            }
        }
    }

    void UpdateDisplay()
    {
        if (ShowFlowManager.Instance == null)
            return;

        // Update player ready status if changed
        if (ShowFlowManager.Instance.isPlayerReady != playerReady)
        {
            playerReady = ShowFlowManager.Instance.isPlayerReady;
            Debug.Log("Status Update: " + GetPlayerReadyStatus());
        }

        // TODO: Update Unity UI elements instead of console logs
    }

    public override void SystemExit()
    {
        Debug.Log("[WaitingForBandUI] Exiting waiting state");
        
        // TODO: Hide and cleanup Unity UI
    }
}
