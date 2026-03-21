# MIDI Output Feature Testing Checklist

## Pre-Test Setup

- [ ] NAudio DLLs are present in `Assets/Plugins/NAudio/`:
  - [ ] NAudio.Core.dll (187,904 bytes)
  - [ ] NAudio.WinMM.dll (57,344 bytes)
- [ ] MidiOutputManager GameObject exists in scene with component attached
- [ ] MidiSettingsMenu component attached to settings panel UI
- [ ] All UI references assigned in Inspector (see Unity Wiring section below)

---

## 1. Local MIDI Device Mode

### Basic Device Detection
- [ ] Open MIDI settings panel
- [ ] Transport mode dropdown shows "Local MIDI Device" and "Network MIDI (TCP)"
- [ ] Device dropdown populates with available MIDI devices
- [ ] If no devices: shows "No MIDI devices found" and dropdown is disabled

### Device Selection & Output
- [ ] Select a MIDI device from dropdown
- [ ] Click Apply
- [ ] Check Console: should see "Opened MIDI device [X]: [DeviceName]"
- [ ] Use MIDI monitor software to verify messages are being sent
- [ ] Test hit/miss CC messages:
  - [ ] Hit a note → verify CC message sent (default: CC 20, value 127)
  - [ ] Miss a note → verify CC message sent (default: CC 20, value 0)

### Channel Configuration
- [ ] Change MIDI channel (1-16)
- [ ] Change Hit CC number (0-127)
- [ ] Change Hit CC value (0-127)
- [ ] Change Miss CC number (0-127)
- [ ] Change Miss CC value (0-127)
- [ ] Click Apply → verify changes take effect
- [ ] Trigger hit/miss → verify new CC numbers/values sent

### Mute Toggle
- [ ] Enable mute output toggle
- [ ] Trigger hit/miss → verify NO MIDI messages sent
- [ ] Disable mute toggle
- [ ] Trigger hit/miss → verify messages resume

---

## 2. Network MIDI (TCP) Mode

### Connection Setup
- [ ] Select "Network MIDI (TCP)" from transport dropdown
- [ ] Local device panel hides
- [ ] Network panel shows
- [ ] Status text shows: "Network mode selected. Press Start to connect."
- [ ] Button shows: "Start Connection"

### TCP Connection Test (Valid Endpoint)
- [ ] Set TCP host to valid IP/hostname (e.g., "127.0.0.1" or mixer IP)
- [ ] Set TCP port (e.g., 5004)
- [ ] Set retry seconds (e.g., 3.0)
- [ ] Set connect timeout (e.g., 3.0)
- [ ] Click "Start Connection" button
- [ ] Status text changes to: "Attempting connection to [host]:[port]..."
- [ ] Status text color changes to yellow (connecting)
- [ ] If connection succeeds:
  - [ ] Status text: "Connected to [host]:[port]."
  - [ ] Status text color: green
  - [ ] Button text: "Stop Connection"
- [ ] Trigger hit/miss → verify raw MIDI bytes sent over TCP

### TCP Connection Test (Invalid Endpoint)
- [ ] Set TCP host to unreachable IP (e.g., "192.168.99.99")
- [ ] Click "Start Connection"
- [ ] Status text: "Attempting connection..."
- [ ] After timeout: "Connection timed out. Will retry."
- [ ] Status text color: orange (retry wait)
- [ ] Countdown shows: "Retrying in X.Xs..."
- [ ] Verify automatic retry attempts continue

### TCP Connection Control
- [ ] While connected: click "Stop Connection"
  - [ ] Status text: "Network connection stopped."
  - [ ] Status text color: red
  - [ ] Retry attempts stop
  - [ ] Button text: "Start Connection"
- [ ] Click "Reconnect Now" while waiting for retry
  - [ ] Immediate reconnect attempt (bypasses retry delay)
- [ ] Change host/port while connected, click Apply
  - [ ] Connection restarts with new settings

