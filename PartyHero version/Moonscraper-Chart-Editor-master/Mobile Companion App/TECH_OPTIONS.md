# Technology Stack Options

**Purpose:** Compare different technology choices for mobile companion app development.

---

## Frontend Options

### Option 1: Progressive Web App (PWA) ⭐ RECOMMENDED

**Tech Stack:**
- React or Svelte + TypeScript
- Canvas 2D API or PixiJS for rendering
- Native WebSocket or Socket.io
- Vite or Webpack for build

**Pros:**
- ✅ No app store approval needed
- ✅ Instant updates (no user downloads)
- ✅ Works on iOS + Android from single codebase
- ✅ Users join via QR code/URL instantly
- ✅ Smaller download size (<5MB)
- ✅ Great for touring (update between cities)
- ✅ Lower barrier to entry for audience

**Cons:**
- ⚠️ iOS Safari has some WebSocket quirks
- ⚠️ Slightly lower performance than native
- ⚠️ Limited access to device features
- ⚠️ Must be online (no offline mode)

**Best For:** MVP, touring shows, rapid iteration

**Learning Curve:** Low (if you know JavaScript)

**Development Time:** 3-4 months MVP

---

### Option 2: Unity Mobile Export

**Tech Stack:**
- Unity 2018.4.23f1 (same as desktop)
- C# scripts (reuse existing gameplay)
- Unity networking or WebSocket plugin
- iOS + Android build modules

**Pros:**
- ✅ Reuse existing Unity gameplay code (30-40%)
- ✅ Better 3D graphics if needed
- ✅ Familiar development environment
- ✅ Good performance

**Cons:**
- ❌ Requires App Store + Google Play approval (weeks delay)
- ❌ Users must download/install (friction)
- ❌ Larger download (50-200MB)
- ❌ Hard to update during tours
- ❌ App Store fees ($99/year iOS, $25 Android)
- ❌ Two separate builds to maintain
- ⚠️ Unity 2018.4 mobile support aging

**Best For:** If you want native app experience, 3D graphics

**Learning Curve:** Low (already using Unity)

**Development Time:** 4-5 months MVP

---

### Option 3: React Native

**Tech Stack:**
- React Native + TypeScript
- Expo (optional, simplifies development)
- React Native WebSocket
- Canvas/SVG for graphics

**Pros:**
- ✅ True native apps (good performance)
- ✅ Single codebase for iOS + Android
- ✅ Large ecosystem
- ✅ Hot reload during development

**Cons:**
- ⚠️ Still requires app store approval
- ⚠️ Larger than PWA (20-50MB)
- ⚠️ More complex than PWA
- ⚠️ Updates require new versions

**Best For:** If native app required but want shared codebase

**Learning Curve:** Medium (need to learn React Native)

**Development Time:** 4-6 months MVP

---

### Option 4: Flutter

**Tech Stack:**
- Flutter + Dart
- WebSocket support
- Canvas/Custom Paint

**Pros:**
- ✅ Great performance
- ✅ Beautiful UI out of box
- ✅ Single codebase
- ✅ Hot reload

**Cons:**
- ⚠️ Requires learning Dart (new language)
- ⚠️ App store approval needed
- ⚠️ Larger download size

**Best For:** If starting fresh, want best mobile performance

**Learning Curve:** High (new language + framework)

**Development Time:** 5-7 months MVP

---

## Server Options

### Option 1: Node.js + Socket.io ⭐ RECOMMENDED FOR MVP

**Tech Stack:**
- Node.js + Express
- Socket.io (WebSocket wrapper)
- TypeScript optional

**Pros:**
- ✅ JavaScript (same as PWA frontend)
- ✅ Huge ecosystem
- ✅ Easy to deploy
- ✅ Socket.io handles reconnection/fallbacks automatically
- ✅ Fast development

**Cons:**
- ⚠️ Single-threaded (but handles I/O well)
- ⚠️ Memory usage with many connections

