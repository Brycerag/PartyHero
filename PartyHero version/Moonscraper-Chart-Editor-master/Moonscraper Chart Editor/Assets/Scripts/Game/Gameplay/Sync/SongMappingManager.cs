using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MoonscraperChartEditor.Song
{
    /// <summary>
    /// Maps DAW/AbleSet track names to chart file paths for automatic song loading.
    /// Mappings are stored in a JSON file for easy editing.
    /// </summary>
    [Serializable]
    public class SongMapping
    {
        public string dawTrackName;      // Track name from AbleSet/DAW
        public string chartFilePath;     // Full path to .chart, .mid, or .msce file
        public bool enabled = true;      // Allow temporarily disabling mappings
        public float timelineStartTime = 0f;  // Where this song starts in continuous Ableton timeline (seconds)
        public float visualPreRoll = 3.0f;    // How many seconds before song start to show chart

        public SongMapping() { }

        public SongMapping(string trackName, string chartPath)
        {
            dawTrackName = trackName;
            chartFilePath = chartPath;
            enabled = true;
            timelineStartTime = 0f;
            visualPreRoll = 3.0f;
        }

        public SongMapping(string trackName, string chartPath, float timelineStart)
        {
            dawTrackName = trackName;
            chartFilePath = chartPath;
            enabled = true;
            timelineStartTime = timelineStart;
            visualPreRoll = 3.0f;
        }
    }

    [Serializable]
    public class SongMappingList
    {
        public List<SongMapping> mappings = new List<SongMapping>();
    }

    /// <summary>
    /// Manages automatic song loading based on external DAW track selection.
    /// Monitors ExternalSyncManager for track name changes and loads matching charts.
    /// </summary>
    public class SongMappingManager : MonoBehaviour
    {
        public static SongMappingManager Instance { get; private set; }

        [Header("Mapping Configuration")]
        [Tooltip("Path to song mapping JSON file (relative to Application.dataPath or absolute)")]
        public string mappingFilePath = "../songsync_mapping.json";

        [Tooltip("Enable automatic song loading when DAW track changes")]
        public bool autoLoadEnabled = false;

        [Tooltip("Delay before auto-loading song (seconds) to avoid rapid switches")]
        public float autoLoadDelay = 0.5f;

        [Header("State")]
        [Tooltip("Current DAW track name being monitored")]
        public string lastTrackedName = "";

        [Tooltip("Time when track name change was detected")]
        private float trackChangeTime = 0f;

        [Tooltip("Pending track to load")]
        private string pendingTrackName = "";

        // Mapping storage
        private Dictionary<string, SongMapping> mappingDict = new Dictionary<string, SongMapping>(StringComparer.OrdinalIgnoreCase);
        private SongMappingList mappingData;

        // Show flow integration
        [Header("Show Flow")]
        [Tooltip("Current song index in setlist (-1 if not in sequential mode)")]
        public int currentSongIndex = -1;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadMappings();
        }

        void Update()
        {
            if (!autoLoadEnabled || ExternalSyncManager.Instance == null)
                return;

            string currentTrackName = ExternalSyncManager.Instance.currentTrackName;

            // Track name changed
            if (!string.IsNullOrEmpty(currentTrackName) && currentTrackName != lastTrackedName)
            {
                lastTrackedName = currentTrackName;
                pendingTrackName = currentTrackName;
                trackChangeTime = Time.unscaledTime;

                Debug.Log($"[SongMappingManager] DAW track changed to: {currentTrackName}");
            }

            // Auto-load after delay
            if (!string.IsNullOrEmpty(pendingTrackName) && 
                Time.unscaledTime - trackChangeTime >= autoLoadDelay)
            {
                LoadSongForTrack(pendingTrackName);
                pendingTrackName = "";
            }
        }

        /// <summary>
        /// Load mappings from JSON file
        /// </summary>
        public void LoadMappings()
        {
            string fullPath = GetFullMappingPath();

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[SongMappingManager] Mapping file not found: {fullPath}");
                Debug.Log("[SongMappingManager] Creating default mapping file...");
                CreateDefaultMappings();
                SaveMappings();
                return;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                mappingData = JsonUtility.FromJson<SongMappingList>(json);

                if (mappingData == null || mappingData.mappings == null)
                {
                    Debug.LogError("[SongMappingManager] Failed to parse mapping JSON");
                    mappingData = new SongMappingList();
                    return;
                }

                // Build dictionary for fast lookups
                mappingDict.Clear();
                foreach (SongMapping mapping in mappingData.mappings)
                {
                    if (!string.IsNullOrEmpty(mapping.dawTrackName))
                    {
                        mappingDict[mapping.dawTrackName] = mapping;
                    }
                }

                Debug.Log($"[SongMappingManager] Loaded {mappingData.mappings.Count} song mappings from {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SongMappingManager] Error loading mappings: {e.Message}");
                mappingData = new SongMappingList();
            }
        }

        /// <summary>
        /// Save mappings to JSON file
        /// </summary>
        public void SaveMappings()
        {
            if (mappingData == null)
                mappingData = new SongMappingList();

            string fullPath = GetFullMappingPath();

            try
            {
                string json = JsonUtility.ToJson(mappingData, true);
                File.WriteAllText(fullPath, json);
                Debug.Log($"[SongMappingManager] Saved {mappingData.mappings.Count} mappings to {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SongMappingManager] Error saving mappings: {e.Message}");
            }
        }

        /// <summary>
        /// Create default example mappings
        /// </summary>
        private void CreateDefaultMappings()
        {
            mappingData = new SongMappingList();
            
            // Add example mappings (user must edit with real paths and timeline positions)
            mappingData.mappings.Add(new SongMapping(
                "Example Song 1",
                "C:/Charts/ExampleSong1/notes.chart",
                0f    // Starts at 0 seconds in timeline
            ));
            
            mappingData.mappings.Add(new SongMapping(
                "Example Song 2",
                "C:/Charts/ExampleSong2/notes.mid",
                180f  // Starts at 3:00 (180 seconds) in timeline
            ));

            mappingData.mappings.Add(new SongMapping(
                "Through the Fire and Flames",
                "C:/Charts/TTFAF/notes.chart",
                360f  // Starts at 6:00 (360 seconds) in timeline
            ));

            Debug.Log($"[SongMappingManager] Created {mappingData.mappings.Count} default mappings");
        }

        /// <summary>
        /// Get full path to mapping file
        /// </summary>
        private string GetFullMappingPath()
        {
            if (Path.IsPathRooted(mappingFilePath))
            {
                return mappingFilePath;
            }

            // Relative to project folder (one level up from Assets)
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectPath, mappingFilePath));
        }

        /// <summary>
        /// Load song chart for given DAW track name
        /// </summary>
        public void LoadSongForTrack(string trackName)
        {
            if (string.IsNullOrEmpty(trackName))
                return;

            if (!mappingDict.TryGetValue(trackName, out SongMapping mapping))
            {
                Debug.LogWarning($"[SongMappingManager] No mapping found for track: {trackName}");
                return;
            }

            if (!mapping.enabled)
            {
                Debug.Log($"[SongMappingManager] Mapping disabled for track: {trackName}");
                return;
            }

            if (!File.Exists(mapping.chartFilePath))
            {
                Debug.LogError($"[SongMappingManager] Chart file not found: {mapping.chartFilePath}");
                return;
            }

            Debug.Log($"[SongMappingManager] Loading chart: {mapping.chartFilePath}");

            // Set timeline offset in ExternalSyncManager for continuous timeline support
            if (ExternalSyncManager.Instance != null)
            {
                ExternalSyncManager.Instance.SetCurrentSongOffset(mapping.timelineStartTime, mapping.visualPreRoll);
            }

            // Use ChartEditor to load the song
            ChartEditor editor = ChartEditor.Instance;
            if (editor != null)
            {
                editor.StartCoroutine(editor._Load(mapping.chartFilePath));
            }
            else
            {
                Debug.LogError("[SongMappingManager] ChartEditor instance not found");
            }
        }

        /// <summary>
        /// Add or update a mapping
        /// </summary>
        public void AddOrUpdateMapping(string trackName, string chartPath, bool enabled = true)
        {
            if (mappingData == null)
                mappingData = new SongMappingList();

            if (mappingDict.TryGetValue(trackName, out SongMapping existing))
            {
                existing.chartFilePath = chartPath;
                existing.enabled = enabled;
            }
            else
            {
                SongMapping newMapping = new SongMapping(trackName, chartPath);
                newMapping.enabled = enabled;
                mappingData.mappings.Add(newMapping);
                mappingDict[trackName] = newMapping;
            }

            SaveMappings();
        }

        /// <summary>
        /// Remove a mapping
        /// </summary>
        public void RemoveMapping(string trackName)
        {
            if (mappingDict.TryGetValue(trackName, out SongMapping mapping))
            {
                mappingData.mappings.Remove(mapping);
                mappingDict.Remove(trackName);
                SaveMappings();
            }
        }

        /// <summary>
        /// Get chart path for track name (if mapped)
        /// </summary>
        public string GetChartPathForTrack(string trackName)
        {
            if (mappingDict.TryGetValue(trackName, out SongMapping mapping))
            {
                return mapping.enabled ? mapping.chartFilePath : null;
            }
            return null;
        }

        /// <summary>
        /// Get all mappings
        /// </summary>
        public List<SongMapping> GetAllMappings()
        {
            return mappingData?.mappings ?? new List<SongMapping>();
        }

        /// <summary>
        /// Get only enabled mappings (for verification/validation)
        /// </summary>
        public List<SongMapping> GetAllEnabledMappings()
        {
            List<SongMapping> enabled = new List<SongMapping>();
            if (mappingData?.mappings != null)
            {
                foreach (var mapping in mappingData.mappings)
                {
                    if (mapping.enabled)
                        enabled.Add(mapping);
                }
            }
            return enabled;
        }

        /// <summary>
        /// Get total count of mappings (including disabled ones)
        /// </summary>
        public int GetMappingCount()
        {
            return mappingData?.mappings?.Count ?? 0;
        }

        // ===== SHOW FLOW INTEGRATION METHODS =====

        /// <summary>
        /// Get the name of the next song in the setlist (sequential mode)
        /// Returns null if no next song or not in sequential mode
        /// </summary>
        public string GetNextSongName()
        {
            SongMapping nextMapping = GetNextSongMapping();
            return nextMapping?.dawTrackName;
        }

        /// <summary>
        /// Get the next song mapping in the setlist
        /// Returns null if no next song
        /// </summary>
        public SongMapping GetNextSongMapping()
        {
            if (mappingData?.mappings == null || mappingData.mappings.Count == 0)
                return null;

            // If not in sequential mode, start from beginning
            if (currentSongIndex < 0)
            {
                // Find first enabled song
                for (int i = 0; i < mappingData.mappings.Count; i++)
                {
                    if (mappingData.mappings[i].enabled)
                        return mappingData.mappings[i];
                }
                return null;
            }

            // Find next enabled song after current index
            for (int i = currentSongIndex + 1; i < mappingData.mappings.Count; i++)
            {
                if (mappingData.mappings[i].enabled)
                    return mappingData.mappings[i];
            }

            // No more songs
            return null;
        }

        /// <summary>
        /// Load the next song in the setlist (for show flow transitions)
        /// </summary>
        public bool LoadNextSong()
        {
            SongMapping nextMapping = GetNextSongMapping();
            if (nextMapping == null)
            {
                Debug.LogWarning("[SongMappingManager] No next song available in setlist");
                return false;
            }

            // Update current index
            currentSongIndex = mappingData.mappings.IndexOf(nextMapping);

            // Load the song
            LoadSongForTrack(nextMapping.dawTrackName);
            return true;
        }

        /// <summary>
        /// Set current song by track name (updates currentSongIndex for sequential flow)
        /// </summary>
        public void SetCurrentSongByTrackName(string trackName)
        {
            if (mappingDict.TryGetValue(trackName, out SongMapping mapping))
            {
                currentSongIndex = mappingData.mappings.IndexOf(mapping);
                Debug.Log($"[SongMappingManager] Current song index set to {currentSongIndex} ({trackName})");
            }
            else
            {
                currentSongIndex = -1;
                Debug.LogWarning($"[SongMappingManager] Track not found: {trackName}");
            }
        }

        /// <summary>
        /// Reset to beginning of setlist
        /// </summary>
        public void ResetSetlist()
        {
            currentSongIndex = -1;
            Debug.Log("[SongMappingManager] Setlist reset to beginning");
        }
    }
}
