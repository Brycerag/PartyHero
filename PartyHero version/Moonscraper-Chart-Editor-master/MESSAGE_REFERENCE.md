# MIDI & OSC Message Reference

Complete list of all MIDI and OSC messages used in the PartyHero/Moonscraper live performance system.

---

## OSC Messages

### Received from AbleSet (Port 39043)

| Event Name | Message | Values | Description |
|------------|---------|--------|-------------|
| Playback State | `/playback/playing` | `0` or `1` | DAW playback state (0=stopped, 1=playing) |
| Timeline Position | `/playback/time` | `float` (seconds) | Absolute timeline position in DAW |
| Tempo Change | `/tempo` | `float` (BPM) | Current tempo/BPM from DAW |
| Track Name | `/track/name` | `string` | Current track name for song mapping |
| Song Complete | `/song/complete` | none | Trigger end of song / show results screen |
| Start Next Song | `/song/start` | none | Start next song from results screen |

### Sent to AbleSet (Port 39045)

| Event Name | Message | Values | Description |
|------------|---------|--------|-------------|
| Cue Song | `/ableset/jump/project` | `string` (track name) | Jump to specific song in AbleSet project |
| Star Power Active (Global) | `/starpower/active` | `0` or `1` | Global star power state (all instruments) |
| Star Power Active (Guitar) | `/starpower/guitar/active` | `0` or `1` | Guitar star power state |
| Star Power Active (Guitar Coop) | `/starpower/guitarcoop/active` | `0` or `1` | Guitar Co-op star power state |
| Star Power Active (Bass) | `/starpower/bass/active` | `0` or `1` | Bass star power state |
| Star Power Active (Rhythm) | `/starpower/rhythm/active` | `0` or `1` | Rhythm guitar star power state |
| Star Power Active (Keys) | `/starpower/keys/active` | `0` or `1` | Keyboard star power state |
| Star Power Active (Drums) | `/starpower/drums/active` | `0` or `1` | Drums star power state |
| Star Power Active (GHL Lead) | `/starpower/ghllead/active` | `0` or `1` | Guitar Hero Live lead star power state |
| Star Power Active (GHL Bass) | `/starpower/ghlbass/active` | `0` or `1` | Guitar Hero Live bass star power state |
| Game State | `/game/state` | `string` | Current game state: "playing", "results", "ready", "loading" |

---

## MIDI Messages

### Note On/Off Messages (Triggers)

MIDI Note format: `0x9n [Note Number] [Velocity]` (Note On) or `0x8n [Note Number] [Velocity]` (Note Off)
- `n` = MIDI channel (0-15)
- Note Number = 0-127
- Velocity = 0-127 (Note On), usually 0 (Note Off)

#### Game Control Triggers (Received)

| Event Name | Note Number | Velocity | Channel | Description |
|------------|------------|----------|---------|-------------|
| Song Complete Trigger | Configurable (default: 127) | `> 0` | Any | External trigger to end song and show results |
| Start Next Song Trigger | Configurable (default: 126) | `> 0` | Any | External trigger to start next song from results |

### Control Change (CC) Messages

All MIDI messages are Control Change (CC) format: `0xBn [CC Number] [CC Value]`
- `n` = MIDI channel (0-15)
- CC Number = 0-127
- CC Value = 0-127

#### Note Hit/Miss Events

| Event Name | CC Number | CC Value | Channel | Description |
|------------|-----------|----------|---------|-------------|
| Note Hit | Configurable (default: 60) | `127` | Per-instrument* | Sent when note successfully hit |
| Note Miss | Configurable (default: 61) | `127` | Per-instrument* | Sent when note missed |

*Channel determined by `InstrumentMidiChannelMap` configuration (default: all on channel 1)

#### Star Power Events

| Event Name | CC Number | CC Value | Channel | Description |
|------------|-----------|----------|---------|-------------|
| Star Power Activate | Configurable (default: 21) | Configurable (default: 127) | Current instrument | Sent when entering star power zone |
| Star Power Deactivate | Configurable (default: 21) | Configurable (default: 0) | Current instrument | Sent when exiting star power zone |

#### Mackie Control Protocol (Mixer Muting)

