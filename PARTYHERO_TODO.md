# PartyHero YARG - Development TODO

## Completed ✅

### Core Architecture
- [x] Core setlist data structures (SetlistData, SetData, SetlistSongEntry)
- [x] PartyHero state tracking in PersistentState
- [x] Show flow state machine base architecture
- [x] Four show flow states implemented:
  - [x] WaitingForBandState (with SetPlayerReady + SetBandReady)
  - [x] WaitingForSwapState
  - [x] SetEndState  
  - [x] ShowEndState
- [x] Setlist loading and validation system
- [x] Console logging for all states
- [x] Development keyboard shortcuts
- [x] Score scene integration (PartyHeroScoreController)
- [x] UI manager component (ShowFlowUIManager)
- [x] Documentation and setup guide
- [x] Example setlist JSON

### Communication System (Session 2)
- [x] MIDI input handler (MidiInputHandler.cs)
  - [x] Band ready trigger via MIDI note
  - [x] Player ready trigger via MIDI CC
  - [x] Force next song via MIDI note
  - [x] Config-driven with live reload
- [x] OSC message system (OscManager.cs)
  - [x] Bidirectional OSC communication
  - [x] State change notifications
  - [x] Song start/end messages
  - [x] Config-driven with live reload
- [x] TCP server/client (TcpManager.cs)
  - [x] Custom protocol (COMMAND:ARGS format)
  - [x] Server and client modes
  - [x] Thread-based with main thread dispatcher
  - [x] Config-driven with live reload
- [x] ShowFlowStateMachine integration
  - [x] External trigger methods (4 total)
  - [x] Communication manager initialization
  - [x] Outbound notifications (state changes, song events)

### Configuration System (Session 2)
- [x] Unified JSON config (PartyHeroConfig.cs)
  - [x] partyhero_config.json in StreamingAssets
  - [x] Singleton pattern with auto-load/save
  - [x] OnConfigReloaded event for live updates
  - [x] Four config sections (midi, osc, tcp, debug)
- [x] YARG Settings integration (SettingsManager)
  - [x] 18 PartyHero settings added
  - [x] New PartyHero tab with 4 sections
  - [x] Bidirectional sync (JSON ↔ YARG UI)
  - [x] Type-safe settings (Toggle, Int ranges, IPv4)
  - [x] Live reload support

### Menu Integration (Session 2)
- [x] PartyHeroMenuIntegration.cs
  - [x] File browser for setlist JSON selection
  - [x] Setlist validation before show start
  - [x] Error/warning dialogs for validation
  - [x] Remember last directory used
  - [x] Start show with populated state
- [x] MainMenu.PartyHero() method added
- [ ] **Unity Editor Work**: Add button to main menu UI (wire OnClick)

### Testing Infrastructure (Session 2)
- [x] Comprehensive testing checklist (PARTYHERO_TESTING.md)
  - [x] P0-P3 priority system
  - [x] 10 test categories
  - [x] 100+ individual test items
- [x] Git workflow documentation (fork + upstream)

## High Priority 🔴

### Unity Scene Setup (P0 - REQUIRED FOR TESTING)
- [ ] Open Score scene in Unity editor
- [ ] Add PartyHeroController GameObject with ShowFlowStateMachine
- [ ] Create UI canvases for four show flow states
  - [ ] Waiting For Band canvas (player/band ready indicators)
  - [ ] Waiting For Swap canvas (timer countdown)
  - [ ] Set End canvas (break message, elapsed time)
  - [ ] Show End canvas (show statistics)
- [ ] Wire up UI references in PartyHeroScoreController
- [ ] Assign TextMeshPro elements in ShowFlowUIManager Inspector
- [ ] Test UI display in editor

### Menu Integration (P0 - CODE COMPLETE)
- [x] Create menu integration code (PartyHeroMenuIntegration.cs)
- [x] Add MainMenu.PartyHero() method
- [x] File browser for loading setlist JSON
- [x] Setlist validation with error/warning dialogs
- [x] Remember last directory
- [ ] **Unity Editor**: Add "Start PartyHero Show" button to main menu canvas
- [ ] **Unity Editor**: Wire button OnClick to MainMenu.PartyHero()

### Package Installation (P1 - REQUIRED FOR OSC)
- [ ] Install OscCore package via Unity Package Manager
  - URL: https://github.com/stella3d/OscCore.git
- [ ] Verify OscManager.cs compiles with #if OSCCORE_IMPORTED enabled
- [ ] Test OSC bidirectional communication

### Testing & Polish (P1 - DEFERRED)
- [ ] Follow PARTYHERO_TESTING.md checklist
- [ ] Test full show flow with 2-song setlist
- [ ] Verify state transitions work correctly
- [ ] Test player swap functionality
- [ ] Test set break functionality
- [ ] Test show end state
- [ ] Verify song loading between states
- [ ] Test all three communication systems (MIDI/OSC/TCP)

## Medium Priority 🟡

### UI Polish (P2)
- [ ] Design attractive UI layouts for each state
- [ ] Add animations/transitions between states
- [ ] Visual indication of ready states (checkmarks, colors)
- [ ] Countdown timers with visual progress
- [ ] Show progress indicator (Song X of Y)
- [ ] Next song preview with album art

