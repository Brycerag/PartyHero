# Show Flow UI - Unity Implementation TODO

This document tracks all Unity UI elements needed to replace the console-based Debug.Log stubs in the show flow system.

**Status:** All logic implemented ✅ | All UI stubs in place ✅ | Unity UI **NOT YET BUILT** ⚠️

---

## Overview

Five show flow screens need Unity UI:
1. **Results Screen** - Post-song statistics
2. **Waiting For Band** - Band coordination screen
3. **Waiting For Swap** - Player swap coordination
4. **Set End Screen** - Between-set break
5. **Show End Screen** - End of show credits

---

## Quick Start Guide

**Scene:** `Main Editor.unity` (Assets/Scenes/Game/)

**Unity Windows You'll Use:**
- **Hierarchy** - Create/organize GameObjects (left panel)
- **Inspector** - Configure components and properties (right panel)
- **Scene View** - Visual layout (center, 2D mode recommended for UI)
- **Project** - Access assets and scripts (bottom panel)

**Unity Components You'll Add:**
- **Canvas** - Container for UI elements
- **Image** - Backgrounds and icons
- **TextMeshProUGUI (TMP)** - All text elements
- **Button** - Interactive elements (optional for now)
- **RectTransform** - Already on all UI GameObjects (controls positioning/size)

**Basic Workflow for Each Task:**
1. Right-click in Hierarchy → Create Empty (or UI → Canvas/Image/Text)
2. Select GameObject, configure in Inspector
3. Repeat for child elements
4. Save scene (Ctrl+S)

---

## UI Architecture

```
DontDestroyOnLoad (Scene Root)
└─ UIServices (existing GameObject)
    ├─ gameplayUICanvas (existing)
    └─ showFlowUICanvas (NEW ⚠️)
        ├─ ResultsCanvas
        ├─ WaitingForBandCanvas
        ├─ WaitingForSwapCanvas
        ├─ SetEndCanvas
        └─ ShowEndCanvas
```

---

## 1. Results Screen

**Current Implementation:** [ResultsUISystem.cs](Moonscraper Chart Editor/Assets/Scripts/Game/Gameplay/ResultsUISystem.cs)

**Current Console Output:**
```
============================
       SONG COMPLETE
============================
Hit: 87.5%
Best Streak: 142
Notes Hit: 456 / 521
Next: Through the Fire and Flames
============================
Press SPACE to continue...
============================
```

### Required UI Components

**Canvas:** `ResultsCanvas`
- **Type:** Screen Space Overlay
- **Sort Order:** 100 (appears on top)
- **Initial State:** Hidden (enabled = false)

**Child Components:**

| GameObject Name | Component Type | Purpose | Inspector Settings |
|----------------|----------------|---------|-------------------|
| `Background` | Image | Dark semi-transparent backdrop | Color: #000000AA (black, 67% opacity) |
| `TitleText` | TextMeshProUGUI | "SONG COMPLETE" header | Font Size: 72, Alignment: Center, Color: White |
| `StatsText` | TextMeshProUGUI | Hit %, Streak, Notes Hit/Total | Font Size: 48, Alignment: Center, Color: Yellow |
| `NextSongText` | TextMeshProUGUI | "Next: [Song Name]" (optional) | Font Size: 32, Alignment: Center, Color: Cyan |
| `ContinuePrompt` | TextMeshProUGUI | "Press SPACE to continue..." | Font Size: 24, Alignment: Center, Color: White (50% opacity) |
| `ContinueButton` | Button | Invisible button for future click handling | Optional: Keep SPACE key as fallback |

**Layout Hierarchy:**
```
ResultsCanvas (Canvas)
├─ Background (Image - full screen)
├─ ContentPanel (RectTransform - centered container)
│   ├─ TitleText (TextMeshProUGUI)
│   ├─ StatsText (TextMeshProUGUI)
│   ├─ NextSongText (TextMeshProUGUI)
│   └─ ContinuePrompt (TextMeshProUGUI)
└─ ContinueButton (Button - full screen invisible clickable area)
```

**Code Integration Points:**
```csharp
// In ResultsUISystem.SystemEnter():
uiManager.resultsCanvas.enabled = true;
uiManager.resultsStatsText.text = $"Hit: {hitPercent:F1}%\n...";
uiManager.resultsNextSongText.text = $"Next: {nextSongName}";
uiManager.resultsContinueButton.onClick.AddListener(OnContinuePressed);
```

