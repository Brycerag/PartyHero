# Setlist Verification - Quick Start

## What Is This?

**Pre-show validation tool** that verifies your song mapping JSON matches your actual Ableton timeline **before** going live. Catches configuration errors during soundcheck, not during the show!

## How It Works

1. **Clone Hero** sends OSC commands to AbleSet: "Jump to Song A"
2. **AbleSet** cues Song A in Ableton and sends back timeline position
3. **Clone Hero** compares received position with expected position in mapping JSON
4. **Result**: ✓ PASS if positions match, ✗ FAIL if mismatch

Runs automatically for all songs in your setlist (2-3 seconds per song).

## Prerequisites

### 1. AbleSet OSC Input Configuration

**Enable AbleSet to receive commands from Clone Hero:**

1. Open **AbleSet Settings**
2. Go to **OSC → Input** tab
3. Enable: **OSC Input** ☑
4. Set **Input Port**: `39045`
5. Click **Save**

### 2. ExternalSyncManager Output Configuration

**Configure Clone Hero to send commands to AbleSet:**

1. Open Unity Editor
2. Find **ExternalSyncManager** GameObject in Hierarchy
3. In Inspector:
   - **DAW IP Address**: `127.0.0.1` (if same computer) or IP of AbleSet computer
   - **OSC Output Port**: `39045` (must match AbleSet input port)
4. Save scene

### 3. SetlistVerifier Setup

**Add verification component to scene:**

1. Create new GameObject: `SetlistVerifier`
2. Add Component: `SetlistVerifier`
3. In Inspector:
   - **Position Tolerance**: `0.1` (seconds, allows for timing precision)
   - **Cue Response Timeout**: `3.0` (how long to wait for DAW response)
   - **Delay Between Cues**: `0.5` (pause between songs to avoid overwhelming DAW)
4. Add to **DawSyncManagers** parent (for persistence)

### 4. UI Setup

**Add verification UI to DAW Sync Settings panel:**

1. Open your **DawSyncSettingsMenu** UI prefab/panel
2. Add UI elements:
   - **Button**: "Verify Setlist" → wire to `OnVerifySetlist()`
   - **Button**: "Cancel Verification" → wire to `OnCancelVerification()`
   - **Text**: Verification Status (e.g., "Ready to verify setlist")
   - **Text**: Progress Text (e.g., "Passed: 0 | Failed: 0")
   - **Scrollbar**: Progress Bar (visual progress 0-100%)
   - **Text**: Results Text (scrollable, shows detailed results)
3. Assign all references in **DawSyncSettingsMenu** Inspector
4. Save scene

## Running Verification

### Step 1: Confirm Prerequisites

- [ ] Ableton Live project loaded with full setlist
- [ ] AbleSet running with all song markers configured
- [ ] AbleSet OSC Input enabled (port 39045)
- [ ] Clone Hero External Sync connected (status: "Connected")
- [ ] Mapping JSON file loaded (shows "X/X mappings active")

### Step 2: Start Verification

1. Open **DAW Sync Settings** in Clone Hero
2. Check **Connection Status**: Should show "Connected" in green
3. Click **"Verify Setlist"** button
4. Watch progress:
   - Status updates: "Verifying... 1/5", "Verifying... 2/5", etc.
   - Progress bar fills from 0% to 100%
   - Each song takes ~3 seconds

### Step 3: Review Results

**All songs passed:**
```
✓ PASS - Welcome to the Jungle
  Expected: 0.00s | Actual: 0.00s

✓ PASS - Sweet Child O Mine
  Expected: 225.50s | Actual: 225.52s

✓ PASS - Paradise City
  Expected: 487.25s | Actual: 487.27s
```

✅ **You're ready for the show!** Print/screenshot results for reference.

**Some songs failed:**
```
✗ FAIL - Sweet Child O Mine
  Expected: 225.50s | Actual: 180.00s
  Error: Position mismatch - Expected 225.50s, got 180.00s (diff: 45.500s)
```

⚠️ **Fix required:**
1. Open Ableton Live
2. Find "Sweet Child O Mine" in timeline
3. Note exact start position (timeline ruler)
4. Open `songsync_mapping.json`
5. Update `timelineStartTime` for that song
6. Save JSON
7. Click **"Reload Mappings"** in Clone Hero
8. Click **"Verify Setlist"** again
9. Verify all songs pass

## Common Issues

### "Cannot verify - External sync not active"

**Cause:** Clone Hero not receiving OSC from AbleSet

**Fix:**
1. Enable **External Sync** toggle in DAW Sync Settings
2. Ensure AbleSet is running
3. Check AbleSet OSC **Output** is enabled (port 39043)
4. Wait for connection status to show "Connected"

### All songs fail with "Failed to send OSC cue command"

**Cause:** AbleSet not receiving commands from Clone Hero

**Fix:**
1. Check AbleSet OSC **Input** is enabled (not just output)
2. Verify port is 39045 (default)
3. Check firewall allows UDP port 39045
4. Verify IP address in ExternalSyncManager matches AbleSet computer
5. Try `localhost` if on same machine

### Track name mismatch errors

**Cause:** Song names in JSON don't match AbleSet exactly

**Example:**
```
Error: DAW returned 'Sweet Child O Mine' but expected 'Sweet Child O' Mine'
```

**Fix:**
- Open AbleSet setlist view
- Copy exact track name (spaces, capitalization, punctuation)
- Update `dawTrackName` in mapping JSON to match exactly
- Reload mappings and re-verify

