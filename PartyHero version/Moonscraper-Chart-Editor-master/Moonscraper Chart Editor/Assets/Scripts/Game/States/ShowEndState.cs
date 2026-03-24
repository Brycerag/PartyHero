// Copyright (c) 2024 PartyHero
// Show end state - displayed at end of show

using UnityEngine;
using MoonscraperEngine;

/// <summary>
/// State that displays at the end of the entire show.
/// Shows "Show End" UI (credits, thank you message, etc).
/// Can only exit by manually stopping or returning to editor.
/// </summary>
public class ShowEndState : SystemManagerState
{
    private ShowEndUISystem uiSystem;

    public ShowEndState()
    {
        uiSystem = new ShowEndUISystem();
        AddSystem(uiSystem);
    }

    public override void Enter()
    {
        base.Enter();

        if (ShowFlowManager.Instance)
        {
            ShowFlowManager.Instance.BroadcastGameState("show_end");

            if (ShowFlowManager.Instance.debugShowFlow)
            {
                Debug.Log("[ShowEndState] Entered show end screen");
            }
        }
    }

    public override void Update()
    {
        base.Update();

        // Show end state typically stays until manual editor return
        // No automatic transitions from here
    }

    /// <summary>
    /// Exit show and return to editor
    /// </summary>
    public void ExitShow()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[ShowEndState] Exiting show");
        }

        ChartEditor.Instance.ChangeStateToEditor();
    }

    public override void Exit()
    {
        if (ShowFlowManager.Instance && ShowFlowManager.Instance.debugShowFlow)
        {
            Debug.Log("[ShowEndState] Exiting show end screen");
        }

        base.Exit();
    }
}
