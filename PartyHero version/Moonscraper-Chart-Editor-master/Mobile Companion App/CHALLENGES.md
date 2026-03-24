# Mobile Companion App - Challenges & Considerations

**Purpose:** Document potential issues, edge cases, and important considerations that may not be obvious.

---

## User Experience Challenges

### 1. Onboarding Friction

**Problem:** Audience member sees QR code, scans it, but doesn't understand what to do.

**Considerations:**
- First-time users need immediate context
- Can't have tutorial during live performance
- Must work for non-gamers

**Solutions:**
- Ultra-simple landing page: "Tap the colored buttons!" with GIF
- 3-second auto-start countdown
- First song on Easy by default
- Visual arrows/hints in first 10 seconds
- Band announce it: "Scan the code, tap along!"

---

### 2. Screen Orientation

**Problem:** Users hold phones in portrait vs landscape.

**Considerations:**
- Portrait: Natural phone holding, but narrow highway
- Landscape: Better game view, but awkward in crowd
- Mixed orientations in audience

**Recommended:** Support portrait only (simpler UI, more natural)

**Alternative:** Detect orientation, switch layout dynamically

---

### 3. Visual Distraction

**Problem:** 200 people staring at phones during performance.

**Considerations:**
- Defeats purpose of live show (everyone looking down)
- Reduces stage presence
- Might annoy band

**Solutions:**
- Use minimal brightness (dark background)
- Encourage "glance and tap" not constant staring
- Only during instrumental sections? (too complex)
- Accept it as trade-off for engagement

**Decision:** Discuss with band - is this the vibe you want?

---

### 4. Accessibility

**Problem:** Not everyone can play rhythm games.

**Considerations:**
- Visual impairment
- Hearing impairment (delay from stage PA)
- Motor skill limitations
- Elderly audience members

**Solutions:**
- "Casual mode" - notes move slower
- "Zen mode" - can't fail, just vibes
- Large tap targets (full lane width)
- High contrast color mode
- Include in settings

---

## Technical Challenges

### 5. Battery Drain

**Problem:** Playing for 3-hour show kills phone battery.

**Data:**
- WebSocket connection: ~2-5% battery/hour
- Screen on full brightness: ~15-20%/hour
- Rendering at 60fps: ~10-15%/hour
- **Total: ~30-40% battery per hour**

**Implications:**
- Most phones dead by end of show
- Reduced participation in later sets

**Solutions:**
- Recommend charging beforehand
- Encourage bringing power banks
- Provide charging stations (infrastructure cost)
- Optimize rendering (30fps instead of 60)
- Sleep mode during set breaks
- Low power mode (reduce visual effects)

**Reality Check:** Accept some phones will die. That's okay.

---

### 6. Network Congestion

**Problem:** 200 devices on venue WiFi.

**Typical Venue WiFi:**
- Consumer router: Max 50 devices
- Small business: Max 100 devices
- Enterprise: 200+ devices, but still saturated

**Bandwidth Math:**
- 200 clients × 0.6 KB/sec = 120 KB/sec down (manageable)
- 200 clients × 0.2 KB/sec = 40 KB/sec up (manageable)
- But: WiFi is half-duplex, shared medium
- Other traffic: social media, photos, texts

**Solutions:**
- Dedicated WiFi network just for app
- 5GHz band (less crowded)
- Multiple access points (distribute load)
- QoS prioritization
- Fallback: Limit to 100 connections, show "room full"

**Cost:** Professional WiFi setup for 200+ users: $500-2000

---

### 7. iOS Safari Quirks

**Problem:** iOS Safari has unique WebSocket and audio limitations.

