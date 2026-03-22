# Star Power to MIDI/OSC - Configuration Guide

## Overview

Moonscraper Chart Editor now detects when playback enters and exits **star power zones** (the glowing phrase sections in charts) and sends configurable MIDI CC or OSC messages to external hardware/software.

**Use Cases:**
- 🎛️ Trigger lighting effects in Ableton Live
- 🔊 Automate volume/FX changes on mixer during intense sections
- 💡 Control DMX stage lights via MIDI-to-DMX converter
- 🎨 Sync visual effects to chart events
- 📡 Send cues to other software (Max/MSP, TouchDesigner, etc.)

---

## How It Works

### Detection Logic

The editor tracks the current playback position and compares it against star power phrases in the chart:

```
Chart Timeline:
[Normal notes]...[★ Star Power Zone ★]...[Normal notes]

Playback enters zone → ACTIVATE event fired
Playback exits zone  → DEACTIVATE event fired
```

**Key Points:**
- Detection runs every frame during playback (bot mode or gameplay)
- Fires once on entry, once on exit (no spam)
- Works with multiple zones (each triggers its own activate/deactivate pair)
- State persists across instrument switches

---

## Configuration

### In Unity Inspector

**MidiOutputManager Component:**

| Setting | Default | Description |
|---------|---------|-------------|
| `starpowerActivateCCNumber` | 21 | MIDI CC number sent when entering star power zone |
| `starpowerActivateCCValue` | 127 | MIDI CC value (0-127) sent on activate |
| `starpowerDeactivateCCNumber` | 21 | MIDI CC number sent when exiting star power zone |
| `starpowerDeactivateCCValue` | 0 | MIDI CC value (0-127) sent on deactivate |
| `sendStarpowerViaOsc` | true | Also send OSC message to DAW (if ExternalSyncManager active) |

### Common Configurations

**1. Simple On/Off Switch (Default)**
```
Activate CC: 21, Value: 127
Deactivate CC: 21, Value: 0

Result: Behaves like a toggle switch - ON during zone, OFF outside
```

**2. Separate Activate/Deactivate CCs**
```
Activate CC: 50, Value: 127
Deactivate CC: 51, Value: 0

Result: Two separate controls - useful if your mixer handles them differently
```

**3. Gradual Fade (Mid-Range Values)**
```
Activate CC: 21, Value: 90
Deactivate CC: 21, Value: 30

Result: Not full on/off - allows smoother transitions if mixer interpolates
```

---

## MIDI Output Configuration

### Protocol Support

Star power messages are sent as **standard MIDI CC messages**, independent of the mixer protocol (Mackie Control, etc.).

**Message Format:**
```
Status Byte: 0xBn (Control Change, channel n)
CC Number: User-configured (e.g., 21)
CC Value: User-configured (e.g., 127 or 0)
```

### Transport Modes

Works with both transport modes:

**Local MIDI Device:**
- Messages sent to selected MIDI output device
- Use MIDI-OX or similar to monitor messages

**Network MIDI (TCP):**
- Messages sent over TCP connection
- Same byte format as local device
- Can be received by network MIDI software or bridge to hardware

---

## OSC Output Configuration

### OSC Messages Sent

When `sendStarpowerViaOsc` is enabled, **TWO** OSC messages are sent:

**1. Global Message (All Instruments)**
```
Activate:
  Address: /starpower/active
  Type: int
  Value: 1

Deactivate:
  Address: /starpower/active
  Type: int
  Value: 0
```

**2. Instrument-Specific Message**
```
Activate (example for Guitar):
  Address: /starpower/guitar/active
  Type: int
  Value: 1

Deactivate (example for Guitar):
  Address: /starpower/guitar/active
  Type: int
  Value: 0
```

**Available Instrument Addresses:**
- `/starpower/guitar/active`
- `/starpower/guitarcoop/active`
- `/starpower/bass/active`
- `/starpower/rhythm/active`
- `/starpower/keys/active`
- `/starpower/drums/active`
- `/starpower/ghllead/active` (Guitar Hero Live lead)
- `/starpower/ghlbass/active` (Guitar Hero Live bass)

**Why Two Messages?**
- **Global message**: Use for overall effects (master volume boost, main light show)
- **Instrument-specific**: Use for per-instrument routing (different lights for guitar vs drums star power, different FX per instrument)

### Ableton Live Integration

**Setup:**
1. In Clone Hero: Enable `sendStarpowerViaOsc` in MidiOutputManager
2. In Clone Hero: Ensure ExternalSyncManager is connected to Ableton
3. In Ableton: Open Max for Live or use OSC-enabled plugin/device
4. Map `/starpower/active` (global) or `/starpower/{instrument}/active` (per-instrument)