| Event Name | CC Number | CC Value | Channel | Description |
|------------|-----------|----------|---------|-------------|
| Guitar Mute | `16` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 1 mute state |
| Bass Mute | `17` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 2 mute state |
| Rhythm Mute | `18` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 3 mute state |
| Keys Mute | `19` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 4 mute state |
| Drums Mute | `20` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 5 mute state |
| Guitar Coop Mute | `21` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 6 mute state |
| GHL Lead Mute | `22` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 7 mute state |
| GHL Bass Mute | `23` | `0` (unmute) / `127` (mute) | 1 | Mixer channel 8 mute state |

---

## Song End Detection & Results Screen

### Automatic Detection (Internal)

Song end is automatically detected using:
1. **Chart Manual Length** (`manualLength` field in .chart file) - preferred
2. **Audio File Length** (from audio stream) - fallback if no manual length
3. Triggers results screen automatically when playback time >= song length

### External Triggers (Optional Override)

**OSC Trigger:**
- Ableton/AbleSet sends `/song/complete` at end of track
- Drummer/band can manually trigger results screen

**MIDI Note Trigger:**
- Drummer hits MIDI pad (Note 127 by default)
- Allows manual control over song transitions

**Use Case:**
- Automatic detection handles normal flow
- External triggers allow early ending or manual control
- Both methods fire the same `songCompleteEvent`

### Results Screen Flow

```
Song Playing (automatic length detection running)
    ↓
Trigger: Auto-detect OR OSC /song/complete OR MIDI Note 127
    ↓
Show Results Screen
    - Display: Hit %, Streak, Notes Hit/Total
    - Background: Preload next song (async)
    - Status: "Loading..." → "Ready"
    ↓
Wait for Next Song Trigger
    ↓
Trigger: OSC /song/start OR MIDI Note 126 OR OSC /playback/playing = 1
    ↓
Start Next Song
```

---

## Show Flow Control Messages

The show flow system manages song transitions, player swaps, demo mode, and show state coordination. These messages are used for live performances with multiple players, band coordination, and set management.

### Received (Incoming) - Show Flow Triggers

**OSC Messages (Port 39043):**

| Message | Type | Description |
|---------|------|-------------|
| `/player/swap` | Trigger | Indicates next song will have a different player (player swap mode) |
| `/player/ready` | Trigger | Current player signals they are ready to proceed |
| `/band/ready` | Trigger | Band signals they are ready to start next song |
| `/song/complete` | Trigger | Force song end and show results screen |
| `/set/end` | Trigger | Display set end screen (between-set break) |
| `/show/end` | Trigger | Display show end screen (end of entire show) |
| `/game/mode/demo` | Trigger | Enable demo/no-player mode for next song (bot auto-play) |

**MIDI Note Triggers (Note On):**

| Note Number | Default | Event Name | Description |
|-------------|---------|------------|-------------|
| Configurable | 124 | Player Swap | Trigger player swap mode |
| Configurable | 125 | Player Ready | Signal player is ready |
| Configurable | 126 | Band Ready | Signal band is ready to start |
| Configurable | 127 | Song Complete | Force song end / results screen |
| Configurable | 122 | Set End | Display set end screen |
| Configurable | 121 | Show End | Display show end screen |
| Configurable | 120 | Demo Mode | Enable demo mode (no player) |

### Sent (Outgoing) - Show Flow State Broadcasting

**OSC Messages (Port 39045):**

| Message | Values | Description |
|---------|--------|-------------|
| `/game/state` | `string` | Current game state: "playing", "results", "waiting_swap", "waiting_band", "set_end", "show_end", "demo_mode" |
| `/player/state` | `string` | Current player state: "playing", "swapping", "ready", "ready_forced", "no_player" |
| `/band/state` | `string` | Band readiness state: "ready", "waiting" |
| `/song/stats` | `int, int, int` | Post-song statistics: notes hit, total notes, best streak |

### Show Flow State Machine

```
Song Playing
    ↓
Song Ends (auto-detect / trigger)
    ↓
Results Screen
    - Display: Stats, optional "Next: [Song Name]"
    - Broadcast: /song/stats [hit] [total] [streak]
    ↓
Player Mode Check:
    ├─ CONTINUING (same player):
    │       ↓
    │   Waiting For Band
    │       - Broadcast: /game/state "waiting_band"
    │       - Wait for: /band/ready trigger
    │       ↓
    │   Start Next Song
    │
    ├─ SWAPPING (new player):
    │       ↓
    │   Waiting For Swap
    │       - Display: "PLAYER SWAP TIME"
    │       - Broadcast: /player/state "swapping"
    │       - Wait for: /player/ready trigger
    │       ↓
    │   [Check requireBandReady setting]
    │       ├─ TRUE → Waiting For Band
    │       └─ FALSE → Start Next Song
    │
    └─ NO PLAYER (demo mode):
            ↓
        Start Next Song (bot-enabled)
            - Broadcast: /game/state "demo_mode"
            - Game auto-hits all notes
```

