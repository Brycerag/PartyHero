# Song Transition UX Scenarios

This document maps out the user experience for song-to-song transitions in the PartyHero live performance system. Use this to decide on the flow before implementing.

---

## Key User Personas

**The Drummer**
- Controls DAW/AbleSet (main timeline controller)
- Trigger point for song starts
- Needs visibility into system state
- May bypass screens for quick transitions

**The Guitarists/Band Members**
- May swap between songs (Guitar 1 → Guitar 2)
- Need time to swap instruments, tune, adjust
- Need to see their performance stats
- Want minimal disruption to show flow

**The Audience Flow**
- Band wants smooth transitions (not too long between songs)
- Also wants natural breaks (banter, tuning, etc.)
- Results screen could display on audience screens/projectors

**The Tech/Monitor Person**
- May need emergency controls
- Wants to see system status
- Might need to manually advance/skip

---

## Scenario 1: Normal Flow (No Player Swap)

**Context:** Same players, normal show flow

```
Song 1 Ends (chart length reached)
    ↓
DECISION: Auto-show results? Or wait for drummer trigger?
    ├─ Option A: Auto-show results immediately
    │   - Band sees stats right away
    │   - Natural break point
    │   - BUT: Band might not be ready (still catching breath)
    │
    └─ Option B: Drummer triggers results display
        - Band controls when to look at stats
        - BUT: Drummer has extra button to remember

Results Screen Displayed
    - Stats: Hit %, Streak, Notes
    - Background: Next song loading
    - Status: "Loading..." → "Ready"
    ↓
DECISION: How long to show results?
    ├─ Option A: Fixed timer (5-10 seconds)
    │   - Automatic progression
    │   - BUT: Not enough time for player swap
    │   - BUT: Too long if band wants quick transition
    │
    ├─ Option B: Wait for drummer "next" trigger
    │   - Band controls pacing
    │   - Can take as long as needed
    │   - BUT: Drummer must remember to trigger
    │
    └─ Option C: Minimum timer + drummer trigger
        - Show for at least N seconds
        - Then wait for drummer
        - Prevents accidental skips

Drummer Triggers "Start Next Song"
    ↓
Next Song Starts
```

**Questions to Answer:**
1. Should results auto-appear or drummer-triggered?
2. Minimum display time for results?
3. Can drummer skip results entirely?

---

## Scenario 2: Player Swap Between Songs

**Context:** Guitar 1 hands off to Guitar 2 mid-setlist

```
Song Ends
    ↓
Results Screen (Guitar 1's stats)
    ↓
PROBLEM: Guitar 2 needs to:
    - Plug in guitar
    - Adjust strap
    - Maybe tune
    - Get ready at screen
    ↓
DECISION: What screen state during swap?
    ├─ Option A: Stay on results indefinitely
    │   - Guitar 1's stats still showing
    │   - Guitar 2 can't prep their display
    │
    ├─ Option B: "Player Swap" screen
    │   - Clears old stats
    │   - Shows "Next: Guitar 2 - Song Title"
    │   - Gives Guitar 2 time to get ready
    │   - Status: "Waiting for player ready..."
    │
    └─ Option C: Back to editor/blank screen
        - Full reset
        - BUT: Loses show flow feeling

DECISION: Who triggers "ready"?
    ├─ Guitar 2 hits button (new guitar plugged in)
    └─ Drummer controls (band coordination)
    
Next Song Starts (with Guitar 2)
```

**Questions to Answer:**
1. Do we need a separate "Player Swap" state?
2. How does new player signal ready?
3. Can results display while player swaps happen?

---

## Scenario 3: Quick Transition (Medley/No Break)

**Context:** Two songs back-to-back, no pause

```
Song 1 Ends
    ↓
DECISION: Can drummer bypass results entirely?
    ├─ Option A: Pre-configure "skip results" flag
    │   - Set in song mapping: skipResults = true
    │   - Auto-advance to next song
    │
    ├─ Option B: Drummer holds "quick transition" button
    │   - During song end, hold button = skip results
    │   - Straight to next song
    │
    └─ Option C: Results always show (no skip)
        - Consistency over flexibility
        - BUT: Can't do medleys smoothly

Next Song Starts Immediately
```

**Questions to Answer:**
1. Should some songs skip results?
2. How to configure/trigger quick transitions?
3. Impact on preloading? (Less time to load)

---

## Scenario 4: Emergency/Error Recovery

**Context:** Something goes wrong mid-show

