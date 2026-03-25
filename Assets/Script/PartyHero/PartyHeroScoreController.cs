using UnityEngine;
using YARG.Core.Logging;

namespace YARG.PartyHero
{
    /// <summary>
    /// Controller component for the Score scene that manages PartyHero show flow
    /// Add this to the Score scene and wire up ShowFlowUIManager
    /// </summary>
    public class PartyHeroScoreController : MonoBehaviour
    {
        [SerializeField]
        private UI.ShowFlowUIManager _uiManager;

        [SerializeField]
        private Canvas _regularScoreCanvas;

        private ShowFlowStateMachine _stateMachine;
        private bool _initialized = false;
        private bool _showingFlowState = false;

        private void Start()
        {
            // Check if PartyHero mode is active
            if (!GlobalVariables.State.IsPartyHeroMode)
            {
                // Not in PartyHero mode, disable this controller
                gameObject.SetActive(false);
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            if (_initialized) return;

            YargLogger.LogInfo("[PartyHero] Initializing Score Controller");

            // Get or create state machine
            _stateMachine = GetComponent<ShowFlowStateMachine>();
            if (_stateMachine == null)
            {
                _stateMachine = gameObject.AddComponent<ShowFlowStateMachine>();
            }

            // Initialize with current PartyHero state
            var partyHeroState = GlobalVariables.State.PartyHero;
            if (partyHeroState == null)
            {
                YargLogger.LogError("[PartyHero] PartyHeroState is null!");
                return;
            }

            _stateMachine.Initialize(partyHeroState, _uiManager);

            _initialized = true;

            // Determine which state to enter based on show progress
            DetermineAndEnterNextState();
        }

        private void DetermineAndEnterNextState()
        {
            var partyHeroState = GlobalVariables.State.PartyHero;

            // Check if this is the last song
            if (partyHeroState.IsLastSongInShow())
            {
                EnterShowFlowState(ShowFlowStateType.ShowEnd);
                return;
            }

            // Check if player swap is next
            if (partyHeroState.ShouldEnterPlayerSwap())
            {
                EnterShowFlowState(ShowFlowStateType.WaitingForSwap);
                return;
            }

            // Check if set break is next
            if (partyHeroState.ShouldEnterSetBreak())
            {
                EnterShowFlowState(ShowFlowStateType.SetEnd);
                return;
            }

            // Default: waiting for band
            EnterShowFlowState(ShowFlowStateType.WaitingForBand);
        }

        private void EnterShowFlowState(ShowFlowStateType stateType)
        {
            YargLogger.LogInfo($"[PartyHero] Entering show flow state: {stateType}");

            // Hide regular score screen
            if (_regularScoreCanvas != null)
            {
                _regularScoreCanvas.gameObject.SetActive(false);
            }

            // Enter the show flow state
            _stateMachine.ChangeState(stateType);
            _showingFlowState = true;
        }

        /// <summary>
        /// Call this to show the regular score screen (e.g., if transitioning out of PartyHero mode)
        /// </summary>
        public void ShowRegularScoreScreen()
        {
            if (_regularScoreCanvas != null)
            {
                _regularScoreCanvas.gameObject.SetActive(true);
            }

            if (_uiManager != null)
            {
                _uiManager.HideAllScreens();
            }

            _showingFlowState = false;
        }
    }
}
