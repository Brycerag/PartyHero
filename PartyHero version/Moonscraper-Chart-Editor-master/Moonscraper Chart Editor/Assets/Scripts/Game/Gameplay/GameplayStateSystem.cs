using UnityEngine;
using TimingConfig;
using MoonscraperEngine;
using MoonscraperEngine.Audio;
using MoonscraperChartEditor.Song;

public class GameplayStateSystem : SystemManagerState.System
{
    // Public access to current instance (for ShowFlowManager)
    public static GameplayStateSystem Instance { get; private set; }

    // Configurable properties
    bool botEnabled = true;

    OneShotSampleStream missSoundSample;
    HitWindowFeeder hitWindowFeeder = new HitWindowFeeder();
    float playFromTime;

    // Star power state tracking
    bool isInStarpowerZone = false;

    delegate void GameplayUpdateFn(float time);
    GameplayUpdateFn gameplayUpdateFn = null;
    
    // Public access to gameplay stats (for ShowFlowManager and results screen)
    public BaseGameplayRulestate currentRulestate;

    const int HIT_WINDOW_DELAY_TOTAL_FRAMES = 2;
    int hitWindowFrameDelayCount = HIT_WINDOW_DELAY_TOTAL_FRAMES;

    delegate void UpdateFn();
    UpdateFn currentUpdate = null;

    public struct GameState
    {
        public BaseGameplayRulestate.NoteStats stats;
    }

    enum GameplayType
    {
        Bot,
        Guitar,
        Drums,

        None,
    }

    public GameplayStateSystem(float playFromTime, bool botEnabled)
    {
        this.botEnabled = botEnabled;
        this.playFromTime = playFromTime;

        currentUpdate = UpdateWaitingForNotesSettled;
    }

    public override void SystemEnter()
    {
        Instance = this;

        songEndTriggered = false; // Reset flag for new song

        ChartEditor editor = ChartEditor.Instance;

        GameplayType gameplayType = DetermineGameplayType(botEnabled, editor.currentGameMode);
        LoadSoundClip();

        DetermineUpdateRulestate(gameplayType, out gameplayUpdateFn, out currentRulestate);

        hitWindowFeeder.hitWindow = CreateHitWindow(gameplayType);

        ChartEditor.Instance.uiServices.SetGameplayUIActive(!botEnabled);
    }

    public override void SystemUpdate()
    {
        currentUpdate();
    }

    void UpdateWaitingForNotesSettled()
    {
        // We need to wait a couple of frames for the physics system to settle down, otherwise notes can be sprawled all over the place if we're being spammy about playing
        --hitWindowFrameDelayCount;

        if (hitWindowFrameDelayCount <= 0)
        {
            currentUpdate = UpdateGameplay;
        }
    }

    void UpdateGameplay()
    {
        hitWindowFeeder.Update();

        float currentTime = ChartEditor.Instance.currentVisibleTime;
        gameplayUpdateFn?.Invoke(currentTime);

        // Check for star power zone entry/exit
        UpdateStarpowerState(currentTime);

        // Check if song has reached its end
        CheckForSongEnd(currentTime);

        GameState gamestate = new GameState();
        gamestate.stats = currentRulestate.stats;

        ChartEditor.Instance.gameplayEvents.gameplayUpdateEvent.Fire(gamestate);
    }

    void UpdateStarpowerState(float currentTime)
    {
        ChartEditor editor = ChartEditor.Instance;
        Chart currentChart = editor.currentChart;

        if (currentChart == null || currentChart.starPower == null || currentChart.starPower.Count == 0)
        {
            // No star power in chart - ensure we're marked as not in zone
            if (isInStarpowerZone)
            {
                isInStarpowerZone = false;
                editor.gameplayEvents.starpowerDeactivateEvent.Fire();
            }
            return;
        }

        // Convert current time to tick position
        uint currentTick = editor.currentSong.TimeToTick(currentTime, editor.currentSong.resolution);

        // Find closest star power zone at or before current position
        int index = SongObjectHelper.FindClosestPositionRoundedDown(currentTick, currentChart.starPower);
        
        if (index >= 0 && index < currentChart.starPower.Count)
        {
            Starpower sp = currentChart.starPower[index];
            
            // Check if current position is within this star power zone
            bool nowInZone = (sp.tick <= currentTick && (sp.tick + sp.length) > currentTick);

            // Detect state change
            if (nowInZone && !isInStarpowerZone)
            {
                // Entered star power zone
                isInStarpowerZone = true;
                editor.gameplayEvents.starpowerActivateEvent.Fire();
            }
            else if (!nowInZone && isInStarpowerZone)
            {
                // Exited star power zone
                isInStarpowerZone = false;
                editor.gameplayEvents.starpowerDeactivateEvent.Fire();
            }
        }
        else if (isInStarpowerZone)
        {
            // No valid star power found but we were in zone - must have exited
            isInStarpowerZone = false;
            editor.gameplayEvents.starpowerDeactivateEvent.Fire();
        }
    }

    bool songEndTriggered = false; // Prevent multiple triggers