```
During Song or Results:
    ↓
Problems that might occur:
    - Audio desync
    - Wrong chart loaded
    - Player needs to restart
    - Equipment issue
    ↓
DECISION: What emergency controls exist?
    ├─ PlayPause key → Editor (current behavior)
    │   - Familiar
    │   - BUT: Breaks show flow completely
    │
    ├─ Special "Restart Song" trigger
    │   - Drummer/tech can restart current song
    │   - Stays in show mode
    │
    ├─ "Exit Show Mode" (deliberate action)
    │   - Hold key for 3 seconds
    │   - Prevents accidental exits
    │
    └─ No exit (must finish show)
        - Forces completion
        - BUT: Risky if problems occur
```

**Questions to Answer:**
1. How to handle emergency exits?
2. Can you restart a song mid-performance?
3. What's the "oh shit" button?

---

## Scenario 5: End of Setlist

**Context:** Final song of the night

```
Final Song Ends
    ↓
Results Screen
    ↓
DECISION: What happens after last song?
    ├─ Option A: Show results, then auto-return to Editor
    │   - Natural end point
    │   - Show is over
    │
    ├─ Option B: Show results indefinitely
    │   - Wait for drummer to end show
    │   - Allows encore songs
    │
    ├─ Option C: "Show Complete" screen
    │   - Special end-of-show display
    │   - Total stats across all songs
    │   - Manual exit to Editor
    │
    └─ Option D: Loop back to first song
        - For continuous performances
        - Multiple sets
```

**Questions to Answer:**
1. How to handle end of setlist?
2. Need total show stats?
3. Encore song workflow?

---

## Control Hierarchy Matrix

Who can do what, and when?

| Action | Drummer (DAW) | Current Player | Tech/Monitor | System Auto |
|--------|---------------|----------------|--------------|-------------|
| **During Song** |
| Pause/Stop | ✓ PlayPause | ✓ PlayPause | ✓ PlayPause | ✗ |
| End Song Early | ✓ MIDI trigger | ? | ? | ✗ |
| **Song End → Results** |
| Show Results | ? | ✗ | ? | ? Auto |
| Skip Results | ? | ✗ | ? | ? Config |
| **During Results** |
| Start Next Song | ✓ MIDI trigger | ? | ✓ | ✗ |
| Return to Editor | ✓ PlayPause hold | ✗ | ✓ PlayPause hold | ✗ |
| Restart Current | ? | ? | ? | ✗ |
| **Player Swap** |
| Enter Swap Mode | ✓ | ? | ✓ | ? Auto detect |
| Signal Ready | ? | ✓ Button press | ✓ | ✗ |

**Legend:**
- ✓ = Recommended control
- ? = Design decision needed
- ✗ = Should not have control

---

## State Machine Proposal

Based on scenarios above, potential states:

```
┌─────────────┐
│   EDITOR    │ ← Manual exit, end of show
└──────┬──────┘
       │ Load song, Start gameplay
       ↓
┌─────────────┐
│   PLAYING   │ ← Active gameplay
└──────┬──────┘
       │ Song ends (auto-detect) OR Drummer trigger
       ↓
┌─────────────┐
│  RESULTS    │ ← Stats display, preloading next song
└──────┬──────┘
       │ Drummer "next" trigger OR Auto-advance (configurable)
       ↓
  ┌────┴─────┐
  │          │
  ↓          ↓
PLAYER     PLAYING
SWAP?      (next song)
(optional)

┌─────────────┐
│ PLAYER SWAP │ ← Optional state for instrument changes
└──────┬──────┘
       │ New player ready
       ↓
┌─────────────┐
│   PLAYING   │ ← Next song starts
└─────────────┘
```

---

## Configuration Options Needed

For each song in the setlist:

```json
{
  "trackName": "Song Title",
  "chartPath": "path/to/chart.chart",
  "timelineStartTime": 0.0,
  "skipResults": false,        // NEW: Skip results screen?
  "allowPlayerSwap": false,    // NEW: Expect player change?
  "minResultsTime": 5.0,       // NEW: Minimum results display (seconds)
  "autoAdvance": false         // NEW: Auto-start next song?
}
```

---

## ✅ FINALIZED DECISIONS (Ready for Implementation)

### Core Philosophy
> **"Song ends like game, next song starts like game"**  
> Engineer out uncertainty between those points. Keep the arcade game experience intact.

---

