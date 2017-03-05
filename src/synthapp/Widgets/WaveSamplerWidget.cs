using ImGuiNET;
using Veldrid.Graphics;

namespace SynthApp.Widgets
{
    [Widget]
    public class WaveSamplerWidget : Drawer<WaveSampler>
    {
        public override bool Draw(string label, ref WaveSampler ws, RenderContext rc)
        {
            {
                bool muted = ws.Muted;
                if (ImGui.Checkbox("Muted", ref muted))
                {
                    ws.Muted = muted;
                }
                ImGui.SameLine();
                float gain = ws.Gain;
                if (ImGui.SliderFloat("Gain", ref gain, 0f, 2f, gain.ToString(), 1f))
                {
                    ws.Gain = gain;
                }

                FilePicker fp = FilePicker.GetFilePicker(ws, ws.WaveFilePath);
                string file = ws.WaveFilePath;
                if (fp.Draw(ref file))
                {
                    ws.WaveFilePath = file;
                }
            }

            return false;
        }
    }
}