### TCP Connection Lost
- [ ] Establish connection to TCP endpoint
- [ ] Disconnect the endpoint (close server, unplug cable, etc.)
- [ ] Trigger MIDI send
- [ ] Status text: "Connection lost: [error]. Will retry."
- [ ] Status text color: orange
- [ ] Verify automatic reconnect attempts begin

---

## 3. Mixer Protocol System

### Protocol Selection
- [ ] Open MIDI settings panel
- [ ] Protocol dropdown shows: "Mackie Control Universal"
- [ ] Select Mackie Control
- [ ] Click Apply
- [ ] Console shows: "Protocol changed to: Mackie Control Universal"

### Mackie Control Protocol Verification
- [ ] Use MIDI monitor to capture messages
- [ ] Send instrument mute/unmute commands
- [ ] Verify Mackie CC format:
  - [ ] MIDI channel 1 (status byte 0xB0)
  - [ ] CC 16-23 (0x10-0x17) for channels 1-8
  - [ ] Value 0x7F = mute, 0x00 = unmute

---

## 4. Per-Instrument Channel Mapping

### Default Mappings
- [ ] Open MIDI settings panel
- [ ] Verify default instrument-to-channel assignments:
  - [ ] Guitar → Channel 1
  - [ ] Bass → Channel 2
  - [ ] Rhythm → Channel 3
  - [ ] Drums → Channel 4
  - [ ] Keys → Channel 5
  - [ ] GuitarCoop → Channel 6 (disabled by default)
  - [ ] GHLiveGuitar → Channel 7 (disabled by default)
  - [ ] GHLiveBass → Channel 8 (disabled by default)

### Instrument Tracking (Auto Mute/Unmute)
- [ ] Enable "Instrument Tracking" toggle
- [ ] Connection must be active (local or TCP)
- [ ] Switch from Guitar to Bass in editor
  - [ ] MIDI monitor shows: CC 16 (Guitar channel 1) = mute (0x7F)
  - [ ] MIDI monitor shows: CC 17 (Bass channel 2) = unmute (0x00)
- [ ] Switch from Bass to Drums in editor
  - [ ] MIDI monitor shows: CC 17 (Bass channel 2) = mute (0x7F)
  - [ ] MIDI monitor shows: CC 19 (Drums channel 4) = unmute (0x00)
- [ ] Disable "Instrument Tracking" toggle
  - [ ] Switch instruments → NO mute/unmute messages sent

### Custom Channel Assignments
- [ ] Change Guitar from channel 1 to channel 5
- [ ] Click Apply
- [ ] Switch to Guitar in editor
  - [ ] MIDI monitor shows: CC 20 (channel 5) = unmute
- [ ] Switch away from Guitar
  - [ ] MIDI monitor shows: CC 20 (channel 5) = mute

### Enable/Disable Per Instrument
- [ ] Disable "Guitar" instrument
- [ ] Switch to Guitar in editor
  - [ ] NO mute/unmute messages sent for Guitar
- [ ] Switch away from Guitar to Bass
  - [ ] Bass unmute message still sent (if enabled)
- [ ] Re-enable Guitar
  - [ ] Switch to Guitar → unmute message resumes

---

## 5. Combined Scenarios

### Local MIDI + Instrument Tracking
- [ ] Transport mode: Local MIDI Device
- [ ] Protocol: Mackie Control
- [ ] Instrument Tracking: Enabled
- [ ] Local MIDI device selected and connected
- [ ] Switch instruments in editor
- [ ] MIDI monitor on local device shows Mackie mute/unmute CC messages

### TCP + Instrument Tracking
- [ ] Transport mode: Network MIDI (TCP)
- [ ] Protocol: Mackie Control
- [ ] Instrument Tracking: Enabled
- [ ] TCP connection active
- [ ] Switch instruments in editor
- [ ] TCP receiver shows raw Mackie CC bytes (3 bytes per message)

