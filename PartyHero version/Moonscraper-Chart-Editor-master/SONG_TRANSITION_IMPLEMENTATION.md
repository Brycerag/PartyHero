# Song Transition Implementation Design

**Status**: Logic Complete ✅ | Unity UI Pending ⚠️  
**Based On**: [SONG_TRANSITION_UX_SCENARIOS.md](SONG_TRANSITION_UX_SCENARIOS.md) - Finalized Decisions  
**Unity UI TODO**: [SHOW_FLOW_UI_TODO.md](SHOW_FLOW_UI_TODO.md) - Complete UI component checklist  
**Last Updated**: 2026-03-23

---

## Overview

This document provides the technical implementation plan for the PartyHero song transition and player management system. All UX decisions have been finalized in the scenarios document - this is the engineering blueprint.

---

## Architecture Overview

### Current State System

Moonscraper uses a `SystemManagerState` pattern managed by `ChartEditor`:

```
ChartEditor.State enum:
- Editor
- Playing  (gameplay active)
- Menu
- Loading
```

### NEW: Extended State System

We will extend the state system to support the show flow:

```
ChartEditor.State enum (EXTENDED):
- Editor          (existing)
- Playing         (existing - gameplay active)
- Results         (NEW - post-song stats display)
- WaitingForSwap  (NEW - player swap transition)
- WaitingForBand  (NEW - band ready waiting)
- SetEnd          (NEW - between-set break)
- ShowEnd         (NEW - end of show)
- Menu            (existing)
- Loading         (existing)
```

Each state gets a corresponding `SystemManagerState` implementation:
- `ResultsState` - Display stats, preload next song
- `WaitingForSwapState` - Player swap UI, wait for new player ready
- `WaitingForBandState` - Ready screen, wait for band trigger
- `SetEndState` - Between-set display
- `ShowEndState` - Final show screen

---

## Class Structure

### 1. New State Classes

#### ResultsState.cs
```csharp
public class ResultsState : SystemManagerState
{
    // Data
    BaseGameplayRulestate.NoteStats stats;
    string nextSongName;
    bool showNextSong;
    string playerName; // Optional, future
    
    // Systems
    ResultsUISystem uiSystem;
    NextSongPreloader preloader;
    
    public ResultsState(NoteStats stats)
    {
        this.stats = stats;
        DetermineNextSongDisplay();
        AddSystem(new ResultsUISystem(stats, nextSongName, showNextSong));
        AddSystem(new NextSongPreloader());
    }
    
    void DetermineNextSongDisplay()
    {
        // Check global setting + per-song AbleSet data
        // Logic from SONG_TRANSITION_UX_SCENARIOS.md
    }
    
    public override void Update()
    {
        // Check for "Next/Continue" button click
        // Check for player swap trigger
        // Transition to appropriate next state
    }
}
```

#### WaitingForBandState.cs
```csharp
public class WaitingForBandState : SystemManagerState
{
    WaitingForBandUISystem uiSystem;
    bool playerReady;
    bool bandReady;
    
    public WaitingForBandState(bool playerReady)
    {
        this.playerReady = playerReady;
        AddSystem(new WaitingForBandUISystem());
    }
    
    public override void Update()
    {
        // Listen for band ready trigger (MIDI/OSC)
        if (CheckBandReadyTrigger())
        {
            bandReady = true;
            TransitionToNextSong();
        }
        
        // Listen for force player ready (band override)
        if (CheckForcePlayerReady())
        {
            playerReady = true;
        }
    }
    
    void TransitionToNextSong()
    {
        // Load next song from preloader
        // Start gameplay (ChartEditor.StartGameplay)
    }
}
```

#### WaitingForSwapState.cs
```csharp
public class WaitingForSwapState : SystemManagerState
{
    WaitingForSwapUISystem uiSystem;
    bool playerReady;
    
    public WaitingForSwapState()
    {
        AddSystem(new WaitingForSwapUISystem());
    }
    
    public override void Update()
    {
        // Wait for new player ready signal
        if (CheckPlayerReady())
        {
            playerReady = true;
            TransitionToWaitingForBand();
        }
        
        // Band can force player ready
        if (CheckForcePlayerReady())
        {
            playerReady = true;
            TransitionToWaitingForBand();
        }
    }
    
    void TransitionToWaitingForBand()
    {
        ChartEditor.Instance.ChangeState(State.WaitingForBand, 
            new WaitingForBandState(playerReady: true));
    }
}
```

