# Network Sync Protocol Design

**Purpose:** Define how stage Unity game communicates with mobile clients for real-time gameplay sync.

---

## Message Types

### 1. Connection Messages

**CLIENT → SERVER:**
```
JOIN_ROOM
{
  roomCode: "SHOW1234",
  playerName: "Alex",
  difficulty: "easy", // easy, medium, hard
  deviceInfo: { /* ... */ }
}
```

**SERVER → CLIENT:**
```
JOIN_SUCCESS
{
  playerId: "uuid",
  serverTime: 1234567890,
  showStatus: "waiting" | "playing" | "intermission"
}
```

### 2. Clock Sync Messages

**Purpose:** Synchronize client clocks to server time for accurate note timing.

**CLIENT → SERVER (every 5 seconds):**
```
PING
{
  clientTimestamp: 1234567890
}
```

**SERVER → CLIENT:**
```
PONG
{
  clientTimestamp: 1234567890,  // echo back
  serverTimestamp: 1234567895
}
```

**Client calculates:**
- Round-trip time (RTT)
- One-way latency estimate
- Clock offset

### 3. Song Start

**UNITY → CLIENTS:**
```
SONG_START
{
  songName: "Through the Fire and Flames",
  difficulty: ["easy", "medium", "hard"], // has tracks for
  bpm: 200,
  duration: 446.5,  // seconds
  startTime: 1234567900,  // server timestamp to start
  chartData: {
    easy: [ /* note data */ ],
    medium: [ /* note data */ ],
    hard: [ /* note data */ ]
  }
}
```

**Notes:**
- Send 2-3 seconds BEFORE actual start
- Clients preload/buffer chart data
- startTime is future timestamp for synchronized start

### 4. Gameplay Sync (During Song)

**UNITY → CLIENTS (every beat or significant event):**
```
BEAT_SYNC
{
  tick: 45782,
  beat: 191,
  measure: 48,
  timestamp: 1234567920
}
```

**Use:** Clients correct drift by comparing their predicted tick vs actual tick.

### 5. Score Updates

**CLIENT → SERVER:**
```
HIT_NOTE
{
  noteId: "note_12345",
  accuracy: 0.95,  // 0-1, how close to perfect timing
  streak: 42
}
```

**SERVER → ALL CLIENTS (optional, for leaderboard):**
```
LEADERBOARD_UPDATE
{
  topScores: [
    { playerName: "Alex", score: 87500, streak: 142 },
    { playerName: "Jordan", score: 85200, streak: 98 },
    // ...
  ]
}
```

### 6. State Changes

**UNITY → CLIENTS:**
```
STATE_CHANGE
{
  state: "waiting_for_band" | "waiting_for_swap" | "set_break" | "show_end",
  message: "Take a break! Back in 10 minutes.",
  nextSong: "Song Title"
}
```

---

## Clock Synchronization Algorithm

**Challenge:** Client needs to know "what time is it on the server?" with <50ms accuracy.

**Simple NTP-style Sync:**

1. **Client sends PING with t0 (client time sent)**
2. **Server receives at t1 (server time)**
3. **Server sends PONG with t1 and t2 (server time sent)**
4. **Client receives at t3 (client time received)**

**Calculations:**
- Round-trip time: `RTT = (t3 - t0)`
- One-way latency estimate: `latency = RTT / 2`
- Clock offset: `offset = ((t1 - t0) + (t2 - t3)) / 2`

**Apply offset:**
```javascript
function getServerTime() {
  return Date.now() + clockOffset;
}
```

**Continuously Update:**
- Sync every 5 seconds during gameplay
- Use moving average of last 10 samples
- Discard outliers (>500ms RTT)

---

## Note Timing Strategy

**Problem:** Client receives chart data, must display notes at exact right time.

**Solution: Predict + Correct**

### Approach 1: Pre-send Full Chart (Recommended for MVP)

1. Send entire chart data on SONG_START (2-3 seconds before)
2. Client buffers all notes locally
3. Client uses clock offset to calculate when to show each note
4. Periodic BEAT_SYNC messages correct drift

**Pros:**
- Simple
- No ongoing bandwidth after song starts
- Works even if connection drops mid-song

**Cons:**
- Initial payload size (1-5MB for complex song)
- All difficulties sent even if player only uses one

### Approach 2: Stream Notes Ahead (Better for Production)

1. Send notes 5-10 seconds ahead of when player sees them
2. Continuous stream during song
3. Client maintains buffer of upcoming notes