**Hosting Options:**
- Heroku (free tier → $7/month)
- DigitalOcean ($5-10/month)
- AWS/GCP ($10-50/month)

**Max Clients:** 1000+ concurrent with good server

**Learning Curve:** Low

---

### Option 2: Go + Gorilla WebSocket

**Tech Stack:**
- Go language
- Gorilla WebSocket library
- Built-in HTTP server

**Pros:**
- ✅ Excellent performance
- ✅ Low memory usage
- ✅ Great concurrency (goroutines)
- ✅ Single compiled binary (easy deployment)
- ✅ Can handle 10,000+ connections

**Cons:**
- ⚠️ Requires learning Go
- ⚠️ Smaller ecosystem than Node.js

**Best For:** Production at scale, performance-critical

**Learning Curve:** Medium

---

### Option 3: Python + FastAPI + WebSockets

**Tech Stack:**
- Python 3.9+
- FastAPI + Uvicorn
- WebSocket support built-in

**Pros:**
- ✅ Clean, readable code
- ✅ Good for MVP/prototyping
- ✅ Easy to add features

**Cons:**
- ⚠️ Slower than Go/Node.js
- ⚠️ Higher memory usage

**Best For:** If team knows Python well

**Learning Curve:** Low-Medium

---

### Option 4: Rust + Actix/Tokio

**Tech Stack:**
- Rust language
- Actix-web or Tokio
- Tungstenite (WebSocket)

**Pros:**
- ✅ Best performance possible
- ✅ Memory safe
- ✅ Can handle massive scale

**Cons:**
- ❌ Steep learning curve
- ❌ Slower development
- ⚠️ Overkill for this project

**Best For:** If scaling to 10,000+ users per show

**Learning Curve:** High

---

## Unity WebSocket Integration

### Option 1: WebSocketSharp (C#) ⭐ RECOMMENDED

**Library:** https://github.com/sta/websocket-sharp

**Pros:**
- ✅ Pure C#, works in Unity
- ✅ Simple API
- ✅ Reliable

**Cons:**
- ⚠️ Not actively maintained
- ⚠️ Some Unity 2018 compatibility quirks

**Installation:**
- Download DLL, place in `Assets/Plugins/`

**Example Usage:**
```csharp
using WebSocketSharp;

WebSocket ws = new WebSocket("ws://localhost:8080");
ws.OnMessage += (sender, e) => {
    Debug.Log("Received: " + e.Data);
};
ws.Connect();
ws.Send("Hello from Unity!");
```

---

### Option 2: NativeWebSocket

**Library:** https://github.com/endel/NativeWebSocket

**Pros:**
- ✅ Works in Unity WebGL builds too
- ✅ Modern, actively maintained
- ✅ Async/await support

