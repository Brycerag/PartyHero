# Quick Start: Continuous Timeline Setup

## For bands using one Ableton timeline with all songs

### Step 1: Measure Your Timeline in Ableton

1. Open your Ableton Live project with all songs
2. For each song, find the **exact start position** in the arrangement:
   - Look at the top timeline ruler
   - Note the position in **minutes:seconds:milliseconds** format
   
3. Convert to **total seconds**:
   ```
   Example positions in Ableton:
   Song 1 starts: 0:00.000     = 0 seconds
   Song 2 starts: 3:45.500     = (3×60) + 45.5 = 225.5 seconds
   Song 3 starts: 8:07.250     = (8×60) + 7.25 = 487.25 seconds
   Song 4 starts: 11:52.000    = (11×60) + 52 = 712 seconds
   ```

4. **Write these down!** You'll need them for the mapping file.

### Step 2: Get Exact Track Names from AbleSet

1. Open AbleSet with your Ableton project
2. Look at your setlist - the **exact names shown** are what AbleSet sends
3. Copy these names **exactly** (case-sensitive, spaces matter)

   Example:
   ```
   ✓ "Song 1 - Opening"      (correct)
   ✗ "song 1 - opening"      (wrong - case matters!)
   ✗ "Song 1"                (wrong - missing " - Opening")
   ```

### Step 3: Create Your Mapping File

Create: `songsync_mapping.json` (in same folder as your Unity project)

```json
{
  "mappings": [
    {
      "dawTrackName": "EXACT NAME FROM ABLESET",
      "chartFilePath": "C:/Full/Path/To/Chart/notes.chart",
      "enabled": true,
      "timelineStartTime": SECONDS_FROM_STEP_1,
      "visualPreRoll": 3.0
    }
  ]
}
```

**Real example:**

```json
{
  "mappings": [
    {
      "dawTrackName": "Welcome to the Jungle",
      "chartFilePath": "C:/Charts/GNR/WTTJ/notes.chart",
      "enabled": true,
      "timelineStartTime": 0.0,
      "visualPreRoll": 3.0
    },
    {
      "dawTrackName": "Sweet Child O Mine",
      "chartFilePath": "C:/Charts/GNR/SCOM/notes.chart",
      "enabled": true,
      "timelineStartTime": 225.5,
      "visualPreRoll": 3.0
    },
    {
      "dawTrackName": "Paradise City",
      "chartFilePath": "C:/Charts/GNR/ParadiseCity/notes.chart",
      "enabled": true,
      "timelineStartTime": 487.25,
      "visualPreRoll": 3.0
    }
  ]
}
```

### Step 4: Configure AbleSet for OSC

1. In AbleSet: **Settings → OSC → Output**
2. Enable: **OSC Output**
3. Set **Output Host**: 
   - Same computer: `localhost` or `127.0.0.1`
   - Different computer: IP address of Clone Hero machine (e.g., `192.168.1.100`)
4. Set **Output Port**: `39043`
5. Enable these messages:
   - ☑ Playback State
   - ☑ Playback Time
   - ☑ Tempo
   - ☑ Track Name
6. Set update rate: **50-100ms** (faster = smoother sync)

### Step 5: Test One Song

1. Launch Clone Hero
2. Open **DAW Sync Settings**
3. Enable **External Sync Enabled**
4. Enable **Auto-Load Songs**
5. Click **Reload Mappings**

In AbleSet:
1. Cue up your first song (use setlist or marker)
2. **Pause at song start** (typical live workflow)
3. Watch Clone Hero:
   - Status should show "Connected"
   - Chart should auto-load
   - Console shows: `Song offset set - Timeline start: X.Xs`
   - Chart displays at **-3 seconds** and **HOLDS** (ready state)
   - Band can see upcoming notes, get ready
4. When ready, drummer clicks **Play/Continue** in Ableton
5. AbleSet sends `/playback/playing 1`
6. Chart **syncs to EXACT DAW position** (which is at count-in bars in your timeline)
   - Example: If DAW starts at 2 measures before song, chart shows that exact position
   - Chart scrolls naturally as DAW plays through count-in into song
7. Band plays when notes reach strikeline (same timing as IEM count-in)
8. Verify sync stays tight throughout song!

### Live Band Workflow (Start/Stop Between Songs)

This is the **typical band workflow** with count-ins:

**Between Songs:**
1. Drummer cues next song in AbleSet
2. Ableton **PAUSES** at song start position
3. Band communicates, swaps guitars, gets ready
4. Clone Hero loads chart and shows it **frozen at -3 seconds**
5. Band sees upcoming notes (visual prep) but chart doesn't scroll yet

**Starting Song:**
1. Drummer sends MIDI continue (or clicks Play)
2. Ableton plays count-in measure (already in timeline, e.g., 2 measures before song)
3. Clone Hero syncs to EXACT DAW position and scrolls with it
   - If DAW is at -8 beats (2 measures @ 4/4), chart shows -8 beats
   - Chart displays same timing band hears in their IEMs