#### SetEndState.cs & ShowEndState.cs
```csharp
public class SetEndState : SystemManagerState
{
    // Simple holding state with special UI
    // Waits for manual trigger to resume
}

public class ShowEndState : SystemManagerState
{
    // Final screen
    // Can only exit to Editor manually
    // Optional: aggregate stats across all songs (future)
}
```

---

### 2. New Manager Classes

#### ShowFlowManager.cs
Central coordinator for show state transitions and triggers.

```csharp
public class ShowFlowManager : MonoBehaviour
{
    public static ShowFlowManager Instance { get; private set; }
    
    // Configuration
    [Header("Show Settings")]
    public bool showNextSongName = true;  // Global toggle
    public bool showPlayerName = false;   // Future feature
    
    // State tracking
    public enum PlayerMode
    {
        Continuing,    // Same player
        Swapping,      // New player
        NoPlayer      // Demo mode
    }
    
    public PlayerMode currentPlayerMode = PlayerMode.Continuing;
    public bool isPlayerReady = false;
    public bool isBandReady = false;
    
    // MIDI/OSC trigger note numbers
    [Header("MIDI Triggers")]
    public int playerSwapNoteNumber = 124;
    public int playerReadyNoteNumber = 125;
    public int bandReadyNoteNumber = 126;
    public int songCompleteNoteNumber = 127;
    public int setEndNoteNumber = 122;
    public int showEndNoteNumber = 121;
    public int noPlayerModeNoteNumber = 120;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        // Register MIDI input callbacks
        // Register OSC message callbacks with ExternalSyncManager
        RegisterTriggers();
    }
    
    void RegisterTriggers()
    {
        // Hook into MIDI input system (needs investigation)
        // Hook into ExternalSyncManager for OSC
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.OnOscMessageReceived += HandleOscMessage;
        }
    }
    
    void HandleOscMessage(string address, object[] args)
    {
        switch (address)
        {
            case "/player/swap":
                TriggerPlayerSwap();
                break;
            case "/player/ready":
                TriggerPlayerReady();
                break;
            case "/band/ready":
                TriggerBandReady();
                break;
            case "/song/complete":
                TriggerSongComplete();
                break;
            case "/set/end":
                TriggerSetEnd();
                break;
            case "/show/end":
                TriggerShowEnd();
                break;
            case "/game/mode/demo":
                TriggerNoPlayerMode();
                break;
        }
    }
    
    // Public trigger methods
    public void TriggerPlayerSwap() { currentPlayerMode = PlayerMode.Swapping; }
    public void TriggerPlayerReady() { isPlayerReady = true; }
    public void TriggerBandReady() { isBandReady = true; }
    public void TriggerNoPlayerMode() { currentPlayerMode = PlayerMode.NoPlayer; }
    
    public void TriggerSongComplete()
    {
        // End current song, go to results
        if (ChartEditor.Instance.currentState == ChartEditor.State.Playing)
        {
            EndSongAndShowResults();
        }
    }
    
    public void TriggerSetEnd()
    {
        ChartEditor.Instance.ChangeState(ChartEditor.State.SetEnd, new SetEndState());
    }
    
    public void TriggerShowEnd()
    {
        ChartEditor.Instance.ChangeState(ChartEditor.State.ShowEnd, new ShowEndState());
    }
    
    void EndSongAndShowResults()
    {
        // Get stats from GameplayStateSystem
        var gameplaySystem = GetCurrentGameplaySystem();
        var stats = gameplaySystem.currentRulestate.stats;
        
        // Transition to Results state
        ChartEditor.Instance.ChangeState(ChartEditor.State.Results, 
            new ResultsState(stats));
    }
    
    // Broadcast current state via OSC
    public void BroadcastState(string stateName)
    {
        if (ExternalSyncManager.Instance != null)
        {
            ExternalSyncManager.Instance.SendOscMessage("/game/state", stateName);
        }
    }
    
    // Get next song info from SongMappingManager
    public string GetNextSongName()
    {
        // Query SongMappingManager for next song
        // Check if it should be shown (per-song override)
        return null;
    }
    
    public bool ShouldShowNextSong()
    {
        if (!showNextSongName)
            return false; // Global override
            
        // Check per-song setting from AbleSet mapping
        // Implementation depends on SongMappingManager structure
        return true;
    }
}
```

