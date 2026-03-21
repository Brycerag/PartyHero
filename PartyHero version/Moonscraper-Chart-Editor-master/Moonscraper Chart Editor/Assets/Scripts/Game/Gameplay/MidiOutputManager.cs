// MidiOutputManager.cs
// Manages real-time MIDI output to external devices (e.g. a digital mixer).
// Uses NAudio.WinMM for device enumeration and message sending.
// Attach this MonoBehaviour to a persistent GameObject in the scene.

using System;
using System.Net.Sockets;
using UnityEngine;
using NAudio.Midi;

public class MidiOutputManager : MonoBehaviour
{
    public enum TransportMode
    {
        LocalMidiDevice = 0,
        NetworkTcp = 1,
    }

    public enum NetworkConnectionState
    {
        Stopped = 0,
        Idle = 1,
        Connecting = 2,
        RetryWait = 3,
        Connected = 4,
    }

    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------
    public static MidiOutputManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Configuration (editable in Inspector or via settings UI)
    // -------------------------------------------------------------------------
    [Tooltip("Index of the MIDI output device to use. Call GetDeviceNames() to find the correct index.")]
    public int deviceIndex = 0;

    [Tooltip("How MIDI messages should be sent.")]
    public TransportMode transportMode = TransportMode.LocalMidiDevice;

    [Tooltip("Network MIDI host/IP for TCP mode.")]
    public string tcpHost = "127.0.0.1";

    [Tooltip("Network MIDI TCP port.")]
    public int tcpPort = 5004;

    [Tooltip("Seconds to wait before another connection attempt after a failed one.")]
    public float reconnectRetrySeconds = 3f;

    [Tooltip("Seconds before a TCP connect attempt times out.")]
    public float connectTimeoutSeconds = 3f;

    [Tooltip("MIDI channel to send messages on (1-16).")]
    [Range(1, 16)]
    public int midiChannel = 1;

    [Tooltip("CC number used to mute/unmute a mixer channel on a note HIT.")]
    public int hitCCNumber = 20;

    [Tooltip("CC value sent on note HIT (e.g. 127 = unmute / full).")]
    [Range(0, 127)]
    public int hitCCValue = 127;

    [Tooltip("CC number used to mute/unmute a mixer channel on a note MISS.")]
    public int missCCNumber = 20;

    [Tooltip("CC value sent on note MISS (e.g. 0 = mute / off).")]
    [Range(0, 127)]
    public int missCCValue = 0;

