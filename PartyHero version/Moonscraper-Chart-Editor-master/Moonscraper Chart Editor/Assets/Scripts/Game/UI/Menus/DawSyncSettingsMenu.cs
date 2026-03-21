using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MoonscraperChartEditor.Song
{
    /// <summary>
    /// UI controller for External DAW Synchronization settings.
    /// Manages OSC connection, song mapping, and sync status display.
    /// </summary>
    public class DawSyncSettingsMenu : MonoBehaviour
    {
        [Header("Sync Enable/Disable")]
        public Toggle syncEnabledToggle;
        public Toggle autoLoadSongsToggle;

        [Header("OSC Connection")]
        public InputField oscPortInput;
        public Text connectionStatusText;
        public Button refreshConnectionButton;

        [Header("Sync Status Display")]
        public Text dawPlayingStatusText;
        public Text dawTimeText;
        public Text dawTempoText;
        public Text dawTrackNameText;

        [Header("Song Mapping")]
        public Text mappingFilePathText;
        public Button reloadMappingsButton;
        public Button openMappingFileButton;
        public Text mappingCountText;

        [Header("Debug")]
        public Toggle debugOscToggle;

        [Header("Setlist Verification")]
        public Button verifySetlistButton;
        public Button cancelVerificationButton;
        public Text verificationStatusText;
        public Text verificationProgressText;
        public Scrollbar verificationProgressBar;
        public Text verificationResultsText;

        void OnEnable()
        {
            LoadCurrentSettings();
            StartCoroutine(RefreshStatusRoutine());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        void Start()
        {
            // Wire up button callbacks
            if (refreshConnectionButton != null)
                refreshConnectionButton.onClick.AddListener(OnRefreshConnection);

            if (reloadMappingsButton != null)
                reloadMappingsButton.onClick.AddListener(OnReloadMappings);

            if (openMappingFileButton != null)
                openMappingFileButton.onClick.AddListener(OnOpenMappingFile);

            // Wire up toggle callbacks
            if (syncEnabledToggle != null)
                syncEnabledToggle.onValueChanged.AddListener(OnSyncEnabledChanged);

            if (autoLoadSongsToggle != null)
                autoLoadSongsToggle.onValueChanged.AddListener(OnAutoLoadChanged);

            if (debugOscToggle != null)
                debugOscToggle.onValueChanged.AddListener(OnDebugOscChanged);

            // Wire up input field callbacks
            if (oscPortInput != null)
                oscPortInput.onEndEdit.AddListener(OnOscPortChanged);

            // Wire up verification callbacks
            if (verifySetlistButton != null)
                verifySetlistButton.onClick.AddListener(OnVerifySetlist);

            if (cancelVerificationButton != null)
                cancelVerificationButton.onClick.AddListener(OnCancelVerification);
        }

        /// <summary>
        /// Load current settings from managers into UI
        /// </summary>
        void LoadCurrentSettings()
        {
            ExternalSyncManager syncManager = ExternalSyncManager.Instance;
            SongMappingManager mappingManager = SongMappingManager.Instance;

            if (syncManager != null)
            {
                if (syncEnabledToggle != null)
                    syncEnabledToggle.isOn = syncManager.syncEnabled;

                if (oscPortInput != null)
                    oscPortInput.text = syncManager.oscPort.ToString();

                if (debugOscToggle != null)
                    debugOscToggle.isOn = syncManager.debugOscMessages;
            }

            if (mappingManager != null)
            {
                if (autoLoadSongsToggle != null)
                    autoLoadSongsToggle.isOn = mappingManager.autoLoadEnabled;

                if (mappingFilePathText != null)
                    mappingFilePathText.text = mappingManager.mappingFilePath;

                UpdateMappingCount();
            }

            RefreshConnectionStatus();
        }

        /// <summary>
        /// Update connection and sync status display
        /// </summary>
        void RefreshConnectionStatus()
        {
            ExternalSyncManager syncManager = ExternalSyncManager.Instance;
            if (syncManager == null)
            {
                if (connectionStatusText != null)
                    connectionStatusText.text = "ExternalSyncManager not found";
                return;
            }

            // Connection status
            if (connectionStatusText != null)
            {
                connectionStatusText.text = syncManager.GetConnectionStatus();

                // Color-code status
                if (syncManager.IsSyncActive())
                    connectionStatusText.color = Color.green;
                else if (syncManager.syncEnabled)
                    connectionStatusText.color = Color.yellow;
                else
                    connectionStatusText.color = Color.gray;
            }

            // Playback status
            if (dawPlayingStatusText != null)
            {
                dawPlayingStatusText.text = syncManager.isPlaying ? "Playing ▶" : "Stopped ■";
                dawPlayingStatusText.color = syncManager.isPlaying ? Color.green : Color.red;
            }

            // Current time
            if (dawTimeText != null)
            {
                int minutes = Mathf.FloorToInt(syncManager.currentTime / 60f);
                int seconds = Mathf.FloorToInt(syncManager.currentTime % 60f);
                int ms = Mathf.FloorToInt((syncManager.currentTime % 1f) * 1000f);
                dawTimeText.text = $"Time: {minutes:D2}:{seconds:D2}.{ms:D3}";
            }

            // Current tempo
            if (dawTempoText != null)
            {
                dawTempoText.text = $"Tempo: {syncManager.currentTempo:F1} BPM";
            }

            // Current track
            if (dawTrackNameText != null)
            {
                if (string.IsNullOrEmpty(syncManager.currentTrackName))
                    dawTrackNameText.text = "Track: (none)";
                else
                    dawTrackNameText.text = $"Track: {syncManager.currentTrackName}";
            }
        }

        /// <summary>
        /// Coroutine to refresh status display periodically
        /// </summary>
        IEnumerator RefreshStatusRoutine()
        {
            while (true)
            {
                RefreshConnectionStatus();
                RefreshVerificationStatus();
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        /// <summary>
        /// Update song mapping count display
        /// </summary>
        void UpdateMappingCount()
        {
            SongMappingManager mappingManager = SongMappingManager.Instance;
            if (mappingManager == null || mappingCountText == null)
                return;

            int totalMappings = mappingManager.GetAllMappings().Count;
            int enabledMappings = 0;

            foreach (var mapping in mappingManager.GetAllMappings())
            {
                if (mapping.enabled)
                    enabledMappings++;
            }

            mappingCountText.text = $"{enabledMappings}/{totalMappings} mappings active";
        }

        // ----- UI Callbacks -----

        void OnSyncEnabledChanged(bool enabled)
        {
            ExternalSyncManager syncManager = ExternalSyncManager.Instance;
            if (syncManager != null)
            {
                syncManager.SetSyncEnabled(enabled);
                Debug.Log($"[DawSyncSettings] External sync {(enabled ? "enabled" : "disabled")}");
            }
        }

        void OnAutoLoadChanged(bool enabled)
        {
            SongMappingManager mappingManager = SongMappingManager.Instance;
            if (mappingManager != null)
            {
                mappingManager.autoLoadEnabled = enabled;
                Debug.Log($"[DawSyncSettings] Auto-load songs {(enabled ? "enabled" : "disabled")}");
            }
        }

        void OnDebugOscChanged(bool enabled)
        {
            ExternalSyncManager syncManager = ExternalSyncManager.Instance;
            if (syncManager != null)
            {
                syncManager.debugOscMessages = enabled;
                Debug.Log($"[DawSyncSettings] OSC debug logging {(enabled ? "enabled" : "disabled")}");
            }
        }

        void OnOscPortChanged(string portText)
        {
            if (int.TryParse(portText, out int port))
            {
                if (port < 1 || port > 65535)
                {
                    Debug.LogWarning($"[DawSyncSettings] Invalid port number: {port}");
                    port = Mathf.Clamp(port, 1024, 65535);
                    oscPortInput.text = port.ToString();
                }

                ExternalSyncManager syncManager = ExternalSyncManager.Instance;
                if (syncManager != null)
                {
                    syncManager.SetOscPort(port);
                    Debug.Log($"[DawSyncSettings] OSC port changed to {port}");
                }
            }
        }

        void OnRefreshConnection()
        {
            RefreshConnectionStatus();
            Debug.Log("[DawSyncSettings] Refreshed connection status");
        }

        void OnReloadMappings()
        {
            SongMappingManager mappingManager = SongMappingManager.Instance;
            if (mappingManager != null)
            {
                mappingManager.LoadMappings();
                UpdateMappingCount();
                Debug.Log("[DawSyncSettings] Reloaded song mappings");
            }
        }

        void OnOpenMappingFile()
        {
            SongMappingManager mappingManager = SongMappingManager.Instance;
            if (mappingManager != null)
            {
                // Get the full path and open in default editor
                string fullPath = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Application.dataPath),
                        mappingManager.mappingFilePath
                    )
                );

                if (System.IO.File.Exists(fullPath))
                {
                    Application.OpenURL("file:///" + fullPath);
                    Debug.Log($"[DawSyncSettings] Opened mapping file: {fullPath}");
                }
                else
                {
                    Debug.LogWarning($"[DawSyncSettings] Mapping file not found: {fullPath}");
                    Debug.Log("[DawSyncSettings] Creating default mapping file...");
                    mappingManager.SaveMappings();
                    Application.OpenURL("file:///" + fullPath);
                }
            }
        }

        // ----- Verification Methods -----

        /// <summary>
        /// Update verification status display
        /// </summary>
        void RefreshVerificationStatus()
        {
            SetlistVerifier verifier = SetlistVerifier.Instance;
            if (verifier == null)
                return;

            // Update verification button state
            if (verifySetlistButton != null)
            {
                verifySetlistButton.interactable = !verifier.isVerifying && ExternalSyncManager.Instance != null && ExternalSyncManager.Instance.IsSyncActive();
            }

            if (cancelVerificationButton != null)
            {
                cancelVerificationButton.interactable = verifier.isVerifying;
            }

            // Update status text
            if (verificationStatusText != null)
            {
                if (verifier.isVerifying)
                {
                    verificationStatusText.text = $"Verifying... {verifier.songsVerified}/{verifier.totalSongs}";
                    verificationStatusText.color = Color.yellow;
                }
                else if (verifier.results.Count > 0)
                {
                    if (verifier.songsFailed == 0)
                    {
                        verificationStatusText.text = "✓ All songs verified successfully!";
                        verificationStatusText.color = Color.green;
                    }
                    else
                    {
                        verificationStatusText.text = $"✗ {verifier.songsFailed} songs failed verification";
                        verificationStatusText.color = Color.red;
                    }
                }
                else
                {
                    verificationStatusText.text = "Ready to verify setlist";
                    verificationStatusText.color = Color.gray;
                }
            }

            // Update progress bar
            if (verificationProgressBar != null)
            {
                verificationProgressBar.size = verifier.GetProgress() / 100f;
            }

            // Update progress text
            if (verificationProgressText != null)
            {
                if (verifier.isVerifying)
                {
                    verificationProgressText.text = $"{verifier.GetProgress():F0}% ({verifier.songsVerified}/{verifier.totalSongs})";
                }
                else if (verifier.results.Count > 0)
                {
                    verificationProgressText.text = $"Passed: {verifier.songsPassed} | Failed: {verifier.songsFailed}";
                }
                else
                {
                    verificationProgressText.text = "No verification run yet";
                }
            }

            // Update results text
            if (verificationResultsText != null)
            {
                verificationResultsText.text = verifier.GetResultsText();
            }
        }

        void OnVerifySetlist()
        {
            SetlistVerifier verifier = SetlistVerifier.Instance;
            if (verifier != null)
            {
                Debug.Log("[DawSyncSettings] Starting setlist verification...");
                verifier.VerifySetlist();
            }
            else
            {
                Debug.LogError("[DawSyncSettings] SetlistVerifier not found");
            }
        }

        void OnCancelVerification()
        {
            SetlistVerifier verifier = SetlistVerifier.Instance;
            if (verifier != null)
            {
                Debug.Log("[DawSyncSettings] Cancelling verification...");
                verifier.CancelVerification();
            }
        }

        /// <summary>
        /// Manual test: Load a specific song by DAW track name
        /// </summary>
        public void TestLoadSong(string trackName)
        {
            SongMappingManager mappingManager = SongMappingManager.Instance;
            if (mappingManager != null)
            {
                mappingManager.LoadSongForTrack(trackName);
            }
        }
    }
}