4. Chart reaches 0s exactly when song content starts (after count-in completes)
5. Band plays in perfect sync!

**Benefits of This Workflow:**
- ✓ Band can take breaks between songs (no rush)
- ✓ Visual preparation before notes arrive
- ✓ Drummer controls exactly when playback starts
- ✓ Count-in built into Ableton timeline (Clone Hero just syncs to it)
- ✓ Perfect sync throughout song

---

## **Understanding the Count-In Ideology**

**Key Concept:** Clone Hero does NOT add count-in bars. It syncs to EXACT DAW position.

### Your Ableton Timeline Structure

**You must have count-in bars IN your Ableton timeline:**

```
Timeline Example (Song B starting at 225s):
[Song A]...[2 measures count-in: 221-225s][Song B content: 225s onwards]
                    ↑                              ↑
              AbleSet cues here               Actual song starts
              (8 beats before)                (beat 1 of song)
```

### What Happens Step-by-Step

1. **Song is cued** (AbleSet jumps to count-in position, e.g., 221s)
   - DAW is **PAUSED** at 221s
   - OSC sends: `/playback/playing 0`, `/playback/time 221.0`, `/track/name "Song B"`
   
2. **Clone Hero loads chart and FREEZES**
   - Calculates song-relative position: `221 - 225 = -4 seconds`
   - Shows chart at `visualPreRoll` position (e.g., -3s or -4s)
   - Chart does NOT scroll - frozen so band can see what's coming
   - Band has time to get ready, check guitars, communicate

3. **Drummer starts playback** (hits Play)
   - DAW starts from 221s (count-in position)
   - OSC sends: `/playback/playing 1`, `/playback/time 221.0`
   - Band hears count-in in IEMs: "1, 2, 3, 4..."

4. **Clone Hero syncs to EXACT DAW position**
   - Time 221s: Chart shows `-4s` (4 seconds before song)
   - Time 222s: Chart shows `-3s` (3 seconds before song)
   - Time 223s: Chart shows `-2s` (2 seconds before song)
   - Time 224s: Chart shows `-1s` (1 second before song)
   - Time 225s: Chart shows `0s` (song content starts!)
   
5. **Band plays in perfect sync**
   - Everyone hears same count-in in IEMs
   - Everyone sees same count-in on chart
   - Notes scroll naturally from count-in into song

### What `visualPreRoll` Really Does

**ONLY affects the frozen waiting state:**
- When DAW is paused: Chart frozen at `-visualPreRoll` (e.g., -3s)
- When DAW is playing: **Ignored** - chart syncs to exact DAW position

**Example with `visualPreRoll: 3.0`:**
```
DAW Paused at 221s → Chart shows -3s (frozen)
Drummer hits Play
DAW at 221s → Chart shows -4s (actual position: 221 - 225 = -4)
DAW at 222s → Chart shows -3s
DAW at 223s → Chart shows -2s
...
```

**Why have `visualPreRoll` if count-in is in the timeline?**
- Band might cue song at exact start (225s), not at count-in (221s)
- `visualPreRoll: 3.0` gives visual prep even if cued at song start
- Ensures band always sees notes coming before playback starts

---

### Common Issues

**"No mapping found for track"**
- Track name doesn't match exactly
- Check Console with "Debug OSC Messages" enabled
- Copy/paste exact name from Console log into JSON

**"Chart file not found"**
- Check file path is correct (use forward slashes: `/`)
- Path must be absolute: `C:/Charts/...` not `Charts/...`
- Verify .chart file actually exists at that location

**Chart starts at wrong position**
- Double-check `timelineStartTime` in Ableton
- Make sure you converted minutes:seconds correctly
- Try adding 0.5s if consistently starting late

**Chart jumps/stutters**
- Increase AbleSet OSC update rate (try 50ms)
- Check network latency (use Ethernet not WiFi)
- Verify no firewall blocking UDP port 39043

### Pro Tips

**Use Ableton Locators:**
- Add locators at each song start position
- Name them exactly like your track names
- AbleSet can jump directly to these

**Pre-roll Customization:**
- Fast songs: `"visualPreRoll": 2.5`
- Complex intros: `"visualPreRoll": 4.0`
- Standard: `"visualPreRoll": 3.0`

**Backup Your Mapping:**
- Keep `songsync_mapping.json` in version control
- Update when you add/remove songs from timeline
- Share with band members for their own screens

### Testing Before the Gig

1. Run through entire setlist
2. Jump to random songs (not in order)
3. Verify each song:
   - ✓ Loads correct chart
   - ✓ Shows pre-roll
   - ✓ Stays synced
   - ✓ No drift after 5+ minutes
4. Test drummer-controlled song changes
5. Test mid-song jumps (if you use them)

---

**You're ready to rock! 🎸🥁**

Questions? Check [DAW_SYNC_SETUP.md](DAW_SYNC_SETUP.md) for detailed docs.