### Some songs pass, some fail

**Likely cause:** You edited your Ableton timeline after creating mapping JSON

**Fix:**
1. For each failed song, measure new position in Ableton
2. Update mapping JSON with correct positions
3. Reload mappings
4. Re-verify

**Tip:** Run verification every time you rearrange songs in Ableton!

### Verification hangs/times out

**Cause:** DAW slow to respond or network latency

**Fix:**
1. In Unity Inspector: `SetlistVerifier` component
2. Increase **Cue Response Timeout** from 3.0 to 5.0 seconds
3. Increase **Delay Between Cues** from 0.5 to 1.0 seconds
4. Re-run verification

## Best Practices

### Before Every Show

1. **Load full setlist in Ableton/AbleSet** (don't verify with test project)
2. **Run verification during soundcheck** (not 5 minutes before showtime)
3. **Print or screenshot passed results** (tape to monitor for confidence)
4. **Keep backup mapping JSON** (dated versions: `songsync_mapping_2024-03-21.json`)

### After Timeline Changes

1. **Moved a song?** → Re-measure position, update JSON, verify
2. **Added count-in bars?** → Update `timelineStartTime` and `visualPreRoll`, verify
3. **Changed song order?** → Update all positions after the change, verify
4. **Removed a song?** → Remove from JSON or set `enabled: false`, verify

### During Multi-Night Tours

1. **Create setlist-specific mapping files:** `setlist_friday.json`, `setlist_saturday.json`
2. **Verify each setlist separately** before each show
3. **Keep verified versions** in case you revert to previous setlist
4. **Document changes** in mapping file comments (JSON supports `//` comments in some parsers)

### Troubleshooting Shows

If a song fails to load during the show:
1. **Check verification results** from soundcheck - was that song verified?
2. **Manual load** - disable auto-load, manually load chart for that song
3. **After show** - re-verify that song, fix mapping, test before next show

## Technical Details

### OSC Messages Used

**Sent by Clone Hero:**
- `/ableset/jump/project <string>` - Cue specific song/track in AbleSet

**Received by Clone Hero (for verification):**
- `/playback/time <float>` - Current timeline position
- `/track/name <string>` - Current track name

### Verification Logic

For each song:
1. Send `/ableset/jump/project "Song Name"` to AbleSet
2. Wait 3 seconds for DAW to cue and respond
3. Read `currentTime` and `currentTrackName` from OSC messages
4. Compare:
   - Track name must match exactly (case-insensitive)
   - Timeline position must match within 0.1s tolerance
5. Record result (PASS or FAIL with error details)

### Performance

- **Per-song time:** ~3 seconds (cue + response + delay)
- **5 song setlist:** ~15 seconds
- **10 song setlist:** ~30 seconds
- **20 song setlist:** ~60 seconds

**Tip:** Run during soundcheck while doing other setup tasks!

### Network Requirements

- **Same computer:** Localhost (`127.0.0.1`), UDP ports 39043 (output) and 39045 (input)
- **Different computers:** Same local network, no firewalls blocking UDP
- **Latency tolerance:** Works fine up to ~100ms network latency
- **Bandwidth:** Minimal (few KB per song)

## Workflow Example

### Friday Night Pre-Show

1. **Soundcheck 6:00 PM:**
   - Drummer loads Friday setlist in AbleSet
   - Tech opens Clone Hero, loads `setlist_friday.json`
   - Tech clicks "Reload Mappings" → shows "15/15 mappings active"
   - Tech clicks "Verify Setlist" → takes 45 seconds
   - Results: 14 PASS, 1 FAIL (Song #7 position off by 30 seconds)

2. **Fix Issue 6:05 PM:**
   - Tech measures Song #7 position in Ableton: 482.5s (was 512.0s in JSON)
   - Edit `setlist_friday.json`: change `"timelineStartTime": 512.0` → `482.5`
   - Save, reload mappings, re-verify
   - Results: 15 PASS, 0 FAIL ✅

3. **Document 6:10 PM:**
   - Screenshot verification results
   - Save as `verification_friday_pass.png`
   - Print and tape to drummer's monitor
   - Drummer confidence: 💯

4. **Show 8:00 PM:**
   - Everything works flawlessly
   - No manual chart loading needed
   - Banner night 🎸

### Saturday Night (Different Setlist)

1. **Drummer loads Saturday setlist** (different song order)
2. **Load `setlist_saturday.json`** (pre-configured for Saturday)
3. **Run verification** → all pass → ready to go
4. **No stress, no surprises**

## Support

### Debugging Tools

Enable **Debug OSC Messages** toggle to see all OSC traffic in console:
- Sent messages: `[ExternalSyncManager] Sent OSC: /ableset/jump/project [Song Name]`
- Received messages: `[ExternalSyncManager] OSC: /playback/time [480.25]`

### Log Files

Verification results are logged to Unity console:
- `[SetlistVerifier] Starting verification of X songs...`
- `[SetlistVerifier] Verifying 'Song Name' (expected position: X.Xs)...`
- `[SetlistVerifier] ✓ PASS - 'Song Name' at X.Xs`
- `[SetlistVerifier] ✗ FAIL - 'Song Name' - Position mismatch...`
- `[SetlistVerifier] ========== VERIFICATION COMPLETE ==========`

You can copy console logs to document verification for later reference.

---

**Ready to verify?** Follow the prerequisites above, then click "Verify Setlist" during soundcheck! 🎤🎸🥁