### Gameplay Hit/Miss + Instrument Tracking
- [ ] Connection active (local or TCP)
- [ ] Instrument Tracking: Enabled
- [ ] Start gameplay on Guitar
  - [ ] Hit notes → Hit CC messages sent (user-configured CC)
  - [ ] Miss notes → Miss CC messages sent
- [ ] Switch to Bass mid-gameplay (if possible in your workflow)
  - [ ] Guitar channel muted (Mackie CC)
  - [ ] Bass channel unmuted (Mackie CC)
  - [ ] Hit/miss CC messages continue on user-configured CC

---

## 6. Error Handling & Edge Cases

### Invalid Settings
- [ ] TCP host empty → Status: "TCP host is empty. Will retry after settings are fixed."
- [ ] TCP port = 0 → Status: "TCP port 0 is invalid. Will retry after settings are fixed."
- [ ] TCP port = 99999 → Status: "TCP port 99999 is invalid. Will retry after settings are fixed."
- [ ] MIDI channel = 0 → Clamped to 1
- [ ] MIDI channel = 20 → Clamped to 16
- [ ] CC numbers < 0 → Clamped to 0
- [ ] CC numbers > 127 → Clamped to 127

### Mute Output Override
- [ ] Enable mute output toggle
- [ ] Instrument tracking: Enabled
- [ ] Switch instruments → NO messages sent (mute overrides all)
- [ ] Hit/miss notes → NO messages sent
- [ ] Disable mute output → messages resume

### No MidiOutputManager Instance
- [ ] Remove MidiOutputManager from scene
- [ ] Open settings panel
- [ ] Try to apply settings
- [ ] Console warning: "No MidiOutputManager instance found in scene."
- [ ] No crashes or errors

### Connection Drop & Recovery
- [ ] Establish TCP connection
- [ ] Unplug network / kill server
- [ ] Trigger MIDI send → connection lost detected
- [ ] Status shows retry countdown
- [ ] Reconnect network / restart server
- [ ] Connection automatically re-establishes
- [ ] MIDI messages resume

---

## Unity Wiring Checklist

### MidiOutputManager GameObject
- [ ] Component attached: `MidiOutputManager`
- [ ] Transport Mode: Local MIDI Device (or Network TCP)
- [ ] Device Index: 0 (or desired device)
- [ ] TCP Host: "127.0.0.1"
- [ ] TCP Port: 5004
- [ ] Reconnect Retry Seconds: 3.0
- [ ] Connect Timeout Seconds: 3.0
- [ ] Protocol Type: MackieControl
- [ ] Enable Instrument Tracking: true
- [ ] Instrument Channel Map: populated with defaults
- [ ] MIDI Channel: 1
- [ ] Hit CC Number: 20
- [ ] Hit CC Value: 127
- [ ] Miss CC Number: 20
- [ ] Miss CC Value: 0
- [ ] Mute Output: false

### MidiSettingsMenu Component
- [ ] Component attached: `MidiSettingsMenu`
- [ ] Transport Mode Dropdown: assigned
- [ ] Device Dropdown: assigned
- [ ] Local Device Panel: assigned
- [ ] Network Panel: assigned
- [ ] TCP Host Input: assigned
- [ ] TCP Port Input: assigned
- [ ] Retry Seconds Input: assigned
- [ ] Connect Timeout Seconds Input: assigned
- [ ] Connection Status Text: assigned
- [ ] Connection Toggle Button Text: assigned
- [ ] Reconnect Button Text: assigned
- [ ] Protocol Dropdown: assigned
- [ ] Instrument Tracking Toggle: assigned
- [ ] Instrument Mapping Container: assigned (optional)
- [ ] Channel Input: assigned
- [ ] Hit CC Number Input: assigned
- [ ] Hit CC Value Input: assigned
- [ ] Miss CC Number Input: assigned
- [ ] Miss CC Value Input: assigned
- [ ] Mute Toggle: assigned
- [ ] Status colors configured (idle, connecting, retry, connected, stopped)