---

### 3. UI System Classes

#### ResultsUISystem.cs
```csharp
public class ResultsUISystem : SystemManagerState.System
{
    NoteStats stats;
    string nextSongName;
    bool showNextSong;
    
    // UI Elements (Unity UI or TextMeshPro)
    Text hitPercentageText;
    Text streakText;
    Text notesHitText;
    Text nextSongText;
    Button continueButton;
    
    public ResultsUISystem(NoteStats stats, string nextSongName, bool showNextSong)
    {
        this.stats = stats;
        this.nextSongName = nextSongName;
        this.showNextSong = showNextSong;
    }
    
    public override void SystemEnter()
    {
        // Load UI prefab
        // Populate stats
        DisplayStats();
        
        // Show/hide next song name
        if (showNextSong && !string.IsNullOrEmpty(nextSongName))
        {
            nextSongText.text = $"Next: {nextSongName}";
            nextSongText.gameObject.SetActive(true);
        }
        
        // Bind button
        continueButton.onClick.AddListener(OnContinueClicked);
    }
    
    void DisplayStats()
    {
        float hitPercent = (stats.notesHit / (float)stats.totalNotes) * 100f;
        hitPercentageText.text = $"{hitPercent:F1}%";
        streakText.text = $"Best Streak: {stats.noteStreak}";
        notesHitText.text = $"{stats.notesHit} / {stats.totalNotes}";
    }
    
    void OnContinueClicked()
    {
        // Determine next state based on PlayerMode
        var flowManager = ShowFlowManager.Instance;
        
        if (flowManager.currentPlayerMode == PlayerMode.Swapping)
        {
            ChartEditor.Instance.ChangeState(State.WaitingForSwap, 
                new WaitingForSwapState());
        }
        else if (flowManager.currentPlayerMode == PlayerMode.NoPlayer)
        {
            // Start next song in demo mode
            StartNextSongInDemoMode();
        }
        else // Continuing
        {
            ChartEditor.Instance.ChangeState(State.WaitingForBand, 
                new WaitingForBandState(playerReady: true));
        }
    }
    
    public override void SystemExit()
    {
        // Cleanup UI
    }
}
```

#### WaitingForBandUISystem.cs
```csharp
public class WaitingForBandUISystem : SystemManagerState.System
{
    Text statusText;
    Text playerReadyIndicator;
    Text bandReadyIndicator;
    
    public override void SystemEnter()
    {
        // Load waiting UI
        statusText.text = "Waiting for band...";
        UpdateReadyIndicators();
    }
    
    public override void SystemUpdate()
    {
        UpdateReadyIndicators();
    }
    
    void UpdateReadyIndicators()
    {
        var flowManager = ShowFlowManager.Instance;
        playerReadyIndicator.text = flowManager.isPlayerReady ? "✓ Player Ready" : "○ Player";
        bandReadyIndicator.text = flowManager.isBandReady ? "✓ Band Ready" : "○ Band";
    }
}
```

#### WaitingForSwapUISystem.cs
```csharp
public class WaitingForSwapUISystem : SystemManagerState.System
{
    Text titleText;
    Text instructionsText;
    
    public override void SystemEnter()
    {
        titleText.text = "Player Swap Time";
        instructionsText.text = "New player, get ready!\nPress Ready when you're set.";
    }
}
```

---

### 4. Modified Existing Classes

#### GameplayStateSystem.cs (existing)
**Changes needed:**
- Track when song naturally ends (chart length reached)
- Fire `SongCompleteEvent` when song ends
- Expose `currentRulestate` and `stats` publicly for ResultsState