### Localization (P2)
- [ ] Add localization strings for 18 PartyHero settings
  - Settings.Setting.PartyHeroEnabled.Name/Description
  - Settings.Setting.PartyHeroMidiEnabled.Name/Description
  - (16 more settings)
- [ ] Add header strings: Settings.Header.General/MIDI/OSC/TCP
- [ ] Test settings UI with different languages

### Enhanced Setlists (P2)
- [ ] Song lookup by name (not just hash)
- [ ] Difficulty selection per song
- [ ] Optional: Auto-difficulty based on player skill
- [ ] Setlist templates/presets
- [ ] Export show stats to JSON after completion

### Error Handling (P2)
- [ ] Graceful handling of missing songs
- [ ] Fallback if setlist invalid mid-show
- [ ] Recovery from state errors
- [ ] Save show progress (resume if crash)

## Low Priority 🟢

### MIDI Hardware Integration (P3)
- [ ] Research Hidrogen/PlasticBand API
- [ ] Hook ProcessMidiNoteOn into YARG's input system
- [ ] Hook ProcessMidiCC into YARG's input system
- [ ] Test with physical MIDI controller
- [ ] Test with virtual MIDI bus
- [ ] Cross-platform MIDI testing (Windows/Mac/Linux)

### DAW Sync Advanced Features (P3)
- [x] ~~MIDI input listener component~~ (COMPLETED)
- [x] ~~OSC message receiver~~ (COMPLETED)
- [x] ~~Band ready trigger via MIDI note~~ (COMPLETED)
- [x] ~~Force state change via MIDI/OSC~~ (COMPLETED)
- [ ] Sync start/stop with DAW transport
- [ ] MIDI clock integration
- [ ] Timecode sync (SMPTE/MTC)

### Advanced Features
- [ ] Save replay of entire show
- [ ] Show statistics dashboard
- [ ] Multi-player band coordination
- [ ] Custom break screen videos/images
- [ ] Venue-specific setlist templates
- [ ] Band member intro screens

### Mobile Companion App (Future)
- [ ] WebSocket server in YARG
- [ ] Mobile web app interface
- [ ] Show current song/state
- [ ] Audience voting/interaction
- [ ] Lighting control from phone

## Nice to Have ⭐

### Developer Tools
- [ ] Setlist editor (in-game)
- [ ] Visual setlist builder
- [ ] Test mode (fast-forward through show)
- [ ] State machine visualizer
- [ ] Debug panel for show flow

### Production Features
- [ ] OBS scene switching integration
- [ ] Stream overlay generation
- [ ] Lighting (DMX) control
- [ ] Fog machine triggers
- [ ] Video projection cues

## Known Issues 🐛

None yet - just created!

## Integration Checklist

When integrating PartyHero into build:

- [ ] Verify all PartyHero scripts compile
- [ ] Test in edit mode
- [ ] Test in play mode
- [ ] Build and test standalone
- [ ] Verify no performance issues
- [ ] Check memory usage during long shows
- [ ] Test scene transitions
- [ ] Verify persistent state survives scene loads

## Documentation TODO

- [ ] Video tutorial for setup
- [ ] Step-by-step Unity scene setup instructions
- [ ] Example setlists for various show lengths
- [ ] MIDI/OSC integration guide
- [ ] Troubleshooting guide
- [ ] FAQ document

## Questions to Resolve

1. ~~Should we add PartyHero toggle in settings?~~ ✅ YES - Added PartyHeroEnabled setting
2. How to handle bot players in show mode?
3. Should practice mode work in shows?
4. What happens if player quits mid-show?
5. Should we auto-save show progress?
6. ~~Do we need both file config and UI settings?~~ ✅ YES - Dual approach implemented

## Next Steps

**Immediate (Unity Editor Work):**
1. Open Unity and set up Score scene with PartyHeroController
2. Create four UI canvases for show flow states
3. Wire up all UI references in Inspector
4. Add "Start PartyHero Show" button to main menu
5. Test basic show flow with example setlist

**After Unity Setup:**
1. Install OscCore package for OSC functionality
2. Follow PARTYHERO_TESTING.md checklist (P0 items first)
3. Test MIDI with hardware controller
4. Test OSC with DAW (Ableton + AbleSet)
5. Test TCP communication system

**Code Complete Status:**
- ✅ All core C# code written and compiling
- ✅ Communication system fully wired (MIDI/OSC/TCP)
- ✅ Configuration system complete (JSON + YARG UI)
- ✅ Menu integration code complete
- ⚠️ Unity Editor work required (scene setup, button wiring)
- ⚠️ Testing deferred per user request

**What Works Right Now (Theoretically):**
- Setlist loading and validation
- Show flow state machine
- Configuration via JSON file or YARG settings UI
- Communication trigger methods (MIDI/OSC/TCP → state machine)
- Menu file browser for setlist selection
- Keyboard shortcuts for development testing

**What Needs Unity Editor:**
- UI canvas creation and layout
- TextMeshPro element assignment
- Main menu button creation and wiring
- Scene setup and testing
2. Create basic UI canvases (can be ugly for now)
3. Test one complete show flow

**Short Term (This Month):**
1. Menu integration for setlist loading
2. UI polish and design
3. Full testing with various setlist configurations

**Long Term (Future):**
1. MIDI/OSC integration
2. Mobile companion app
3. Advanced production features

---

**Last Updated**: March 24, 2026
**Status**: Core code complete, awaiting Unity scene setup and testing