### Flow 1: Repeat Player (Default Behavior)
**Context:** Same player continues to next song

```
Song Ends (auto-detect chart length)
    ↓
Results Screen (auto-show immediately)
    - Hit %, Streak, Notes Hit/Total
    - Optional "Next Up: [Song Name]" (global + per-song control)
    - Next/Continue button
    ↓
Player Clicks Next/Continue
    ↓
"Waiting for Band" Screen
    - Shows system is ready, waiting for band
    - Player is marked ready
    ↓
Band Ready Trigger (single MIDI/OSC input)
    ↓
Next Song Starts
```

**Assumptions:**
- Player continuing is the **default state**
- Results auto-show immediately after song end
- Player advances themselves by clicking Next/Continue
- Band ready is a single external trigger (manual control)

---

### Flow 2: Player Swap
**Context:** New person takes over the game

```
Song Ends
    ↓
Results Screen (previous player's stats)
    ↓
Player Swap Triggered (manual MIDI/OSC input or 3rd party scheduler)
    ↓
"Waiting for Player Swap" Screen
    - Visible UI indicating swap time
    - Previous player's results cleared
    - New player gets ready at screen
    ↓
New Player Ready (system tracks player state)
    ↓
"Waiting for Band" Screen
    ↓
Band Ready Trigger
    ↓
Next Song Starts
```

**Key Points:**
- Swap must be **manually triggered** each time (not pre-configured per song)
- Separate "Waiting for Player Swap" UI state
- Band has manual control to force player ready state (override)

---

### Flow 3: No Player Mode (Band Only)
**Context:** No audience member playing, band uses game as backdrop

```
Song Ends (or No Player Triggered)
    ↓
"No Player" Transition UI
    - Indicates band is moving to "band only" mode
    - Game opportunity closed for this song
    ↓
Demo Mode Gameplay
    - Game plays chart automatically
    - Every hittable note is hit perfectly
    - Acts as visual backdrop for band performance
    - Projected behind band for VFX effect
    ↓
Song Ends
    ↓
(Back to Results or continue loop)
```

**Implementation:**
- Demo mode: game engine plays chart, auto-hits all notes
- Visual-only mode, no actual player input
- Can be triggered manually during show

---

### Flow 4: Set End & Show End
**Context:** Breaks between sets or end of show

```
Regular Song Flow
    ↓
Set End Triggered (manual MIDI/OSC or AbleSet)
    ↓
"Set End" Screen
    - Custom UI for between-set breaks
    - May trigger other audience/lighting cues
    ↓
(Resume normal flow when ready)

OR

    ↓
Show End Triggered (manual MIDI/OSC or AbleSet)
    ↓
"Show End" Screen
    - Game Over style (positive context)
    - Final screen for the night
    - May show aggregate stats (future feature)
    ↓
Manual Exit to Editor
```

**Triggers:**
- Both Set End and Show End are manual input-based
- Can come from AbleSet or direct MIDI controller
- Distinct from normal song transitions

---

### Critical Answers Summary

| Decision | Answer |
|----------|--------|
| **Results trigger** | Auto-show immediately when song ends |
| **Results duration** | Player controls (clicks Next/Continue button) |
| **Quick transitions** | Medleys charted as ONE song (no special handling needed) |
| **Player swaps** | Separate "Waiting for Player Swap" state, manually triggered |
| **Band ready** | Single MIDI/OSC input (external manual control) |
| **Demo mode** | Game engine plays chart, auto-hits all notes |
| **Results content** | Hit %, Streak, Notes Hit/Total + optional Next Song name |
| **Next song display** | Global ON/OFF + per-song override from AbleSet mapping |
| **Player name** | Foundation/variables only, disabled by default (future feature) |
| **Set/Show end** | Manual triggers, distinct UI screens |
| **Chart length** | No concerns, medleys ≤3 songs work fine |

---

### Results Screen Display Logic

**"Next Up" Song Name Display:**
```
IF Global Setting "Show Next Song" = OFF
    → Never show next song name (master kill switch)
    
ELSE IF Global Setting "Show Next Song" = ON
    → Check song mapping data:
        IF showNextSong: false in AbleSet mapping
            → Hide next song name (surprise song)
        ELSE
            → Show next song name
```

**Player Name Display:**
- Foundation in place (variables, structure)
- Global ON/OFF setting (default OFF)
- Low priority, future integration with external systems

---

### State Machine (Finalized)

