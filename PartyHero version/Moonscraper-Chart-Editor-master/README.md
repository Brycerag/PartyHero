> **⚠️ NOTE: THIS IS NOT THE APPLICATION PROGRAM, THESE ARE THE SOURCE FILES. ⚠️**
>
> If you are looking to download Moonscraper Chart Editor please see the
> [releases page](https://github.com/FireFox2000000/Moonscraper-Chart-Editor/releases).

## About
Moonscraper Chart Editor is a song editor for Guitar Hero style rhythm games mainly intended to support the custom song creation for games such as Guitar Hero, Clone Hero and Rock Band.

Trailer- https://www.youtube.com/watch?v=G8Qd32TZz4A

### Games that use Moonscraper code:
- Clone Hero (https://clonehero.net/)

### Games that use Moonscraper as a song editor:
- Everhood (https://store.steampowered.com/app/1229380/Everhood/)

## Compiling from Source
Follow the instructions below for your desired platform to build and run from source.

### All Platforms
1. Download and install Unity 2018.4.23f1
2. Run Unity and open the project folder with it
3. Use the menu option Build Processes > Build Full Releases
  - Note that 7zip and Inno Setup are required to be installed to build distributables and installers respectively. 

### Runtime dependencies (Windows)
Required runtime dependencies are included with the build.

### Runtime dependencies (Linux)
The application requires the following dependencies to be installed:
- `ffmpeg sdl2 libx11-6 libgtk-3-0`
- `libbass` (included with the build)

A [`PKGBUILD` file for Arch Linux](aur/PKGBUILD) is included in the repository.

Other distribution packagers can use the `PKGBUILD` file for reference.

## PartyHero Live Performance Documentation

This fork includes PartyHero-specific features for live band performances with audience participation. See the following documentation:

**Show Flow System:**
- [SHOW_FLOW_UI_TODO.md](SHOW_FLOW_UI_TODO.md) - Unity UI implementation checklist (12 phases, 90+ tasks)
- [SONG_TRANSITION_UX_SCENARIOS.md](SONG_TRANSITION_UX_SCENARIOS.md) - UX design decisions and flow diagrams
- [SONG_TRANSITION_IMPLEMENTATION.md](SONG_TRANSITION_IMPLEMENTATION.md) - Technical implementation details
- [MESSAGE_REFERENCE.md](MESSAGE_REFERENCE.md) - OSC/MIDI message reference for triggers and state broadcasting
- [MIDI_TESTING_CHECKLIST.md](MIDI_TESTING_CHECKLIST.md) - Comprehensive testing procedures (Section 8: Show Flow)

**External Sync:**
- [DAW_SYNC_SETUP.md](DAW_SYNC_SETUP.md) - AbleSet/Ableton Live integration guide
- [CONTINUOUS_TIMELINE_QUICKSTART.md](CONTINUOUS_TIMELINE_QUICKSTART.md) - Setlist timeline configuration
- [SETLIST_VERIFICATION_QUICKSTART.md](SETLIST_VERIFICATION_QUICKSTART.md) - Song mapping verification tools
- [STARPOWER_MIDI_OSC_GUIDE.md](STARPOWER_MIDI_OSC_GUIDE.md) - Star power automation for lighting/effects

## Who do I talk to?
* Alexander "FireFox" Ong
* Discord (Easiest link to contact me through)- https://discord.gg/bjsKTwd
* YouTube- https://www.youtube.com/user/FireFox2000000
* Twitter- https://twitter.com/FireFox2000000

## License
- See [attribution.txt](https://github.com/FireFox2000000/Moonscraper-Chart-Editor/blob/master/Moonscraper%20Chart%20Editor/Assets/Documentation/attribution.txt) for third party libraries and resources included in this repository.
- See [LICENSE](LICENSE).
- The BASS audio library (a dependency of this application) is a commercial product. While it is free for non-commercial use, please ensure to obtain a valid licence if you plan on distributing any application using it commercially.