---

## 2. Waiting For Band Screen

**Current Implementation:** [WaitingForBandUISystem.cs](Moonscraper Chart Editor/Assets/Scripts/Game/Gameplay/WaitingForBandUISystem.cs)

**Current Console Output:**
```
============================
   WAITING FOR BAND
============================
✓ Player Ready
○ Band Not Ready
============================
(Updates every 0.5 seconds)
```

### Required UI Components

**Canvas:** `WaitingForBandCanvas`

**Child Components:**

| GameObject Name | Component Type | Purpose | Inspector Settings |
|----------------|----------------|---------|-------------------|
| `Background` | Image | Dark backdrop | Color: #001122AA (dark blue, 67% opacity) |
| `TitleText` | TextMeshProUGUI | "WAITING FOR BAND" header | Font Size: 60, Color: White |
| `PlayerReadyIcon` | Image | Check mark or circle indicator | Sprite: ✓ or ○, Color: Green/Gray |
| `PlayerReadyText` | TextMeshProUGUI | "Player Ready" status | Font Size: 36, Color: White |
| `BandReadyIcon` | Image | Check mark or circle indicator | Sprite: ✓ or ○, Color: Green/Gray |
| `BandReadyText` | TextMeshProUGUI | "Band Ready" status | Font Size: 36, Color: White |
| `DevHelpText` | TextMeshProUGUI | "Dev: P = Force Player, B = Band Ready" | Font Size: 18, Color: Gray, Optional |

**Layout Hierarchy:**
```
WaitingForBandCanvas (Canvas)
├─ Background (Image)
├─ TitleText (TextMeshProUGUI)
├─ PlayerStatusRow (HorizontalLayoutGroup)
│   ├─ PlayerReadyIcon (Image)
│   └─ PlayerReadyText (TextMeshProUGUI)
├─ BandStatusRow (HorizontalLayoutGroup)
│   ├─ BandReadyIcon (Image)
│   └─ BandReadyText (TextMeshProUGUI)
└─ DevHelpText (TextMeshProUGUI)
```

**Code Integration Points:**
```csharp
// In WaitingForBandUISystem.SystemUpdate() - every 0.5s:
uiManager.playerReadyIcon.sprite = ShowFlowManager.Instance.isPlayerReady ? checkSprite : circleSprite;
uiManager.playerReadyIcon.color = ShowFlowManager.Instance.isPlayerReady ? Color.green : Color.gray;
uiManager.bandReadyIcon.sprite = ShowFlowManager.Instance.isBandReady ? checkSprite : circleSprite;
uiManager.bandReadyIcon.color = ShowFlowManager.Instance.isBandReady ? Color.green : Color.gray;
```

---

## 3. Waiting For Swap Screen

**Current Implementation:** [WaitingForSwapUISystem.cs](Moonscraper Chart Editor/Assets/Scripts/Game/Gameplay/WaitingForSwapUISystem.cs)

**Current Console Output:**
```
=============================================
         PLAYER SWAP TIME
=============================================

  Please swap to the next player position

  Press the player ready trigger when ready
  (Development: Press R to simulate)

=============================================
Player Status: ○ Swapping...
(Updates every 0.5 seconds)
```

### Required UI Components

**Canvas:** `WaitingForSwapCanvas`

**Child Components:**

| GameObject Name | Component Type | Purpose | Inspector Settings |
|----------------|----------------|---------|-------------------|
| `Background` | Image | Dark backdrop | Color: #220000AA (dark red, 67% opacity) |
| `TitleText` | TextMeshProUGUI | "PLAYER SWAP TIME" header | Font Size: 60, Color: Yellow |
| `InstructionText` | TextMeshProUGUI | "Please swap to the next player position" | Font Size: 32, Color: White |
| `PromptText` | TextMeshProUGUI | "Press the player ready trigger when ready" | Font Size: 24, Color: White (70% opacity) |
| `PlayerStatusIcon` | Image | Check mark or circle indicator | Sprite: ✓ or ○, Color: Green/Gray |
| `PlayerStatusText` | TextMeshProUGUI | "Ready" or "Swapping..." | Font Size: 36, Color: White |
| `DevHelpText` | TextMeshProUGUI | "Dev: R = Player Ready" | Font Size: 18, Color: Gray, Optional |