```
┌──────────────┐
│    EDITOR    │ ← Manual exit only (emergency or end of show)
└──────┬───────┘
       │ Start song
       ↓
┌──────────────┐
│   PLAYING    │ ← Active gameplay (player or demo mode)
└──────┬───────┘
       │ Song ends (auto-detect)
       ↓
┌──────────────┐
│   RESULTS    │ ← Stats display + Next/Continue button
└──────┬───────┘
       │ Player clicks Next/Continue
       ↓
    ┌──┴───────────────────┐
    │                      │
    ↓                      ↓
┌────────────┐      ┌────────────┐
│  WAITING   │      │  WAITING   │
│   FOR      │ OR   │   FOR      │
│ PLAYER     │      │  BAND      │
│   SWAP     │      │            │
└────┬───────┘      └─────┬──────┘
     │                    │
     │ Swap confirmed     │
     └──────────┬─────────┘
                │ Band ready trigger
                ↓
         ┌──────────────┐
         │   PLAYING    │ ← Next song
         └──────────────┘

Special States (triggered manually):
┌──────────────┐
│   SET END    │ ← Between sets
└──────────────┘

┌──────────────┐
│  SHOW END    │ ← End of night
└──────────────┘

┌──────────────┐
│  NO PLAYER   │ ← Demo mode enabled
│  (DEMO MODE) │
└──────────────┘
```

---

### Updated Configuration Schema

**songsync_mapping.json:**
```json
{
  "trackName": "Song Title",
  "chartPath": "path/to/chart.chart",
  "timelineStartTime": 0.0,
  "showNextSong": true          // NEW: Show this song on previous results? (Optional)
}
```

**Global Settings (in Editor):**
- `Show Next Song Name` (ON/OFF) - Master control
- `Show Player Name` (ON/OFF) - Future feature, default OFF
- `Demo Mode Auto-Hit Timing` - Timing window for perfect hits
- (Other existing settings remain)

---

### MIDI/OSC Message Requirements

**New Input Messages Needed:**
- `/player/swap` or MIDI Note - Trigger player swap state
- `/player/ready` or MIDI Note - Force player ready
- `/band/ready` or MIDI Note - Band ready, start next song
- `/set/end` or MIDI Note - Trigger Set End screen
- `/show/end` or MIDI Note - Trigger Show End screen
- `/game/mode/demo` or MIDI Note - Enable demo mode (no player)

**New Output Messages Needed:**
- `/game/state` - Current state (playing, results, waiting_swap, waiting_band, set_end, show_end)
- `/player/state` - Player state (active, ready, waiting, swapped)
- `/band/state` - Band state (ready, waiting)

*(All messages must be added to MESSAGE_REFERENCE.md during implementation)*

---

### FCB1010 MIDI Foot Controller Notes

**Future Integration Ideas:**
- Foot switch 1-2: Player ready, Band ready
- Foot switch 3-4: Set end, Show end
- Foot switch 5: Force player swap
- Foot switch 6: Demo mode toggle
- Expression pedal: (Future - difficulty adjustment, visual effects)

*(No special implementation needed now, just mapping notes for future)*

---

### Implementation Priorities

**Phase 1 (Core Flow):**
1. ✅ Results screen UI with stats display
2. ✅ "Waiting for Band" screen
3. ✅ Band ready input handling
4. ✅ Player continue flow (default behavior)

**Phase 2 (Player Management):**
5. ✅ "Waiting for Player Swap" screen
6. ✅ Player swap triggers and state management
7. ✅ Player ready state tracking
8. ✅ Band override for player ready

**Phase 3 (Special Modes):**
9. ✅ Demo mode (auto-hit gameplay)
10. ✅ "No Player" transition UI
11. ✅ Set End screen
12. ✅ Show End screen

**Phase 4 (Polish):**
13. ✅ "Next Up" song name display logic
14. ✅ Player name foundation (variables only)
15. ✅ All states broadcasted via OSC
16. ✅ MESSAGE_REFERENCE.md updated

---

### Ready for Implementation

All design decisions are finalized. Implementation can begin.

**Next Steps:**
1. Create detailed technical design document
2. Define C# classes and state management architecture
3. Design Unity UI prefabs for each screen state
4. Implement state machine transitions
5. Add MIDI/OSC message handlers
6. Update MESSAGE_REFERENCE.md with new messages
7. Add test cases to MIDI_TESTING_CHECKLIST.md

---

**Need help thinking through any specific scenario?** Pick one from above and we can drill into it.
