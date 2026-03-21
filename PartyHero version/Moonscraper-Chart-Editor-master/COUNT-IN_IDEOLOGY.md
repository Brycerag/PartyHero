# Count-In Ideology - How It Actually Works

## Core Principle

**Clone Hero does NOT add count-in bars. It syncs to EXACT DAW position.**

## The Real Workflow

### Your Ableton Timeline Structure

Count-in measures are **physically in your Ableton timeline** before each song:

```
[Previous content]...[2 measures count-in][Song content]
                           ↑                    ↑
                     AbleSet cues here    Actual song starts
                     (e.g., 221s)         (e.g., 225s)
```

The count-in is what your band **actually hears in their IEMs**.

### Step-by-Step Process

**1. Song Cued (DAW Paused at Count-In)**
```
DAW Position: 221s (start of count-in, 4 seconds before song)
OSC Messages: /playback/playing 0
              /playback/time 221.0
              /track/name "Song B"

Clone Hero:
  - Loads chart
  - Calculates: 221 - 225 (timelineStartTime) = -4s
  - Shows chart FROZEN at -3s (visualPreRoll)
  - Band sees upcoming notes, gets ready
```

**2. Drummer Starts Playback**
```
Drummer hits Play
DAW starts from 221s

Band hears in IEMs: "1... 2... 3... 4..." (count-in clicks/audio)
```

**3. Clone Hero Syncs to EXACT DAW Position**
```
Time 221s: DAW position - song start = 221 - 225 = -4s
           Chart shows: -4 seconds (4 seconds before song)
           
Time 222s: 222 - 225 = -3s
           Chart shows: -3 seconds
           
Time 223s: 223 - 225 = -2s
           Chart shows: -2 seconds
           
Time 224s: 224 - 225 = -1s
           Chart shows: -1 second
           
Time 225s: 225 - 225 = 0s
           Chart shows: 0 seconds (SONG STARTS!)
```

**4. Perfect Sync Throughout**
- Band hears count-in → sees count-in on chart
- Band hears verse start → sees verse notes arrive
- Same timing, no offset, no artificial delays
- Just direct sync to DAW position

## What visualPreRoll Actually Does

### ONLY Affects Frozen Waiting State

When DAW is **PAUSED**:
- Chart freezes at `-visualPreRoll` position
- Example: `visualPreRoll: 3.0` → chart shows -3s
- Gives band time to see what's coming

When DAW is **PLAYING**:
- `visualPreRoll` is **IGNORED**
- Chart syncs to exact song-relative position
- Formula: `displayTime = currentTime - timelineStartTime`

### Example Scenario

```json
{
  "timelineStartTime": 225.0,
  "visualPreRoll": 3.0
}
```

**DAW paused at 221s:**
- Song-relative time: `221 - 225 = -4s`
- Display time: `-3s` (visualPreRoll value, frozen)

**DAW playing at 221s:**
- Song-relative time: `221 - 225 = -4s`
- Display time: `-4s` (exact position, NOT -3s!)

**DAW playing at 222s:**
- Song-relative time: `222 - 225 = -3s`
- Display time: `-3s` (exact position)

**DAW playing at 225s:**
- Song-relative time: `225 - 225 = 0s`
- Display time: `0s` (song content starts)

## Why This Design?

### Matches Live Band Experience

Your band hears this in IEMs:
```
[Count-in clicks: 1, 2, 3, 4] → [Song starts]
```

Your band sees this on chart:
```
[-4s, -3s, -2s, -1s] → [0s song starts]
```

**Same timing. Direct sync. No artificial offsets.**

### Flexible Configuration

**If you cue at exact song start (no count-in):**
- DAW paused at 225s
- Song-relative: `225 - 225 = 0s`
- Display (paused): `-3s` (visualPreRoll gives prep time)
- When playing starts: syncs to actual position

**If you cue 2 measures early (with count-in):**
- DAW paused at 221s (count-in position)
- Song-relative: `221 - 225 = -4s`
- Display (paused): `-3s` (visualPreRoll)
- When playing starts: syncs to -4s, scrolls naturally

