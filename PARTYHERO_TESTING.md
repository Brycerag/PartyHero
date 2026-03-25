# PartyHero Testing Checklist

## Unity Scene Setup (Requires Unity Editor)
- [ ] Open Score scene
- [ ] Add PartyHeroController GameObject
- [ ] Create 4 UI canvases:
  - [ ] WaitingForBand canvas with TextMeshPro elements
  - [ ] WaitingForSwap canvas with timer display
  - [ ] SetEnd canvas with break message/timer
  - [ ] ShowEnd canvas with show statistics
- [ ] Wire ShowFlowStateMachine reference in PartyHeroScoreController
- [ ] Wire ShowFlowUIManager reference in PartyHeroScoreController
- [ ] Assign UI element references in ShowFlowUIManager Inspector

## Settings Integration
- [ ] Open Unity and verify PartyHero tab appears in Settings menu
- [ ] Test all settings save correctly to settings.json
- [ ] Test settings sync to partyhero_config.json
- [ ] Verify JSON file edits (while game running) reload properly
- [ ] Add localization strings for all 18 settings (optional but recommended)

## Configuration System
- [ ] Verify partyhero_config.json created in StreamingAssets on first run
- [ ] Test manual JSON editing (game closed)
- [ ] Test live reload (edit JSON while game running)
- [ ] Verify all three managers (MIDI/OSC/TCP) receive config updates

## Show Flow State Machine
- [ ] Load a 2-song test setlist
- [ ] Verify WaitingForBand state displays correctly
- [ ] Test keyboard shortcuts in dev mode:
  - [ ] SPACE = Toggle player ready
  - [ ] B = Toggle band ready
  - [ ] R = Force resume/skip
  - [ ] ESC = End show early
- [ ] Verify both player AND band ready triggers song load
- [ ] Test WaitingForSwap state (if setlist has player swap)
- [ ] Test SetEnd state (if setlist has set break)
- [ ] Test ShowEnd state displays stats correctly
- [ ] Verify console logging provides clear state information

## Setlist Management
- [ ] Load valid setlist JSON
- [ ] Test setlist validation catches:
  - [ ] Missing songs
  - [ ] Invalid song hashes
  - [ ] Malformed JSON
- [ ] Verify setlist preview shows song list before starting
- [ ] Test navigation through multi-set show
- [ ] Verify GetNextSong() correctly handles:
  - [ ] Normal song progression
  - [ ] Player swap detection
  - [ ] Set break detection
  - [ ] Show end detection

