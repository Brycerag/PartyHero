using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using YARG.Core.Logging;
using YARG.Core.Song;
using YARG.Song;

namespace YARG.PartyHero
{
    /// <summary>
    /// Manages loading and validation of PartyHero setlists
    /// </summary>
    public static class SetlistManager
    {
        /// <summary>
        /// Load a setlist from a JSON file
        /// </summary>
        public static SetlistData LoadSetlist(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    YargLogger.LogError($"[PartyHero] Setlist file not found: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                var setlist = JsonUtility.FromJson<SetlistData>(json);

                if (setlist == null)
                {
                    YargLogger.LogError($"[PartyHero] Failed to parse setlist JSON: {filePath}");
                    return null;
                }

                YargLogger.LogInfo($"[PartyHero] Loaded setlist: {setlist.showName}");
                return setlist;
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, $"Failed to load setlist from {filePath}");
                return null;
            }
        }

        /// <summary>
        /// Validate a setlist and check if all songs exist in the YARG library
        /// </summary>
        public static SetlistValidationResult ValidateSetlist(SetlistData setlist)
        {
            var result = new SetlistValidationResult
            {
                isValid = true,
                errors = new List<string>(),
                warnings = new List<string>()
            };

            if (setlist == null)
            {
                result.isValid = false;
                result.errors.Add("Setlist is null");
                return result;
            }

            if (setlist.sets == null || setlist.sets.Count == 0)
            {
                result.isValid = false;
                result.errors.Add("Setlist has no sets");
                return result;
            }

            int totalSongs = 0;
            foreach (var set in setlist.sets)
            {
                if (set.songs == null || set.songs.Count == 0)
                {
                    result.warnings.Add($"Set {set.setNumber} has no songs");
                    continue;
                }

                foreach (var song in set.songs)
                {
                    totalSongs++;

                    // Validate song hash
                    if (string.IsNullOrEmpty(song.songHash))
                    {
                        result.errors.Add($"Song '{song.songName}' has no songHash");
                        result.isValid = false;
                        continue;
                    }

                    // Try to find the song in YARG's library
                    var songEntry = SongContainer.SongsByHash.TryGetValue(song.songHash, out var entry) 
                        ? entry 
                        : null;

                    if (songEntry == null)
                    {
                        result.errors.Add($"Song not found in library: '{song.songName}' (hash: {song.songHash})");
                        result.isValid = false;
                    }
                }
            }

            if (totalSongs == 0)
            {
                result.isValid = false;
                result.errors.Add("Setlist has no songs");
            }

            result.totalSongs = totalSongs;
            return result;
        }

        /// <summary>
        /// Convert a setlist to YARG's native ShowSongs list
        /// </summary>
        public static List<SongEntry> ConvertToShowSongs(SetlistData setlist)
        {
            var showSongs = new List<SongEntry>();

            if (setlist == null || setlist.sets == null)
            {
                return showSongs;
            }

            foreach (var set in setlist.sets)
            {
                if (set.songs == null) continue;

                foreach (var song in set.songs)
                {
                    if (string.IsNullOrEmpty(song.songHash))
                    {
                        YargLogger.LogWarning($"[PartyHero] Skipping song '{song.songName}' - no hash");
                        continue;
                    }

                    // Look up the song in YARG's library
                    if (SongContainer.SongsByHash.TryGetValue(song.songHash, out var songEntry))
                    {
                        showSongs.Add(songEntry);
                    }
                    else
                    {
                        YargLogger.LogWarning($"[PartyHero] Song not found in library: {song.songName}");
                    }
                }
            }

            return showSongs;
        }

        /// <summary>
        /// Create a sample setlist for testing
        /// </summary>
        public static string CreateSampleSetlist()
        {
            var setlist = new SetlistData
            {
                showName = "PartyHero Test Show",
                venue = "Test Venue",
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                endMessage = "Thank you for testing PartyHero!",
                sets = new List<SetData>
                {
                    new SetData
                    {
                        setNumber = 1,
                        songs = new List<SetlistSongEntry>
                        {
                            new SetlistSongEntry
                            {
                                songName = "Test Song 1",
                                songHash = "REPLACE_WITH_ACTUAL_HASH",
                                difficulty = "Expert",
                                playerSwapAfter = false
                            },
                            new SetlistSongEntry
                            {
                                songName = "Test Song 2",
                                songHash = "REPLACE_WITH_ACTUAL_HASH",
                                difficulty = "Expert",
                                playerSwapAfter = true,
                                swapMessage = "Guitarist trades with bassist!",
                                minimumSwapTime = 10
                            }
                        },
                        breakAfter = true,
                        breakMessage = "15 minute intermission - grab a drink!",
                        breakDurationSeconds = 900
                    },
                    new SetData
                    {
                        setNumber = 2,
                        songs = new List<SetlistSongEntry>
                        {
                            new SetlistSongEntry
                            {
                                songName = "Test Song 3",
                                songHash = "REPLACE_WITH_ACTUAL_HASH",
                                difficulty = "Expert",
                                playerSwapAfter = false
                            }
                        },
                        breakAfter = false
                    }
                }
            };

            return JsonUtility.ToJson(setlist, true);
        }
    }

    /// <summary>
    /// Result of setlist validation
    /// </summary>
    public class SetlistValidationResult
    {
        public bool isValid;
        public List<string> errors;
        public List<string> warnings;
        public int totalSongs;

        public void LogResults()
        {
            YargLogger.LogInfo($"[PartyHero] Setlist Validation: {(isValid ? "PASSED" : "FAILED")}");
            YargLogger.LogInfo($"[PartyHero] Total Songs: {totalSongs}");

            if (warnings.Count > 0)
            {
                YargLogger.LogInfo($"[PartyHero] Warnings ({warnings.Count}):");
                foreach (var warning in warnings)
                {
                    YargLogger.LogWarning($"  - {warning}");
                }
            }

            if (errors.Count > 0)
            {
                YargLogger.LogInfo($"[PartyHero] Errors ({errors.Count}):");
                foreach (var error in errors)
                {
                    YargLogger.LogError($"  - {error}");
                }
            }
        }
    }
}
