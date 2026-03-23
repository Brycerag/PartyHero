// Copyright (c) 2024 PartyHero
// Show end UI system

using UnityEngine;

/// <summary>
/// UI System for the show end screen.
/// Phase 5: Console-based stub implementation with development helper keys.
/// Phase 7: Replace with proper Unity Canvas UI with credits, thank you message, etc.
/// </summary>
public class ShowEndUISystem : SystemManagerState.System
{
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 5f; // Update display every 5 seconds
    private float displayStartTime;

    public override void SystemEnter()
    {
        Debug.Log("[ShowEndUI] Show end screen activated");
        displayStartTime = Time.time;
        DisplayShowEndScreen();
    }

    public override void SystemUpdate()
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

    public override void SystemExit()
    {
        Debug.Log("[ShowEndUI] Show end screen deactivated");
    }

    private void DisplayShowEndScreen()
    {
        Debug.Log("=============================================");
        Debug.Log("          SHOW COMPLETE!");
        Debug.Log("=============================================");
        Debug.Log("");
        Debug.Log("  Thank you for an amazing performance!");
        Debug.Log("");
        Debug.Log("  Hope to see you again soon!");
        Debug.Log("");
        Debug.Log("  (Development: Press ESC to return to editor)");
        Debug.Log("");
        Debug.Log("=============================================");
    }

    private void UpdateDisplay()
    {
        float elapsedTime = Time.time - displayStartTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        Debug.Log($"[ShowEndUI] Show ended {minutes:00}:{seconds:00} ago");
    }

    /// <summary>
    /// Development helper keys for testing without external triggers.
    /// </summary>
    private void HandleDevelopmentInput()
    {
        var showEndState = GetComponentInParent<ShowEndState>();
        if (showEndState == null)
            return;

        // ESC key: Exit show and return to editor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[ShowEndUI Dev] ESC key pressed - exiting show");
            showEndState.ExitShow();
        }
    }

    /// <summary>
    /// Helper to get the parent state (ShowEndState) from the system
    /// </summary>
    private ShowEndState GetComponentInParent()
    {
        var editor = ChartEditor.Instance;
        if (editor != null && editor.currentState == ChartEditor.State.ShowEnd)
        {
            var currentSystemState = editor.currentSystemManagerState;
            return currentSystemState as ShowEndState;
        }
        return null;
    }
}