### UI Buttons
- [ ] "Apply" button OnClick → `MidiSettingsMenu.ApplySettings()`
- [ ] "Start/Stop Connection" button OnClick → `MidiSettingsMenu.OnConnectionTogglePressed()`
- [ ] "Reconnect Now" button OnClick → `MidiSettingsMenu.OnReconnectPressed()`
- [ ] Transport dropdown OnValueChanged → `MidiSettingsMenu.OnTransportModeChanged()`

### ExternalSyncManager GameObject
- [ ] Component attached: `ExternalSyncManager`
- [ ] OSC Port: 39043 (or custom port for your setup)
- [ ] Sync Enabled: false (enable via UI when ready)
- [ ] Debug OSC Messages: false (enable for troubleshooting)

### SongMappingManager GameObject
- [ ] Component attached: `SongMappingManager`
- [ ] Mapping File Path: "../songsync_mapping.json" (relative to project folder)
- [ ] Auto Load Enabled: false (enable via UI when ready)
- [ ] Auto Load Delay: 0.5 seconds

### DawSyncSettingsMenu Component
- [ ] Component attached: `DawSyncSettingsMenu`
- [ ] Sync Enabled Toggle: assigned
- [ ] Auto Load Songs Toggle: assigned
- [ ] OSC Port Input: assigned
- [ ] Connection Status Text: assigned
- [ ] Refresh Connection Button: assigned
- [ ] DAW Playing Status Text: assigned
- [ ] DAW Time Text: assigned
- [ ] DAW Tempo Text: assigned
- [ ] DAW Track Name Text: assigned
- [ ] Mapping File Path Text: assigned
- [ ] Reload Mappings Button: assigned
- [ ] Open Mapping File Button: assigned
- [ ] Mapping Count Text: assigned
- [ ] Debug OSC Toggle: assigned

### DAW Sync UI Buttons
- [ ] Refresh button OnClick → `DawSyncSettingsMenu.OnRefreshConnection()`
- [ ] Reload Mappings button OnClick → `DawSyncSettingsMenu.OnReloadMappings()`
- [ ] Open Mapping File button OnClick → `DawSyncSettingsMenu.OnOpenMappingFile()`
- [ ] Sync Enabled toggle OnValueChanged → `DawSyncSettingsMenu.OnSyncEnabledChanged()`
- [ ] Auto Load toggle OnValueChanged → `DawSyncSettingsMenu.OnAutoLoadChanged()`
- [ ] Debug OSC toggle OnValueChanged → `DawSyncSettingsMenu.OnDebugOscChanged()`
- [ ] OSC Port input OnEndEdit → `DawSyncSettingsMenu.OnOscPortChanged()`

---

## Hardware Testing (Optional)

### With Real MIDI Devices
- [ ] USB MIDI interface connected
- [ ] Device shows in dropdown
- [ ] Send messages → verify on hardware MIDI monitor/LED
- [ ] Keyboard/synth/module responds to messages

### With Digital Mixer (Mackie-Compatible)
- [ ] Mixer IP/port configured
- [ ] TCP connection established
- [ ] Switch instruments in editor → mixer channels mute/unmute
- [ ] Verify correct channels respond (1-8)
- [ ] Test with actual audio routed through mixer

### With AbleSet + Ableton Live (Full Band Setup)
- [ ] AbleSet configured to send OSC to Clone Hero (Settings → OSC → Output)
- [ ] OSC messages enabled for: playback state, time, tempo, track name
- [ ] Output host: Clone Hero computer IP address
- [ ] Output port: 39043 (match Clone Hero OSC port)
- [ ] Load setlist in AbleSet with songs matching Clone Hero mapping JSON
- [ ] Drummer controls playback via Ableton/AbleSet
- [ ] Clone Hero chart scrolling stays locked with Ableton playback
- [ ] Song changes in setlist auto-load charts in Clone Hero
- [ ] All band members hear identical timing (no drift over 5+ minute songs)
- [ ] MIDI mixer control (mute/unmute) works simultaneously with DAW sync