```csharp
// ADD to GameplayStateSystem
public BaseGameplayRulestate currentRulestate { get; private set; }

void UpdateGameplay()
{
    // ... existing gameplay code ...
    
    // Check for song end
    if (HasReachedSongEnd())
    {
        OnSongComplete();
    }
}

bool HasReachedSongEnd()
{
    Song song = ChartEditor.Instance.currentSong;
    float currentTime = ChartEditor.Instance.currentSongTime;
    
    // Check manual length or audio length
    float songLength = song.manualLength > 0 ? song.manualLength : song.GetAudioLength();
    
    return currentTime >= songLength;
}

void OnSongComplete()
{
    // Trigger results state
    ShowFlowManager.Instance.TriggerSongComplete();
}
```

#### ExternalSyncManager.cs (existing)
**Changes needed:**
- Add event for OSC message received (for ShowFlowManager to hook into)
- Add new OSC message handlers in ProcessOscMessage

```csharp
// ADD to ExternalSyncManager
public delegate void OscMessageReceived(string address, object[] args);
public event OscMessageReceived OnOscMessageReceived;

private void ProcessOscMessage(byte[] data, int length)
{
    OscMessage msg = OscMessage.Parse(data, length);
    if (msg == null)
        return;

    // ... existing handlers ...
    
    // NEW: Forward to ShowFlowManager
    OnOscMessageReceived?.Invoke(msg.Address, msg.Arguments.ToArray());
}
```

#### SongMappingManager.cs (existing, if it exists)
**Changes needed:**
- Add `showNextSong` field to song mapping data
- Parse from songsync_mapping.json

```csharp
// ADD to SongMapping class
public class SongMapping
{
    public string trackName;
    public string chartPath;
    public float timelineStartTime;
    public bool showNextSong = true;  // NEW: default true
}
```

---

## Demo Mode Implementation

### DemoGameplayRulestate Enhancement

The existing `BotGameplayRulestate` can be used for demo mode with minimal changes:

```csharp
// In GameplayStateSystem
public GameplayStateSystem(float playFromTime, bool botEnabled, bool demoMode = false)
{
    this.botEnabled = botEnabled || demoMode;
    this.demoMode = demoMode;
    this.playFromTime = playFromTime;
    
    currentUpdate = UpdateWaitingForNotesSettled;
}

// When starting demo mode
if (demoMode)
{
    // BotGameplayRulestate already auto-hits all notes
    // Just need to ensure it's enabled
}
```

**BotGameplayRulestate already:**
- Auto-hits all notes in the hit window
- Never misses
- Perfect for visual backdrop

No changes needed to BotGameplayRulestate itself!

---

## Data Flow Diagrams

### Song End → Results Flow
```
GameplayStateSystem
    └─> HasReachedSongEnd() = true
        └─> ShowFlowManager.TriggerSongComplete()
            └─> Get stats from currentRulestate
            └─> ChartEditor.ChangeState(Results, new ResultsState(stats))
                └─> ResultsUISystem.DisplayStats()
                └─> NextSongPreloader.Start()
```

### Results → Next Song Flow (Repeat Player)
```
Player clicks Continue button
    └─> ResultsUISystem.OnContinueClicked()
        └─> Check ShowFlowManager.currentPlayerMode
            └─> PlayerMode.Continuing
                └─> ChangeState(WaitingForBand, playerReady: true)
                    └─> WaitingForBandUISystem shows ready screen
                    └─> Wait for bandReadyTrigger (MIDI/OSC)
                        └─> Load next song
                        └─> ChartEditor.StartGameplay()
```

### Results → Player Swap Flow
```
Player clicks Continue button
    └─> ShowFlowManager.currentPlayerMode = Swapping (triggered earlier via MIDI)
        └─> ChangeState(WaitingForSwap)
            └─> WaitingForSwapUISystem shows swap screen
            └─> Wait for playerReadyTrigger
                └─> ChangeState(WaitingForBand, playerReady: true)
                    └─> Wait for bandReadyTrigger
                        └─> Load next song
                        └─> ChartEditor.StartGameplay()
```