**Layout Hierarchy:**
```
WaitingForSwapCanvas (Canvas)
├─ Background (Image)
├─ ContentPanel (VerticalLayoutGroup - centered)
│   ├─ TitleText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 40)
│   ├─ InstructionText (TextMeshProUGUI)
│   ├─ PromptText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 60)
│   ├─ PlayerStatusRow (HorizontalLayoutGroup)
│   │   ├─ PlayerStatusIcon (Image)
│   │   └─ PlayerStatusText (TextMeshProUGUI)
│   └─ DevHelpText (TextMeshProUGUI)
```

**Code Integration Points:**
```csharp
// In WaitingForSwapUISystem.UpdateDisplay():
bool ready = ShowFlowManager.Instance.isPlayerReady;
uiManager.playerSwapStatusIcon.sprite = ready ? checkSprite : circleSprite;
uiManager.playerSwapStatusIcon.color = ready ? Color.green : Color.gray;
uiManager.playerSwapStatusText.text = ready ? "✓ Ready" : "○ Swapping...";
```

---

## 4. Set End Screen

**Current Implementation:** [SetEndUISystem.cs](Moonscraper Chart Editor/Assets/Scripts/Game/Gameplay/SetEndUISystem.cs)

**Current Console Output:**
```
=============================================
            SET BREAK
=============================================

  Take a breather! Next set coming up...

  Press trigger to resume or end show
  (Development: R = Resume, E = End Show)

=============================================
Still in set break...
(Updates every 2 seconds)
```

### Required UI Components

**Canvas:** `SetEndCanvas`

**Child Components:**

| GameObject Name | Component Type | Purpose | Inspector Settings |
|----------------|----------------|---------|-------------------|
| `Background` | Image | Dark backdrop | Color: #000022AA (dark purple, 67% opacity) |
| `TitleText` | TextMeshProUGUI | "SET BREAK" header | Font Size: 72, Color: Cyan |
| `MessageText` | TextMeshProUGUI | "Take a breather! Next set coming up..." | Font Size: 36, Color: White |
| `PromptText` | TextMeshProUGUI | "Press trigger to resume or end show" | Font Size: 24, Color: White (70% opacity) |
| `ResumeButton` | Button | Optional visual button for resume | Optional: Keep R key as fallback |
| `EndShowButton` | Button | Optional visual button for end show | Optional: Keep E key as fallback |
| `DevHelpText` | TextMeshProUGUI | "Dev: R = Resume, E = End Show" | Font Size: 18, Color: Gray, Optional |

**Layout Hierarchy:**
```
SetEndCanvas (Canvas)
├─ Background (Image)
├─ ContentPanel (VerticalLayoutGroup - centered)
│   ├─ TitleText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 40)
│   ├─ MessageText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 60)
│   ├─ PromptText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 40)
│   ├─ ButtonRow (HorizontalLayoutGroup)
│   │   ├─ ResumeButton (Button)
│   │   └─ EndShowButton (Button)
│   └─ DevHelpText (TextMeshProUGUI)
```

**Code Integration Points:**
```csharp
// In SetEndUISystem.SystemEnter():
uiManager.setEndCanvas.enabled = true;
// Optional: Hook buttons
uiManager.setEndResumeButton?.onClick.AddListener(() => GetSetEndState().ResumeShow());
uiManager.setEndEndShowButton?.onClick.AddListener(() => GetSetEndState().EndShow());
```

---

## 5. Show End Screen

**Current Implementation:** [ShowEndUISystem.cs](Moonscraper Chart Editor/Assets/Scripts/Game/Gameplay/ShowEndUISystem.cs)

**Current Console Output:**
```
=============================================
          SHOW COMPLETE!
=============================================

  Thank you for an amazing performance!

  Hope to see you again soon!

  (Development: Press ESC to return to editor)

=============================================
Show ended 02:45 ago
(Updates every 5 seconds)
```

### Required UI Components

**Canvas:** `ShowEndCanvas`

**Child Components:**

