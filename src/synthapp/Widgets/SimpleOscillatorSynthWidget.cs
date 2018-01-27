using ImGuiNET;
using System;
using Veldrid;

namespace SynthApp.Widgets
{
    [Widget]
    public class SimpleOscillatorSynthWidget : Drawer<SimpleOscillatorSynth>
    {
        public override bool Draw(string label, ref SimpleOscillatorSynth sos, GraphicsDevice rc)
        {
            bool muted = sos.Muted;
            if (ImGui.Checkbox("Muted", ref muted))
            {
                sos.Muted = muted;
            }
            ImGui.SameLine();
            float gain = sos.Gain;
            if (ImGui.SliderFloat("Gain", ref gain, 0f, 2f, gain.ToString(), 1f))
            {
                sos.Gain = gain;
            }

            var enumHelper = ImGuiEnumHelper.GetHelper<SimpleWaveformGenerator.WaveformType>();
            var waveform = sos.Generator.Type;
            int waveformIndex = Array.IndexOf(enumHelper.Values, waveform);
            if (ImGui.Combo("Waveform", ref waveformIndex, enumHelper.Names))
            {
                sos.Generator.Type = enumHelper.Values[waveformIndex];
            }

            return false;
        }
    }
}