**Global Automation (applies to all instruments):**
- **Volume Automation**: Map `/starpower/active` to track volume (boost during star power)
- **Effect Send**: Map to reverb/delay send level (more FX during intense sections)
- **Master Effect**: Toggle filter/compressor on master bus

**Per-Instrument Automation (different routing per instrument):**
- **Guitar Star Power**: `/starpower/guitar/active` → Blue light scene, chorus FX on guitar track
- **Bass Star Power**: `/starpower/bass/active` → Red light scene, distortion boost
- **Drums Star Power**: `/starpower/drums/active` → Strobe effect, reverb on snare
- **Keys Star Power**: `/starpower/keys/active` → Purple lights, delay feedback increase

**Max for Live Devices:**
- Use `udpreceive` to capture OSC messages
- Route to Live parameters via `live.remote~` or direct parameter mapping
- Port must match ExternalSyncManager's OSC output port (39045 default)

**Example Max for Live Routing:**
```
udpreceive 39045
|
route /starpower/guitar/active /starpower/bass/active /starpower/drums/active
|           |                   |
[sel 1 0]   [sel 1 0]          [sel 1 0]
|           |                   |
Blue        Red                 Strobe
Lights      Lights              Lights
```

---

## Use Case Examples

### 1. Stage Lighting (MIDI to DMX)

**Hardware Setup:**
- MIDI Output (local device or TCP) → MIDI-to-DMX converter → DMX stage lights

**Configuration:**
```
starpowerActivateCCNumber: 21
starpowerActivateCCValue: 127
starpowerDeactivateCCNumber: 21
starpowerDeactivateCCValue: 0
```

**DMX Converter Mapping:**
- CC21 value 127 → Trigger preset 1 (intense red/white strobe)
- CC21 value 0 → Trigger preset 2 (normal blue ambient)

**Result:** Lights flash during star power sections!

---

### 2. Mixer FX Send (X32 / Behringer)

**Hardware Setup:**
- MIDI Output → Behringer X32 MIDI input

**X32 Configuration:**
- MIDI CC21 mapped to Aux Send 3 level (reverb/delay bus)
- CC value 0 = send muted
- CC value 127 = send at +6dB

**Result:** Extra reverb/delay during star power zones for dramatic effect

---

### 3. Ableton Volume Boost (Global)

**Setup:**
- ExternalSyncManager connected to Ableton (OSC)
- Max for Live device on Master track receiving `/starpower/active`

**Max Device Logic:**
```
udpreceive 39045
|
route /starpower/active
|
[sel 1 0]  → 1 = boost, 0 = normal
|
scale 0 1 → 0.0 3.0  (0dB to +3dB)
|
live.remote~ "Master Volume"
```

**Result:** Automatic volume boost during intense sections (any instrument)

---

### 3b. Ableton Per-Instrument Lighting

**Setup:**
- ExternalSyncManager connected to Ableton (OSC)
- Max for Live device routing star power to light scenes

**Max Device Logic:**
```
udpreceive 39045
|
route /starpower/guitar/active /starpower/bass/active /starpower/drums/active
|                               |                       |
[sel 1 0]                      [sel 1 0]              [sel 1 0]
|                               |                       |
send_osc                       send_osc               send_osc
"light_scene 1"               "light_scene 2"        "light_scene 3"
(Blue)                        (Red)                  (Strobe)
```

**Result:** 
- Guitar star power = Blue light scene
- Bass star power = Red light scene  
- Drums star power = Strobe effect

---

### 4. Video/Projection Sync

**Setup:**
- OSC messages sent to TouchDesigner or Resolume via network
- Star power triggers video clip playback or effect layer

**TouchDesigner:**
- OSC In CHOP receiving `/starpower/active`
- When value = 1: Enable particle effect layer
- When value = 0: Disable particle effect layer

**Result:** Visual effects sync perfectly with chart star power zones

---

## Testing & Troubleshooting

### Verify Detection

1. Load a chart with star power phrases (glowing sections)
2. Enable `Debug OSC Messages` in ExternalSyncManager (if using OSC)
3. Start playback (Play mode)
4. Watch Unity console:
   ```
   [MidiOutputManager] Star power ACTIVATED - CC21=127
   [MidiOutputManager] Star power DEACTIVATE - CC21=0
   ```

### Check MIDI Output