    void CheckForSongEnd(float currentTime)
    {
        // Only check if show flow is enabled and we haven't already triggered
        if (!ShowFlowManager.Instance || !ShowFlowManager.Instance.showFlowEnabled || songEndTriggered)
            return;

        ChartEditor editor = ChartEditor.Instance;
        Song song = editor.currentSong;

        // Determine song length
        float songLength = GetSongLength(song);

        // Check if we've reached or passed the song end
        if (currentTime >= songLength)
        {
            songEndTriggered = true;

            if (ShowFlowManager.Instance.debugShowFlow)
            {
                Debug.Log($"[GameplayStateSystem] Song end detected at {currentTime:F2}s (length: {songLength:F2}s)");
            }

            // Trigger song complete via ShowFlowManager
            ShowFlowManager.Instance.TriggerSongComplete();
        }
    }

    float GetSongLength(Song song)
    {
        // Priority 1: Manual length (explicitly set in song properties)
        if (song.manualLength.HasValue && song.manualLength.Value > 0)
            return song.manualLength.Value;

        // Priority 2: Audio file length
        var audioManager = ChartEditor.Instance.currentSongAudio;
        if (audioManager != null)
        {
            AudioStream audioStream = audioManager.mainSongAudio;
            if (audioStream != null)
            {
                float audioLength = audioStream.ChannelLengthInSeconds();
                if (audioLength > 0)
                    return audioLength;
            }
        }

        // Fallback: Chart length (last note + buffer)
        // This is less ideal as songs often have outro after last note
        Chart chart = ChartEditor.Instance.currentChart;
        if (chart != null && chart.chartObjects.Count > 0)
        {
            var lastObject = chart.chartObjects[chart.chartObjects.Count - 1];
            float chartEndTime = song.TickToTime(lastObject.tick, song.resolution);
            return chartEndTime + 2.0f; // 2 second buffer after last note
        }

        // No valid length found
        Debug.LogWarning("[GameplayStateSystem] Could not determine song length");
        return float.MaxValue; // Never end
    }

    public override void SystemExit()
    {
        Instance = null;
        
        missSoundSample = null;
        ChartEditor.Instance.uiServices.SetGameplayUIActive(false);
    }

    void LoadSoundClip()
    {
        missSoundSample = ChartEditor.Instance.sfxAudioStreams.GetSample(SkinKeys.break0);
        Debug.Assert(missSoundSample != null);
        missSoundSample.onlyPlayIfStopped = true;
    }

    void KickMissFeedback()
    {
        if (missSoundSample.Play())      // If we try to play this again before the sample has ended we'll get rejected. Should also reject the whole event.
        {
            ChartEditor.Instance.gameplayEvents.explicitMissEvent.Fire();
        }
    }

    void DetermineUpdateRulestate(GameplayType gameplayType, out GameplayUpdateFn gameplayUpdateFn, out BaseGameplayRulestate currentRulestate)
    {
        gameplayUpdateFn = null;
        currentRulestate = null;

        switch (gameplayType)
        {
            case GameplayType.Bot:
                {
                    gameplayUpdateFn = UpdateBotGameplay;
                    currentRulestate = new BotGameplayRulestate(KickMissFeedback); 
                }
                break;
            case GameplayType.Guitar:
                {
                    gameplayUpdateFn = UpdateGuitarGameplay;
                    currentRulestate = new GuitarGameplayRulestate(KickMissFeedback);
                }
                break;
            case GameplayType.Drums:
                {
                    gameplayUpdateFn = UpdateDrumsGameplay;
                    currentRulestate = new DrumsGameplayRulestate(KickMissFeedback);
                }
                break;
            default:
                {
                }
                break;
        }
    }

    void UpdateBotGameplay(float time)
    {
        ((BotGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<NoteHitKnowledge>);
    }

    void UpdateGuitarGameplay(float time)
    {
        ((GuitarGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<GuitarNoteHitKnowledge>);
    }

    void UpdateDrumsGameplay(float time)
    {
        ((DrumsGameplayRulestate)currentRulestate).Update(time, hitWindowFeeder.hitWindow as HitWindow<DrumsNoteHitKnowledge>);
    }

    void UpdateUIStats(BaseGameplayRulestate currentRulestate)
    {
        BaseGameplayRulestate.NoteStats stats = currentRulestate.stats;
    }

    IHitWindow CreateHitWindow(GameplayType gameplayType)
    {
        switch (gameplayType)
        {
            case GameplayType.Bot:
                {
                    return new HitWindow<NoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
                }

            case GameplayType.Guitar:
                {
                    return new HitWindow<GuitarNoteHitKnowledge>(GuitarTiming.frontendHitWindowTime, GuitarTiming.backendHitWindowTime);
                }

            case GameplayType.Drums:
                {
                    return new HitWindow<DrumsNoteHitKnowledge>(DrumsTiming.frontendHitWindowTime, DrumsTiming.backendHitWindowTime);
                }
        }

        return null;
    }

    static GameplayType DetermineGameplayType(bool botEnabled, Chart.GameMode gameMode)
    {
        if (botEnabled)
        {
            return GameplayType.Bot;
        }
        else if (gameMode == Chart.GameMode.Guitar)
        {
            return GameplayType.Guitar;
        }
        else if (gameMode == Chart.GameMode.Drums)
        {
            return GameplayType.Drums;
        }

        return GameplayType.None;
    }
}
