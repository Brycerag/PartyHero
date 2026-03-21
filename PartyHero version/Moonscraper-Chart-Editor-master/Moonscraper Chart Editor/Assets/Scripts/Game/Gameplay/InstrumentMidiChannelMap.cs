// InstrumentMidiChannelMap.cs
// Maps each Song.Instrument to a mixer channel number for protocol-based mute/unmute.

using System;
using System.Collections.Generic;
using UnityEngine;
using MoonscraperChartEditor.Song;

[Serializable]
public class InstrumentChannelMapping
{
    public Song.Instrument instrument;
    public int mixerChannel; // 1-8 (or higher depending on protocol)
    public bool enabled; // Whether to send mute/unmute for this instrument
}

[Serializable]
public class InstrumentMidiChannelMap
{
    public List<InstrumentChannelMapping> mappings = new List<InstrumentChannelMapping>();

    public InstrumentMidiChannelMap()
    {
        InitializeDefaults();
    }

    void InitializeDefaults()
    {
        mappings.Clear();

        // Default mapping: assign each main instrument to a mixer channel
        AddMapping(Song.Instrument.Guitar,       1, true);
        AddMapping(Song.Instrument.Bass,         2, true);
        AddMapping(Song.Instrument.Rhythm,       3, true);
        AddMapping(Song.Instrument.Drums,        4, true);
        AddMapping(Song.Instrument.Keys,         5, true);
        AddMapping(Song.Instrument.GuitarCoop,   6, false);
        AddMapping(Song.Instrument.GHLiveGuitar, 7, false);
        AddMapping(Song.Instrument.GHLiveBass,   8, false);
        AddMapping(Song.Instrument.GHLiveRhythm, 8, false);
        AddMapping(Song.Instrument.GHLiveCoop,   8, false);
    }

    void AddMapping(Song.Instrument instrument, int channel, bool enabled)
    {
        mappings.Add(new InstrumentChannelMapping
        {
            instrument = instrument,
            mixerChannel = channel,
            enabled = enabled
        });
    }

    public int GetMixerChannel(Song.Instrument instrument)
    {
        foreach (var mapping in mappings)
        {
            if (mapping.instrument == instrument)
                return mapping.mixerChannel;
        }
        return 1; // Default to channel 1 if not found
    }

    public bool IsEnabled(Song.Instrument instrument)
    {
        foreach (var mapping in mappings)
        {
            if (mapping.instrument == instrument)
                return mapping.enabled;
        }
        return false;
    }

    public void SetMixerChannel(Song.Instrument instrument, int channel)
    {
        foreach (var mapping in mappings)
        {
            if (mapping.instrument == instrument)
            {
                mapping.mixerChannel = Mathf.Clamp(channel, 1, 8);
                return;
            }
        }

        // If not found, add it
        AddMapping(instrument, channel, true);
    }

    public void SetEnabled(Song.Instrument instrument, bool enabled)
    {
        foreach (var mapping in mappings)
        {
            if (mapping.instrument == instrument)
            {
                mapping.enabled = enabled;
                return;
            }
        }
    }
}