## MIDI Input (Requires MIDI Device or Virtual MIDI)
- [ ] Connect MIDI device
- [ ] Configure MIDI note numbers in settings
- [ ] Test Band Ready note (default: C4/60)
- [ ] Test Force Next State note (default: C#4/61)
- [ ] Test Player Ready CC (default: CC 20)
- [ ] Verify minimum velocity threshold works
- [ ] Test MIDI triggers only work in WaitingForBand state
- [ ] Enable/Disable MIDI via settings

## OSC Communication (Requires OscCore Package)
- [ ] Install OscCore via Package Manager
- [ ] Configure OSC ports in settings
- [ ] Test receiving from DAW/AbleSet:
  - [ ] /partyhero/band_ready
  - [ ] /partyhero/player_ready
  - [ ] /partyhero/force_state
  - [ ] /partyhero/sync_time
- [ ] Test sending to DAW:
  - [ ] /partyhero/song_start
  - [ ] /partyhero/song_end
  - [ ] /partyhero/state_change
- [ ] Verify bidirectional communication
- [ ] Enable/Disable OSC via settings

## TCP Communication
- [ ] Test Server Mode (listen for connections)
- [ ] Test Client Mode (connect to remote)
- [ ] Configure TCP ports in settings
- [ ] Test commands:
  - [ ] BAND_READY
  - [ ] PLAYER_READY
  - [ ] FORCE_STATE:stateName
  - [ ] SYNC_TIME:seconds
  - [ ] PING/PONG
- [ ] Verify TCP reconnection after disconnect
- [ ] Test message protocol with custom client
- [ ] Enable/Disable TCP via settings

## Menu Integration
**Status**: Code complete, Unity Editor work needed (P0)

**Unity Editor TODO** (must be done in Unity Editor):
- Add "PartyHero" button to main menu UI canvas
- Wire button's OnClick() to MainMenu.PartyHero() method
- Position button appropriately in menu layout
- Set button text: "Start PartyHero Show" or similar
- Optional: Add icon/styling to match YARG theme

**Testing Checklist**:
- [ ] "Start PartyHero Show" button appears in main menu
- [ ] Browse for setlist JSON works (FileExplorerHelper opens)
- [ ] Remembers last directory used for setlists
- [ ] Setlist validation shows errors for missing songs
- [ ] Setlist validation shows warnings for non-critical issues
- [ ] Warning dialog allows continue or cancel
- [ ] Cancel returns to menu
- [ ] Start launches gameplay with first song
- [ ] PersistentState.PartyHero populated with show data
- [ ] GlobalVariables.State.ShowSongs contains all songs

## Integration with YARG Systems
- [ ] Verify PersistentState.PartyHero persists across scenes
- [ ] Test PartyHero mode flag (IsPartyHeroMode property)
- [ ] Verify score screen transitions to show flow states
- [ ] Test song loading between states uses YARG's systems
- [ ] Verify GlobalVariables.Instance.LoadScene works
- [ ] Test with ShowSongs list population

## Edge Cases & Error Handling
- [ ] Missing setlist file
- [ ] Corrupted JSON
- [ ] Song not found in library
- [ ] Empty setlist
- [ ] Single-song show (no swaps/breaks)
- [ ] Player quits mid-show
- [ ] Device disconnect during show
- [ ] Config file missing (creates default)
- [ ] Invalid MIDI note numbers
- [ ] Invalid port numbers
- [ ] Network connection failures

## Performance & Polish
- [ ] Show runs smoothly for 2+ hour performance
- [ ] Memory usage stable during long shows
- [ ] No GC spikes during state transitions
- [ ] UI animations smooth
- [ ] Console logging not excessive
- [ ] Config file changes don't cause hitches

## Multi-Platform Testing
- [ ] Windows build
- [ ] Linux build
- [ ] macOS build
- [ ] Verify MIDI works on all platforms (via Hidrogen/PlasticBand)
- [ ] Verify TCP sockets work cross-platform
- [ ] Verify OSC works cross-platform

## Documentation Verification
- [ ] PARTYHERO_YARG_GUIDE.md accurate
- [ ] PARTYHERO_TODO.md updated
- [ ] PARTYHERO_QUICK_REFERENCE.md covers all features
- [ ] Example setlist JSON works
- [ ] README instructions clear

## Regression Testing (After YARG Updates)
- [ ] Normal YARG gameplay still works
- [ ] Settings menu not broken
- [ ] Other tabs/features unaffected
- [ ] PersistentState modifications persist
- [ ] No compile errors after merge
- [ ] Show mode toggle works

---

## Testing Priority

### P0 - Critical (Must Work)
- [ ] Show flow state machine transitions
- [ ] Setlist loading and validation
- [ ] Song progression through show
- [ ] Keyboard shortcuts (dev mode)
- [ ] Configuration system

### P1 - High (Core Features)
- [ ] Settings UI integration
- [ ] MIDI input (with hardware)
- [ ] Menu setlist browser
- [ ] UI canvases display correctly
- [ ] End-to-end 2-song show

### P2 - Medium (Enhanced Features)
- [ ] OSC bidirectional communication
- [ ] TCP server/client modes
- [ ] Player swap functionality
- [ ] Set break functionality
- [ ] Show statistics

### P3 - Low (Polish)
- [ ] Localization strings
- [ ] UI animations
- [ ] Performance optimization
- [ ] Multi-platform verification

---

**Last Updated:** March 25, 2026
**Testing Status:** Awaiting Unity scene setup and initial integration tests
