using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using YARG.Core.Logging;

namespace YARG.PartyHero
{
    /// <summary>
    /// Configuration data for PartyHero
    /// Loaded from partyhero_config.json in StreamingAssets
    /// </summary>
    [Serializable]
    public class PartyHeroConfig
    {
        public MidiConfig midi = new();
        public OscConfig osc = new();
        public TcpConfig tcp = new();
        public DebugConfig debug = new();
    }

    [Serializable]
    public class MidiConfig
    {
        public int bandReadyNote = 60;
        public int forceNextStateNote = 61;
        public int playerReadyCC = 20;
        public int minimumVelocity = 64;
        public bool enabled = true;
    }

    [Serializable]
    public class OscConfig
    {
        public int receivePort = 8000;
        public string sendAddress = "127.0.0.1";
        public int sendPort = 9000;
        public bool enabled = true;
        public OscAddresses addresses = new();
    }

    [Serializable]
    public class OscAddresses
    {
        public string bandReady = "/partyhero/band_ready";
        public string playerReady = "/partyhero/player_ready";
        public string forceState = "/partyhero/force_state";
        public string syncTime = "/partyhero/sync_time";
        public string songStart = "/partyhero/song_start";
        public string songEnd = "/partyhero/song_end";
        public string stateChange = "/partyhero/state_change";
    }

    [Serializable]
    public class TcpConfig
    {
        public bool serverMode = true;
        public int listenPort = 9001;
        public string remoteAddress = "127.0.0.1";
        public int remotePort = 9002;
        public bool enabled = true;
    }

    [Serializable]
    public class DebugConfig
    {
        public bool developmentMode = true;
        public bool logAllMessages = true;
    }

    /// <summary>
    /// Manages loading and saving PartyHero configuration
    /// Singleton access via PartyHeroConfigManager.Instance
    /// </summary>
    public class PartyHeroConfigManager
    {
        private static PartyHeroConfigManager _instance;
        public static PartyHeroConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PartyHeroConfigManager();
                    _instance.LoadConfig();
                }
                return _instance;
            }
        }

        private const string CONFIG_FILENAME = "partyhero_config.json";
        private string ConfigPath => Path.Combine(Application.streamingAssetsPath, CONFIG_FILENAME);

        public PartyHeroConfig Config { get; private set; }

        private PartyHeroConfigManager()
        {
            Config = new PartyHeroConfig();
        }

        /// <summary>
        /// Load configuration from JSON file
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                string path = ConfigPath;

                if (!File.Exists(path))
                {
                    YargLogger.LogWarning($"[PartyHero] Config file not found at {path}, using defaults");
                    CreateDefaultConfig();
                    return;
                }

                string json = File.ReadAllText(path);
                Config = JsonConvert.DeserializeObject<PartyHeroConfig>(json);

                if (Config == null)
                {
                    YargLogger.LogError("[PartyHero] Failed to parse config file, using defaults");
                    Config = new PartyHeroConfig();
                    return;
                }

                YargLogger.LogInfo("[PartyHero] Configuration loaded successfully");
                LogConfigSummary();
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to load PartyHero config");
                Config = new PartyHeroConfig();
            }
        }

        /// <summary>
        /// Save current configuration to JSON file
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                string path = ConfigPath;
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
                YargLogger.LogInfo($"[PartyHero] Configuration saved to {path}");
            }
            catch (Exception ex)
            {
                YargLogger.LogException(ex, "Failed to save PartyHero config");
            }
        }

        /// <summary>
        /// Create default configuration file
        /// </summary>
        private void CreateDefaultConfig()
        {
            Config = new PartyHeroConfig();
            SaveConfig();
            YargLogger.LogInfo("[PartyHero] Created default configuration file");
        }

        /// <summary>
        /// Reload configuration from disk
        /// Useful for live-editing config file
        /// </summary>
        public void ReloadConfig()
        {
            YargLogger.LogInfo("[PartyHero] Reloading configuration...");
            LoadConfig();
            
            // Notify components that config changed
            OnConfigReloaded?.Invoke(Config);
        }

        /// <summary>
        /// Event fired when configuration is reloaded
        /// Components can subscribe to update their values
        /// </summary>
        public event Action<PartyHeroConfig> OnConfigReloaded;

        private void LogConfigSummary()
        {
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo("  PartyHero Configuration");
            YargLogger.LogInfo("============================");
            YargLogger.LogInfo($"MIDI Enabled: {Config.midi.enabled}");
            YargLogger.LogInfo($"  Band Ready Note: {Config.midi.bandReadyNote}");
            YargLogger.LogInfo($"  Player Ready CC: {Config.midi.playerReadyCC}");
            YargLogger.LogInfo($"OSC Enabled: {Config.osc.enabled}");
            YargLogger.LogInfo($"  Receive Port: {Config.osc.receivePort}");
            YargLogger.LogInfo($"  Send To: {Config.osc.sendAddress}:{Config.osc.sendPort}");
            YargLogger.LogInfo($"TCP Enabled: {Config.tcp.enabled}");
            YargLogger.LogInfo($"  Mode: {(Config.tcp.serverMode ? "Server" : "Client")}");
            YargLogger.LogInfo($"  Port: {(Config.tcp.serverMode ? Config.tcp.listenPort : Config.tcp.remotePort)}");
            YargLogger.LogInfo($"Development Mode: {Config.debug.developmentMode}");
            YargLogger.LogInfo("============================");
        }

        /// <summary>
        /// Get the full path to the config file for external editing
        /// </summary>
        public string GetConfigPath()
        {
            return ConfigPath;
        }
    }
}