| GameObject Name | Component Type | Purpose | Inspector Settings |
|----------------|----------------|---------|-------------------|
| `Background` | Image | Dark backdrop | Color: #000000DD (black, 87% opacity) |
| `TitleText` | TextMeshProUGUI | "SHOW COMPLETE!" header | Font Size: 84, Color: Gold/Yellow |
| `ThankYouText` | TextMeshProUGUI | "Thank you for an amazing performance!" | Font Size: 48, Color: White |
| `ClosingText` | TextMeshProUGUI | "Hope to see you again soon!" | Font Size: 32, Color: White (80% opacity) |
| `ElapsedTimeText` | TextMeshProUGUI | "Show ended XX:XX ago" | Font Size: 24, Color: Gray |
| `CreditsScrollView` | ScrollRect | Optional: Scrolling credits | Optional: For future expansion |
| `ExitPrompt` | TextMeshProUGUI | "Press ESC to return to editor" | Font Size: 18, Color: Gray, Optional |

**Layout Hierarchy:**
```
ShowEndCanvas (Canvas)
├─ Background (Image)
├─ ContentPanel (VerticalLayoutGroup - centered)
│   ├─ TitleText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 60)
│   ├─ ThankYouText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 40)
│   ├─ ClosingText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 80)
│   ├─ ElapsedTimeText (TextMeshProUGUI)
│   ├─ Spacer (LayoutElement - height: 40)
│   └─ ExitPrompt (TextMeshProUGUI)
```

**Code Integration Points:**
```csharp
// In ShowEndUISystem.SystemUpdate() - every 5 seconds:
float elapsed = Time.time - displayStartTime;
int min = Mathf.FloorToInt(elapsed / 60f);
int sec = Mathf.FloorToInt(elapsed % 60f);
uiManager.showEndElapsedTimeText.text = $"Show ended {min:00}:{sec:00} ago";
```

---

## ShowFlowUIManager Component

**File:** `Assets/Scripts/Game/UI/ShowFlowUIManager.cs` (NEW - needs creation ⚠️)

**Purpose:** Central manager for all show flow UI references. Attached to `showFlowUICanvas` GameObject.

### Required Public Fields

```csharp
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowFlowUIManager : MonoBehaviour
{
    [Header("Canvas References")]
    public Canvas resultsCanvas;
    public Canvas waitingForBandCanvas;
    public Canvas waitingForSwapCanvas;
    public Canvas setEndCanvas;
    public Canvas showEndCanvas;
    
    [Header("Results Screen")]
    public TextMeshProUGUI resultsTitleText;
    public TextMeshProUGUI resultsStatsText;
    public TextMeshProUGUI resultsNextSongText;
    public TextMeshProUGUI resultsContinuePrompt;
    public Button resultsContinueButton;
    
    [Header("Waiting For Band Screen")]
    public TextMeshProUGUI bandTitleText;
    public Image bandPlayerReadyIcon;
    public TextMeshProUGUI bandPlayerReadyText;
    public Image bandBandReadyIcon;
    public TextMeshProUGUI bandBandReadyText;
    
    [Header("Waiting For Swap Screen")]
    public TextMeshProUGUI swapTitleText;
    public TextMeshProUGUI swapInstructionText;
    public TextMeshProUGUI swapPromptText;
    public Image swapPlayerStatusIcon;
    public TextMeshProUGUI swapPlayerStatusText;
    
    [Header("Set End Screen")]
    public TextMeshProUGUI setEndTitleText;
    public TextMeshProUGUI setEndMessageText;
    public TextMeshProUGUI setEndPromptText;
    public Button setEndResumeButton;  // Optional
    public Button setEndEndShowButton; // Optional
    
    [Header("Show End Screen")]
    public TextMeshProUGUI showEndTitleText;
    public TextMeshProUGUI showEndThankYouText;
    public TextMeshProUGUI showEndClosingText;
    public TextMeshProUGUI showEndElapsedTimeText;
    
    [Header("Shared Resources")]
    public Sprite checkMarkSprite;     // ✓ icon
    public Sprite circleSprite;        // ○ icon
    
    void Awake()
    {
        HideAllScreens();
    }
    
    public void HideAllScreens()
    {
        resultsCanvas.enabled = false;
        waitingForBandCanvas.enabled = false;
        waitingForSwapCanvas.enabled = false;
        setEndCanvas.enabled = false;
        showEndCanvas.enabled = false;
    }
}
```

---

## UIServices Integration

**File:** `Assets/Scripts/Game/UI/Menu Component Lookups/UIServices.cs` (MODIFY ⚠️)