    [Tooltip("When true, MIDI messages will not be sent (useful for testing without hardware).")]
    public bool muteOutput = false;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------
    MidiOut midiOut;
    TcpClient tcpClient;
    NetworkStream tcpStream;
    IAsyncResult tcpConnectResult;
    bool tcpConnectInProgress;
    bool networkConnectionRequested;
    float nextConnectAttemptTime;
    float connectAttemptDeadline;
    string networkStatus = "Network connection stopped.";
    NetworkConnectionState networkConnectionState = NetworkConnectionState.Stopped;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transportMode == TransportMode.LocalMidiDevice)
        {
            OpenDevice(deviceIndex);
            networkStatus = "Local MIDI device mode.";
            networkConnectionState = NetworkConnectionState.Idle;
        }
        else
        {
            networkStatus = "Network mode selected. Press Start to connect.";
            networkConnectionState = NetworkConnectionState.Stopped;
        }
    }

    void Update()
    {
        TickNetworkConnection();
    }

    void OnDestroy()
    {
        CloseDevice();
        DisconnectTcp();
        if (Instance == this)
            Instance = null;
    }

    // -------------------------------------------------------------------------
    // Device management
    // -------------------------------------------------------------------------

    /// <summary>Returns names of all available MIDI output devices.</summary>
    public static string[] GetDeviceNames()
    {
        int count = MidiOut.NumberOfDevices;
        var names = new string[count];
        for (int i = 0; i < count; i++)
            names[i] = MidiOut.DeviceInfo(i).ProductName;
        return names;
    }

    /// <summary>Opens the MIDI output device at the given index. Closes any previously open device.</summary>
    public void OpenDevice(int index)
    {
        CloseDevice();

        int count = MidiOut.NumberOfDevices;
        if (count == 0)
        {
            Debug.LogWarning("[MidiOutputManager] No MIDI output devices found.");
            return;
        }

        if (index < 0 || index >= count)
        {
            Debug.LogWarningFormat("[MidiOutputManager] Device index {0} is out of range (0-{1}). Defaulting to 0.", index, count - 1);
            index = 0;
        }

        try
        {
            midiOut = new MidiOut(index);
            deviceIndex = index;
            Debug.LogFormat("[MidiOutputManager] Opened MIDI device [{0}]: {1}", index, MidiOut.DeviceInfo(index).ProductName);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("[MidiOutputManager] Failed to open MIDI device {0}: {1}", index, ex.Message);
            midiOut = null;
        }
    }

    /// <summary>Closes the currently open MIDI output device.</summary>
    public void CloseDevice()
    {
        if (midiOut != null)
        {
            midiOut.Close();
            midiOut.Dispose();
            midiOut = null;
        }
    }

    /// <summary>Connects to the currently configured TCP endpoint for network MIDI mode.</summary>
    public bool ConnectTcp()
    {
        StartNetworkConnection();
        return IsTcpConnected();
    }

    /// <summary>Disconnects from TCP endpoint if connected.</summary>
    public void DisconnectTcp()
    {
        tcpConnectResult = null;
        tcpConnectInProgress = false;

        if (tcpStream != null)
        {
            tcpStream.Close();
            tcpStream.Dispose();
            tcpStream = null;
        }

        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
    }

    public bool IsTcpConnected()
    {
        return tcpClient != null && tcpClient.Connected && tcpStream != null;
    }

    public bool IsNetworkConnectionRequested()
    {
        return networkConnectionRequested;
    }

    public bool IsTcpConnecting()
    {
        return tcpConnectInProgress;
    }

    public NetworkConnectionState GetNetworkConnectionState()
    {
        if (transportMode != TransportMode.NetworkTcp)
            return NetworkConnectionState.Idle;

        return networkConnectionState;
    }

    public string GetNetworkStatus()
    {
        if (transportMode != TransportMode.NetworkTcp)
            return "Local MIDI device mode.";

        if (networkConnectionRequested && !IsTcpConnected() && !tcpConnectInProgress)
        {
            float wait = Mathf.Max(0f, nextConnectAttemptTime - Time.unscaledTime);
            if (wait > 0.05f)
                return string.Format("Retrying in {0:0.0}s...", wait);
        }

        return networkStatus;
    }

    public void StartNetworkConnection()
    {
        transportMode = TransportMode.NetworkTcp;
        networkConnectionRequested = true;
        nextConnectAttemptTime = 0f;
        networkStatus = "Starting network connection...";
        networkConnectionState = NetworkConnectionState.Connecting;
    }

    public void StopNetworkConnection()
    {
        networkConnectionRequested = false;
        DisconnectTcp();
        networkStatus = "Network connection stopped.";
        networkConnectionState = NetworkConnectionState.Stopped;
    }

    public void ForceReconnectNetwork()
    {
        if (transportMode != TransportMode.NetworkTcp)
            return;

        if (!networkConnectionRequested)
            return;

        DisconnectTcp();
        nextConnectAttemptTime = 0f;
        networkStatus = "Forcing reconnect...";
        networkConnectionState = NetworkConnectionState.Connecting;
    }

    public void SetTransportMode(TransportMode mode)
    {
        if (transportMode == mode)
            return;

        transportMode = mode;

        if (transportMode == TransportMode.LocalMidiDevice)
        {
            StopNetworkConnection();
            networkStatus = "Local MIDI device mode.";
            networkConnectionState = NetworkConnectionState.Idle;
            DisconnectTcp();
            OpenDevice(deviceIndex);
        }
        else
        {
            CloseDevice();
            networkStatus = "Network mode selected. Press Start to connect.";
            networkConnectionState = NetworkConnectionState.Stopped;
        }
    }

    void TickNetworkConnection()
    {
        if (transportMode != TransportMode.NetworkTcp)
            return;

        if (!networkConnectionRequested)
            return;

        if (IsTcpConnected())
        {
            networkStatus = "Connected.";
            networkConnectionState = NetworkConnectionState.Connected;
            return;
        }

        if (tcpConnectInProgress)
        {
            bool timedOut = Time.unscaledTime >= connectAttemptDeadline;
            bool completed = tcpConnectResult != null && tcpConnectResult.IsCompleted;

            if (!timedOut && !completed)
                return;

            if (timedOut)
            {
                networkStatus = "Connection timed out. Will retry.";
                networkConnectionState = NetworkConnectionState.RetryWait;
                DisconnectTcp();
                ScheduleRetry();
                return;
            }

            try
            {
                tcpClient.EndConnect(tcpConnectResult);
                tcpStream = tcpClient.GetStream();
                tcpConnectResult = null;
                tcpConnectInProgress = false;
                networkStatus = string.Format("Connected to {0}:{1}.", tcpHost, tcpPort);
                networkConnectionState = NetworkConnectionState.Connected;
                Debug.LogFormat("[MidiOutputManager] Connected TCP MIDI endpoint: {0}:{1}", tcpHost, tcpPort);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat("[MidiOutputManager] TCP connect failed: {0}", ex.Message);
                networkStatus = string.Format("Connection failed: {0}. Will retry.", ex.Message);
                networkConnectionState = NetworkConnectionState.RetryWait;
                DisconnectTcp();
                ScheduleRetry();
                return;
            }
        }

        if (Time.unscaledTime < nextConnectAttemptTime)
            return;

        BeginTcpConnectAttempt();
    }

    void BeginTcpConnectAttempt()
    {
        if (string.IsNullOrWhiteSpace(tcpHost))
        {
            networkStatus = "TCP host is empty. Will retry after settings are fixed.";
            networkConnectionState = NetworkConnectionState.RetryWait;
            ScheduleRetry();
            return;
        }

        if (tcpPort <= 0 || tcpPort > 65535)
        {
            networkStatus = string.Format("TCP port {0} is invalid. Will retry after settings are fixed.", tcpPort);
            networkConnectionState = NetworkConnectionState.RetryWait;
            ScheduleRetry();
            return;
        }

        try
        {
            DisconnectTcp();

            tcpClient = new TcpClient();
            tcpConnectResult = tcpClient.BeginConnect(tcpHost, tcpPort, null, null);
            tcpConnectInProgress = true;
            connectAttemptDeadline = Time.unscaledTime + Mathf.Max(0.5f, connectTimeoutSeconds);
            networkStatus = string.Format("Attempting connection to {0}:{1}...", tcpHost, tcpPort);
            networkConnectionState = NetworkConnectionState.Connecting;
        }
        catch (Exception ex)
        {
            networkStatus = string.Format("Connection error: {0}. Will retry.", ex.Message);
            networkConnectionState = NetworkConnectionState.RetryWait;
            ScheduleRetry();
        }
    }

    void ScheduleRetry()
    {
        tcpConnectResult = null;
        tcpConnectInProgress = false;
        nextConnectAttemptTime = Time.unscaledTime + Mathf.Max(0.5f, reconnectRetrySeconds);
        networkConnectionState = NetworkConnectionState.RetryWait;
    }

    // -------------------------------------------------------------------------
    // Gameplay event hooks — called by BaseGameplayRulestate
    // -------------------------------------------------------------------------

    /// <summary>Send the configured CC message for a note hit.</summary>
    public void OnNoteHit()
    {
        SendControlChange(hitCCNumber, hitCCValue);
    }

    /// <summary>Send the configured CC message for a note miss.</summary>
    public void OnNoteMiss()
    {
        SendControlChange(missCCNumber, missCCValue);
    }

    // -------------------------------------------------------------------------
    // Low-level send
    // -------------------------------------------------------------------------

    /// <summary>Send a MIDI Control Change message.</summary>
    public void SendControlChange(int ccNumber, int value)
    {
        if (muteOutput)
            return;

        // Clamp to valid MIDI ranges
        ccNumber = Mathf.Clamp(ccNumber, 0, 127);
        value    = Mathf.Clamp(value,    0, 127);
        int channel = Mathf.Clamp(midiChannel, 1, 16);

        SendThreeByteMessage(0xB0 | (channel - 1), ccNumber, value, "ControlChange");
    }

    /// <summary>Send a raw MIDI Note On message.</summary>
    public void SendNoteOn(int note, int velocity)
    {
        if (muteOutput)
            return;

        note     = Mathf.Clamp(note,     0, 127);
        velocity = Mathf.Clamp(velocity, 0, 127);
        int channel = Mathf.Clamp(midiChannel, 1, 16);

        SendThreeByteMessage(0x90 | (channel - 1), note, velocity, "NoteOn");
    }

    /// <summary>Send a raw MIDI Note Off message.</summary>
    public void SendNoteOff(int note)
    {
        if (muteOutput)
            return;

        note = Mathf.Clamp(note, 0, 127);
        int channel = Mathf.Clamp(midiChannel, 1, 16);

        SendThreeByteMessage(0x80 | (channel - 1), note, 0, "NoteOff");
    }

    void SendThreeByteMessage(int status, int data1, int data2, string logName)
    {
        if (transportMode == TransportMode.LocalMidiDevice)
        {
            if (midiOut == null)
                return;

            int message = (data2 << 16) | (data1 << 8) | status;

            try
            {
                midiOut.Send(message);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[MidiOutputManager] Failed to send {0} via local MIDI: {1}", logName, ex.Message);
            }
            return;
        }

        // TCP mode sends raw 3-byte MIDI packets. Receiver must support this framing.
        if (!IsTcpConnected())
            return;

        try
        {
            byte[] packet = new byte[] { (byte)status, (byte)data1, (byte)data2 };
            tcpStream.Write(packet, 0, packet.Length);
            tcpStream.Flush();
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("[MidiOutputManager] Failed to send {0} via TCP: {1}", logName, ex.Message);
            DisconnectTcp();
            networkStatus = string.Format("Connection lost: {0}. Will retry.", ex.Message);
            networkConnectionState = NetworkConnectionState.RetryWait;

            if (networkConnectionRequested)
                ScheduleRetry();
        }
    }
}
