// MidiSettingsMenu.cs
// Settings UI panel for configuring MIDI output.
// Attach to a UI panel GameObject that contains:
//   - a Dropdown for device selection ("deviceDropdown")
//   - an InputField for MIDI channel ("channelInput")
//   - an InputField for hit CC number ("hitCCNumberInput")
//   - an InputField for hit CC value ("hitCCValueInput")
//   - an InputField for miss CC number ("missCCNumberInput")
//   - an InputField for miss CC value ("missCCValueInput")
//   - a Toggle for mute output ("muteToggle")
//   - a Button for refresh/apply ("refreshButton")

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MidiSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Dropdown transportModeDropdown;
    public Dropdown deviceDropdown;
    public GameObject localDevicePanel;
    public GameObject networkPanel;

    public InputField tcpHostInput;
    public InputField tcpPortInput;
    public InputField retrySecondsInput;
    public InputField connectTimeoutSecondsInput;
    public Text connectionStatusText;
    public Text connectionToggleButtonText;
    public Text reconnectButtonText;

    [Header("Connection Status Colors")]
    public Color statusIdleColor = Color.white;
    public Color statusConnectingColor = new Color(1f, 0.85f, 0.2f);
    public Color statusRetryColor = new Color(1f, 0.6f, 0.2f);
    public Color statusConnectedColor = new Color(0.2f, 1f, 0.3f);
    public Color statusStoppedColor = new Color(1f, 0.45f, 0.45f);

    public InputField channelInput;
    public InputField hitCCNumberInput;
    public InputField hitCCValueInput;
    public InputField missCCNumberInput;
    public InputField missCCValueInput;
    public Toggle muteToggle;

    void OnEnable()
    {
        PopulateTransportDropdown();
        PopulateDeviceDropdown();
        LoadCurrentSettings();
        UpdateTransportUi();
        CancelInvoke("RefreshConnectionUi");
        InvokeRepeating("RefreshConnectionUi", 0f, 0.2f);
    }

    void OnDisable()
    {
        CancelInvoke("RefreshConnectionUi");
    }

    // -------------------------------------------------------------------------
    // Populate
    // -------------------------------------------------------------------------

    void PopulateTransportDropdown()
    {
        if (transportModeDropdown == null) return;

        transportModeDropdown.ClearOptions();
        transportModeDropdown.AddOptions(new List<string>
        {
            "Local MIDI Device",
            "Network MIDI (TCP)",
        });
    }

    void PopulateDeviceDropdown()
    {
        if (deviceDropdown == null) return;

        deviceDropdown.ClearOptions();

        string[] devices = MidiOutputManager.GetDeviceNames();

        if (devices.Length == 0)
        {
            deviceDropdown.AddOptions(new System.Collections.Generic.List<string> { "No MIDI devices found" });
            deviceDropdown.interactable = false;
            return;
        }

        deviceDropdown.interactable = true;
        deviceDropdown.AddOptions(new System.Collections.Generic.List<string>(devices));

        if (MidiOutputManager.Instance != null)
            deviceDropdown.value = Mathf.Clamp(MidiOutputManager.Instance.deviceIndex, 0, devices.Length - 1);
    }

    void LoadCurrentSettings()
    {
        var mgr = MidiOutputManager.Instance;
        if (mgr == null) return;

        if (transportModeDropdown) transportModeDropdown.value = (int)mgr.transportMode;

        if (tcpHostInput) tcpHostInput.text = mgr.tcpHost;
        if (tcpPortInput) tcpPortInput.text = mgr.tcpPort.ToString();
        if (retrySecondsInput) retrySecondsInput.text = mgr.reconnectRetrySeconds.ToString("0.0");
        if (connectTimeoutSecondsInput) connectTimeoutSecondsInput.text = mgr.connectTimeoutSeconds.ToString("0.0");

        if (channelInput)      channelInput.text      = mgr.midiChannel.ToString();
        if (hitCCNumberInput)  hitCCNumberInput.text  = mgr.hitCCNumber.ToString();
        if (hitCCValueInput)   hitCCValueInput.text   = mgr.hitCCValue.ToString();
        if (missCCNumberInput) missCCNumberInput.text = mgr.missCCNumber.ToString();
        if (missCCValueInput)  missCCValueInput.text  = mgr.missCCValue.ToString();
        if (muteToggle)        muteToggle.isOn        = mgr.muteOutput;
    }

    // -------------------------------------------------------------------------
    // Apply — called by the Apply/Refresh button
    // -------------------------------------------------------------------------

    public void ApplySettings()
    {
        var mgr = MidiOutputManager.Instance;
        if (mgr == null)
        {
            Debug.LogWarning("[MidiSettingsMenu] No MidiOutputManager instance found in scene.");
            return;
        }

        MidiOutputManager.TransportMode selectedMode = mgr.transportMode;
        if (transportModeDropdown)
            selectedMode = (MidiOutputManager.TransportMode)Mathf.Clamp(transportModeDropdown.value, 0, 1);

        mgr.SetTransportMode(selectedMode);

        mgr.tcpHost = ParseString(tcpHostInput, mgr.tcpHost);
        mgr.tcpPort = ParseInt(tcpPortInput, mgr.tcpPort, 1, 65535);
        mgr.reconnectRetrySeconds = ParseFloat(retrySecondsInput, mgr.reconnectRetrySeconds, 0.5f, 30f);
        mgr.connectTimeoutSeconds = ParseFloat(connectTimeoutSecondsInput, mgr.connectTimeoutSeconds, 0.5f, 30f);

        mgr.midiChannel    = ParseInt(channelInput,      mgr.midiChannel,    1, 16);
        mgr.hitCCNumber    = ParseInt(hitCCNumberInput,  mgr.hitCCNumber,    0, 127);
        mgr.hitCCValue     = ParseInt(hitCCValueInput,   mgr.hitCCValue,     0, 127);
        mgr.missCCNumber   = ParseInt(missCCNumberInput, mgr.missCCNumber,   0, 127);
        mgr.missCCValue    = ParseInt(missCCValueInput,  mgr.missCCValue,    0, 127);

        if (muteToggle)
            mgr.muteOutput = muteToggle.isOn;

        if (mgr.transportMode == MidiOutputManager.TransportMode.LocalMidiDevice)
        {
            if (deviceDropdown != null && deviceDropdown.interactable)
                mgr.OpenDevice(deviceDropdown.value);
        }
        else
        {
            if (mgr.IsNetworkConnectionRequested())
                mgr.StartNetworkConnection();
        }

        UpdateTransportUi();
        RefreshConnectionUi();
    }

    public void OnTransportModeChanged()
    {
        UpdateTransportUi();
    }

    void UpdateTransportUi()
    {
        bool useNetwork = transportModeDropdown != null && transportModeDropdown.value == (int)MidiOutputManager.TransportMode.NetworkTcp;

        if (localDevicePanel != null)
            localDevicePanel.SetActive(!useNetwork);

        if (networkPanel != null)
            networkPanel.SetActive(useNetwork);

        RefreshConnectionUi();
    }

    public void OnConnectionTogglePressed()
    {
        var mgr = MidiOutputManager.Instance;
        if (mgr == null)
            return;

        if (mgr.transportMode != MidiOutputManager.TransportMode.NetworkTcp)
            return;

        if (mgr.IsNetworkConnectionRequested())
            mgr.StopNetworkConnection();
        else
            mgr.StartNetworkConnection();

        RefreshConnectionUi();
    }

    public void OnReconnectPressed()
    {
        var mgr = MidiOutputManager.Instance;
        if (mgr == null)
            return;

        if (mgr.transportMode != MidiOutputManager.TransportMode.NetworkTcp)
            return;

        if (!mgr.IsNetworkConnectionRequested())
            mgr.StartNetworkConnection();
        else
            mgr.ForceReconnectNetwork();

        RefreshConnectionUi();
    }

    void RefreshConnectionUi()
    {
        var mgr = MidiOutputManager.Instance;
        if (mgr == null)
            return;

        bool inNetworkMode = transportModeDropdown != null && transportModeDropdown.value == (int)MidiOutputManager.TransportMode.NetworkTcp;

        if (connectionStatusText != null)
        {
            if (inNetworkMode)
                connectionStatusText.text = mgr.GetNetworkStatus();
            else
                connectionStatusText.text = "Local MIDI device mode.";

            connectionStatusText.color = GetStatusColor(mgr, inNetworkMode);
        }

        if (connectionToggleButtonText != null)
        {
            if (inNetworkMode && mgr.IsNetworkConnectionRequested())
                connectionToggleButtonText.text = "Stop Connection";
            else
                connectionToggleButtonText.text = "Start Connection";
        }

        if (reconnectButtonText != null)
            reconnectButtonText.text = "Reconnect Now";
    }

    Color GetStatusColor(MidiOutputManager mgr, bool inNetworkMode)
    {
        if (!inNetworkMode)
            return statusIdleColor;

        switch (mgr.GetNetworkConnectionState())
        {
            case MidiOutputManager.NetworkConnectionState.Connected:
                return statusConnectedColor;
            case MidiOutputManager.NetworkConnectionState.Connecting:
                return statusConnectingColor;
            case MidiOutputManager.NetworkConnectionState.RetryWait:
                return statusRetryColor;
            case MidiOutputManager.NetworkConnectionState.Stopped:
                return statusStoppedColor;
            default:
                return statusIdleColor;
        }
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    int ParseInt(InputField field, int fallback, int min, int max)
    {
        if (field == null) return fallback;
        if (int.TryParse(field.text, out int val))
            return Mathf.Clamp(val, min, max);
        return fallback;
    }

    float ParseFloat(InputField field, float fallback, float min, float max)
    {
        if (field == null) return fallback;
        if (float.TryParse(field.text, out float val))
            return Mathf.Clamp(val, min, max);
        return fallback;
    }

    string ParseString(InputField field, string fallback)
    {
        if (field == null) return fallback;
        if (!string.IsNullOrWhiteSpace(field.text))
            return field.text.Trim();
        return fallback;
    }
}
