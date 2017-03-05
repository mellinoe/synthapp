using ImGuiNET;
using Veldrid.Graphics;

namespace SynthApp.Widgets
{
    [Widget]
    public class PitchDrawer : Drawer<Pitch>
    {
        private readonly EnumDrawer _pitchClassDrawer = new EnumDrawer(typeof(PitchClass));

        public override bool Draw(string label, ref Pitch obj, RenderContext rc)
        {
            bool changed = false;
            object pc = obj.PitchClass;
            object octave = (int)obj.Octave;

            changed = (_pitchClassDrawer.Draw("Pitch", ref pc, rc));
            ImGui.SameLine();
            changed |= DrawerCache.GetDrawer(typeof(int)).Draw("Octave", ref octave, rc);

            if (changed)
            {
                obj = new Pitch((PitchClass)pc, (uint)octave);
            }

            return changed;
        }
    }
}
