// MidiProtocols.cs
// MIDI protocol definitions for mixer control.
// Currently supports Mackie Control Universal (MCU) protocol.
// Designed to be extensible for future protocol additions.

using System;
using UnityEngine;

namespace MidiProtocols
{
    public enum ProtocolType
    {
        MackieControl = 0,
        // Future protocols can be added here:
        // BehringerX32 = 1,
        // YamahaQL = 2,
        // etc.
    }

    public interface IMidiProtocol
    {
        string Name { get; }
        void SendMute(int channel, bool muted, Action<byte[]> sendCallback);
        void SendUnmute(int channel, Action<byte[]> sendCallback);
    }

    // -------------------------------------------------------------------------
    // Mackie Control Universal Protocol
    // -------------------------------------------------------------------------
    public class MackieControlProtocol : IMidiProtocol
    {
        public string Name => "Mackie Control Universal";

        // Mackie Control uses Channel Strip commands on MIDI channel 1
        // Mute: F0 00 00 66 14 0C tt vv F7
        // where tt = track number (00-07 for channels 1-8, etc.)
        // vv = value (00 = unmute, 7F = mute)
        
        // However, for simplicity we're using CC-based mute which is more common:
        // CC 16-23 (0x10-0x17) = Channel 1-8 Mute (value 0 = off, 127 = on)
        
        const int MACKIE_CC_MUTE_BASE = 0x10; // CC 16-23 map to channels 1-8
        const int MACKIE_MIDI_CHANNEL = 0; // Mackie uses MIDI channel 1 (0-indexed)

        public void SendMute(int channel, bool muted, Action<byte[]> sendCallback)
        {
            if (channel < 1 || channel > 8)
            {
                Debug.LogWarningFormat("[MackieControl] Channel {0} out of range (1-8)", channel);
                return;
            }

            int ccNumber = MACKIE_CC_MUTE_BASE + (channel - 1);
            int value = muted ? 0x7F : 0x00;
            
            // MIDI CC message: [status, cc#, value]
            byte[] message = new byte[]
            {
                (byte)(0xB0 | MACKIE_MIDI_CHANNEL), // Control Change on channel 1
                (byte)ccNumber,
                (byte)value
            };

            sendCallback?.Invoke(message);
        }

        public void SendUnmute(int channel, Action<byte[]> sendCallback)
        {
            SendMute(channel, false, sendCallback);
        }
    }

    // -------------------------------------------------------------------------
    // Protocol Factory
    // -------------------------------------------------------------------------
    public static class MidiProtocolFactory
    {
        public static IMidiProtocol CreateProtocol(ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.MackieControl:
                    return new MackieControlProtocol();
                default:
                    Debug.LogWarningFormat("[MidiProtocolFactory] Unknown protocol type: {0}", type);
                    return new MackieControlProtocol();
            }
        }

        public static string[] GetProtocolNames()
        {
            return new string[]
            {
                "Mackie Control Universal",
                // Add future protocol names here
            };
        }
    }
}
