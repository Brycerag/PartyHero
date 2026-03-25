# PartyHero for YARG - Setup and Usage Guide

## Overview

PartyHero transforms YARG into a live performance rhythm game system with continuous show flow, band coordination, and enhanced setlist management. This is a port of the PartyHero concept from Moonscraper Chart Editor to YARG.

## Key Features

- **Continuous Show Flow**: No menu navigation between songs - shows flow like real concerts
- **Band Coordination**: Waiting states between songs for tuning, talking, etc.
- **Player Swaps**: Built-in support for mid-show instrument switches
- **Set Breaks**: Formal intermissions between sets
- **Enhanced Setlists**: JSON-based show definitions with metadata
- **Development Mode**: Keyboard shortcuts for testing without MIDI/OSC
- **Show Statistics**: Track full show metrics across all sets

## Installation

### 1. Core Files Created

All PartyHero code is in `Assets/Script/PartyHero/`:

- `SetlistData.cs` - Data structures for setlists
- `ShowFlowStateMachine.cs` - State machine for show flow
- `ShowFlowStates.cs` - Four show flow states (WaitingForBand, WaitingForSwap, SetEnd, ShowEnd)
- `SetlistManager.cs` - Load and validate setlists
- `ShowFlowUIManager.cs` - UI management for show flow screens
- `PartyHeroScoreController.cs` - Score scene integration

### 2. Modified Files

- `Assets/Script/Persistent/PersistentState.cs` - Added PartyHero state tracking

### 3. Unity Scene Setup (Required)

The Score scene needs to be modified to support PartyHero:

1. Open the **Score** scene in Unity
2. Add a new GameObject called "PartyHeroController"
3. Attach the `PartyHeroScoreController` component to it
4. Create UI canvases for the four show flow states (see UI section below)
5. Wire up the UI references in the controller

## Setlist Format

Create a JSON file with the following structure:

```json
{
  "showName": "Your Show Name",
  "venue": "Venue Name",
  "date": "2026-03-24",
  "endMessage": "Thank you!",
  "sets": [
    {
      "setNumber": 1,
      "songs": [
        {
          "songName": "Song Display Name",
          "songHash": "HASH_FROM_YARG_LIBRARY",
          "difficulty": "Expert",
          "playerSwapAfter": false,
          "swapMessage": "",
          "minimumSwapTime": 10
        }
      ],
      "breakAfter": true,
      "breakMessage": "15 minute break!",
      "breakDurationSeconds": 900
    }
  ]
}
```

### Finding Song Hashes

Song hashes are unique identifiers in YARG's library. To find them:

1. Look in YARG's song database
2. Or use `SongContainer.SongsByHash` to enumerate songs
3. Song hash is a string like "a1b2c3d4..."

### Setlist Fields

**Show Level:**
- `showName` - Display name for the show
- `venue` - Venue name (optional)
- `date` - Show date (optional)
- `endMessage` - Message displayed when show ends

**Set Level:**
- `setNumber` - Set number (1, 2, etc.)
- `songs` - Array of songs in this set
- `breakAfter` - Whether to take a break after this set
- `breakMessage` - Message during break
- `breakDurationSeconds` - Break duration (suggested, not enforced)

**Song Level:**
- `songName` - Display name
- `songHash` - YARG library hash (required!)
- `difficulty` - Difficulty to play
- `playerSwapAfter` - Trigger player swap after this song
- `swapMessage` - Custom message during swap
- `minimumSwapTime` - Minimum seconds for swap (default 10)

## How to Use

### Starting a PartyHero Show

1. **Load Setlist** (currently requires code integration):
```csharp
using YARG.PartyHero;

// Load setlist from JSON
var setlist = SetlistManager.LoadSetlist("path/to/setlist.json");

// Validate it
var validation = SetlistManager.ValidateSetlist(setlist);
validation.LogResults();

if (validation.isValid)
{
    // Convert to YARG's show format
    var showSongs = SetlistManager.ConvertToShowSongs(setlist);
    
    // Set up PartyHero state
    var partyHeroState = new PartyHeroState
    {
        currentSetlist = setlist,
        overallSongIndex = 0,
        showStartTime = System.DateTime.Now,
        partyHeroMode = true,
        developmentMode = true // Enable keyboard shortcuts
    };
    
    // Set up YARG state
    GlobalVariables.State.PartyHero = partyHeroState;
    GlobalVariables.State.PlayingAShow = true;
    GlobalVariables.State.ShowSongs = showSongs;
    GlobalVariables.State.ShowIndex = 0;
    GlobalVariables.State.CurrentSong = showSongs[0];
    
    // Start gameplay
    GlobalVariables.Instance.LoadScene(SceneIndex.Gameplay);
}
```

### Development Mode Keyboard Shortcuts

When `developmentMode = true`, these keys work:

**Waiting For Band State:**
- `SPACE` - Toggle player ready
- `B` - Toggle band ready (simulates MIDI input)
- `R` - Force resume (skip waiting)

