# Mobile Companion App - Development Checklist

**Project Status:** Pre-Development / Planning Phase  
**Start Date:** TBD  
**Target Launch:** TBD

---

## Prerequisites (Before Starting)

- [ ] Unity show flow system complete and tested
- [ ] Stable setlist with 5-10 test songs
- [ ] Access to test devices (2+ iOS, 2+ Android)
- [ ] Reliable WiFi network for testing
- [ ] Decision made on tech stack (see TECH_OPTIONS.md)
- [ ] Development environment set up

---

## Phase 1: Proof of Concept (2-4 weeks)

**Goal:** Prove that network sync feels real-time with minimal features.

### Unity Side (Week 1)
- [ ] Install WebSocketSharp in Unity (`Assets/Plugins/`)
- [ ] Create `WebSocketBroadcaster.cs` component
- [ ] Add to ChartEditor GameObject
- [ ] Implement connection handling (clients can connect)
- [ ] Broadcast "song started" message
- [ ] Broadcast beat sync messages (every beat)
- [ ] Test: Open browser console, see messages arriving

### Server Side (Week 1)
- [ ] Set up Node.js project
- [ ] Install Socket.io (`npm install socket.io`)
- [ ] Create basic WebSocket relay server (relay Unity → Clients)
- [ ] Handle client connections
- [ ] Broadcast Unity messages to all connected clients
- [ ] Test: Unity → Server → Browser console

### Mobile Client (Week 2-3)
- [ ] Create simple HTML page with Canvas
- [ ] Connect to WebSocket server
- [ ] Display "Connected" status
- [ ] Receive song start message → show song name
- [ ] Draw 3 vertical lanes (left, middle, right)
- [ ] Draw colored circles (notes) falling down lanes
- [ ] Implement touch input (tap on lanes)
- [ ] Show "HIT!" when tap matches note
- [ ] Test with hardcoded notes first (no sync)

### Sync Integration (Week 3-4)
- [ ] Implement basic clock sync (PING/PONG)
- [ ] Calculate clock offset
- [ ] Display notes at correct time based on server clock
- [ ] Test with 1 phone while Unity plays song
- [ ] Test with 2 phones simultaneously
- [ ] Measure perceived latency (should feel instant)

### Success Criteria
- [ ] Notes appear in sync with Unity game (±50ms)
- [ ] Touch input feels responsive
- [ ] Works on iPhone Safari
- [ ] Works on Android Chrome
- [ ] Can reconnect if WiFi drops

**Decision Point:** If sync doesn't feel good here, investigate before proceeding.

---

## Phase 2: Core Gameplay (4-6 weeks)

**Goal:** Make it actually fun to play.

### Chart Data Integration (Week 5-6)
- [ ] Parse .chart file notes in Unity
- [ ] Convert to simplified format for mobile (3 lanes)
- [ ] Send chart data on SONG_START message
- [ ] Mobile client receives and buffers chart
- [ ] Display notes based on chart data (not hardcoded)
- [ ] Test with 3-4 different songs

### Scoring System (Week 7)
- [ ] Implement hit detection with timing windows
  - [ ] Perfect: ±50ms
  - [ ] Good: ±100ms
  - [ ] Miss: >100ms
- [ ] Calculate score per note
- [ ] Display running score
- [ ] Implement streak counter
- [ ] Display streak milestones (10, 25, 50, 100)
- [ ] Send score updates to server (optional)

### Visual Feedback (Week 8)
- [ ] Add hit animations (flash, particles)
- [ ] Show "Perfect!" / "Good" / "Miss" text
- [ ] Haptic feedback on hits (mobile vibration)
- [ ] Note colors (green/red/yellow for lanes)
- [ ] Highway scrolling effect
- [ ] Fret bar / timing indicator at bottom

### Polish (Week 9-10)
- [ ] Add background (not distracting)
- [ ] Font selection / text styling
- [ ] Color scheme (match stage branding?)
- [ ] Loading screen
- [ ] FPS optimization (target 30fps minimum)
- [ ] Battery optimization

---

## Phase 3: Join Experience (2-3 weeks)

**Goal:** Make it easy for audience to join mid-show.

### QR Code System (Week 11)
- [ ] Generate room code (6-char alphanumeric)
- [ ] Unity displays QR code in corner of screen
- [ ] QR code encodes URL: `https://play.partyhero.app?room=SHOW42`
- [ ] Test QR scanning with phone camera

### Landing Page (Week 11)
- [ ] Create mobile-optimized landing page
- [ ] Parse room code from URL
- [ ] Display "Joining SHOW42..." message
- [ ] Auto-connect to WebSocket with room code
- [ ] Handle invalid/expired room codes

### Lobby/Waiting Room (Week 12)
- [ ] Show "Connected - Waiting for next song" screen
- [ ] Display current song if in progress
- [ ] Show player count (optional)
- [ ] "Get Ready!" countdown before song starts

