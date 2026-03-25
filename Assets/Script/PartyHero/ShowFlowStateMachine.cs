using UnityEngine;
using YARG.Core.Logging;
using YARG.PartyHero.UI;

namespace YARG.PartyHero
{
    /// <summary>
    /// Base class for all show flow states (WaitingForBand, WaitingForSwap, SetEnd, ShowEnd)
    /// Similar to Moonscraper's SystemManagerState approach but adapted for YARG
    /// </summary>
    public abstract class BaseShowFlowState
    {
        protected PartyHeroState partyHeroState;
        protected ShowFlowStateMachine stateMachine;
        protected ShowFlowUIManager uiManager;
        
        public BaseShowFlowState(PartyHeroState state, ShowFlowStateMachine machine, ShowFlowUIManager ui)
        {
            partyHeroState = state;
            stateMachine = machine;
            uiManager = ui;
        }

        /// <summary>
        /// Called when entering this state
        /// </summary>
        public virtual void OnStateEnter()
        {
            YargLogger.LogInfo($"[PartyHero] Entering state: {GetType().Name}");
        }

        /// <summary>
        /// Called every frame while in this state
        /// </summary>
        public virtual void OnStateUpdate()
        {
        }

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        public virtual void OnStateExit()
        {
            YargLogger.LogInfo($"[PartyHero] Exiting state: {GetType().Name}");
        }

        /// <summary>
        /// Determine the next state based on current show position
        /// </summary>
        public ShowFlowStateType GetNextStateType()
        {
            // If on last song, go to show end
            if (partyHeroState.IsLastSongInShow())
            {
                return ShowFlowStateType.ShowEnd;
            }

            // Check if we should do a player swap
            if (partyHeroState.ShouldEnterPlayerSwap())
            {
                return ShowFlowStateType.WaitingForSwap;
            }

            // Check if we should take a set break
            if (partyHeroState.ShouldEnterSetBreak())
            {
                return ShowFlowStateType.SetEnd;
            }

            // Default: wait for band between songs
            return ShowFlowStateType.WaitingForBand;
        }
    }

    /// <summary>
    /// Types of show flow states
    /// </summary>
    public enum ShowFlowStateType
    {
        None,
        Results,           // Show results (existing YARG score screen)
        WaitingForBand,    // Waiting for band to be ready for next song
        WaitingForSwap,    // Waiting for player swap
        SetEnd,            // Set break/intermission
        ShowEnd            // Show complete
    }

    /// <summary>
    /// Manages the show flow state machine
    /// This should be attached to the Score scene or managed by GlobalVariables
    /// </summary>
    public class ShowFlowStateMachine : MonoBehaviour
    {
        private BaseShowFlowState currentState;
        private PartyHeroState partyHeroState;
        private ShowFlowUIManager uiManager;
        
        // Communication managers
        private MidiInputHandler midiHandler;
        private OscManager oscManager;
        private TcpManager tcpManager;
        
        public ShowFlowStateType CurrentStateType { get; private set; }

        public void Initialize(PartyHeroState state, ShowFlowUIManager ui)
        {
            partyHeroState = state;
            uiManager = ui;
            CurrentStateType = ShowFlowStateType.None;
            
            // Initialize communication managers
            InitializeCommunication();
        }
        
        private void InitializeCommunication()
        {
            // MIDI input handler
            midiHandler = gameObject.AddComponent<MidiInputHandler>();
            midiHandler.Initialize(this, partyHeroState);
            
            // OSC manager (requires OscCore package)
            oscManager = gameObject.AddComponent<OscManager>();
            oscManager.Initialize(this, partyHeroState);
            
            // TCP manager
            tcpManager = gameObject.AddComponent<TcpManager>();
            tcpManager.Initialize(this, partyHeroState);
            
            YargLogger.LogInfo("[PartyHero] Communication managers initialized");
        }

        public void ChangeState(ShowFlowStateType stateType)
        {
            // Exit current state
            if (currentState != null)
            {
                currentState.OnStateExit();
            }

            CurrentStateType = stateType;

            // Create and enter new state
            currentState = stateType switch
            {
                ShowFlowStateType.WaitingForBand => new WaitingForBandState(partyHeroState, this, uiManager),
                ShowFlowStateType.WaitingForSwap => new WaitingForSwapState(partyHeroState, this, uiManager),
                ShowFlowStateType.SetEnd => new SetEndState(partyHeroState, this, uiManager),
                ShowFlowStateType.ShowEnd => new ShowEndState(partyHeroState, this, uiManager),
                _ => null
            };

            if (currentState != null)
            {
                currentState.OnStateEnter();
                
                // Notify communication managers of state change
                NotifyStateChange(stateType);
            }
        }

        private void Update()
        {
            if (currentState != null)
            {
                currentState.OnStateUpdate();
            }
            
            // Process main thread actions from TCP
            UnityMainThreadDispatcher.ExecuteQueue();
        }

        /// <summary>
        /// Transition to the next song in the show
        /// </summary>
        public void LoadNextSong()
        {
            // Increment song index
            partyHeroState.overallSongIndex++;
            
            // Load the next song using YARG's systems
            var nextSong = partyHeroState.GetCurrentSong();
            if (nextSong != null)
            {
                YargLogger.LogInfo($"[PartyHero] Loading next song: {nextSong.songName}");
                
                // Notify communication managers
                NotifySongStart(nextSong.songName);
                
                GlobalVariables.Instance.LoadScene(SceneIndex.Gameplay);
            }
            else
            {
                YargLogger.LogWarning("[PartyHero] No next song found!");
            }
        }
        
        /// <summary>
        /// Notify that a song has ended (call from GameManager after song completes)
        /// </summary>
        public void OnSongEnd(int score)
        {
            YargLogger.LogInfo($"[PartyHero] Song ended with score: {score}");
            
            // Notify communication managers
            oscManager?.SendSongEnd(score);
            tcpManager?.SendSongEnd(score);
        }

        #region Communication Notifications
        
        private void NotifyStateChange(ShowFlowStateType stateType)
        {
            string stateName = stateType.ToString();
            
            oscManager?.SendStateChange(stateName);
            tcpManager?.SendStateChange(stateName);
            
            YargLogger.LogInfo($"[PartyHero] Notified state change: {stateName}");
        }
        
        private void NotifySongStart(string songName)
        {
            oscManager?.SendSongStart(songName);
            tcpManager?.SendSongStart(songName);
            
            YargLogger.LogInfo($"[PartyHero] Notified song start: {songName}");
        }
        
        #endregion

        /// <summary>
        /// End the show early and return to menu
        /// </summary>
        public void EndShowEarly()
        {
            YargLogger.LogInfo("[PartyHero] Ending show early");
            partyHeroState.partyHeroMode = false;
            GlobalVariables.State.PlayingAShow = false;
            GlobalVariables.Instance.LoadScene(SceneIndex.Menu);
        }
    }
}