**Waiting For Swap State:**
- `SPACE` - Confirm ready (after minimum time)
- `R` - Force resume (skip minimum time)

**Set End State:**
- `R` - Resume show
- `ESC` - End show early

**Show End State:**
- `ESC` - Return to menu

### Show Flow

The show progresses through these states automatically:

1. **Gameplay** - Player plays the song
2. **Results** - Score screen shows stats (existing YARG)
3. **Show Flow State** - PartyHero takes over:
   - If last song → **Show End** (thank you screen)
   - If player swap flagged → **Waiting For Swap** (coordination)
   - If set break flagged → **Set End** (intermission)
   - Otherwise → **Waiting For Band** (between songs)
4. **Next Song** - Load next song and return to Gameplay

This cycle repeats until the show ends.

## UI Setup (Unity Editor)

The Score scene needs four UI canvases for PartyHero states. For now, you can leave these null and the system will work with console logging only.

### Future UI Layout

Each canvas should contain:

**Waiting For Band Canvas:**
- Title: "WAITING FOR BAND"
- Player ready indicator
- Band ready indicator  
- Next song name
- Instructions

**Waiting For Swap Canvas:**
- Title: "PLAYER SWAP TIME"
- Custom swap message
- Timer countdown
- Next song name
- Instructions

**Set End Canvas:**
- Title: "SET BREAK"
- Break message
- Elapsed time counter
- Instructions (R=Resume, ESC=End)

**Show End Canvas:**
- Title: "SHOW COMPLETE!"
- Thank you message
- Show statistics
- Instructions (ESC=Menu)

## Console Logging

Until UI is implemented, PartyHero outputs formatted banners to the Unity console:

```
============================
   WAITING FOR BAND
============================
Player Ready: ✓
Band Ready:   ○
Next Song: Through the Fire
============================
DEV KEYS: SPACE=Player Ready, B=Band Ready, R=Resume
============================
```

Watch the console to see show flow progression!

## Current Limitations

1. **No UI Yet**: Console logging only (UI components exist but need Unity scene setup)
2. **No MIDI/OSC**: Development keyboard shortcuts only (MIDI integration planned)
3. **Manual Setlist Loading**: No in-game setlist picker yet
4. **No DAW Sync**: External audio sync not yet implemented

## Future Enhancements

From the original PartyHero concept, these features are planned:

### DAW/Live Sync
- MIDI clock synchronization
- OSC message integration
- TCP socket support
- Live audio instead of pre-recorded

### Advanced Features
- Mobile companion app for audience
- Lighting control (DMX)
- Streaming integration (OBS)
- Recording/replay of full shows
- Multi-player local band coordination

## Development Notes

### Architecture

- **State Machine Pattern**: Show flow uses a clean state machine
- **YARG Integration**: Leverages existing `PlayingAShow` system
- **Scene-Based**: Works with YARG's scene loading
- **Non-Intrusive**: Minimal changes to existing YARG code

### Testing

To test without a full show:

1. Create a 2-song setlist
2. Enable development mode
3. Play first song (or skip to results)
4. Use keyboard shortcuts to progress through states
5. Watch console output

### Code Organization

```
Assets/Script/
├── PartyHero/              # All PartyHero code
│   ├── SetlistData.cs      # Data structures
│   ├── ShowFlowStateMachine.cs
│   ├── ShowFlowStates.cs   # Four state implementations
│   ├── SetlistManager.cs   # Load/validate setlists
│   ├── ShowFlowUIManager.cs
│   └── PartyHeroScoreController.cs
│
└── Persistent/
    └── PersistentState.cs  # Modified to add PartyHero field
```

## Troubleshooting

### "PartyHero mode not activating"
- Check that `GlobalVariables.State.PartyHero` is not null
- Verify `partyHeroMode = true` in PartyHeroState
- Ensure `PlayingAShow = true` in GlobalVariables.State

### "Songs not loading"
- Verify song hashes are correct
- Check `SetlistManager.ValidateSetlist()` output
- Ensure songs exist in YARG's library

### "States not transitioning"
- Check console for state entry/exit logs
- Verify keyboard shortcuts work (dev mode enabled?)
- Look for exceptions in Unity console

### "UI not showing"
- UI components not yet set up in Score scene
- For now, rely on console logging
- See "UI Setup" section above

## Getting Help

- Reference `Moonscraper Reference Notes/PARTYHERO_ADDITIONS_REFERENCE.md` for original concept
- Check Unity console for PartyHero log messages (prefixed with `[PartyHero]`)
- All state transitions and events are logged

## Credits

PartyHero concept and original implementation for Moonscraper Chart Editor.
Ported to YARG with adaptations for YARG's architecture.

---

**Last Updated**: March 24, 2026
**YARG Version**: v0.14
**Status**: Core implementation complete, UI and DAW sync pending