---

## Performance & Stability

- [ ] Rapid instrument switching (10+ times) → no crashes, no lag
- [ ] Long duration gameplay (10+ min) → no memory leaks, stable connection
- [ ] Rapid hit/miss triggers → all messages sent, no dropped packets
- [ ] Connection flapping (connect/disconnect repeatedly) → handles gracefully
- [ ] Scene transitions → MidiOutputManager persists (DontDestroyOnLoad)
- [ ] Multiple settings panel open/close cycles → no errors

---

## Console Log Verification

### Expected Log Messages (Success Cases)
- `[MidiOutputManager] Opened MIDI device [0]: [DeviceName]`
- `[MidiOutputManager] Connected TCP MIDI endpoint: 127.0.0.1:5004`
- `[MidiOutputManager] Protocol changed to: Mackie Control Universal`

### Expected Warnings (Error Cases)
- `[MidiOutputManager] No MIDI output devices found.`
- `[MidiOutputManager] Device index X is out of range (0-Y). Defaulting to 0.`
- `[MidiOutputManager] TCP host is empty.`
- `[MidiOutputManager] TCP port X is invalid.`
- `[MidiOutputManager] TCP connect failed: [error message]`
- `[MidiOutputManager] Connection timed out. Will retry.`
- `[MidiOutputManager] Failed to send MIDI message: [error]`
- `[MidiSettingsMenu] No MidiOutputManager instance found in scene.`

---

## 7. External DAW Synchronization (OSC)

### Pre-Test Setup
- [ ] ExternalSyncManager GameObject exists in scene with component attached
- [ ] SongMappingManager GameObject exists in scene with component attached
- [ ] DawSyncSettingsMenu component attached to DAW sync panel UI
- [ ] Song mapping JSON file created at project root: `songsync_mapping.json`
- [ ] All UI references assigned in DawSyncSettingsMenu Inspector

### OSC Connection

**Test with OSC Test Sender (e.g., OSCSend, TouchOSC, or AbleSet)**

- [ ] Open DAW Sync settings panel
- [ ] Verify default OSC port (39043) is displayed
- [ ] Enable "External Sync Enabled" toggle
- [ ] Connection status shows: "Listening on port 39043, waiting for messages..."
- [ ] Send test OSC message `/playback/time 0.5` to localhost:39043
  - [ ] Status changes to "Connected (last message X.Xs ago)"
  - [ ] Status text color: green
- [ ] Stop sending OSC messages for 3+ seconds
  - [ ] Status shows: "Connection timeout (last message X.Xs ago)"

### OSC Message Parsing

**Send various OSC messages and verify parsing:**

- [ ] Send `/playback/playing 1` → DAW status shows "Playing ▶" (green)
- [ ] Send `/playback/playing 0` → DAW status shows "Stopped ■" (red)
- [ ] Send `/playback/time 123.456` → Time display shows "Time: 02:03.456"
- [ ] Send `/tempo 140.5` → Tempo display shows "Tempo: 140.5 BPM"
- [ ] Send `/track/name "Welcome to the Jungle"` → Track display shows "Track: Welcome to the Jungle"
- [ ] Enable "Debug OSC Messages" toggle
  - [ ] Console shows each received OSC message with address and arguments
- [ ] Disable debug toggle → console logging stops

### OSC Alternative Address Patterns

**Verify alternate OSC address support:**

- [ ] Send `/playing 1` (short form) → parsed correctly
- [ ] Send `/time 5.0` (short form) → parsed correctly
- [ ] Send `/playback/tempo 130` → parsed correctly
- [ ] Send `/song/name "Test Song"` → parsed correctly

