using System;
using System.IO;
using UnityEngine;
using YARG.Core.Logging;
using YARG.Helpers;
using YARG.Menu.MusicLibrary;

namespace YARG.PartyHero
{
    /// <summary>
    /// Menu integration for PartyHero
    /// Handles setlist file browsing and show starting
    /// </summary>
    public static class PartyHeroMenuIntegration
    {
        /// <summary>
        /// Open file browser to select a setlist JSON
        /// Called from MainMenu button
        /// </summary>
        public static void BrowseForSetlist()
        {
            YargLogger.LogInfo("[PartyHero] Opening setlist browser...");

            // Start from last used directory or default to user documents
            string startingDir = PlayerPrefs.GetString("PartyHero.LastSetlistDir", 
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            FileExplorerHelper.OpenChooseFile(startingDir, "json", OnSetlistSelected);
        }

        private static void OnSetlistSelected(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                YargLogger.LogInfo("[PartyHero] Setlist selection cancelled");
                return;
            }

            YargLogger.LogInfo($"[PartyHero] Setlist selected: {path}");

            try
            {
                // Remember this directory for next time
                string directory = Path.GetDirectoryName(path);
                PlayerPrefs.SetString("PartyHero.LastSetlistDir", directory);
                PlayerPrefs.Save();

                // Load and validate the setlist
                var setlistData = SetlistManager.LoadSetlist(path);
                if (setlistData == null)
                {
                    ShowError("Failed to load setlist file. Check console for details.");
                    return;
                }

                // Validate songs exist in library
                var validation = SetlistManager.ValidateSetlist(setlistData);
                
                if (!validation.IsValid)
                {
                    ShowValidationErrors(validation);
                    return;
                }

                if (validation.Warnings.Count > 0)
                {
                    ShowValidationWarnings(validation, () => StartShow(setlistData));
                }
                else
                {
                    StartShow(setlistData);
                }
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to process setlist");
                ShowError($"Error loading setlist: {ex.Message}");
            }
        }

        private static void StartShow(SetlistData setlistData)
        {
            YargLogger.LogInfo($"[PartyHero] Starting show: {setlistData.showName}");

            // Create PartyHero state
            var partyHeroState = new PartyHeroState
            {
                partyHeroMode = true,
                currentSetlist = setlistData,
                overallSongIndex = 0,
                currentSetIndex = 0,
                showStartTime = DateTime.Now
            };

            // Configure development mode from settings
            partyHeroState.developmentMode = Settings.SettingsManager.Settings.PartyHeroDevelopmentMode.Value;

            // Convert setlist to YARG ShowSongs format
            var showSongs = SetlistManager.ConvertToShowSongs(setlistData);
            
            // Set up persistent state for the show
            GlobalVariables.State.PlayingAShow = true;
            GlobalVariables.State.ShowIndex = 0;
            GlobalVariables.State.ShowSongs = showSongs;
            GlobalVariables.State.PartyHero = partyHeroState;

            YargLogger.LogInfo($"[PartyHero] Show initialized with {showSongs.Count} songs");
            
            // Load first song
            GlobalVariables.Instance.LoadScene(SceneIndex.Gameplay);
        }

        private static void ShowError(string message)
        {
            // TODO: Show proper dialog box
            // For now, just log
            YargLogger.LogError($"[PartyHero] {message}");
            
            // In Unity, you'd show a dialog:
            // DialogManager.Instance.ShowDialog("PartyHero Error", message, () => { });
        }

        private static void ShowValidationErrors(SetlistValidationResult validation)
        {
            string errorMessage = "Cannot start show. The following songs are missing:\n\n";
            
            foreach (var error in validation.Errors)
            {
                errorMessage += $"• {error}\n";
            }

            errorMessage += "\nPlease add these songs to your library or update the setlist.";
            
            ShowError(errorMessage);
        }

        private static void ShowValidationWarnings(SetlistValidationResult validation, Action onContinue)
        {
            string warningMessage = "Warning: The following issues were found:\n\n";
            
            foreach (var warning in validation.Warnings)
            {
                warningMessage += $"• {warning}\n";
            }

            warningMessage += "\nDo you want to continue anyway?";
            
            // TODO: Show confirmation dialog with Yes/No
            // For now, just log and continue
            YargLogger.LogWarning($"[PartyHero] {warningMessage}");
            onContinue?.Invoke();
            
            // In Unity, you'd show a dialog:
            // DialogManager.Instance.ShowConfirmDialog(
            //     "PartyHero Warning", 
            //     warningMessage, 
            //     onContinue,
            //     () => YargLogger.LogInfo("[PartyHero] User cancelled show start")
            // );
        }

        /// <summary>
        /// Preview setlist without starting show
        /// </summary>
        public static void PreviewSetlist(string path)
        {
            try
            {
                var setlistData = SetlistManager.LoadSetlist(path);
                if (setlistData == null) return;

                YargLogger.LogInfo("============================");
                YargLogger.LogInfo("  SETLIST PREVIEW");
                YargLogger.LogInfo("============================");
                YargLogger.LogInfo($"Show: {setlistData.showName}");
                YargLogger.LogInfo($"Venue: {setlistData.venueName}");
                YargLogger.LogInfo($"Date: {setlistData.showDate}");
                YargLogger.LogInfo($"Sets: {setlistData.sets.Count}");
                
                int totalSongs = 0;
                foreach (var set in setlistData.sets)
                {
                    totalSongs += set.songs.Count;
                }
                YargLogger.LogInfo($"Total Songs: {totalSongs}");
                YargLogger.LogInfo("============================");
                
                foreach (var set in setlistData.sets)
                {
                    YargLogger.LogInfo($"\n{set.setName}:");
                    foreach (var song in set.songs)
                    {
                        YargLogger.LogInfo($"  • {song.songName}");
                    }
                }
                YargLogger.LogInfo("============================");
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to preview setlist");
            }
        }
    }
}
