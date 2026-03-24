# PartyHero Mobile Companion App

**Status:** Planning / Future Development  
**Purpose:** Live audience participation app that syncs with stage performance  
**Target:** iOS, Android, Web (PWA recommended)

---

## Concept Overview

**The Vision:**
- Band performs live on stage
- Stage performer plays PartyHero (full Unity game) projected for audience
- Audience members join via mobile app (scan QR code)
- Phones sync in real-time to show simplified note highway
- Everyone plays along with the live performance
- Audience scores aggregate to communal leaderboard

**Key Differentiators:**
- NOT a standalone mobile rhythm game
- Companion/synchronized experience only
- Visual sync to stage audio (phones don't play music)
- Simplified gameplay (3-lane "tap zones" vs 5-button expert mode)
- Zero latency tolerance - must feel instant
- Supports 50-500+ concurrent players per show

---

## Architecture

```
Stage Computer (Unity PartyHero)
  └─ Sends sync messages (OSC/WebSocket)
       └─ Sync Server (Node.js/Go/Rust)
            └─ Broadcasts to audience phones
                 └─ Mobile Web App (PWA)
                      └─ Displays notes, handles touch input
```

**Core Components:**
1. **Stage Game (Unity)** - Already exists, needs WebSocket broadcaster
2. **Sync Server** - Relay server for timing/note data
3. **Mobile Client** - Progressive Web App for audience
4. **Join System** - QR code/URL with room codes
5. **Leaderboard** (Optional) - Aggregate scores

---

## What Reuses from Unity Project

**Highly Reusable (~30-40% of gameplay code):**
- Show flow state machine concepts
- Note data structures (Chart, NoteController logic)
- Scoring algorithms (hit detection, streak calculations)
- Timing/beat tracking systems
- OSC/TCP infrastructure (extend for WebSocket)

**Not Needed (99% of editor):**
- Chart editor UI
- File management systems
- Desktop-specific input handling
- Complex menu systems
- Editing tools

---

## Technical Decisions

### Recommended: Progressive Web App (PWA)

**Why PWA over Native Apps:**
- ✅ No App Store approval (can update instantly)
- ✅ Works on iOS + Android from one codebase
- ✅ Users join via URL/QR code - no download
- ✅ Lower barrier to entry for audience
- ✅ Easier deployment for touring shows
- ✅ WebSocket support built-in

**Drawbacks:**
- ⚠️ Slightly less performance than native
- ⚠️ Some iOS Safari limitations
- ⚠️ No offline capability (requires connection)

**Tech Stack (Recommended):**
- **Frontend:** React or Svelte + TypeScript
- **Graphics:** Canvas 2D API or Three.js (if 3D)
- **Networking:** WebSocket (Socket.io or native WebSocket API)
- **Sync:** Custom clock sync protocol
- **Hosting:** Cloudflare Workers, AWS Lambda@Edge, or Vercel

### Alternative: Unity Mobile Export

**Pros:**
- More code reuse from existing Unity project
- Better 3D graphics
- Easier to port gameplay systems

**Cons:**
- Requires App Store/Google Play approval
- Users must download/install (friction)
- Larger file size (50-200MB vs <5MB for PWA)
- Harder to update during tours
- App Store fees ($99/year iOS, $25 one-time Android)

---

## Major Challenges

### 1. Network Synchronization ⚠️⚠️⚠️ (HARDEST PART)

**Problem:**
- Stage game at tick 45,782
- Phone receives message 150ms later
- Phone must show notes "as if" it's at tick 45,782

**Solutions Needed:**
- Clock synchronization algorithm (NTP-like)
- Latency measurement and compensation
- Client-side prediction
- Jitter buffer for network variance

### 2. WiFi Capacity

**Problem:**
- Venue WiFi with 200 audience members
- All need <100ms latency
- Bandwidth consumption

**Solutions:**
- UDP broadcast (if local network)
- Efficient message protocol (binary, not JSON)
- Batch messages per beat instead of per-frame
- Consider dedicated WiFi access points

### 3. Difficulty Scaling

**Problem:**
- Stage performer plays Expert (200+ notes/min)
- Audience skill levels vary wildly

**Solutions:**
- Send Easy/Medium/Hard tracks in sync messages
- Clients choose difficulty on join
- Option: Auto-difficulty based on performance

### 4. Visual-Only Sync (No Audio on Phones)

**Problem:**
- Phones can't play audio (stage PA is audio source)
- Audio delay from stage to back of venue (~1-3ms per foot)
- Must rely on visual note timing only

**Solutions:**
- Calibration screen (like Rock Band)
- User-adjustable offset
- Clear visual feedback on hits

### 5. Battery Drain

**Problem:**
- Continuous WebSocket + rendering for 3-hour show
- Phones will die

**Solutions:**
- Optimize rendering (30fps instead of 60fps)
- Sleep mode between songs
- Low-power mode option
- Encourage charging beforehand

---

## Development Phases

### Phase 1: Proof of Concept (2-4 weeks)
- [ ] Build minimal WebSocket server (Node.js)
- [ ] Create basic web page with 3-column note display
- [ ] Unity: Add WebSocket broadcaster component
- [ ] Test sync with 1-2 devices locally
- [ ] Validate: "Does it feel real-time?"

### Phase 2: Core Gameplay (4-6 weeks)
- [ ] Implement clock sync algorithm
- [ ] Add touch input handling
- [ ] Score calculation and display
- [ ] Hit feedback (visual/haptic)
- [ ] Note prefetch and rendering
- [ ] Latency compensation
- [ ] Reconnection handling

### Phase 3: Join Experience (2-3 weeks)
- [ ] QR code generation (Unity side)
- [ ] Mobile landing page with room code
- [ ] Lobby/waiting room
- [ ] Calibration screen
- [ ] Difficulty selection
- [ ] Connection status indicators

### Phase 4: Production Features (3-4 weeks)
- [ ] Leaderboard (real-time top scores)
- [ ] Show flow integration (set breaks, song transitions)
- [ ] Visual polish (themes, particles, animations)
- [ ] Error recovery (lost connection, rejoin)
- [ ] Analytics (connection quality, hit rates)

### Phase 5: Scale Testing (2-3 weeks)
- [ ] Load testing with 50+ simulated clients
- [ ] Bandwidth optimization
- [ ] Server scaling strategy
- [ ] Fallback for network congestion
- [ ] Stress test at rehearsal show

### Phase 6: Deployment (1-2 weeks)
- [ ] Production server setup
- [ ] SSL certificates
- [ ] Domain/subdomain
- [ ] Monitoring and logging
- [ ] Backup server strategy
- [ ] Tour tech documentation

---

## Estimated Development Time

**Solo Developer:**
- Minimum Viable Product: 3-4 months
- Production Ready: 6-9 months
- Polish + Testing: +2-3 months

**Small Team (2-3 devs):**
- MVP: 2-3 months
- Production Ready: 4-6 months

**Blockers/Dependencies:**
- Unity WebSocket integration (extends current OSC work)
- Reliable venue WiFi infrastructure
- Testing environment with multiple devices

---

## Cost Considerations

**Server Hosting:**
- Development: Free tier (Vercel/Cloudflare)
- Production: $20-100/month (mid-tier VPS or serverless)
- High-traffic shows: $100-500/month (dedicated server or CDN)

**Development Tools:**
- All open-source (React, Node.js, TypeScript) = $0
- Domain name: ~$12/year
- SSL certificate: Free (Let's Encrypt)

**If Unity Mobile Export Instead:**
- iOS Developer Account: $99/year
- Google Play Developer: $25 one-time
- Unity Pro (if needed): $185/month

---

## Risk Assessment

**HIGH RISK:**
- ❌ Network synchronization complexity
- ❌ Venue WiFi reliability
- ❌ User experience with latency/lag

**MEDIUM RISK:**
- ⚠️ Battery drain during long shows
- ⚠️ Browser compatibility (iOS Safari quirks)
- ⚠️ Scaling to 200+ concurrent users

**LOW RISK:**
- ✅ Core gameplay (proven patterns from Unity)
- ✅ WebSocket technology (mature, stable)
- ✅ Touch input (straightforward)

---

## Success Criteria

**MVP Success:**
- [ ] 5+ people play simultaneously without noticeable lag
- [ ] Players can hit 70%+ of notes on Easy difficulty
- [ ] Reconnection works when WiFi drops briefly
- [ ] Works on iPhone (Safari) and Android (Chrome)

**Production Success:**
- [ ] 100+ concurrent players with <50ms perceived latency
- [ ] 99% uptime during 3-hour show
- [ ] Players rate experience 4+ stars
- [ ] Less than 5% drop-out rate due to technical issues

---

## Next Steps When Ready

1. **Start Small:** Build WebSocket broadcaster in Unity first
2. **Test Sync:** Create basic HTML page that receives messages
3. **Validate Concept:** Get 2 phones syncing to Unity game locally
4. **Iterate:** Only invest in full app if sync feels good

**Don't Build Full App Until:** Sync feels real-time in local tests with 5+ devices

---

## Related Documentation

- [Unity OSC/TCP Setup](../DAW_SYNC_SETUP.md) - Existing network sync for DAWs
- [Show Flow System](../MESSAGE_REFERENCE.md) - State machines to sync with
- [Unity WebSocket Libraries](WEBSOCKET_OPTIONS.md) - See technical notes below
