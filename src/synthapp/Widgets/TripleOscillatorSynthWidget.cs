using ImGuiNET;
using System;
using Veldrid;

namespace SynthApp.Widgets
{
    [Widget]
    public class TripleOscillatorSynthWidget : Drawer<TripleOscillatorSynth>
    {
        public override bool Draw(string label, ref TripleOscillatorSynth tos, GraphicsDevice rc)
        {
            bool muted = tos.Muted;
            if (ImGui.Checkbox("Muted", ref muted))
            {
                tos.Muted = muted;
            }
            ImGui.SameLine();
            float gain = tos.Gain;
            if (ImGui.SliderFloat("Gain", ref gain, 0f, 2f, gain.ToString(), 1f))
            {
                tos.Gain = gain;
            }

            ImGui.Separator();
            DrawGenerator(0, tos.Generator1);
            ImGui.Separator();
            DrawGenerator(1, tos.Generator2);
            ImGui.Separator();
            DrawGenerator(2, tos.Generator3);

            return false;
        }

        private static void DrawGenerator(int id, SimpleWaveformGenerator generator)
        {
            ImGui.PushID(id);
            var enumHelper = ImGuiEnumHelper.GetHelper<SimpleWaveformGenerator.WaveformType>();
            var waveform = generator.Type;
            int waveformIndex = Array.IndexOf(enumHelper.Values, waveform);
            if (ImGui.Combo("Waveform", ref waveformIndex, enumHelper.Names))
            {
                generator.Type = enumHelper.Values[waveformIndex];
            }

            float gain = generator.Gain;
            if (ImGui.SliderFloat("Gain", ref gain, 0f, 2f, gain.ToString(), 1f))
            {
                generator.Gain = gain;
            }

            ImGui.SameLine();

            float pitchScale = (float)generator.PitchScale;
            if (ImGui.DragFloat("Frequency", ref pitchScale, 0.25f, 4.0f, dragSpeed: .1f))
            {
                generator.PitchScale = pitchScale;
            }

            ImGui.SameLine();

            float phaseOffset = (float)generator.PhaseOffset;
            if (ImGui.SliderFloat("Phase Offset", ref phaseOffset, (float)(-2 * Math.PI), (float)(2 * Math.PI), phaseOffset.ToString(), 1f))
            {
                generator.PhaseOffset = phaseOffset;
            }

            ImGui.PopID();
        }
    }
}
