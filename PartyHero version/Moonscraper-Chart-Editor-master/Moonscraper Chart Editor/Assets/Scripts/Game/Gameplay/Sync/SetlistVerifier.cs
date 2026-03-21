using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoonscraperChartEditor.Song
{
    /// <summary>
    /// Pre-show verification tool that validates song mapping JSON against actual DAW timeline.
    /// Cues each song in AbleSet, waits for timeline position response, and compares to expected values.
    /// 
    /// Usage:
    /// 1. Ensure AbleSet is running with your show setlist loaded
    /// 2. Configure OSC input in AbleSet (to receive cue commands from Clone Hero)
    /// 3. Click "Verify Setlist" button in DAW Sync Settings
    /// 4. Wait for each song to be cued and verified (~2-3 seconds per song)
    /// 5. Review results - any mismatches indicate mapping JSON needs correction
    /// </summary>
    public class SetlistVerifier : MonoBehaviour
    {
        public static SetlistVerifier Instance { get; private set; }

        [Header("Verification Settings")]
        [Tooltip("Tolerance for position matching (seconds) - accounts for DAW timing precision")]
        public float positionTolerance = 0.1f;

        [Tooltip("How long to wait for DAW response after cueing each song (seconds)")]
        public float cueResponseTimeout = 3.0f;

        [Tooltip("Delay between song cues to allow DAW to settle (seconds)")]
        public float delayBetweenCues = 0.5f;

        [Header("Verification State")]
        public bool isVerifying = false;
        public int totalSongs = 0;
        public int songsVerified = 0;
        public int songsPassed = 0;
        public int songsFailed = 0;

        // Results storage
        public class VerificationResult
        {
            public string trackName;
            public string chartPath;
            public float expectedPosition;
            public float actualPosition;
            public bool passed;
            public string errorMessage;

            public VerificationResult(string track, string chart, float expected, float actual, bool pass, string error = "")
            {
                trackName = track;
                chartPath = chart;
                expectedPosition = expected;
                actualPosition = actual;
                passed = pass;
                errorMessage = error;
            }
        }

        public List<VerificationResult> results = new List<VerificationResult>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        /// <summary>
        /// Start the verification process for all songs in the mapping file.
        /// </summary>
        public void VerifySetlist()
        {
            if (isVerifying)
            {
                Debug.LogWarning("[SetlistVerifier] Verification already in progress");
                return;
            }

            if (!ExternalSyncManager.Instance.IsSyncActive())
            {
                Debug.LogError("[SetlistVerifier] Cannot verify - External sync not active. Enable sync and ensure AbleSet is sending OSC messages.");
                return;
            }

            if (SongMappingManager.Instance == null)
            {
                Debug.LogError("[SetlistVerifier] SongMappingManager not found");
                return;
            }

            // Load mappings if not already loaded
            if (SongMappingManager.Instance.GetMappingCount() == 0)
            {
                SongMappingManager.Instance.LoadMappings();
            }

            if (SongMappingManager.Instance.GetMappingCount() == 0)
            {
                Debug.LogError("[SetlistVerifier] No song mappings found. Create a mapping file first.");
                return;
            }

            StartCoroutine(VerifyAllSongsCoroutine());
        }

        /// <summary>
        /// Coroutine that iterates through all songs and verifies their timeline positions.
        /// </summary>
        private IEnumerator VerifyAllSongsCoroutine()
        {
            isVerifying = true;
            results.Clear();

            // Get all enabled mappings
            List<SongMappingManager.SongMapping> mappings = SongMappingManager.Instance.GetAllEnabledMappings();
            totalSongs = mappings.Count;
            songsVerified = 0;
            songsPassed = 0;
            songsFailed = 0;

            Debug.Log($"[SetlistVerifier] Starting verification of {totalSongs} songs...");

            foreach (var mapping in mappings)
            {
                Debug.Log($"[SetlistVerifier] Verifying '{mapping.dawTrackName}' (expected position: {mapping.timelineStartTime}s)...");

                // Cue the song in AbleSet
                bool cueSent = ExternalSyncManager.Instance.CueSong(mapping.dawTrackName);
                if (!cueSent)
                {
                    string error = "Failed to send OSC cue command";
                    results.Add(new VerificationResult(mapping.dawTrackName, mapping.chartFilePath, mapping.timelineStartTime, 0f, false, error));
                    songsFailed++;
                    songsVerified++;
                    Debug.LogError($"[SetlistVerifier] {error} for '{mapping.dawTrackName}'");
                    continue;
                }

                // Wait for DAW to respond and settle
                yield return new WaitForSeconds(cueResponseTimeout);

                // Check if we received an updated position
                float receivedPosition = ExternalSyncManager.Instance.currentTime;
                string receivedTrackName = ExternalSyncManager.Instance.currentTrackName;

                // Verify track name matches (case-insensitive)
                bool trackNameMatches = string.Equals(receivedTrackName, mapping.dawTrackName, System.StringComparison.OrdinalIgnoreCase);
                if (!trackNameMatches)
                {
                    string error = $"Track name mismatch - DAW returned '{receivedTrackName}' but expected '{mapping.dawTrackName}'";
                    results.Add(new VerificationResult(mapping.dawTrackName, mapping.chartFilePath, mapping.timelineStartTime, receivedPosition, false, error));
                    songsFailed++;
                    songsVerified++;
                    Debug.LogWarning($"[SetlistVerifier] {error}");
                    yield return new WaitForSeconds(delayBetweenCues);
                    continue;
                }

                // Verify timeline position matches (within tolerance)
                float positionDifference = Mathf.Abs(receivedPosition - mapping.timelineStartTime);
                bool positionMatches = positionDifference <= positionTolerance;

                if (positionMatches)
                {
                    results.Add(new VerificationResult(mapping.dawTrackName, mapping.chartFilePath, mapping.timelineStartTime, receivedPosition, true));
                    songsPassed++;
                    Debug.Log($"[SetlistVerifier] ✓ PASS - '{mapping.dawTrackName}' at {receivedPosition}s (expected {mapping.timelineStartTime}s, diff: {positionDifference:F3}s)");
                }
                else
                {
                    string error = $"Position mismatch - Expected {mapping.timelineStartTime}s, got {receivedPosition}s (diff: {positionDifference:F3}s)";
                    results.Add(new VerificationResult(mapping.dawTrackName, mapping.chartFilePath, mapping.timelineStartTime, receivedPosition, false, error));
                    songsFailed++;
                    Debug.LogError($"[SetlistVerifier] ✗ FAIL - '{mapping.dawTrackName}' - {error}");
                }

                songsVerified++;

                // Delay before next song to avoid overwhelming the DAW
                yield return new WaitForSeconds(delayBetweenCues);
            }

            isVerifying = false;

            // Print summary
            Debug.Log($"[SetlistVerifier] ========== VERIFICATION COMPLETE ==========");
            Debug.Log($"[SetlistVerifier] Total: {totalSongs} | Passed: {songsPassed} | Failed: {songsFailed}");
            if (songsFailed > 0)
            {
                Debug.LogWarning($"[SetlistVerifier] {songsFailed} songs failed verification. Check mapping JSON and Ableton timeline positions.");
            }
            else
            {
                Debug.Log($"[SetlistVerifier] All songs verified successfully! Ready for the show.");
            }
        }

        /// <summary>
        /// Cancel ongoing verification.
        /// </summary>
        public void CancelVerification()
        {
            if (isVerifying)
            {
                StopAllCoroutines();
                isVerifying = false;
                Debug.Log("[SetlistVerifier] Verification cancelled");
            }
        }

        /// <summary>
        /// Get formatted results as a string for UI display.
        /// </summary>
        public string GetResultsText()
        {
            if (results.Count == 0)
                return "No verification results yet. Click 'Verify Setlist' to begin.";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Verification Results ({songsPassed} passed, {songsFailed} failed):");
            sb.AppendLine();

            foreach (var result in results)
            {
                string status = result.passed ? "✓ PASS" : "✗ FAIL";
                sb.AppendLine($"{status} - {result.trackName}");
                sb.AppendLine($"  Expected: {result.expectedPosition:F2}s | Actual: {result.actualPosition:F2}s");
                if (!result.passed && !string.IsNullOrEmpty(result.errorMessage))
                {
                    sb.AppendLine($"  Error: {result.errorMessage}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get current progress as percentage (0-100).
        /// </summary>
        public float GetProgress()
        {
            if (totalSongs == 0)
                return 0f;

            return (float)songsVerified / totalSongs * 100f;
        }
    }
}