**Known Issues:**
- WebSocket auto-closes after 5 minutes in background
- Audio requires user interaction to start (can't auto-play)
- ServiceWorker (PWA) limitations
- Memory limits (tabs killed if using too much)

**Solutions:**
- Keep app in foreground (user education)
- Test reconnection flow thoroughly
- Don't use audio on mobile (visual only)
- Optimize memory usage
- Test on real iOS devices early

**Reality:** iOS is 50%+ of audience, must work flawlessly.

---

### 8. Latency Variance

**Problem:** Not all clients have same latency.

**Scenario:**
- Client A: 30ms latency (near router)
- Client B: 150ms latency (back of venue, 2.4GHz)
- Client C: 250ms latency (poor connection)

**Impact:**
- Client A sees notes first
- Client C sees notes 220ms later
- Notes appear out of sync if clients look at each other

**Solutions:**
- Client-side compensation (each adjusts for own latency)
- Clients don't see each other's screens (non-issue?)
- Provide calibration for personal delay (stage audio delay)
- Accept variance (it's single-player experience)

**Decision:** Each client plays in its own world. Sync to server, not to each other.

---

### 9. Clock Drift

**Problem:** Client clocks drift from server over time.

**Data:**
- JavaScript `Date.now()` can drift ±5-10ms/minute
- After 4-minute song: 20-40ms drift
- Accumulates over show

**Solutions:**
- Sync every 5 seconds (PING/PONG)
- Use moving average of clock offset
- Gradual adjustment (not sudden jumps)
- Server sends BEAT_SYNC to correct drift

**Validation:** After 3 hours, clients should still be <50ms off.

---

### 10. Chart Data Simplification

**Problem:** Unity has 5-button Expert charts. Mobile has 3 touch zones.

**Mapping Strategy:**

**Option 1: Reduce to 3 lanes (Green, Red, Yellow)**
- Map Green → Lane 1
- Map Red → Lane 2  
- Map Yellow → Lane 3
- Discard Blue and Orange notes

**Option 2: Combine buttons**
- Lane 1: Green OR Blue
- Lane 2: Red
- Lane 3: Yellow OR Orange

**Option 3: Send Easy/Medium difficulty (already 3 notes)**
- Stage plays Expert
- Mobile plays Medium
- Cleaner mapping

**Recommended:** Option 3 (use existing Medium chart)

---

## Business/Operational Challenges

### 11. Venue Cooperation

**Problem:** Need venue's help for WiFi and setup.

**Requirements:**
- WiFi passwords / access
- Power outlets for server/router
- Setup time before doors open
- Tech support if internet down

**Considerations:**
- Small venues may not have capacity
- Venue WiFi might block WebSocket ports
- IT staff may not be available
- Liability if WiFi affects venue POS systems

**Solutions:**
- Bring own hardware (router, access points)
- Use cellular hotspot as backup
- Get venue tech contact in advance
- Test day before show

---

### 12. User Privacy

**Problem:** Collecting data from audience members.

**Data Collected:**
- Device info (for debugging)
- IP address (inherent to WebSocket)
- Play data (scores, hit accuracy)
- Connection quality metrics

**Legal Requirements:**
- GDPR (Europe): Must disclose and allow opt-out
- CCPA (California): Must allow deletion
- Children: COPPA compliance if <13 years old

**Solutions:**
- Privacy policy (link on landing page)
- No personal data collection (no names/emails)
- Anonymous player IDs
- Auto-delete data after show
- Don't track users across shows

**Keep It Simple:** Store data only during show, delete after. No accounts.

---

### 13. Content Moderation

**Problem:** If app has usernames/chat, could be abused.

**Avoid:**
- User-generated names displayed publicly
- Chat features
- Public profiles

**If Leaderboard Required:**
- Auto-generated names ("Player 1234")
- OR: Pull from preset list ("Blue Turtle", "Fast Penguin")
- Profanity filter if manual names allowed

**Best Practice:** No social features in MVP. Just scores.

---

### 14. Show Flow Complexity

**Problem:** App must follow Unity's show flow state machine.

**States to Handle:**
- Pre-show (lobby waiting)
- Song active (gameplay)
- Song end results
- Waiting for band
- Player swap
- Set break
- Show end

**Edge Cases:**
- What if user joins during set break?
- What if user joins at song 7 of 12?
- What if show ends early (band emergency)?
- What if band extends set break?

**Solutions:**
- Always show current state clearly
- Late joiners get "Waiting for next song" screen
- Mid-show joiners miss previous songs (okay)
- Manual controls for stage tech (force state change)

---

### 15. Abandonment/Drop-Off

**Problem:** Users join, play one song, then stop.

**Why?**
- Too hard (failed too many notes, felt bad)
- Battery dying
- Phone overheating
- Lost interest
- Left venue

**Metrics to Track:**
- Join rate (% who scan QR)
- Completion rate (% who finish first song)
- Retention (% who play 3+ songs)
- Average session duration

**Solutions:**
- Make first song easy (builds confidence)
- Show encouraging messages even if failing
- Auto-difficulty (easier if struggling)
- Fun even if not "winning"

**Expectation:** 40-60% retention through full show is good.

---

## Design Philosophy

### 16. Simplicity vs Features

**Tension:** 
- Want rich features (leaderboards, power-ups, profiles)
- BUT: Barrier to entry increases

**Principle:** Favor simplicity for live environment.

**Rationale:**
- Users have 10 seconds to understand it
- New users every show (can't rely on learned behavior)
- No tutorial possible during performance
- Technical issues have immediate consequences (show must go on)

**Examples:**
- ✅ Three buttons. Tap when note arrives. That's it.
- ❌ Collect stars, unlock power-ups, customize avatar

**Rule:** If feature requires reading text to understand, cut it.

---

### 17. Graceful Degradation

**Principle:** App should work even if things break.

**Scenarios:**
- Server dies → Show cached "connection lost" message, allow replay of last song
- WiFi weak → Adjust quality (skip animations)
- Song data corrupt → Show fallback message, skip to next song
- Client out of sync → Show "syncing..." overlay, attempt recovery

**Never:** Show raw error messages or crash silently.

**Always:** Give user something to do or clear status update.

---

### 18. Single-Player Focus

**Design Decision:** Treat as single-player experience, not multiplayer.

**Why?**
- Simplifies sync (no P2P coordination)
- Privacy (don't need to show other players)
- Reduces server complexity
- Works even if others disconnect

**Trade-off:** Miss social elements (seeing friends' scores real-time)

**Future:** Could add "friends in crowd" feature later.

---

## Future Enhancements to Consider

### 19. Audio Sync (Advanced)

**Concept:** Use phone mic to detect stage audio, auto-calibrate.

**How:**
- Record ambient audio via microphone
- Audio fingerprinting (like Shazam)
- Compare to known song waveform
- Calculate delay (your position in venue)
- Auto-adjust note timing

**Pros:**
- Perfect sync regardless of WiFi latency
- Compensates for speed of sound delay
- No calibration screen needed

**Cons:**
- Complex implementation
- Requires microphone permission (privacy concern)
- Loud venues may interfere
- Battery drain

**Verdict:** Cool idea for future, overkill for MVP.

---

### 20. Offline Mode

**Concept:** Download charts beforehand, play without connection.

**Use Case:**
- Cellular data at outdoor festival (no WiFi)
- Practice at home before show
- Play songs after show ends

**Implementation:**
- ServiceWorker caching
- IndexedDB for chart storage
- No leaderboard, no live sync
- Single-player only

**Verdict:** Nice-to-have, not essential for live use case.

---

### 21. Multi-Instrument Support

**Concept:** Choose guitar, drums, bass, vocals.

**Implementation:**
- Unity sends all instrument tracks
- User selects on join
- Different note columns per instrument
- Drums = 4 pads, Bass = 3-4 notes, etc.

**Pros:**
- More variety
- Replayability
- Matches Rock Band model

**Cons:**
- More complex UI
- Larger chart data
- Harder to balance difficulty

**Verdict:** Phase 2 feature after MVP proven.

---

## Questions to Answer Before Starting

### Product Questions

1. **Target audience age?** (Kids 8+, teens 13+, adults 18+?)
   - Affects UI complexity, privacy requirements, difficulty tuning

2. **Free or paid?** (Free for audience, band pays for hosting?)
   - Affects monetization, features

3. **Single band or platform?** (Just your shows, or license to other bands?)
   - Affects branding, customization, scalability goals

4. **Casual or competitive?** (Fun participation vs serious leaderboards?)
   - Affects scoring emphasis, social features

5. **Visual aesthetic?** (Match band branding, or generic rhythm game?)
   - Affects design, asset creation

### Technical Questions

1. **Minimum supported phone?** (iPhone 8 / Android 2018+?)
   - Affects optimization, features

2. **Expected crowd size?** (50-100 people or 500-1000?)
   - Affects infrastructure, costs

3. **Tour schedule?** (Weekly shows or monthly?)
   - Affects update frequency, testing window

4. **Internet at venues?** (Reliable WiFi or must bring own?)
   - Affects deployment, backup plans

5. **Budget for infrastructure?** ($10/month or $100/month?)
   - Affects hosting choices, scaling

---

## Final Recommendations

### Start Small
- Build proof-of-concept with 5 people first
- Test at private show before public launch
- Don't invest in polish until sync works

### Prioritize Reliability
- Better to have simple app that works 99% of time
- Than fancy app that crashes on stage

### Get Band Buy-In
- Ensure band actually wants this (changes audience behavior)
- Get their input on aesthetics, song selection
- They'll be your best marketing

### Have Backup Plan
- If tech fails mid-show, what happens?
- Can show continue without it?
- Stage tech must know how to disable if needed

### Measure Success
- Track what works (engagement, retention)
- Iterate based on real usage data
- Kill features that don't land

---

**Bottom Line:** This is a fantastic idea, but complex execution. Plan carefully, prototype early, and be ready for surprises.
