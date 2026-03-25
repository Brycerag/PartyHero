using UnityEngine;
using UnityEngine.InputSystem;
using YARG.Core.Logging;
using YARG.PartyHero.UI;

namespace YARG.PartyHero
{
    /// <summary>
    /// Waiting for Band state - Coordination screen while band prepares for next song
    /// Both player and band must signal ready before continuing
    /// </summary>
    public class WaitingForBandState : BaseShowFlowState
    {
        private bool playerReady = false;
        private bool bandReady = false;

        public WaitingForBandState(PartyHeroState state, ShowFlowStateMachine machine, ShowFlowUIManager ui) 
            : base(state, machine, ui)
        {
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            playerReady = false;
            bandReady = false;

            // Log to console and update UI
            LogBanner();
            UpdateUI();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // SPACE = Toggle player ready
                if (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
                {
                    playerReady = !playerReady;
                    YargLogger.LogInfo($"[PartyHero] Player ready: {playerReady}");
                    LogBanner();
                    UpdateUI();
                }

                // B = Toggle band ready (simulates MIDI input)
                if (Keyboard.current?.bKey.wasPressedThisFrame ?? false)
                {
                    bandReady = !bandReady;
                    YargLogger.LogInfo($"[PartyHero] Band ready: {bandReady}");
                    LogBanner();
                    UpdateUI();
                }

                // R = Resume immediately (skip waiting)
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Forcing resume (dev override)");
                    ContinueToNextSong();
                    return;
                }
            }

            // Check if both ready
            if (playerReady && bandReady)
            {
                YargLogger.LogInfo("[PartyHero] Both player and band ready - continuing!");
                ContinueToNextSong();
            }
        }

        private void ContinueToNextSong()
        {
            stateMachine?.LoadNextSong();
        }

        public void SetPlayerReady(bool ready)
        {
            playerReady = ready;
            YargLogger.LogInfo($"[PartyHero] Player ready updated via external input: {ready}");
            LogBanner();
            UpdateUI();
            
            // Check if both ready after update
            if (playerReady && bandReady)
            {
                YargLogger.LogInfo("[PartyHero] Both player and band ready - continuing!");
                ContinueToNextSong();
            }
        }
        
        public void SetBandReady(bool ready)
        {
            bandReady = ready;
            YargLogger.LogInfo($"[PartyHero] Band ready updated via external input: {ready}");
            LogBanner();
            UpdateUI();
            
            // Check if both ready after update
            if (playerReady && bandReady)
            {
                YargLogger.LogInfo("[PartyHero] Both player and band ready - continuing!");
                ContinueToNextSong();
            }
        }

        private void UpdateUI()
        {
            if (uiManager == null) return;

            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";

            uiManager.ShowWaitingForBand(playerReady, bandReady, nextSongName);
        }