**Pros:**
- Lower initial payload
- Can adapt difficulty mid-song
- Better for long songs

**Cons:**
- More complex buffering logic
- Requires stable connection throughout song

---

## Data Format Optimization

**Chart Data Compression:**

Instead of full Note objects:
```javascript
// Inefficient (JSON):
{
  tick: 45782,
  lane: 2,
  type: "strum",
  duration: 0
}
// ~60 bytes per note
```

Use binary format or compressed array:
```javascript
// Efficient (packed array):
[45782, 2, 0, 0]  // tick, lane, type, duration
// ~16 bytes per note
```

Or even:
```javascript
// Base64-encoded binary buffer
"eNpjYGBgYGJg..."
```

**Expected Sizes:**
- Easy difficulty: 200-400 notes → ~3-6KB compressed
- Medium: 400-800 notes → ~6-12KB
- Hard: 800-1500 notes → ~12-25KB
- Expert: 1500-3000 notes → ~25-50KB

---

## Network Patterns

### WebSocket vs UDP vs Server-Sent Events

**WebSocket (Recommended):**
- ✅ Bi-directional
- ✅ Works through firewalls
- ✅ Browser support
- ✅ Reliable delivery
- ⚠️ TCP overhead (slight latency)

**UDP:**
- ✅ Lowest latency
- ✅ No connection overhead
- ❌ Not available in browsers
- ❌ Requires native app

**Server-Sent Events (SSE):**
- ✅ One-way server→client (good enough?)
- ✅ Simple
- ❌ No client→server (can use HTTP POST)
- ⚠️ HTTP/2 overhead

**Verdict:** Use WebSocket for MVP, consider UDP if latency becomes issue.

---

## Error Handling

### Connection Lost Mid-Song

**Options:**
1. **Continue Playing (Optimistic)** - Use last known sync
2. **Pause/Buffer** - Show "reconnecting..." overlay
3. **End Song** - Gracefully fail

**Recommended:** Option 1 with 10-second timeout, then Option 2.

### Late Join

**If user joins mid-song:**
- Send current tick + remaining chart data
- Client jumps into middle of song
- No score for missed notes (fair)

### Clock Drift

**If client drifts >100ms from server:**
- Gradually adjust (don't jump instantly, jarring)
- Adjust by 5-10ms per frame until in sync
- Log drift events for debugging

---

## Bandwidth Estimation

**Per Client:**
- Chart data at start: 10-50 KB
- Beat sync messages: ~100 bytes × 3 BPS = 300 bytes/sec
- Score updates: ~50 bytes × 2 per second = 100 bytes/sec
- Leaderboard (optional): 1KB every 5 seconds = 200 bytes/sec

**Total per client:** ~0.6 KB/sec = **~2 MB per 60-minute show**

**For 200 clients:** 400 MB total over 60 minutes = **6.6 MB/sec server bandwidth**

This is manageable on most VPS hosting.

---

## Testing Strategy

### Simulated Latency

Add artificial network delay to test resilience:
```javascript
// In dev mode:
setTimeout(() => {
  sendMessage(msg);
}, Math.random() * 200); // 0-200ms random delay
```

### Clock Skew Simulation

Test with intentionally wrong client clocks:
```javascript
const FAKE_OFFSET = -500; // 500ms behind server
```

### Packet Loss

Drop random WebSocket messages:
```javascript
if (Math.random() > 0.95) return; // 5% packet loss
```

---

## Security Considerations

**Room Codes:**
- 6-character alphanumeric (e.g., "SHOW42")
- Expire after show ends
- Rate limit join attempts

**Score Validation:**
- Don't trust client scores blindly
- Server validates hit timing (was note actually there?)
- Basic anti-cheat (impossible streaks)

**DDoS Protection:**
- Rate limit connections per IP
- Use reverse proxy (Cloudflare)
- Graceful degradation if server overloaded

---

## Future Enhancements

- **Adaptive Sync:** Detect network quality, adjust strategy
- **P2P Sync:** Clients sync to each other (reduce server load)
- **Audio Fingerprinting:** Sync to stage audio via phone mic
- **Offline Mode:** Download charts, play without connection
- **Replay System:** Save performances, watch later

---

## References

- NTP Protocol: https://en.wikipedia.org/wiki/Network_Time_Protocol
- WebSocket API: https://developer.mozilla.org/en-US/docs/Web/API/WebSocket
- Socket.io Documentation: https://socket.io/docs/