### Changes Needed

**Add Fields:**
```csharp
[SerializeField] GameObject showFlowUICanvas;

ShowFlowUIManager m_showFlowUI = null;
public ShowFlowUIManager showFlowUI
{
    get
    {
        if (!m_showFlowUI)
            m_showFlowUI = GetComponentInChildren<ShowFlowUIManager>();
        return m_showFlowUI;
    }
}
```

**Add Method:**
```csharp
public void SetShowFlowUIActive(bool active)
{
    if (showFlowUICanvas != null)
        showFlowUICanvas.SetActive(active);
}
```

---

## UI System Updates

Each of the five UI system classes needs to be updated to use `ShowFlowUIManager` instead of `Debug.Log()`.

### Files to Modify:

1. ✅ **ResultsUISystem.cs** - Replace Debug.Log with TextMeshPro updates
2. ✅ **WaitingForBandUISystem.cs** - Replace Debug.Log with Icon/Text updates
3. ✅ **WaitingForSwapUISystem.cs** - Replace Debug.Log with Icon/Text updates
4. ✅ **SetEndUISystem.cs** - Replace Debug.Log with TextMeshPro updates
5. ✅ **ShowEndUISystem.cs** - Replace Debug.Log with TextMeshPro updates

**Access Pattern:**
```csharp
public override void SystemEnter()
{
    var uiManager = ChartEditor.Instance.uiServices.showFlowUI;
    uiManager.HideAllScreens();
    uiManager.resultsCanvas.enabled = true;
    uiManager.resultsStatsText.text = "...";
}
```

---

## Implementation Checklist

**All work happens in:** `Main Editor.unity` scene (Assets/Scenes/Game/)  
**Tip:** Keep this scene open while working through phases below.

### Phase 1: Unity Scene Setup

- [ ] Open Unity project
- [ ] Load scene: `Main Editor.unity` (Assets/Scenes/Game/)
- [ ] In Hierarchy, locate `UIServices` GameObject (under DontDestroyOnLoad)
- [ ] Right-click `UIServices` → Create Empty → Rename to `showFlowUICanvas`
- [ ] Select `showFlowUICanvas` → Inspector → Add Component → Canvas
- [ ] Set Canvas render mode to "Screen Space - Overlay"

### Phase 2: Create UI Canvases

**Tool:** Right-click `showFlowUICanvas` in Hierarchy → UI → Canvas (or Create Empty + add Canvas component)

- [ ] Create `ResultsCanvas` child under `showFlowUICanvas`
- [ ] Create `WaitingForBandCanvas` child under `showFlowUICanvas`
- [ ] Create `WaitingForSwapCanvas` child under `showFlowUICanvas`
- [ ] Create `SetEndCanvas` child under `showFlowUICanvas`
- [ ] Create `ShowEndCanvas` child under `showFlowUICanvas`
- [ ] For each: In Inspector, set initial state `enabled = false` (un-check the checkbox)

### Phase 3: Build Results Screen

**Tools:** Right-click `ResultsCanvas` → UI → Image / Text - TextMeshPro

- [ ] Add Background Image to ResultsCanvas (UI → Image)
- [ ] Add TitleText (UI → Text - TextMeshPro) - "SONG COMPLETE"
- [ ] Add StatsText (UI → Text - TextMeshPro) - Hit %, Streak, Notes
- [ ] Add NextSongText (UI → Text - TextMeshPro) - "Next: [Song Name]"
- [ ] Add ContinuePrompt (UI → Text - TextMeshPro) - "Press SPACE to continue..."
- [ ] Add ContinueButton (UI → Button) - Optional invisible full-screen button

### Phase 4: Build Waiting For Band Screen

**Tools:** Right-click `WaitingForBandCanvas` → UI → Image / Text - TextMeshPro

- [ ] Add Background Image to WaitingForBandCanvas (UI → Image)
- [ ] Add TitleText (UI → Text - TextMeshPro) - "WAITING FOR BAND"
- [ ] Add PlayerReadyIcon (UI → Image) + PlayerReadyText (UI → Text - TextMeshPro)
- [ ] Add BandReadyIcon (UI → Image) + BandReadyText (UI → Text - TextMeshPro)
- [ ] Create/import check mark sprite (✓) in Assets/Art/UI/Icons/
- [ ] Create/import circle sprite (○) in Assets/Art/UI/Icons/
- [ ] Set initial state: Canvas enabled = false