Use [MIDI-OX](http://www.midiox.com/) (Windows) or [MIDI Monitor](https://www.snoize.com/MIDIMonitor/) (Mac):

1. Configure Moonscraper to send to virtual MIDI port
2. Open MIDI monitoring tool
3. Play chart with star power zones
4. Verify CC messages appear at zone boundaries

### Check OSC Output

Use [OSC Monitor](https://github.com/kalineh/OSCMonitor) or similar:

1. Configure to listen on port 39045 (or your ExternalSyncManager output port)
2. Play chart with star power zones
3. Verify you see **TWO** OSC messages per activate/deactivate:
   - `/starpower/active 1` (global)
   - `/starpower/guitar/active 1` (instrument-specific)
   - `/starpower/active 0` (global)
   - `/starpower/guitar/active 0` (instrument-specific)

### Common Issues

**No messages sent:**
- Check `muteOutput` is false in MidiOutputManager
- Verify chart has star power phrases marked
- Confirm playback is active (not paused in editor)
- Check MIDI device connected (local mode) or TCP connected (network mode)

**Duplicate messages:**
- Should not happen - each zone only fires once on entry/exit
- If you see duplicates, check for overlapping star power phrases in chart

**Missing deactivate:**
- Check that playback fully exits the star power zone
- Verify zone length is correct (not extending to end of song)

---

## Advanced: Per-Instrument Star Power ✓ IMPLEMENTED

**OSC routing now includes per-instrument messages!** Both global and instrument-specific OSC messages are sent automatically.

### How It Works

When `sendStarpowerViaOsc` is enabled, the system sends **TWO** OSC messages on every star power activate/deactivate:

1. **Global message**: `/starpower/active` (value: 1 or 0)
2. **Instrument-specific message**: `/starpower/{instrument}/active` (value: 1 or 0)

The instrument name is automatically detected from the currently playing track.

### Available Instruments

The system sends to these addresses based on the current instrument:
- `/starpower/guitar/active`
- `/starpower/guitarcoop/active`
- `/starpower/bass/active`
- `/starpower/rhythm/active`
- `/starpower/keys/active`
- `/starpower/drums/active`
- `/starpower/ghllead/active`
- `/starpower/ghlbass/active`

### Use Cases

**Simple Global Automation** - Use `/starpower/active` if you want the same effect for all instruments:
- Master volume boost
- Overall lighting scene
- Single effect parameter

**Complex Per-Instrument Routing** - Use `/starpower/{instrument}/active` for instrument-specific effects:
- Different light colors per instrument (guitar = blue, bass = red, drums = strobe)
- Different FX chains (guitar gets chorus, bass gets distortion, drums gets reverb)
- Instrument-specific video clips
- Per-instrument mixer automation

### MIDI Per-Instrument (Manual Configuration)

MIDI currently sends the same CC for all instruments. If you need per-instrument MIDI:

**Option 1: Multiple MIDI Channels**
- Configure instrument channel map in MidiOutputManager
- Star power CC sent on instrument's channel
- Mixer/DMX controller responds based on channel

**Option 2: Different CC Numbers (Code Modification)**
- Modify `OnStarpowerActivate()` to check current instrument
- Send different CC numbers: CC21=guitar, CC22=bass, CC23=drums
- Configure mixer to respond to each CC differently

---

## API Reference

### Events (GameplayEvents)

```csharp
public MoonscraperEngine.Event starpowerActivateEvent;
public MoonscraperEngine.Event starpowerDeactivateEvent;
```

### Callbacks (MidiOutputManager)

```csharp
public void OnStarpowerActivate()
public void OnStarpowerDeactivate()
```

### OSC Message Format

```
Global Message:
  Address: /starpower/active
  Arguments: [int] 1 (activate) or 0 (deactivate)
  
Per-Instrument Message:
  Address: /starpower/{instrument}/active  (e.g., /starpower/guitar/active)
  Arguments: [int] 1 (activate) or 0 (deactivate)
  
Target: ExternalSyncManager.dawIpAddress:oscOutputPort
```

---

## Future Enhancements

Potential additions (not yet implemented):

- **Star power meter value** - Track % of star power collected (0-100%)
- **Star power usage** - Detect when player activates star power (not just zones)
- **OSC bundle support** - Send multiple OSC messages in one bundle
- **MIDI Note On/Off** - Alternative to CC for devices that prefer note-based triggers
- **Per-instrument MIDI** - Send different MIDI CC numbers based on current instrument (currently OSC only)

---

**Ready to sync your lights and effects to star power? Configure the settings above and start testing!** 🌟🎸
