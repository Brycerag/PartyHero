using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YARG.Core.Logging;

namespace YARG.PartyHero.UI
{
    /// <summary>
    /// Manages the show flow UI overlay in the Score scene
    /// Shows waiting screens, set breaks, and show end
    /// </summary>
    public class ShowFlowUIManager : MonoBehaviour
    {
        [Header("UI Canvases")]
        [SerializeField]
        private Canvas _waitingForBandCanvas;
        [SerializeField]
        private Canvas _waitingForSwapCanvas;
        [SerializeField]
        private Canvas _setEndCanvas;
        [SerializeField]
        private Canvas _showEndCanvas;

        [Header("Waiting For Band UI")]
        [SerializeField]
        private TextMeshProUGUI _bandWaitingTitle;
        [SerializeField]
        private TextMeshProUGUI _bandWaitingPlayerStatus;
        [SerializeField]
        private TextMeshProUGUI _bandWaitingBandStatus;
        [SerializeField]
        private TextMeshProUGUI _bandWaitingNextSong;
        [SerializeField]
        private TextMeshProUGUI _bandWaitingInstructions;

        [Header("Waiting For Swap UI")]
        [SerializeField]
        private TextMeshProUGUI _swapTitle;
        [SerializeField]
        private TextMeshProUGUI _swapMessage;
        [SerializeField]
        private TextMeshProUGUI _swapTimer;
        [SerializeField]
        private TextMeshProUGUI _swapInstructions;
        [SerializeField]
        private TextMeshProUGUI _swapNextSong;

        [Header("Set End UI")]
        [SerializeField]
        private TextMeshProUGUI _setEndTitle;
        [SerializeField]
        private TextMeshProUGUI _setEndMessage;
        [SerializeField]
        private TextMeshProUGUI _setEndTimer;
        [SerializeField]
        private TextMeshProUGUI _setEndInstructions;

        [Header("Show End UI")]
        [SerializeField]
        private TextMeshProUGUI _showEndTitle;
        [SerializeField]
        private TextMeshProUGUI _showEndMessage;
        [SerializeField]
        private TextMeshProUGUI _showEndStats;
        [SerializeField]
        private TextMeshProUGUI _showEndInstructions;

        private ShowFlowStateMachine _stateMachine;

        private void Awake()
        {
            // Hide all canvases by default
            HideAllScreens();
        }

        public void Initialize(ShowFlowStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void HideAllScreens()
        {
            if (_waitingForBandCanvas != null) _waitingForBandCanvas.gameObject.SetActive(false);
            if (_waitingForSwapCanvas != null) _waitingForSwapCanvas.gameObject.SetActive(false);
            if (_setEndCanvas != null) _setEndCanvas.gameObject.SetActive(false);
            if (_showEndCanvas != null) _showEndCanvas.gameObject.SetActive(false);
        }

        public void ShowWaitingForBand(bool playerReady, bool bandReady, string nextSong)
        {
            HideAllScreens();
            
            if (_waitingForBandCanvas == null)
            {
                YargLogger.LogWarning("[PartyHero] WaitingForBand canvas not assigned!");
                return;
            }

            _waitingForBandCanvas.gameObject.SetActive(true);

            if (_bandWaitingTitle != null)
                _bandWaitingTitle.text = "WAITING FOR BAND";

            if (_bandWaitingPlayerStatus != null)
                _bandWaitingPlayerStatus.text = $"Player Ready: {(playerReady ? "✓" : "○")}";

            if (_bandWaitingBandStatus != null)
                _bandWaitingBandStatus.text = $"Band Ready: {(bandReady ? "✓" : "○")}";

            if (_bandWaitingNextSong != null)
                _bandWaitingNextSong.text = $"Next: {nextSong}";

            if (_bandWaitingInstructions != null)
                _bandWaitingInstructions.text = "Press SPACE when ready";
        }

        public void ShowWaitingForSwap(string message, float elapsedTime, float minimumTime, string nextSong)
        {
            HideAllScreens();
            
            if (_waitingForSwapCanvas == null)
            {
                YargLogger.LogWarning("[PartyHero] WaitingForSwap canvas not assigned!");
                return;
            }

            _waitingForSwapCanvas.gameObject.SetActive(true);

            if (_swapTitle != null)
                _swapTitle.text = "PLAYER SWAP TIME";

            if (_swapMessage != null)
                _swapMessage.text = message;

            bool canContinue = elapsedTime >= minimumTime;
            if (_swapTimer != null)
            {
                if (canContinue)
                {
                    _swapTimer.text = "Ready to Continue!";
                }
                else
                {
                    float remaining = minimumTime - elapsedTime;
                    _swapTimer.text = $"Time Remaining: {remaining:F1}s";
                }
            }

            if (_swapInstructions != null)
            {
                _swapInstructions.text = canContinue 
                    ? "Press SPACE when new player ready" 
                    : "Please wait...";
            }

            if (_swapNextSong != null)
                _swapNextSong.text = $"Next: {nextSong}";
        }

        public void ShowSetEnd(string message, float elapsedTime)
        {
            HideAllScreens();
            
            if (_setEndCanvas == null)
            {
                YargLogger.LogWarning("[PartyHero] SetEnd canvas not assigned!");
                return;
            }

            _setEndCanvas.gameObject.SetActive(true);

            if (_setEndTitle != null)
                _setEndTitle.text = "SET BREAK";

            if (_setEndMessage != null)
                _setEndMessage.text = message;

            if (_setEndTimer != null)
            {
                int minutes = (int)(elapsedTime / 60);
                int seconds = (int)(elapsedTime % 60);
                _setEndTimer.text = $"Elapsed: {minutes}:{seconds:D2}";
            }

            if (_setEndInstructions != null)
                _setEndInstructions.text = "Press R to resume show\nPress ESC to end show";
        }

        public void ShowShowEnd(string showName, string message, int totalSongs, int durationMinutes)
        {
            HideAllScreens();
            
            if (_showEndCanvas == null)
            {
                YargLogger.LogWarning("[PartyHero] ShowEnd canvas not assigned!");
                return;
            }

            _showEndCanvas.gameObject.SetActive(true);

            if (_showEndTitle != null)
                _showEndTitle.text = "SHOW COMPLETE!";

            if (_showEndMessage != null)
                _showEndMessage.text = message;

            if (_showEndStats != null)
            {
                _showEndStats.text = $"Show: {showName}\n" +
                                    $"Total Songs: {totalSongs}\n" +
                                    $"Duration: {durationMinutes} minutes";
            }

            if (_showEndInstructions != null)
                _showEndInstructions.text = "Press ESC to return to menu";
        }
    }
}