### Port Configuration

- [ ] Change OSC port from 39043 to 8000
- [ ] Click outside input field to apply
- [ ] Console shows: "OSC port changed to 8000"
- [ ] Send OSC message to port 8000 → received correctly
- [ ] Send to old port 39043 → not received (as expected)
- [ ] Change port back to 39043 for remaining tests

### Sync Time Integration

**Test chart scrolling sync with external DAW:**

- [ ] Load a chart file in Clone Hero
- [ ] Enable External Sync
- [ ] Start playback in Clone Hero (internal audio muted recommended)
- [ ] Send `/playback/time 0.0` → chart position resets to start
- [ ] Send `/playback/time 10.0` → chart jumps to 10 second mark
- [ ] Send continuous time updates (e.g., 0.0, 0.1, 0.2, 0.3...)
  - [ ] Chart scrolls smoothly following external time
  - [ ] No jittering or stuttering
- [ ] Disable External Sync
  - [ ] Chart ignores OSC time, uses internal audio

### Song Selection Mapping

**Edit `songsync_mapping.json`:**

```json
{
  "mappings": [
    {
      "dawTrackName": "Test Song 1",
      "chartFilePath": "C:/path/to/chart1/notes.chart",
      "enabled": true
    },
    {
      "dawTrackName": "Test Song 2",
      "chartFilePath": "C:/path/to/chart2/notes.chart",
      "enabled": true
    }
  ]
}
```

- [ ] Click "Open Mapping File" button
  - [ ] JSON file opens in default text editor
  - [ ] If file doesn't exist, default mappings are created
- [ ] Edit mappings with real chart paths
- [ ] Save JSON file
- [ ] Click "Reload Mappings" button
  - [ ] Console shows: "Loaded X song mappings from [path]"
  - [ ] Mapping count shows: "X/X mappings active"

### Automatic Song Loading

- [ ] Enable "Auto-Load Songs" toggle
- [ ] Make sure External Sync is enabled
- [ ] Send OSC message `/track/name "Test Song 1"`
  - [ ] After 0.5 second delay, chart loads automatically
  - [ ] Console shows: "Loading chart: C:/path/to/chart1/notes.chart"
- [ ] Send `/track/name "Test Song 2"`
  - [ ] Different chart loads after delay
- [ ] Send `/track/name "Unknown Song"`
  - [ ] Console warning: "No mapping found for track: Unknown Song"
  - [ ] Current chart remains loaded
- [ ] Disable "Auto-Load Songs" toggle
  - [ ] Send track name changes → no auto-loading occurs

### Mapping Enable/Disable

**Edit JSON to disable a mapping:**

```json
{
  "dawTrackName": "Test Song 1",
  "chartFilePath": "C:/path/to/chart1/notes.chart",
  "enabled": false
}
```

- [ ] Reload mappings
- [ ] Mapping count shows: "1/2 mappings active" (one disabled)
- [ ] Send `/track/name "Test Song 1"` with auto-load enabled
  - [ ] Console shows: "Mapping disabled for track: Test Song 1"
  - [ ] Chart does not load

### Invalid Chart Paths

**Edit JSON with non-existent path:**

- [ ] Set `chartFilePath` to fake path: `C:/nonexistent/file.chart`
- [ ] Reload mappings
- [ ] Send matching track name with auto-load enabled
  - [ ] Console error: "Chart file not found: C:/nonexistent/file.chart"
  - [ ] No crash or freeze

### Combined MIDI + DAW Sync Test

- [ ] Enable both MIDI output and DAW sync
- [ ] Connect MIDI to mixer (TCP or local device)
- [ ] Enable instrument tracking
- [ ] Send `/playback/playing 1` from DAW
- [ ] Send time updates to scroll chart
- [ ] Switch instruments in editor
  - [ ] Mackie mute/unmute messages still sent correctly