### No Player (Demo Mode) Flow
```
ShowFlowManager.TriggerNoPlayerMode()
    └─> currentPlayerMode = NoPlayer
        └─> ResultsState skipped OR special "No Player" UI
            └─> ChartEditor.StartGameplay(enableBot: true, demoMode: true)
                └─> BotGameplayRulestate hits all notes
                └─> Visual backdrop for band
```

---

## MIDI/OSC Message Mapping

### Input Messages (Receive)

| Message | MIDI Note | OSC Address | Purpose |
|---------|-----------|-------------|---------|
| **Player Swap** | 124 | `/player/swap` | Signal player swap needed |
| **Player Ready** | 125 | `/player/ready` | New player is ready |
| **Band Ready** | 126 | `/band/ready` | Band ready to start next song |
| **Song Complete** | 127 | `/song/complete` | External song end trigger |
| **Set End** | 122 | `/set/end` | Trigger Set End screen |
| **Show End** | 121 | `/show/end` | Trigger Show End screen |
| **No Player Mode** | 120 | `/game/mode/demo` | Enable demo mode |

### Output Messages (Send)

| Message | OSC Address | Arguments | When Sent |
|---------|-------------|-----------|-----------|
| **Game State** | `/game/state` | [string] state | On every state change |
| **Player State** | `/player/state` | [string] state | When player state changes |
| **Band State** | `/band/state` | [string] state | When band state changes |
| **Song Stats** | `/song/stats` | [int] hits, [int] total, [int] streak | When results are calculated |

**State Names:**
- `"playing"` - Active gameplay
- `"results"` - Results screen showing
- `"waiting_swap"` - Waiting for player swap
- `"waiting_band"` - Waiting for band ready
- `"set_end"` - Set end screen
- `"show_end"` - Show end screen
- `"editor"` - Back in editor

---

## UI Prefab Structure

### Results Screen Prefab
**Hierarchy:**
```
ResultsScreen (Canvas)
├─ Background (Image)
├─ StatsContainer (Panel)
│  ├─ Title (Text: "Song Complete")
│  ├─ HitPercentage (Text: "95.2%")
│  ├─ Streak (Text: "Best Streak: 142")
│  ├─ NotesHit (Text: "324 / 340")
│  └─ NextSongDisplay (Text: "Next: Song Name")
│     └─ (Hidden if showNextSong = false)
├─ ContinueButton (Button)
│  └─ Text ("Continue")
└─ LoadingIndicator (Text: "Loading next song...")
```

### Waiting For Band Screen Prefab
```
WaitingForBandScreen (Canvas)
├─ Background
├─ StatusText (Text: "Waiting for band...")
├─ ReadyIndicators (Panel)
│  ├─ PlayerReady (Text: "✓ Player Ready" or "○ Player")
│  └─ BandReady (Text: "○ Band" → "✓ Band Ready")
└─ InstructionsText (Text: "Band: Press Ready when set")
```

### Waiting For Swap Screen Prefab
```
WaitingForSwapScreen (Canvas)
├─ Background
├─ Title (Text: "Player Swap Time")
├─ Instructions (Text: "New player, get ready!")
├─ SwapIcon (Image/Animation)
└─ ReadyPrompt (Text: "Press Ready when you're set")
```

### Set End / Show End Screens
```
SetEndScreen (Canvas)
├─ Background
├─ Title (Text: "Set Break" or "Show Complete")
├─ Message (Text: customizable)
└─ StatusText (Text: "Waiting to resume...")
```

---

## Configuration Files

### Updated songsync_mapping.json Schema
```json
{
  "songs": [
    {
      "trackName": "Song 1",
      "chartPath": "charts/song1.chart",
      "timelineStartTime": 0.0,
      "showNextSong": true
    },
    {
      "trackName": "Surprise Song",
      "chartPath": "charts/surprise.chart",
      "timelineStartTime": 240.5,
      "showNextSong": false
    }
  ]
}
```

### Show Flow Settings (Editor Settings or Config)
```json
{
  "showSettings": {
    "showNextSongName": true,
    "showPlayerName": false,
    "autoTransitionToResults": true,
    "demoModeEnabled": false
  },
  "midiTriggers": {
    "playerSwap": 124,
    "playerReady": 125,
    "bandReady": 126,
    "songComplete": 127,
    "setEnd": 122,
    "showEnd": 121,
    "noPlayerMode": 120
  }
}
```