### Phase 5: Build Waiting For Swap Screen

**Tools:** Right-click `WaitingForSwapCanvas` → UI → Image / Text - TextMeshPro

- [ ] Add Background Image to WaitingForSwapCanvas (UI → Image)
- [ ] Add TitleText (UI → Text - TextMeshPro) - "PLAYER SWAP TIME"
- [ ] Add InstructionText (UI → Text - TextMeshPro) - Swap instructions
- [ ] Add PromptText (UI → Text - TextMeshPro) - Ready trigger prompt
- [ ] Add PlayerStatusIcon (UI → Image) + PlayerStatusText (UI → Text - TextMeshPro)

### Phase 6: Build Set End Screen

**Tools:** Right-click `SetEndCanvas` → UI → Image / Text - TextMeshPro / Button

- [ ] Add Background Image to SetEndCanvas (UI → Image)
- [ ] Add TitleText (UI → Text - TextMeshPro) - "SET BREAK"
- [ ] Add MessageText (UI → Text - TextMeshPro) - Break message
- [ ] Add PromptText (UI → Text - TextMeshPro) - Resume/end prompt
- [ ] Add ResumeButton (UI → Button) - Optional
- [ ] Add EndShowButton (UI → Button) - Optional

### Phase 7: Build Show End Screen

**Tools:** Right-click `ShowEndCanvas` → UI → Image / Text - TextMeshPro

- [ ] Add Background Image to ShowEndCanvas (UI → Image)
- [ ] Add TitleText (UI → Text - TextMeshPro) - "SHOW COMPLETE!"
- [ ] Add ThankYouText (UI → Text - TextMeshPro) - Thank you message
- [ ] Add ClosingText (UI → Text - TextMeshPro) - Closing message
- [ ] Add ElapsedTimeText (UI → Text - TextMeshPro) - Time counter

### Phase 8: Create ShowFlowUIManager

**Tools:** Project panel → Assets/Scripts/Game/UI/ → Right-click → Create → C# Script

- [ ] Create `ShowFlowUIManager.cs` in `Assets/Scripts/Game/UI/`
- [ ] Copy component code from this document (see ShowFlowUIManager section below)
- [ ] In Hierarchy, select `showFlowUICanvas` GameObject
- [ ] In Inspector, click Add Component → search "ShowFlowUIManager" → Add

### Phase 9: Assign References in Inspector

**Tools:** Inspector panel (drag GameObjects from Hierarchy onto fields)

- [ ] In Hierarchy, select `showFlowUICanvas` GameObject
- [ ] In Inspector, find ShowFlowUIManager component
- [ ] Drag Canvas GameObjects from Hierarchy to fields:
  - [ ] Drag ResultsCanvas to `resultsCanvas` field
  - [ ] Drag WaitingForBandCanvas to `waitingForBandCanvas` field
  - [ ] Drag WaitingForSwapCanvas to `waitingForSwapCanvas` field
  - [ ] Drag SetEndCanvas to `setEndCanvas` field
  - [ ] Drag ShowEndCanvas to `showEndCanvas` field
- [ ] Expand canvases in Hierarchy and drag child elements:
  - [ ] Assign all Results Screen text/button references
  - [ ] Assign all Waiting For Band text/icon references
  - [ ] Assign all Waiting For Swap text/icon references
  - [ ] Assign all Set End text/button references
  - [ ] Assign all Show End text references
- [ ] Drag sprites from Project panel:
  - [ ] Assign checkMarkSprite from Assets/Art/UI/Icons/
  - [ ] Assign circleSprite from Assets/Art/UI/Icons/

### Phase 10: Update UIServices

**Tools:** Code editor (VS Code/Visual Studio) + Unity Inspector

- [ ] Open `UIServices.cs` in code editor (double-click in Project panel)
- [ ] Add `showFlowUICanvas` SerializeField at top of class
- [ ] Add `showFlowUI` property with GetComponentInChildren<ShowFlowUIManager>()
- [ ] Add `SetShowFlowUIActive(bool active)` helper method
- [ ] Save file, return to Unity (wait for recompile)
- [ ] In Hierarchy, select UIServices GameObject
- [ ] In Inspector, drag `showFlowUICanvas` from Hierarchy to new `showFlowUICanvas` field

