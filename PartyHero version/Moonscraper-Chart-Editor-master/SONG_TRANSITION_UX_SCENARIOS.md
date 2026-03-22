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

## Questions to Answer Before Implementation

### Critical Flow Decisions:
1. **Results Screen Trigger**: Auto-show or drummer-triggered?
2. **Results Screen Duration**: Fixed timer, drummer-triggered, or hybrid?
3. **Quick Transitions**: Can results be skipped? How?
4. **Player Swaps**: Separate state or handled in results screen?

### Control Decisions:
5. **Who starts next song**: Drummer only, or Ableton /playback/playing, or either?
6. **Emergency exit**: PlayPause immediate, or hold-to-confirm?
7. **Restart song**: Needed? Who can trigger?

### Display Decisions:
8. **Results content**: Just stats, or also "next up" info?
9. **Player swap UI**: What does swapping player see?
10. **End of show**: Special screen or return to editor?

### Technical Decisions:
11. **Preloading priority**: Always preload, or skip if quick transition?
12. **State transitions**: Can skip states, or must go through all?
13. **OSC status messages**: What states to broadcast to Ableton/lights?

---

## Recommended Next Steps

1. **Map your ideal show flow** - Walk through a typical setlist scenario
2. **Identify must-haves vs nice-to-haves** - What's essential for v1?
3. **Define control responsibilities** - Drummer vs system vs player
4. **Sketch UI states** - What appears on screen in each state?
5. **Review with band** - Do actual performers agree with flow?

Once you've answered these questions, the implementation becomes straightforward - it's just translating your UX decisions into code.

---

**Need help thinking through any specific scenario?** Pick one from above and we can drill into it.
