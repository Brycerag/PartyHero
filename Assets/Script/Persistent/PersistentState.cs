using System.Collections.Generic;
using YARG.Core.Replays;
using YARG.Core.Song;
using YARG.Menu.ScoreScreen;
using YARG.Replays;
using YARG.PartyHero;

namespace YARG
{
    public struct PersistentState
    {
        public static PersistentState Default => new()
        {
            SongSpeed = 1f,
            ShowSongs = new List<SongEntry>(),
            PartyHero = null,
        };

        public SongEntry   CurrentSong;
#nullable enable
        public ReplayInfo? CurrentReplay;
#nullable disable

        public ScoreScreenStats? ScoreScreenStats;

        public float SongSpeed;

        public          bool            PlayingAShow { get; set; }
        public          List<SongEntry> ShowSongs    { get; set; }
        public          int             ShowIndex    { get; set; }

        public          bool IsPractice;
        public readonly bool IsReplay => CurrentReplay is not null;
        public          bool PlayingWithReplay;

        // PartyHero: Enhanced show flow with band coordination, breaks, and swaps
        public PartyHeroState PartyHero { get; set; }

        // Helper to check if PartyHero mode is active
        public readonly bool IsPartyHeroMode => PartyHero != null && PartyHero.partyHeroMode;
    }
}