        private void LogBanner()
        {
            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("   WAITING FOR BAND");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo($"Player Ready: {(playerReady ? "✓" : "○")}");
            YargLogger.LogInfo($"Band Ready:   {(bandReady ? "✓" : "○")}");
            YargLogger.LogInfo($"Next Song: {nextSongName}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEYS: SPACE=Player Ready, B=Band Ready, R=Resume");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Waiting for Swap state - Player swap coordination
    /// Gives time for physical controller handoff between players
    /// </summary>
    public class WaitingForSwapState : BaseShowFlowState
    {
        private float swapTimer = 0f;
        private float minimumSwapTime;
        private string swapMessage;

        public WaitingForSwapState(PartyHeroState state, ShowFlowStateMachine machine, ShowFlowUIManager ui) 
            : base(state, machine, ui)
        {
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            swapTimer = 0f;
            
            // Get swap configuration from current song
            var currentSong = partyHeroState.GetCurrentSong();
            minimumSwapTime = currentSong?.minimumSwapTime ?? 10f;
            swapMessage = currentSong?.swapMessage ?? "Player Swap Time!";

            LogBanner();
            UpdateUI();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            
            swapTimer += Time.unscaledDeltaTime;

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // After minimum time, SPACE allows continue
                if (swapTimer >= minimumSwapTime)
                {
                    if (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
                    {
                        YargLogger.LogInfo("[PartyHero] Player swap complete - continuing!");
                        ContinueToNextSong();
                        return;
                    }
                }

                // R = Force resume immediately (dev override)
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Forcing swap complete (dev override)");
                    ContinueToNextSong();
                    return;
                }
            }

            // Update UI every frame (for timer)
            UpdateUI();

            // Update console log every second
            if ((int)swapTimer != (int)(swapTimer - Time.unscaledDeltaTime))
            {
                LogBanner();
            }
        }

        private void ContinueToNextSong()
        {
            stateMachine?.LoadNextSong();
        }

        private void UpdateUI()
        {
            if (uiManager == null) return;

            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";

            uiManager.ShowWaitingForSwap(swapMessage, swapTimer, minimumSwapTime, nextSongName);
        }

        private void LogBanner()
        {
            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";
            
            float timeRemaining = Mathf.Max(0, minimumSwapTime - swapTimer);
            bool canContinue = swapTimer >= minimumSwapTime;

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("    PLAYER SWAP TIME");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(swapMessage);
            YargLogger.LogInfo($"Time Elapsed: {swapTimer:F1}s");
            if (!canContinue)
            {
                YargLogger.LogInfo($"Minimum Time: {minimumSwapTime:F1}s");
                YargLogger.LogInfo($"Time Remaining: {timeRemaining:F1}s");
            }
            else
            {
                YargLogger.LogInfo("Press SPACE when new player ready");
            }
            YargLogger.LogInfo($"Next Song: {nextSongName}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode && !canContinue)
            {
                YargLogger.LogInfo("DEV KEY: R=Force Resume");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Set End state - Set break/intermission
    /// Formal break between sets with custom message
    /// </summary>
    public class SetEndState : BaseShowFlowState
    {
        private float breakStartTime;
        private string breakMessage;
        private int breakDuration;

        public SetEndState(PartyHeroState state, ShowFlowStateMachine machine, ShowFlowUIManager ui) 
            : base(state, machine, ui)
        {
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            breakStartTime = Time.unscaledTime;
            
            // Get break configuration from current set
            var currentSet = partyHeroState.GetCurrentSet();
            breakMessage = currentSet?.breakMessage ?? "Set Break - Back in 15 minutes!";
            breakDuration = currentSet?.breakDurationSeconds ?? 900;

            LogBanner();
            UpdateUI();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // R = Resume show
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Resuming show from set break");
                    ContinueToNextSong();
                    return;
                }

                // ESC = End show early
                if (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Ending show early from set break");
                    stateMachine?.EndShowEarly();
                    return;
                }
            }

            // Update UI every frame (for timer)
            UpdateUI();

            // Update console log every 30 seconds
            float elapsed = Time.unscaledTime - breakStartTime;
            if ((int)(elapsed / 30) != (int)((elapsed - Time.unscaledDeltaTime) / 30))
            {
                LogBanner();
            }
        }

        private void ContinueToNextSong()
        {
            stateMachine?.LoadNextSong();
        }

        private void UpdateUI()
        {
            if (uiManager == null) return;

            float elapsed = Time.unscaledTime - breakStartTime;
            uiManager.ShowSetEnd(breakMessage, elapsed);
        }

        private void LogBanner()
        {
            float elapsed = Time.unscaledTime - breakStartTime;
            int elapsedMinutes = (int)(elapsed / 60);
            int elapsedSeconds = (int)(elapsed % 60);

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("       SET BREAK");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(breakMessage);
            YargLogger.LogInfo($"Elapsed Time: {elapsedMinutes}:{elapsedSeconds:D2}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEYS: R=Resume Show, ESC=End Show");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Show End state - Final state after show is complete
    /// Shows thank you message and overall stats
    /// </summary>
    public class ShowEndState : BaseShowFlowState
    {
        private float showEndTime;

        public ShowEndState(PartyHeroState state, ShowFlowStateMachine machine, ShowFlowUIManager ui) 
            : base(state, machine, ui)
        {
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            showEndTime = Time.unscaledTime;

            LogBanner();
            UpdateUI();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // ESC = Return to editor/menu
                if (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Returning to menu");
                    ReturnToMenu();
                    return;
                }
            }
        }

        private void ReturnToMenu()
        {
            stateMachine?.EndShowEarly();
        }

        private void UpdateUI()
        {
            if (uiManager == null) return;

            var setlistData = partyHeroState.currentSetlist;
            string showName = setlistData?.showName ?? "Unknown Show";
            string endMessage = setlistData?.endMessage ?? "Thank you!";
            int totalSongs = partyHeroState.GetTotalSongCount();
            
            // Calculate show duration
            var showDuration = System.DateTime.Now - partyHeroState.showStartTime;
            int durationMinutes = (int)showDuration.TotalMinutes;

            uiManager.ShowShowEnd(showName, endMessage, totalSongs, durationMinutes);
        }

        private void LogBanner()
        {
            var setlistData = partyHeroState.currentSetlist;
            string endMessage = setlistData?.endMessage ?? "Thank you!";
            int totalSongs = partyHeroState.GetTotalSongCount();
            
            // Calculate show duration
            var showDuration = System.DateTime.Now - partyHeroState.showStartTime;
            int durationMinutes = (int)showDuration.TotalMinutes;

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("    SHOW COMPLETE!");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(endMessage);
            YargLogger.LogInfo("");
            YargLogger.LogInfo($"Total Songs Played: {totalSongs}");
            YargLogger.LogInfo($"Show Duration: {durationMinutes} minutes");
            if (setlistData != null)
            {
                YargLogger.LogInfo($"Show: {setlistData.showName}");
                if (!string.IsNullOrEmpty(setlistData.venue))
                {
                    YargLogger.LogInfo($"Venue: {setlistData.venue}");
                }
            }
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEY: ESC=Return to Menu");
            }
            YargLogger.LogInfo("============================");
        }
    }
}


        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            playerReady = false;
            bandReady = false;

            // Log to console (UI will come later)
            LogBanner();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // SPACE = Toggle player ready
                if (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
                {
                    playerReady = !playerReady;
                    YargLogger.LogInfo($"[PartyHero] Player ready: {playerReady}");
                    LogBanner();
                }

                // B = Toggle band ready (simulates MIDI input)
                if (Keyboard.current?.bKey.wasPressedThisFrame ?? false)
                {
                    bandReady = !bandReady;
                    YargLogger.LogInfo($"[PartyHero] Band ready: {bandReady}");
                    LogBanner();
                }

                // R = Resume immediately (skip waiting)
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Forcing resume (dev override)");
                    ContinueToNextSong();
                    return;
                }
            }

