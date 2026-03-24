# Mobile Companion App - Quick Reference

**One-page overview for quick decisions.**

---

## 🎯 The Concept

Live audience plays along via phones while band performs. Stage computer runs Unity PartyHero, mobile web app syncs over WiFi.

---

## 📊 Is This Worth Doing?

**Pros:**
- ✅ Unique audience engagement
- ✅ Viral potential (everyone playing together)
- ✅ Reuses 30-40% of Unity code
- ✅ Modern tech stack
- ✅ Low ongoing cost ($10-20/month)

**Cons:**
- ⚠️ 6-8 months development time
- ⚠️ Complex network sync challenge
- ⚠️ Requires venue WiFi cooperation
- ⚠️ Changes audience behavior (looking at phones)
- ⚠️ Adds tech risk to live show

**Verdict:** Great idea if band is committed and has time to test properly.

---

## 🛠️ Recommended Tech Stack (MVP)

| Component | Technology | Why |
|-----------|-----------|-----|
| **Mobile Client** | React PWA + Canvas 2D | No app store, instant access |
| **Server** | Node.js + Socket.io | Easy development, good enough |
| **Unity Bridge** | WebSocketSharp | Works with Unity 2018 |
| **Hosting** | Vercel + Heroku | Free/cheap, easy deploy |

**Total Cost:** $0-7/month for development, $10-30/month for production

---

## ⏱️ Timeline

| Milestone | Duration | What You Get |
|-----------|----------|--------------|
| **Proof of Concept** | 2-4 weeks | Basic sync test with 2 phones |
| **Playable MVP** | 3 months | 3-lane game, no polish |
| **Production Ready** | 6 months | Join flow, polish, tested |
| **Battle Tested** | 8 months | Beta shows, optimized |

**Can You Wait?** If needed sooner, reduce scope (remove leaderboard, fancy features).

---

## 🚧 Hardest Parts (Focus Here)

1. **Network Sync** - Making it feel instant (<50ms latency)
2. **Venue WiFi** - Handling 200 devices reliably
3. **iOS Safari** - Browser quirks and limitations
4. **Battery Drain** - Phones lasting through 3-hour show
5. **User Onboarding** - Making it obvious without tutorial

**Risk Mitigation:** Build proof-of-concept early (2-4 weeks), validate sync feels good before investing more.

---

## 💰 Costs

### Development
- **DIY:** 6-8 months of your time
- **Hired Dev:** $40,000-80,000 (800 hours @ $50-100/hr)

### Infrastructure
- **Hosting:** $10-30/month
- **WiFi (if venue lacks):** $500-2000 one-time for pro setup
- **Domain:** $12/year

---

## 📏 Scope Options

### Minimal MVP (Phase 1-2)
**3 months development**
- 3-lane tap game
- Basic sync
- No leaderboard, no social features
- Works with 50 people

### Full Production (Phase 1-7)
**6 months development**
- Join via QR code
- Show flow integration
- Leaderboard
- 200+ concurrent users
- Polish and effects

### Deluxe (Phase 1-8 + extras)
**8-12 months development**
- Everything above
- Multi-instrument
- Analytics dashboard
- Admin controls
- Tested at scale

**Recommendation:** Start with Minimal MVP, expand if successful.

---

## 🎮 Gameplay Design

**Interface:**
```
┌─────────────────────┐
│  [Score: 12,430]    │
│  [Streak: 42]       │
│                     │
│   ◯   ◯   ◯         │  ← Notes falling
│   │   │   │         │
│   │   │   │         │
│   │   │   │         │
│   │   ◯   │         │
│   │   │   │         │
│   │   │   │         │
│  ━━━━━━━━━━━━━━     │  ← Timing bar
│  [L] [M] [R]        │  ← Tap zones
└─────────────────────┘
```

**Controls:** Tap left/middle/right when colored note crosses timing bar.