### Calibration (Week 12-13)
- [ ] Create calibration screen (like Rock Band)
- [ ] Show pulsing note, ask user to tap on beat
- [ ] Measure average offset
- [ ] Apply offset to note timing
- [ ] Save calibration to localStorage
- [ ] "Recalibrate" option in settings

### Difficulty Selection (Week 13)
- [ ] Show Easy/Medium/Hard buttons on join
- [ ] Save preference to localStorage
- [ ] Allow changing between songs
- [ ] Fetch appropriate chart from server

---

## Phase 4: Show Flow Integration (3-4 weeks)

**Goal:** Sync mobile app with Unity's show flow state machine.

### State Sync (Week 14)
- [ ] Unity broadcasts STATE_CHANGE events
- [ ] Mobile receives: "results", "waiting_for_band", "set_break", "show_end"
- [ ] Create mobile UI for each state
- [ ] Test transitions

### Results Screen (Week 15)
- [ ] Display final score
- [ ] Display streak
- [ ] Display accuracy percentage
- [ ] Show rank (if leaderboard exists)
- [ ] "Next Song: [Title]" message
- [ ] Countdown to next song

### Set Break (Week 15)
- [ ] Show "Set Break - Back in 10 minutes" message
- [ ] Display elapsed break time
- [ ] Optional: Simple mini-game to keep engagement

### Show End (Week 16)
- [ ] Show "Thank you for playing!" message
- [ ] Display total show stats (optional)
- [ ] Link to social media / band website
- [ ] Disconnect from server

### Leaderboard (Week 16-17, Optional)
- [ ] Server aggregates scores
- [ ] Display top 10 in real-time
- [ ] Highlight current player
- [ ] Update every 5 seconds
- [ ] Clear leaderboard at end of show

---

## Phase 5: Production Features (3-4 weeks)

**Goal:** Make it production-ready and robust.

### Error Handling (Week 18)
- [ ] Detect connection lost
- [ ] Show "Reconnecting..." overlay
- [ ] Attempt reconnection (exponential backoff)
- [ ] Resume from current state on reconnect
- [ ] Gracefully fail after 10 reconnect attempts

### Late Join Handling (Week 18)
- [ ] Allow joining mid-song
- [ ] Sync to current tick
- [ ] Send only remaining notes
- [ ] Display partial score (notes missed shown as grayed out)

### Settings Menu (Week 19)
- [ ] Access via hamburger menu / gear icon
- [ ] Calibration adjustment (±100ms slider)
- [ ] Difficulty change
- [ ] Volume (if audio added)
- [ ] Visual effects toggle (for low-end phones)
- [ ] About / Credits

### Analytics (Week 19-20)
- [ ] Track join rate (QR scans → successful connections)
- [ ] Track average latency per client
- [ ] Track drop-out rate
- [ ] Track average score per song
- [ ] Send to analytics service (Google Analytics, Mixpanel, or custom)

### Admin Dashboard (Week 20-21, Optional)
- [ ] Web dashboard for stage tech
- [ ] Display connected client count
- [ ] Display average latency
- [ ] Kick/ban abusive clients
- [ ] Emergency broadcast message
- [ ] Force disconnect all (end of show)

---

## Phase 6: Testing & Optimization (2-3 weeks)

### Load Testing (Week 22)
- [ ] Set up simulated clients (50+)
- [ ] Use tool like Artillery or k6
- [ ] Measure server CPU/memory under load
- [ ] Identify bottlenecks
- [ ] Optimize message frequency
- [ ] Test with 100 simulated clients
- [ ] Test with 200 simulated clients

### Device Testing (Week 22-23)
- [ ] Test on iPhone 12+ (modern)
- [ ] Test on iPhone 8 (older)
- [ ] Test on Android flagship (Pixel, Samsung)
- [ ] Test on Android budget phone
- [ ] Test on tablets
- [ ] Document minimum supported devices

### Network Testing (Week 23)
- [ ] Test on WiFi (5GHz)
- [ ] Test on WiFi (2.4GHz)
- [ ] Test with artificial latency (50ms, 100ms, 200ms)
- [ ] Test with packet loss (5%, 10%)
- [ ] Test connection drop/reconnect
- [ ] Test in actual venue WiFi (rehearsal)

### Performance Optimization (Week 24)
- [ ] Profile rendering performance
- [ ] Reduce draw calls
- [ ] Object pooling for notes
- [ ] Optimize garbage collection
- [ ] Minimize bundle size (lazy loading)
- [ ] Target: 30fps on iPhone 8
- [ ] Target: 60fps on iPhone 12+

---

## Phase 7: Deployment (1-2 weeks)