- [ ] Trigger hit/miss during playback
  - [ ] MIDI CC messages sent despite DAW sync active
- [ ] Both systems run simultaneously without conflict

### AbleSet Integration Test (Live Hardware)

**With actual AbleSet controlling Ableton Live:**

- [ ] Configure AbleSet to send OSC to Clone Hero IP:port
- [ ] Enable External Sync in Clone Hero
- [ ] Load setlist in AbleSet matching song mappings
- [ ] Play song in Ableton
  - [ ] Clone Hero status shows "Playing"
  - [ ] Chart scrolls in perfect sync with Ableton playback
  - [ ] Tempo changes in Ableton reflect in sync display
- [ ] Change to next song in AbleSet
  - [ ] Clone Hero auto-loads matching chart (if mapped)
- [ ] Stop playback in Ableton
  - [ ] Clone Hero status shows "Stopped"
- [ ] Drummer starts playback from Ableton/AbleSet
  - [ ] Clone Hero immediately starts in sync
  - [ ] Band/game stays perfectly locked throughout song

### Error Handling & Edge Cases

- [ ] Start Clone Hero without DAW/OSC sender running
  - [ ] Connection status: "Listening... waiting for messages"
  - [ ] No errors or crashes
- [ ] Send malformed OSC packets (garbage bytes) to port
  - [ ] Messages ignored gracefully
  - [ ] No crashes
- [ ] Send very high tempo (e.g., 999 BPM)
  - [ ] Clamped or handled gracefully (no crash)
- [ ] Send negative time value (e.g., -5.0)
  - [ ] Handled gracefully (no crash)
- [ ] Rapidly send 100+ OSC messages per second
  - [ ] All messages processed
  - [ ] No slowdown or memory leaks
- [ ] Change OSC port while receiving messages
  - [ ] Old listener stops
  - [ ] New listener starts on new port
  - [ ] Messages resume on new port

### Performance & Stability (DAW Sync)

- [ ] Long session (30+ minutes) with continuous OSC sync
  - [ ] No memory leaks
  - [ ] Sync remains accurate
- [ ] Rapid song changes (10+ in quick succession)
  - [ ] All songs load correctly
  - [ ] No crashes or hangs
- [ ] Scene transitions with DAW sync active
  - [ ] ExternalSyncManager persists (DontDestroyOnLoad)
  - [ ] OSC listener continues receiving

---

## 8. Console Log Verification (DAW Sync)

### Expected Log Messages (Success Cases)
- `[ExternalSyncManager] OSC receiver started on port 39043`
- `[ExternalSyncManager] OSC: /playback/time [123.456]`
- `[SongMappingManager] Loaded X song mappings from [path]`
- `[SongMappingManager] DAW track changed to: [TrackName]`
- `[SongMappingManager] Loading chart: [path]`

### Expected Warnings (Error Cases)
- `[SongMappingManager] Mapping file not found: [path]`
- `[SongMappingManager] No mapping found for track: [TrackName]`
- `[SongMappingManager] Chart file not found: [path]`
- `[SongMappingManager] Mapping disabled for track: [TrackName]`
- `[ExternalSyncManager] Failed to start OSC receiver: [error]`

---

## Future Protocol Testing Template

When adding new protocols (e.g., Behringer X32, Yamaha QL):

- [ ] Protocol appears in dropdown
- [ ] Protocol can be selected and applied
- [ ] Protocol-specific message format verified with MIDI monitor
- [ ] Mute/unmute commands work with target hardware
- [ ] Channel range appropriate for protocol (e.g., 1-32 for X32)

---

## Notes & Issues

Use this section to track problems found during testing:

- Issue:
  - Steps to reproduce:
  - Expected:
  - Actual:
  - Fix needed:

---

**Testing Date:** _______________  
**Tested By:** _______________  
**Build Version:** _______________  
**Hardware Used:** _______________
