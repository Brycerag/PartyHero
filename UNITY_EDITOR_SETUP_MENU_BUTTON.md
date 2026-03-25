# Unity Editor Setup Guide - PartyHero Menu Button

## Quick Steps (5 minutes)

### 1. Find the Main Menu
- Open Unity Editor
- Navigate to: `Assets/Scenes/Menu.unity` (or wherever main menu is)
- OR find the MainMenu prefab in Project window

### 2. Add the Button
- In Hierarchy, find the button container (likely under Canvas → MainMenu → Buttons or similar)
- Right-click an existing button (like "Practice") → Duplicate
- Rename to "PartyHeroButton"
- Reposition below Practice button (or wherever you prefer)

### 3. Change Button Text
- Select the new button in Hierarchy
- Expand it to find the Text/TextMeshPro child
- In Inspector, change text to: **"Start PartyHero Show"**
  - Or shorter: **"PartyHero"**
  - Or: **"Live Show"**

### 4. Wire the Click Event
- Select PartyHeroButton in Hierarchy
- Find the **Button** component in Inspector
- Scroll to **OnClick()** section
- Click the **+** button to add a new event
- Drag **MainMenu** object from Hierarchy to the object slot
- In the function dropdown, select: **MainMenu → PartyHero()**

### 5. Optional: Enable/Disable Based on Settings
If you want the button to be hidden when PartyHero is disabled:

Add this to MainMenu.cs Start() or Update():
```csharp
// In Start() method:
if (partyHeroButton != null)
{
    partyHeroButton.SetActive(SettingsManager.Settings.PartyHeroEnabled.Value);
}
```

Then add this field to MainMenu.cs:
```csharp
[SerializeField]
private GameObject partyHeroButton;
```

And drag the button object to this field in Inspector.

### 6. Test It!
- Enter Play mode
- Click "Start PartyHero Show" button
- File browser should open
- Navigate to example setlist: `Assets/StreamingAssets/example_setlist.json`
- Select it
- If setlist is valid, should load gameplay scene

## Troubleshooting

**Button doesn't appear:**
- Make sure it's enabled in Hierarchy
- Check parent containers are active
- Verify button is within canvas bounds

**Click does nothing:**
- Check OnClick() event is wired correctly
- Verify MainMenu object is assigned
- Check console for errors

**File browser doesn't open:**
- Verify FileExplorerHelper is working on your platform
- Check console for "Opening setlist browser..." message
- May need platform-specific file dialog support

**Setlist validation fails:**
- Make sure songs are in your YARG library
- Check console for specific errors
- Verify JSON is valid format
- Compare with `example_setlist.json`

## Visual Design Suggestions

**Button Styling:**
- Match YARG's existing button theme
- Use consistent colors/fonts with other menu buttons
- Consider adding an icon (Generic or music note)

**Placement:**
- Suggested: After "Practice" button (similar concept - non-standard play mode)
- Alternative: In settings menu under PartyHero section
- Alternative: New "Live Show" top-level menu item

**States:**
- Normal: Standard YARG button style
- Hover: Highlight effect
- Disabled: Grayed out when PartyHeroEnabled = false

## Next Steps After Button Works

Once the button successfully opens the file browser and loads a show:

1. **Score Scene Setup** (P0 - Critical)
   - Open `Assets/Scenes/Score.unity`
   - Add PartyHeroController GameObject
   - Create four UI canvases (see PARTYHERO_TESTING.md)
   - Wire all references

2. **Dialog System Integration** (P1 - Important)
   - Replace YargLogger calls in PartyHeroMenuIntegration
   - Use DialogManager.Instance.ShowDialog() for errors
   - Use DialogManager.Instance.ShowConfirmDialog() for warnings

3. **Testing** (P1)
   - Follow PARTYHERO_TESTING.md checklist
   - Test with multiple setlists
   - Verify validation works correctly

## That's It!

The code is complete. Just add the button and you're ready to test PartyHero shows!

```
[Main Menu] → [Start PartyHero Show] → [Select JSON] → [Validate] → [Play!]
```