### Server Deployment (Week 25)
- [ ] Choose hosting provider (Heroku, DigitalOcean, etc.)
- [ ] Set up production server
- [ ] Configure SSL certificate (Let's Encrypt)
- [ ] Set up domain: play.partyhero.app
- [ ] Configure environment variables
- [ ] Set up logging (Winston, Papertrail)
- [ ] Set up monitoring (Uptime Robot, Datadog)
- [ ] Test production deployment

### Frontend Deployment (Week 25)
- [ ] Build production bundle (`npm run build`)
- [ ] Deploy to Vercel/Netlify or server
- [ ] Configure CDN
- [ ] Test PWA install (Add to Home Screen)
- [ ] Test on mobile networks (not just WiFi)
- [ ] Test with production WebSocket URL

### Unity Integration (Week 26)
- [ ] Update Unity to use production server URL
- [ ] Add toggle for dev/prod server
- [ ] Generate production QR codes
- [ ] Test full flow: Unity → Server → Mobile
- [ ] Test with 5+ people in rehearsal

### Documentation (Week 26)
- [ ] Create setup guide for stage tech
- [ ] Document troubleshooting steps
- [ ] Create FAQ for users
- [ ] Write deployment runbook
- [ ] Document emergency procedures (server down, etc.)

---

## Phase 8: Beta Testing (4-6 weeks)

### Private Beta (Week 27-28)
- [ ] Test at band practice (10-20 people)
- [ ] Collect feedback on usability
- [ ] Fix critical bugs
- [ ] Iterate on UI based on feedback

### Public Beta (Week 29-30)
- [ ] Test at small venue (50-100 people)
- [ ] Monitor server performance under real load
- [ ] Collect user feedback
- [ ] Fix issues discovered
- [ ] Measure engagement (% who join, % who complete songs)

### Final Rehearsal (Week 31-32)
- [ ] Full dress rehearsal with complete show
- [ ] Test all songs in setlist
- [ ] Test set breaks, show end
- [ ] Verify WiFi capacity with expected crowd size
- [ ] Have backup plan if tech fails

---

## Launch Readiness Checklist

### Pre-Show (Day Of)
- [ ] Server is running and accessible
- [ ] QR codes displayed in Unity
- [ ] Test with 3 devices (iOS, Android, tablet)
- [ ] Venue WiFi tested and stable
- [ ] Backup server ready (if primary fails)
- [ ] Stage tech knows how to restart if needed

### During Show
- [ ] Monitor server logs
- [ ] Watch connected client count
- [ ] Be ready to troubleshoot
- [ ] Collect user feedback after show

### Post-Show
- [ ] Export analytics data
- [ ] Review logs for errors
- [ ] Collect feedback from band and audience
- [ ] Plan improvements for next show

---

## Success Metrics

**MVP Success:**
- [ ] 50+ people play simultaneously without lag
- [ ] <5% connection failure rate
- [ ] 70%+ players can hit notes on Easy
- [ ] Works on 90%+ of devices tested

**Production Success:**
- [ ] 200+ concurrent players
- [ ] <2% connection failure rate
- [ ] 99% uptime during show
- [ ] 80%+ of audience joins when shown QR code
- [ ] 4+ star average user rating

---

## Estimated Timeline Summary

| Phase | Duration | Cumulative |
|-------|----------|------------|
| **Phase 1: Proof of Concept** | 2-4 weeks | 4 weeks |
| **Phase 2: Core Gameplay** | 4-6 weeks | 10 weeks |
| **Phase 3: Join Experience** | 2-3 weeks | 13 weeks |
| **Phase 4: Show Flow** | 3-4 weeks | 17 weeks |
| **Phase 5: Production Features** | 3-4 weeks | 21 weeks |
| **Phase 6: Testing** | 2-3 weeks | 24 weeks |
| **Phase 7: Deployment** | 1-2 weeks | 26 weeks |
| **Phase 8: Beta Testing** | 4-6 weeks | 32 weeks |

**Total:** ~6-8 months from start to launch

**MVP (Phase 1-2 only):** ~2.5-3 months

---

## Cost Estimate

### Development (Solo Developer)
- Time: 6-8 months @ $0 (your time)
- OR: Hire developer @ $50-100/hour × 800 hours = $40,000-80,000

### Ongoing Costs
- Server hosting: $10-50/month
- Domain name: $12/year
- SSL certificate: Free (Let's Encrypt)
- Analytics: Free tier (Google Analytics)
- Monitoring: $0-10/month

**First Year Total:** $150-650

---

## Risk Mitigation

**High Risk Items:**
1. **Network sync feels laggy** → Prototype early (Phase 1), validate before proceeding
2. **Venue WiFi insufficient** → Test in actual venue, bring backup router
3. **Server crashes during show** → Have backup server, know how to restart

**Backup Plans:**
- If sync fails: Fall back to non-synced gameplay (just fun animation)
- If server crashes: Have restart procedure, estimated downtime <2 minutes
- If too many users: Gracefully limit connections, show "room full" message

---

## Next Steps

**To begin development:**

1. **Decide on tech stack** (see TECH_OPTIONS.md)
2. **Set up development environment:**
   - Install Node.js (server)
   - Install VS Code or editor (web app)
   - Install WebSocketSharp in Unity
3. **Start Phase 1, Week 1:** Unity WebSocket broadcaster
4. **Track progress** in this checklist

**Don't start until:** Unity show flow is complete and tested.
