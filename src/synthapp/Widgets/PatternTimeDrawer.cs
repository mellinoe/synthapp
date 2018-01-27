using ImGuiNET;
using Veldrid;

namespace SynthApp.Widgets
{
    [Widget]
    public class PatternTimeDrawer : Drawer<PatternTime>
    {
        public override bool Draw(string label, ref PatternTime pt, GraphicsDevice rc)
        {
            uint tick = pt.Tick;
            int step = (int)pt.Step;
            if (ImGui.SliderInt(label, ref step, 0, 999, null))
            {
                pt = new PatternTime((uint)step, tick);
                return true;
            }

            return false;
        }
    }
}
