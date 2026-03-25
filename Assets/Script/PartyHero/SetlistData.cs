using System;
using System.Collections.Generic;
using YARG.Core.Song;

namespace YARG.PartyHero
{
    /// <summary>
    /// Represents a complete multi-set show with breaks, player swaps, and custom messages.
    /// This is the PartyHero enhanced setlist system.
    /// </summary>
    [Serializable]
    public class SetlistData
    {
        public string showName;
        public string venue;
        public string date;
        public string endMessage;
        public List<SetData> sets = new();
    }

    [Serializable]
    public class SetData
    {
        public int setNumber;
        public List<SetlistSongEntry> songs = new();
        public bool breakAfter;
        public string breakMessage;
        public int breakDurationSeconds = 900; // Default 15 minutes
    }

    [Serializable]
    public class SetlistSongEntry
    {
        /// <summary>
        /// Display name for this song in the show
        /// </summary>
        public string songName;
        
        /// <summary>
        /// Hash of the song to load from the YARG library
        /// </summary>
        public string songHash;
        
        /// <summary>
        /// Difficulty to play (e.g., "Expert", "Hard")
        /// </summary>
        public string difficulty;
        
        /// <summary>
        /// Whether a player swap should occur after this song
        /// </summary>
        public bool playerSwapAfter;
        
        /// <summary>
        /// Custom message to display during player swap
        /// </summary>
        public string swapMessage;
        
        /// <summary>
        /// Minimum time (in seconds) required for the player swap
        /// </summary>
        public int minimumSwapTime = 10;
    }

    /// <summary>
    /// Enhanced persistent state for PartyHero show flow
    /// </summary>
    public class PartyHeroState
    {
        public SetlistData currentSetlist;
        
        /// <summary>
        /// Current overall song index across all sets
        /// </summary>
        public int overallSongIndex;
        
        /// <summary>
        /// Show start time (for statistics)
        /// </summary>
        public DateTime showStartTime;
        
        /// <summary>
        /// Enable PartyHero show flow mode
        /// </summary>
        public bool partyHeroMode;
        
        /// <summary>
        /// Development mode enables keyboard shortcuts for testing without MIDI
        /// </summary>
        public bool developmentMode = true;

        /// <summary>
        /// Get the current song across all sets
        /// </summary>
        public SetlistSongEntry GetCurrentSong()
        {
            if (currentSetlist == null) return null;
            
            int count = 0;
            foreach (var set in currentSetlist.sets)
            {
                foreach (var song in set.songs)
                {
                    if (count == overallSongIndex)
                    {
                        return song;
                    }
                    count++;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the next song in the show (null if no more songs)
        /// </summary>
        public SetlistSongEntry GetNextSong()
        {
            if (currentSetlist == null) return null;
            
            int count = 0;
            foreach (var set in currentSetlist.sets)
            {
                foreach (var song in set.songs)
                {
                    if (count == overallSongIndex + 1)
                    {
                        return song;
                    }
                    count++;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if current song is the last in its set
        /// </summary>
        public bool IsLastSongInSet()
        {
            if (currentSetlist == null) return false;
            
            int count = 0;
            foreach (var set in currentSetlist.sets)
            {
                int songsInSet = set.songs.Count;
                if (overallSongIndex >= count && overallSongIndex < count + songsInSet)
                {
                    // We're in this set
                    return overallSongIndex == count + songsInSet - 1;
                }
                count += songsInSet;
            }
            return false;
        }

        /// <summary>
        /// Check if current song is the last in the entire show
        /// </summary>
        public bool IsLastSongInShow()
        {
            if (currentSetlist == null) return false;
            
            int totalSongs = GetTotalSongCount();
            return overallSongIndex >= totalSongs - 1;
        }

        /// <summary>
        /// Get the set that the current song is in
        /// </summary>
        public SetData GetCurrentSet()
        {
            if (currentSetlist == null) return null;
            
            int count = 0;
            foreach (var set in currentSetlist.sets)
            {
                int songsInSet = set.songs.Count;
                if (overallSongIndex >= count && overallSongIndex < count + songsInSet)
                {
                    return set;
                }
                count += songsInSet;
            }
            return null;
        }

        /// <summary>
        /// Get total number of songs in the show
        /// </summary>
        public int GetTotalSongCount()
        {
            if (currentSetlist == null) return 0;
            
            int count = 0;
            foreach (var set in currentSetlist.sets)
            {
                count += set.songs.Count;
            }
            return count;
        }

        /// <summary>
        /// Check if next state should be a set break
        /// </summary>
        public bool ShouldEnterSetBreak()
        {
            var currentSet = GetCurrentSet();
            return currentSet != null && currentSet.breakAfter && IsLastSongInSet();
        }

        /// <summary>
        /// Check if next state should be a player swap
        /// </summary>
        public bool ShouldEnterPlayerSwap()
        {
            var currentSong = GetCurrentSong();
            return currentSong != null && currentSong.playerSwapAfter;
        }
    }
}
