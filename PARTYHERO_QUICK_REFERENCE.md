# PartyHero Quick Reference Card

## What is PartyHero?

Continuous show flow system for rhythm games. No menus between songs - shows flow like real concerts with band coordination, player swaps, and set breaks.

## Quick Start

1. Create setlist JSON (see `partyhero_setlist_example.json`)
2. Load setlist in code (menu integration coming)
3. Play show - it flows automatically!

## Development Keyboard Shortcuts

**Press these during show flow states (not during gameplay):**

### Waiting For Band
- `SPACE` - Player ready (toggle)
- `B` - Band ready (simulates MIDI)
- `R` - Resume now (skip waiting)

### Waiting For Swap
- `SPACE` - New player ready (after minimum time)
- `R` - Force resume

### Set Break
- `R` - Resume show
- `ESC` - End show early

### Show End
- `ESC` - Back to menu

## Show Flow Diagram

```
Gameplay → Results → [Show Flow State] → Next Song → Loop...
                            ↓
                    ┌───────┴────────┐
                    │ Which state?   │
                    └───────┬────────┘
                            │
         ┌──────────────────┼──────────────────┐
         ↓                  ↓                  ↓
    Last Song?         Player Swap?       Set Break?
         │                  │                  │
         ↓                  ↓                  ↓
    Show End         Waiting Swap         Set End
                                              
    Otherwise → Waiting For Band
```

## Files Created

```
Assets/Script/PartyHero/
├── SetlistData.cs              # Data structures
├── ShowFlowStateMachine.cs     # State machine
├── ShowFlowStates.cs           # 4 states
├── SetlistManager.cs           # Load/validate
├── ShowFlowUIManager.cs        # UI control
└── PartyHeroScoreController.cs # Score scene integration

Docs:
├── PARTYHERO_YARG_GUIDE.md    # Full setup guide
├── PARTYHERO_TODO.md          # What's left
└── partyhero_setlist_example.json
```

## Code Snippets

### Load Setlist

```csharp
using YARG.PartyHero;

var setlist = SetlistManager.LoadSetlist("path/to/setlist.json");
var validation = SetlistManager.ValidateSetlist(setlist);

if (validation.isValid) {
    var showSongs = SetlistManager.ConvertToShowSongs(setlist);
    // ... set up state and start show
}
```

### Check Show State

```csharp
if (GlobalVariables.State.IsPartyHeroMode) {
    // PartyHero mode active
}

if (GlobalVariables.State.PlayingAShow) {
    // Playing a show (YARG's native flag)
}
```

### Manual State Control

```csharp
var partyHero = GlobalVariables.State.PartyHero;
bool lastSong = partyHero.IsLastSongInShow();
bool needsSwap = partyHero.ShouldEnterPlayerSwap();
bool needsBreak = partyHero.ShouldEnterSetBreak();
```

## Setlist JSON Keys

```json
{
  "showName": "Show title",
  "venue": "Venue name",
  "date": "YYYY-MM-DD",
  "endMessage": "Thank you!",
  "sets": [
    {
      "setNumber": 1,
      "songs": [
        {
          "songName": "Display name",
          "songHash": "YARG_LIBRARY_HASH",
          "difficulty": "Expert",
          "playerSwapAfter": false,
          "swapMessage": "Custom message",
          "minimumSwapTime": 10
        }
      ],
      "breakAfter": true,
      "breakMessage": "15 min break!",
      "breakDurationSeconds": 900
    }
  ]
}
```

## Console Output

Watch for these logs:
- `[PartyHero] Entering state: WaitingForBandState`
- `[PartyHero] Loading next song: Song Name`
- `[PartyHero] Ending show early`

All states output formatted banners to console!

## Common Issues

**States not transitioning?**
→ Check `developmentMode = true` in PartyHeroState

**Songs not loading?**
→ Verify song hashes with SetlistManager.ValidateSetlist()

**UI not showing?**
→ UI setup not complete yet - use console logs

**Show not starting?**
→ Check `GlobalVariables.State.PartyHero != null`

## What's Working Now

✅ **Core Logic**: All state transitions, song loading, show flow
✅ **Console Logging**: Full visibility into show flow
✅ **Dev Shortcuts**: Test without MIDI
✅ **Setlist System**: Load, validate, convert to YARG format

## What Needs Work

⚠️ **Unity UI**: Canvases exist but need scene setup
⚠️ **Menu Integration**: No in-game setlist picker yet
⚠️ **MIDI/OSC**: Development shortcuts only
⚠️ **Testing**: Needs full show testing

## Getting Help

1. Read `PARTYHERO_YARG_GUIDE.md` for full documentation
2. Check `PARTYHERO_TODO.md` for what's left to do
3. Reference `Moonscraper Reference Notes/` for original concept
4. Watch Unity console for `[PartyHero]` messages

---

**Version**: 1.0 (Initial YARG Port)
**Date**: March 24, 2026
**Status**: Core Complete, UI Pending