            // Check if both ready
            if (playerReady && bandReady)
            {
                YargLogger.LogInfo("[PartyHero] Both player and band ready - continuing!");
                ContinueToNextSong();
            }
        }

        private void ContinueToNextSong()
        {
            if (stateMachine != null)
            {
                stateMachine.LoadNextSong();
            }
        }

        public void SetBandReady(bool ready)
        {
            bandReady = ready;
            YargLogger.LogInfo($"[PartyHero] Band ready updated via external input: {ready}");
        }

        private void LogBanner()
        {
            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("   WAITING FOR BAND");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo($"Player Ready: {(playerReady ? "✓" : "○")}");
            YargLogger.LogInfo($"Band Ready:   {(bandReady ? "✓" : "○")}");
            YargLogger.LogInfo($"Next Song: {nextSongName}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEYS: SPACE=Player Ready, B=Band Ready, R=Resume");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Waiting for Swap state - Player swap coordination
    /// Gives time for physical controller handoff between players
    /// </summary>
    public class WaitingForSwapState : BaseShowFlowState
    {
        private float swapTimer = 0f;
        private float minimumSwapTime;
        private string swapMessage;
        
        private ShowFlowStateMachine stateMachine;

        public WaitingForSwapState(PartyHeroState state) : base(state)
        {
        }

        public void SetStateMachine(ShowFlowStateMachine machine)
        {
            stateMachine = machine;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            swapTimer = 0f;
            
            // Get swap configuration from current song
            var currentSong = partyHeroState.GetCurrentSong();
            minimumSwapTime = currentSong?.minimumSwapTime ?? 10f;
            swapMessage = currentSong?.swapMessage ?? "Player Swap Time!";

            LogBanner();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            
            swapTimer += Time.unscaledDeltaTime;

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // After minimum time, SPACE allows continue
                if (swapTimer >= minimumSwapTime)
                {
                    if (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
                    {
                        YargLogger.LogInfo("[PartyHero] Player swap complete - continuing!");
                        ContinueToNextSong();
                        return;
                    }
                }

                // R = Force resume immediately (dev override)
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Forcing swap complete (dev override)");
                    ContinueToNextSong();
                    return;
                }
            }

            // Update display every second
            if ((int)swapTimer != (int)(swapTimer - Time.unscaledDeltaTime))
            {
                LogBanner();
            }
        }

        private void ContinueToNextSong()
        {
            if (stateMachine != null)
            {
                stateMachine.LoadNextSong();
            }
        }

        private void LogBanner()
        {
            var nextSong = partyHeroState.GetNextSong();
            string nextSongName = nextSong != null ? nextSong.songName : "Unknown";
            
            float timeRemaining = Mathf.Max(0, minimumSwapTime - swapTimer);
            bool canContinue = swapTimer >= minimumSwapTime;

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("    PLAYER SWAP TIME");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(swapMessage);
            YargLogger.LogInfo($"Time Elapsed: {swapTimer:F1}s");
            if (!canContinue)
            {
                YargLogger.LogInfo($"Minimum Time: {minimumSwapTime:F1}s");
                YargLogger.LogInfo($"Time Remaining: {timeRemaining:F1}s");
            }
            else
            {
                YargLogger.LogInfo("Press SPACE when new player ready");
            }
            YargLogger.LogInfo($"Next Song: {nextSongName}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode && !canContinue)
            {
                YargLogger.LogInfo("DEV KEY: R=Force Resume");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Set End state - Set break/intermission
    /// Formal break between sets with custom message
    /// </summary>
    public class SetEndState : BaseShowFlowState
    {
        private float breakStartTime;
        private string breakMessage;
        private int breakDuration;
        
        private ShowFlowStateMachine stateMachine;

        public SetEndState(PartyHeroState state) : base(state)
        {
        }

        public void SetStateMachine(ShowFlowStateMachine machine)
        {
            stateMachine = machine;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            breakStartTime = Time.unscaledTime;
            
            // Get break configuration from current set
            var currentSet = partyHeroState.GetCurrentSet();
            breakMessage = currentSet?.breakMessage ?? "Set Break - Back in 15 minutes!";
            breakDuration = currentSet?.breakDurationSeconds ?? 900;

            LogBanner();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // R = Resume show
                if (Keyboard.current?.rKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Resuming show from set break");
                    ContinueToNextSong();
                    return;
                }

                // ESC = End show early
                if (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Ending show early from set break");
                    if (stateMachine != null)
                    {
                        stateMachine.EndShowEarly();
                    }
                    return;
                }
            }

            // Update display every 30 seconds
            float elapsed = Time.unscaledTime - breakStartTime;
            if ((int)(elapsed / 30) != (int)((elapsed - Time.unscaledDeltaTime) / 30))
            {
                LogBanner();
            }
        }

        private void ContinueToNextSong()
        {
            if (stateMachine != null)
            {
                stateMachine.LoadNextSong();
            }
        }

        private void LogBanner()
        {
            float elapsed = Time.unscaledTime - breakStartTime;
            int elapsedMinutes = (int)(elapsed / 60);
            int elapsedSeconds = (int)(elapsed % 60);

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("       SET BREAK");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(breakMessage);
            YargLogger.LogInfo($"Elapsed Time: {elapsedMinutes}:{elapsedSeconds:D2}");
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEYS: R=Resume Show, ESC=End Show");
            }
            YargLogger.LogInfo("============================");
        }
    }

    /// <summary>
    /// Show End state - Final state after show is complete
    /// Shows thank you message and overall stats
    /// </summary>
    public class ShowEndState : BaseShowFlowState
    {
        private float showEndTime;
        
        private ShowFlowStateMachine stateMachine;

        public ShowEndState(PartyHeroState state) : base(state)
        {
        }

        public void SetStateMachine(ShowFlowStateMachine machine)
        {
            stateMachine = machine;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            
            showEndTime = Time.unscaledTime;

            LogBanner();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();

            // Development mode: Use keyboard shortcuts
            if (partyHeroState.developmentMode)
            {
                // ESC = Return to editor/menu
                if (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
                {
                    YargLogger.LogInfo("[PartyHero] Returning to menu");
                    ReturnToMenu();
                    return;
                }
            }
        }

        private void ReturnToMenu()
        {
            if (stateMachine != null)
            {
                stateMachine.EndShowEarly();
            }
        }

        private void LogBanner()
        {
            var setlistData = partyHeroState.currentSetlist;
            string endMessage = setlistData?.endMessage ?? "Thank you!";
            int totalSongs = partyHeroState.GetTotalSongCount();
            
            // Calculate show duration
            var showDuration = System.DateTime.Now - partyHeroState.showStartTime;
            int durationMinutes = (int)showDuration.TotalMinutes;

            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("    SHOW COMPLETE!");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo(endMessage);
            YargLogger.LogInfo("");
            YargLogger.LogInfo($"Total Songs Played: {totalSongs}");
            YargLogger.LogInfo($"Show Duration: {durationMinutes} minutes");
            if (setlistData != null)
            {
                YargLogger.LogInfo($"Show: {setlistData.showName}");
                if (!string.IsNullOrEmpty(setlistData.venue))
                {
                    YargLogger.LogInfo($"Venue: {setlistData.venue}");
                }
            }
            YargLogger.LogInfo("============================");
            if (partyHeroState.developmentMode)
            {
                YargLogger.LogInfo("DEV KEY: ESC=Return to Menu");
            }
            YargLogger.LogInfo("============================");
        }
    }
}