---

## Implementation Phases

### Phase 1: Core Results Flow (PRIORITY 1)
**Goal:** Player can finish song, see stats, continue to next song

**Tasks:**
1. Create `ResultsState.cs` class
2. Create `ResultsUISystem.cs` and UI prefab
3. Modify `GameplayStateSystem` to detect song end
4. Create `ShowFlowManager.cs` singleton
5. Implement stats display (hit %, streak, notes)
6. Implement "Continue" button → WaitingForBand transition
7. Test: Song end → Results → Continue flow

**Estimated Time:** 2-3 days

---

### Phase 2: Band Ready System (PRIORITY 2)
**Goal:** Band can trigger "ready" to start next song

**Tasks:**
1. Create `WaitingForBandState.cs`
2. Create `WaitingForBandUISystem.cs` and UI prefab
3. Implement MIDI input handler for band ready trigger
4. Add OSC handler in ExternalSyncManager for `/band/ready`
5. Implement band ready → Start Next Song transition
6. Add OSC state broadcasts (`/game/state`)
7. Test: Full flow from song end → results → waiting → band ready → next song

**Estimated Time:** 2 days

---

### Phase 3: Player Swap System (PRIORITY 3)
**Goal:** Support player swapping between songs

**Tasks:**
1. Create `WaitingForSwapState.cs`
2. Create `WaitingForSwapUISystem.cs` and UI prefab
3. Implement player swap trigger (MIDI/OSC)
4. Implement player ready trigger
5. Implement swap → waiting for band flow
6. Add band override for player ready
7. Test: Swap trigger → swap screen → player ready → band ready → play

**Estimated Time:** 2 days

---

### Phase 4: Demo Mode & No Player (PRIORITY 4)
**Goal:** Support "no player" visual backdrop mode

**Tasks:**
1. Add demo mode flag to `GameplayStateSystem`
2. Create "No Player" transition UI (optional)
3. Implement no player mode trigger
4. Test: Demo mode gameplay (BotGameplayRulestate)
5. Verify all notes are auto-hit
6. Test visual appearance for backdrop use

**Estimated Time:** 1 day

---

### Phase 5: Set End & Show End (PRIORITY 5)
**Goal:** Support set breaks and show ending

**Tasks:**
1. Create `SetEndState.cs` and `ShowEndState.cs`
2. Create UI prefabs for both
3. Implement triggers (MIDI/OSC)
4. Implement manual resume from set end
5. Implement show end → editor exit
6. Test: Set end and show end flows

**Estimated Time:** 1 day

---

### Phase 6: Next Song Display Logic (PRIORITY 6)
**Goal:** Support showing/hiding "Next Up" song name

**Tasks:**
1. Update `SongMappingManager` to parse `showNextSong`
2. Implement global setting toggle
3. Implement per-song override logic
4. Add UI toggle to results screen
5. Test: Global ON + per-song OFF (surprise songs)

**Estimated Time:** 1 day

---

### Phase 7: Polish & Testing (PRIORITY 7)
**Goal:** Full integration testing and polish

**Tasks:**
1. Full show run-through testing
2. MIDI trigger verification
3. OSC message verification
4. Update MIDI_TESTING_CHECKLIST.md with new tests
5. Update MESSAGE_REFERENCE.md with all new messages
6. UI polish and animations
7. Error handling and edge cases

**Estimated Time:** 2-3 days

---

## Testing Plan

### Unit Tests
- State transitions work correctly
- Stats are calculated properly
- Player mode changes correctly
- MIDI/OSC triggers fire correct events

### Integration Tests
- Full song → results → next song flow
- Player swap flow
- Demo mode flow
- Set/show end flows
- External triggers (AbleSet) work correctly

### Show Simulation Tests
**Test setlist:**
1. Song 1 (Player A continues)
2. Song 2 (Player A continues)
3. Song 3 (Player swap to Player B)
4. Song 4 (Player B continues)
5. Set break
6. Song 5 (No player - demo mode)
7. Song 6 (Player C joins)
8. Song 7 (Show end)

