# PartyHero YARG - Development TODO

## Completed ✅

- [x] Core setlist data structures (SetlistData, SetData, SetlistSongEntry)
- [x] PartyHero state tracking in PersistentState
- [x] Show flow state machine base architecture
- [x] Four show flow states implemented:
  - [x] WaitingForBandState
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

## High Priority 🔴

### Unity Scene Setup
- [ ] Open Score scene in Unity editor
- [ ] Add PartyHeroController GameObject
- [ ] Create UI canvases for four show flow states
  - [ ] Waiting For Band canvas
  - [ ] Waiting For Swap canvas
  - [ ] Set End canvas
  - [ ] Show End canvas
- [ ] Wire up UI references in PartyHeroScoreController
- [ ] Test UI display in editor

### Menu Integration
- [ ] Create setlist picker UI in menu
- [ ] Add "Play PartyHero Show" option
- [ ] File browser for loading setlist JSON
- [ ] Show setlist validation results in UI
- [ ] Preview setlist before starting show

### Testing & Polish
- [ ] Test full show flow with 2-song setlist
- [ ] Verify state transitions work correctly
- [ ] Test player swap functionality
- [ ] Test set break functionality
- [ ] Test show end state
- [ ] Verify song loading between states

## Medium Priority 🟡

### UI Polish
- [ ] Design attractive UI layouts for each state
- [ ] Add animations/transitions between states
- [ ] Visual indication of ready states (checkmarks, colors)
- [ ] Countdown timers with visual progress
- [ ] Show progress indicator (Song X of Y)
- [ ] Next song preview with album art

### Enhanced Setlists
- [ ] Song lookup by name (not just hash)
- [ ] Difficulty selection per song
- [ ] Optional: Auto-difficulty based on player skill
- [ ] Setlist templates/presets
- [ ] Export show stats to JSON after completion

### Error Handling
- [ ] Graceful handling of missing songs
- [ ] Fallback if setlist invalid mid-show
- [ ] Recovery from state errors
- [ ] Save show progress (resume if crash)

## Low Priority 🟢

### DAW/MIDI Sync
- [ ] MIDI input listener component
- [ ] OSC message receiver
- [ ] Band ready trigger via MIDI note
- [ ] Force state change via MIDI/OSC
- [ ] Sync start/stop with DAW
- [ ] MIDI clock integration

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

1. Should we add PartyHero toggle in settings?
2. How to handle bot players in show mode?
3. Should practice mode work in shows?
4. What happens if player quits mid-show?
5. Should we auto-save show progress?

## Next Steps

**Immediate (This Week):**
1. Open Unity and set up Score scene with PartyHeroController
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