### Universal Solution

Works for any workflow:
- ✓ Count-in in timeline (recommended for live bands)
- ✓ No count-in (cue at song start)
- ✓ Long intros in timeline
- ✓ Songs with pickups
- ✓ Songs starting mid-measure

**All cases:** Direct sync to DAW position. Simple. Predictable. Accurate.

## Common Misconceptions

### ❌ WRONG: "visualPreRoll adds offset during playback"

**Reality:** `visualPreRoll` only affects frozen waiting state. When playing, it's ignored.

### ❌ WRONG: "Clone Hero scrolls from -3s through count-in"

**Reality:** Clone Hero syncs to exact DAW position. If DAW is at -4s, chart shows -4s. If DAW is at -2s, chart shows -2s.

### ❌ WRONG: "Clone Hero adds 3 seconds before the song"

**Reality:** Clone Hero adds nothing. It displays exact DAW position. The count-in is IN your Ableton timeline.

### ✅ CORRECT: "Clone Hero is a mirror of DAW position"

When DAW plays, Clone Hero shows exactly where the DAW is, relative to the song start. That's it.

## Technical Implementation

### GetDisplayTime() Logic

```csharp
float GetDisplayTime() {
    float songRelativeTime = currentTime - currentSongTimelineStart;
    
    // Paused: show frozen pre-roll position
    if (!isPlaying) {
        return -currentSongPreRoll;  // e.g., -3.0
    }
    
    // Playing: sync to exact position
    return songRelativeTime;  // Direct sync, no offset
}
```

**That's it. Simple. Direct. Accurate.**

### Why It Works

1. **When paused:** Band sees chart frozen at reasonable prep position
2. **When playing:** Chart shows exact same timing as IEM audio
3. **No complexity:** No gradual scrolling, no offset math, just direct sync
4. **Predictable:** What drummer hears = what band sees

## Setup Requirements

### In Your Ableton Timeline

For each song, add count-in measures BEFORE the song content:

```
Song A:
  [0s-2s: Count-in clicks] [2s onwards: Song content]

Song B (in continuous timeline):
  [225s-229s: Count-in clicks] [229s onwards: Song content]
```

Count-in audio can be:
- Click track (metronome)
- Sampled drum count
- "1, 2, 3, 4" vocal cue
- Whatever your band uses in IEMs

### In Your Mapping JSON

Set `timelineStartTime` to where **song content starts** (after count-in):

```json
{
  "dawTrackName": "Song B",
  "timelineStartTime": 229.0,    // Where verse/intro starts
  "visualPreRoll": 3.0            // Frozen waiting position
}
```

**NOT** where the count-in starts (225s). The count-in is part of the song's playback, not a separate element.

### In AbleSet

Cue songs at **count-in position** (not song content start):

```
Song B marker position: 225s (count-in start)
Song B content starts: 229s
timelineStartTime in JSON: 229.0
```

When AbleSet cues Song B, it jumps to 225s. DAW pauses. Clone Hero calculates `225 - 229 = -4s`, freezes display at -3s. Band gets ready. Drummer hits play. Count-in plays. Chart syncs naturally.

## Summary

- **Count-in is IN Ableton timeline** (what band hears in IEMs)
- **Clone Hero syncs to EXACT DAW position** (no offset)
- **visualPreRoll is ONLY for frozen waiting state** (when paused)
- **When playing: direct sync, simple math, perfect accuracy**

This ideology matches live band workflow perfectly. Band hears count-in, sees count-in, plays together. No artificial delays, no software-added offsets, just pure sync.

---

**Questions?**

- "Where do I add the count-in?" → **In your Ableton timeline, before each song**
- "Does Clone Hero add pre-roll bars?" → **No, it syncs to your timeline**
- "What does visualPreRoll do?" → **Frozen waiting position only, ignored during playback**
- "Why does my chart show -4s when paused at 221s?" → **Because 221 - 225 (song start) = -4s, that's the exact position**

---

**This is the ideology. This is how it works. Simple. Direct. Accurate.** 🎸