Run through full simulated show, verify all transitions.

---

## Files to Create

### New C# Scripts
- `ShowFlowManager.cs` - Central coordinator
- `ResultsState.cs` - Results screen state
- `WaitingForBandState.cs` - Band ready waiting state
- `WaitingForSwapState.cs` - Player swap state
- `SetEndState.cs` - Set end state
- `ShowEndState.cs` - Show end state
- `ResultsUISystem.cs` - Results UI system
- `WaitingForBandUISystem.cs` - Band waiting UI
- `WaitingForSwapUISystem.cs` - Swap UI
- `NextSongPreloader.cs` - Background preload next song

### New Unity Prefabs
- `ResultsScreen.prefab`
- `WaitingForBandScreen.prefab`
- `WaitingForSwapScreen.prefab`
- `SetEndScreen.prefab`
- `ShowEndScreen.prefab`

### Modified Files
- `ChartEditor.cs` - Add new State enum values
- `GameplayStateSystem.cs` - Add song end detection, expose stats
- `ExternalSyncManager.cs` - Add OSC event forwarding
- `SongMappingManager.cs` - Add showNextSong parsing (if file exists)

### Documentation Updates
- `MESSAGE_REFERENCE.md` - Add all new MIDI/OSC messages
- `MIDI_TESTING_CHECKLIST.md` - Add test cases for new features
- `DAW_SYNC_SETUP.md` - Document new OSC messages for AbleSet setup

---

## Risk Mitigation

### Risk 1: MIDI Input System Unknown
**Risk:** We don't know how MIDI Note input is currently handled  
**Mitigation:** Phase 1 can use OSC only, investigate MIDI system during Phase 2  
**Fallback:** OSC-only for all triggers (Ableton-based)

### Risk 2: SongMappingManager May Not Exist
**Risk:** Unclear if song mapping system is already implemented  
**Mitigation:** Investigate existing code, create if needed  
**Fallback:** Hard-code next song lookup for v1

### Risk 3: UI System Performance
**Risk:** Loading/unloading UI prefabs may cause frame drops  
**Mitigation:** Preload UI prefabs, use object pooling  
**Test:** Performance test during Phase 7

### Risk 4: State Transition Edge Cases
**Risk:** Unexpected state transitions may cause crashes  
**Mitigation:** Add state transition validation, logging  
**Test:** Comprehensive edge case testing in Phase 7

---

## Open Questions

1. **MIDI Input System**: How are MIDI Note On events currently received during gameplay?
   - Need to investigate existing MIDI input handling
   - May need to create new MIDI input listener system

2. **SongMappingManager**: Does it exist? What's its current structure?
   - Need to search codebase for song mapping/setlist management
   - May need to create from scratch

3. **Next Song Preloading**: How to preload next song without disrupting current state?
   - Investigate chart loading system
   - May need background loading thread

4. **Player Name Integration**: What external system provides player names?
   - Low priority, but need to understand integration points
   - Likely web API or database integration (future)

5. **FCB1010 Foot Controller**: What MIDI channel/notes does it send?
   - User will need to configure
   - Document recommended mappings

---

## Success Criteria

### Minimum Viable Product (MVP)
- ✅ Song ends, shows results screen
- ✅ Results show hit %, streak, notes hit/total
- ✅ Player can click Continue
- ✅ System waits for band ready
- ✅ Band can trigger ready via OSC
- ✅ Next song starts

### Full Feature Set
- ✅ Player swap flow works
- ✅ Demo mode (no player) works
- ✅ Set end / show end screens work
- ✅ Next song name display (with global + per-song control)
- ✅ All triggers work (both MIDI and OSC)
- ✅ State broadcasts via OSC for external systems
- ✅ All documentation updated

---

## Next Steps

1. **Confirm understanding** - Review this design with user
2. **Answer open questions** - Investigate MIDI input, SongMappingManager
3. **Start Phase 1** - Begin implementation of ResultsState
4. **Iterative development** - Build and test each phase sequentially
5. **Integration testing** - Full show simulation at end

---

**Ready to begin implementation? Review this document and confirm the approach.**