### Phase 11: Update UI System Classes

**Tools:** Code editor - Replace Debug.Log() calls with ShowFlowUIManager calls

- [ ] Update `ResultsUISystem.cs` - Replace Debug.Log with uiManager calls
- [ ] Update `WaitingForBandUISystem.cs` - Replace Debug.Log with uiManager calls
- [ ] Update `WaitingForSwapUISystem.cs` - Replace Debug.Log with uiManager calls
- [ ] Update `SetEndUISystem.cs` - Replace Debug.Log with uiManager calls
- [ ] Update `ShowEndUISystem.cs` - Replace Debug.Log with uiManager calls
- [ ] See "UI System Updates" section below for code patterns

### Phase 12: Testing

**Tools:** Unity Play mode + Unity Console (Window → General → Console)

- [ ] Hit Play button in Unity Editor
- [ ] Test Results screen appears after song ends
- [ ] Test stats display correctly on Results screen
- [ ] Test "Next: [Song]" shows when enabled
- [ ] Test SPACE key continues from Results screen
- [ ] Test Waiting For Band screen appears (continuing mode)
- [ ] Test ready indicators update in real-time
- [ ] Test P/B dev keys work on Waiting For Band screen (see MESSAGE_REFERENCE.md)
- [ ] Test Waiting For Swap screen appears (swap mode)
- [ ] Test R key works on Waiting For Swap screen
- [ ] Test Set End screen appears on /set/end trigger
- [ ] Test R/E keys work on Set End screen
- [ ] Test Show End screen appears on /show/end trigger
- [ ] Test ESC key works on Show End screen
- [ ] Test elapsed time counter updates on Show End screen

---

## Design Notes

### Color Scheme Recommendations

- **Results:** Dark background (#000000AA), White text, Yellow stats, Cyan "Next Song"
- **Waiting For Band:** Dark blue background (#001122AA), White text, Green check marks
- **Waiting For Swap:** Dark red background (#220000AA), Yellow title, White text
- **Set End:** Dark purple background (#000022AA), Cyan title, White text
- **Show End:** Very dark background (#000000DD), Gold/Yellow title, White text

### Font Size Hierarchy

- **Titles:** 60-84pt (e.g., "SONG COMPLETE", "SHOW COMPLETE!")
- **Body Text:** 32-48pt (e.g., stats, instructions)
- **Prompts:** 24-32pt (e.g., "Press SPACE to continue...")
- **Dev Help Text:** 18pt (e.g., "Dev: R = Resume")

### Layout Tips

- Use **VerticalLayoutGroup** for stacked content (centered)
- Use **HorizontalLayoutGroup** for icon + text pairs
- Use **LayoutElement** for spacing between sections
- Use **ContentSizeFitter** for automatic text sizing
- Set all text to **Alignment: Center (Horizontal and Vertical)**

### Icon Resources

You'll need two simple sprite assets:
1. **Check Mark (✓)** - 64x64px, white, transparent background
2. **Circle (○)** - 64x64px, white, transparent background

Save as PNG in `Assets/Art/UI/Icons/` or use TextMeshPro's built-in sprites.

---

## Current Status

**Logic:** ✅ Complete - All state machines, transitions, and triggers working  
**Console Stubs:** ✅ Complete - All Debug.Log output functional for testing  
**Unity UI:** ⚠️ **NOT STARTED** - All visual elements need to be built  
**Integration:** ⚠️ Pending - UI System classes need updates after Unity UI is built

**Next Step:** Start with Phase 1 (Unity Scene Setup) and work through the checklist!

---

## Related Documentation

- [MESSAGE_REFERENCE.md](MESSAGE_REFERENCE.md) - OSC/MIDI triggers for show flow
- [SONG_TRANSITION_UX_SCENARIOS.md](SONG_TRANSITION_UX_SCENARIOS.md) - UX design decisions
- [MIDI_TESTING_CHECKLIST.md](MIDI_TESTING_CHECKLIST.md) - Section 8 for show flow testing
- [SONG_TRANSITION_IMPLEMENTATION.md](SONG_TRANSITION_IMPLEMENTATION.md) - Implementation plan

---

**Last Updated:** Session with show flow implementation completion  
**Author:** PartyHero Development Team
