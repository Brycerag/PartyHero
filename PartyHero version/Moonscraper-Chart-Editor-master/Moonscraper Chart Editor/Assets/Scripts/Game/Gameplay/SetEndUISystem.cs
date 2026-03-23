// Copyright (c) 2024 PartyHero
// Set end UI system

using UnityEngine;

/// <summary>
/// UI System for the set end screen.
/// Phase 5: Console-based stub implementation with development helper keys.
/// Phase 7: Replace with proper Unity Canvas UI with buttons and TextMeshPro.
/// </summary>
public class SetEndUISystem : SystemManagerState.System
{
    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 2f; // Update display every 2 seconds

    public override void SystemEnter()
    {
        Debug.Log("[SetEndUI] Set end screen activated");
        DisplaySetEndScreen();
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
        Debug.Log("[SetEndUI] Set end screen deactivated");
    }

    private void DisplaySetEndScreen()
    {
        Debug.Log("=============================================");
        Debug.Log("            SET BREAK");
        Debug.Log("=============================================");
        Debug.Log("");
        Debug.Log("  Take a breather! Next set coming up...");
        Debug.Log("");
        Debug.Log("  Press trigger to resume or end show");
        Debug.Log("  (Development: R = Resume, E = End Show)");
        Debug.Log("");
        Debug.Log("=============================================");
    }

    private void UpdateDisplay()
    {
        // Just log that we're still in set break
        Debug.Log("[SetEndUI] Still in set break...");
    }

    /// <summary>
    /// Development helper keys for testing without external MIDI/OSC triggers.
    /// </summary>
    private void HandleDevelopmentInput()
    {
        var setEndState = GetComponentInParent<SetEndState>();
        if (setEndState == null)
            return;

        // R key: Resume show (start next set)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[SetEndUI Dev] R key pressed - resuming show");
            setEndState.ResumeShow();
        }

        // E key: End show (go to show end screen)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[SetEndUI Dev] E key pressed - ending show");
            setEndState.EndShow();
        }
    }

    /// <summary>
    /// Helper to get the parent state (SetEndState) from the system
    /// </summary>
    private SetEndState GetComponentInParent()
    {
        // In the SystemManagerState architecture, systems don't have direct reference to their parent state
        // We'll need to access it through ChartEditor
        var editor = ChartEditor.Instance;
        if (editor != null && editor.currentState == ChartEditor.State.SetEnd)
        {
            // Access the state instance through reflection or a stored reference
            // For now, we'll use a simpler approach via the manager's current state
            var currentSystemState = editor.currentSystemManagerState;
            return currentSystemState as SetEndState;
        }
        return null;
    }
}