**Cons:**
- ⚠️ Requires Unity 2020+ (you're on 2018.4)

**Not Compatible with Current Project**

---

### Option 3: Simple WebSocket (Custom)

**Build your own using .NET TcpClient:**

**Pros:**
- ✅ Full control
- ✅ No dependencies

**Cons:**
- ❌ Must implement WebSocket handshake
- ❌ Must handle framing protocol
- ❌ Time-consuming

**Best For:** Learning exercise, not production

---

### Option 4: Unity3d-OpenUnified-WebSocket

**Library:** https://github.com/Unity3dAzure/UnityWebSocket

**Pros:**
- ✅ Works in Unity 2018
- ✅ Open source

**Cons:**
- ⚠️ Less documentation
- ⚠️ Smaller community

---

## Graphics Rendering (Mobile Client)

### Option 1: Canvas 2D API ⭐ RECOMMENDED FOR MVP

**Native HTML5 Canvas**

**Pros:**
- ✅ Built into browsers
- ✅ Simple API
- ✅ No dependencies
- ✅ Good performance for 2D

**Cons:**
- ⚠️ Manual optimizations needed for complex scenes

**Use For:** Simple 3-lane highway, text, sprites

---

### Option 2: PixiJS

**Library:** https://pixijs.com/

**Pros:**
- ✅ WebGL accelerated
- ✅ Excellent 2D performance
- ✅ Sprite batching
- ✅ Great for particle effects
- ✅ Good documentation

**Cons:**
- ⚠️ Adds ~300KB to bundle

**Use For:** If you want fancy visual effects, particles, smooth animations

---

### Option 3: Three.js

**Library:** https://threejs.org/

**Pros:**
- ✅ 3D graphics in browser
- ✅ Huge ecosystem
- ✅ Can replicate Unity 3D highway

**Cons:**
- ⚠️ Overkill for 2D game
- ⚠️ Larger bundle size (~600KB)
- ⚠️ More battery drain

**Use For:** If you want full 3D note highway

---

### Option 4: Phaser

**Library:** https://phaser.io/

**Pros:**
- ✅ Full game framework
- ✅ Good for 2D games
- ✅ Built-in physics, input handling

**Cons:**
- ⚠️ Heavier than needed (~1MB)
- ⚠️ Designed for standalone games, not synced clients

**Use For:** If building full standalone mobile game

---

## Hosting Options

### Option 1: Vercel (PWA) / Heroku (Server) ⭐ RECOMMENDED FOR MVP

**Vercel (Frontend):**
- Free tier: Unlimited bandwidth
- CI/CD from GitHub
- Automatic SSL
- Global CDN

**Heroku (Backend WebSocket Server):**
- Free tier (with sleeps)
- $7/month hobby tier (no sleep)
- Easy deployment
- Good for Node.js

**Total Cost:** $0 (dev) → $7/month (production)

---

### Option 2: DigitalOcean Droplet

**Single VPS for everything:**
- $5-10/month (1-2GB RAM)
- Full control
- Install Node.js, Nginx, PM2
- Host frontend + backend on same server

**Good For:** Low-cost production, full control

---

### Option 3: AWS/GCP/Azure

**Cloud platforms:**
- More complex setup
- Better scaling options
- CloudFront (AWS) or Cloud CDN (GCP) for frontend
- Lambda/Cloud Functions for backend
- More expensive at scale

**Good For:** Professional production deployment

---

### Option 4: Cloudflare Workers

**Serverless edge computing:**
- Frontend on Cloudflare Pages (free)
- Backend on Cloudflare Workers
- Durable Objects for WebSocket state
- Global edge network

**Pros:**
- ✅ Extremely low latency
- ✅ Scales automatically
- ✅ Generous free tier

**Cons:**
- ⚠️ Different programming model
- ⚠️ Learning curve for Durable Objects

**Good For:** Global tours, low latency critical

---

## Recommended Stack (Final Verdict)

### For MVP / First Version:

**Frontend:** React PWA + Canvas 2D API  
**Backend:** Node.js + Socket.io  
**Unity:** WebSocketSharp  
**Hosting:** Vercel (frontend) + Heroku (backend)

**Total Cost:** ~$7/month  
**Development Time:** 3-4 months solo  
**Can Scale To:** 200+ concurrent users easily

---

### For Production / Tours:

**Frontend:** React PWA + PixiJS (for visual polish)  
**Backend:** Go + Gorilla WebSocket (better performance)  
**Unity:** WebSocketSharp (same)  
**Hosting:** DigitalOcean ($10/month) or Cloudflare Workers

**Total Cost:** ~$10-30/month  
**Can Scale To:** 1000+ concurrent users

---

## Migration Path

1. **Start:** React PWA + Node.js MVP
2. **If Successful:** Add PixiJS for better visuals
3. **If Scaling Issues:** Migrate backend to Go
4. **If Global Tours:** Move to Cloudflare Workers for edge latency

No need to over-engineer at start. Build simple, iterate based on real usage.