### Set End / Show End Flow

```
Playing or Results
    ↓
Trigger: /set/end OR MIDI Note 122
    ↓
Set End Screen
    - Display: "SET BREAK" message
    - Broadcast: /game/state "set_end"
    - Wait for manual resume or show end
    ↓
Options:
    ├─ Resume (R key / external trigger):
    │       ↓
    │   Load first song of next set
    │   Continue show flow
    │
    └─ End Show (E key / /show/end):
            ↓
        Show End Screen
            - Display: "SHOW COMPLETE" message
            - Broadcast: /game/state "show_end"
            - Manual exit to editor (ESC key)
```

### Show Flow Configuration

**ShowFlowManager Inspector Settings:**

- `Show Flow Enabled` - Master toggle for entire show flow system
- `Show Next Song Name` - Global toggle for "Next: [Song]" display on results
- `Require Band Ready` - Whether to wait for band ready signal before starting songs
- `Debug Show Flow` - Log all state transitions and triggers to console

**MIDI Trigger Note Numbers (Configurable):**

- `playerSwapNoteNumber` (default: 124)
- `playerReadyNoteNumber` (default: 125)
- `bandReadyNoteNumber` (default: 126)
- `songCompleteNoteNumber` (default: 127)
- `setEndNoteNumber` (default: 122)
- `showEndNoteNumber` (default: 121)
- `noPlayerModeNoteNumber` (default: 120)

### Development Helper Keys

**Results Screen:**
- `SPACE` - Continue to next state

**Waiting For Swap:**
- `R` - Simulate player ready trigger

**Waiting For Band:**
- `P` - Force player ready (band override)
- `B` - Trigger band ready

**Set End Screen:**
- `R` - Resume show (load next set)
- `E` - End show (to show end screen)

**Show End Screen:**
- `ESC` - Exit to editor

---

## Configuration Notes

### MIDI Output Modes

**Local Device Mode:**
- Messages sent to physical MIDI device connected to computer
- Configure device name in MidiOutputManager Inspector

**TCP Network Mode:**
- Messages sent over TCP network to remote MIDI receiver
- Configure host IP and port in MidiOutputManager Inspector
- Useful for wireless MIDI or routing to DAW on different computer

### Per-Instrument Channel Mapping

By default, all instruments send on MIDI channel 1. Configure `InstrumentMidiChannelMap` to route different instruments to different channels:

```json
{
  "guitar": 1,
  "bass": 2,
  "drums": 10,
  "keys": 3,
  "rhythm": 4
}
```

### Message Flow

```
Gameplay Event → MidiOutputManager → MIDI CC + OSC
                                    ↓           ↓
                              MIDI Device   AbleSet
                                    ↓           ↓
                              Mixer/DMX    Automation
```

---

## Use Case Examples

### Lighting Control
- **MIDI**: Star Power CC → MIDI-to-DMX converter → Stage lights
- **OSC**: `/starpower/guitar/active` → Max for Live → Light scene trigger

### Mixer Automation
- **MIDI**: Note Hit CC → X32 mixer channel EQ boost
- **OSC**: `/playback/playing` → Auto-mute unused channels

### Effect Processing
- **MIDI**: Star Power CC → Effect pedal automation
- **OSC**: `/starpower/active` → Ableton reverb send increase

### Video Sync
- **OSC**: `/starpower/{instrument}/active` → TouchDesigner/Resolume clip trigger
- **OSC**: `/playback/time` → Video timeline sync

### Results Screen Control
- **MIDI Note**: Drummer triggers results (Note 127) + starts next song (Note 126)
- **OSC**: Ableton sends `/song/complete` at track end → Results display
- **Automatic**: Chart/audio length detection → Results at song end
- **OSC**: Game sends `/game/state` to update lighting/visuals during results

---

**See Also:**
- [STARPOWER_MIDI_OSC_GUIDE.md](STARPOWER_MIDI_OSC_GUIDE.md) - Detailed star power configuration
- [DAW_SYNC_SETUP.md](DAW_SYNC_SETUP.md) - OSC sync setup guide
- [MIDI_TESTING_CHECKLIST.md](MIDI_TESTING_CHECKLIST.md) - Testing procedures