**Difficulty:** Easy (stage plays Expert), auto-scales if struggling.

---

## 📡 How Sync Works (Simplified)

1. Unity broadcasts: "Song starts at server time 12:34:56.789"
2. Mobile clients sync clocks with server (±10ms accuracy)
3. Clients receive full chart data before song starts
4. Clients display notes at local time = (server time + latency offset)
5. Periodic sync messages correct drift

**Key:** Each client plays independently using shared time reference.

---

## ✅ Success Criteria

### MVP Success
- [ ] 20+ people play simultaneously
- [ ] Feels instant (<50ms perceived latency)
- [ ] Works on iPhone Safari + Android Chrome
- [ ] 50%+ QR scan → successful join rate

### Production Success
- [ ] 200+ concurrent players
- [ ] 99% uptime during show
- [ ] <5% connection failure rate
- [ ] 60%+ retention through full show
- [ ] Band and audience both love it

---

## 🚦 Go / No-Go Decision Points

**After Phase 1 (Proof of Concept):**
- **GO:** If sync feels instant with 5 devices
- **NO-GO:** If latency > 100ms or feels laggy → investigate or abandon

**After First Test Show:**
- **GO:** If 70%+ of audience successfully plays and enjoys
- **NO-GO:** If tech issues dominate experience → more testing needed

**After 3 Shows:**
- **GO:** If reliable and band wants to keep using
- **NO-GO:** If constant troubleshooting, not worth effort

---

## 📝 Pre-Development Checklist

Before writing code:
- [ ] Band approves concept (changes audience dynamic)
- [ ] Unity show flow complete and stable
- [ ] Access to 3+ test devices (iOS, Android)
- [ ] Reliable WiFi network for testing
- [ ] 6-8 months available for development
- [ ] Budget for hosting ($10-30/month)
- [ ] Test venue has WiFi or budget for equipment

**Don't start if:** Any of above are "no" or uncertain.

---

## 🔄 Alternative Concepts (If Full App Too Much)

### 1. Visual-Only (No Interaction)
- Phones just display visualizer synced to music
- No gameplay, just eye candy
- Much simpler, still engaging

### 2. Vote/Poll System
- Phones vote on next song, setlist decisions
- Minimal sync requirements
- Social engagement without gameplay

### 3. Lyric Karaoke
- Display lyrics synced to music
- Taps advance manually
- Sing-along focus

### 4. Light Show
- Phones become synchronized light show (colors, patterns)
- No gameplay, ambient experience
- Easy to implement

**Consider:** If rhythm game too complex, pivot to simpler interaction.

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| **README.md** | Full overview, architecture, phases |
| **TODO.md** | Week-by-week development checklist |
| **SYNC_PROTOCOL.md** | Network message specifications |
| **TECH_OPTIONS.md** | Technology stack comparisons |
| **CHALLENGES.md** | Risks, edge cases, considerations |
| **QUICK_REFERENCE.md** | This file (high-level summary) |

---

## 🎯 Next Step

**If seriously considering:**
1. Read through full [README.md](README.md)
2. Review [TECH_OPTIONS.md](TECH_OPTIONS.md) and choose stack
3. Assess [CHALLENGES.md](CHALLENGES.md) risks
4. Start [TODO.md](TODO.md) Phase 1 when ready

**If just curious:**
- Keep this folder for future reference
- Focus on finishing Unity show flow UI first
- Revisit after a few successful shows

---

## 💡 Final Thought

This is an **innovative, ambitious project** with real potential to create unique concert experiences. The tech is proven, the timeline is achievable, but it requires commitment and thorough testing.

**Best Path Forward:**
1. Finish Unity show flow UI first
2. Run a few console-based shows
3. Revisit mobile app when Unity version is rock-solid
4. Start with 2-4 week proof-of-concept
5. Only proceed if sync feels amazing

Don't build this until Unity side is production-ready. One step at a time.
