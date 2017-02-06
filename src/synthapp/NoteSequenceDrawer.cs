using ImGuiNET;
using System.Collections.Generic;
using Veldrid.Graphics;

namespace SynthApp
{
    public class NoteSequenceDrawer : Drawer<NoteSequence>
    {
        public override bool Draw(string label, ref NoteSequence ns, RenderContext rc)
        {
            List<Note> notes = ns.Notes;
            int count = notes.Count;
            for (int i = 0; i < count; i++)
            {
                Note n = notes[i];
                if (i != 0)
                {
                    ImGui.SameLine();
                }
                if (ImGui.BeginChildFrame((uint)i, new System.Numerics.Vector2(80, 80), WindowFlags.ShowBorders))
                {
                    object pitch = n.Pitch;
                    if (DrawerCache.GetDrawer(typeof(Pitch)).Draw("###Pitch", ref pitch, rc))
                    {
                        n.Pitch = (Pitch)pitch;
                    }

                    object start = n.StartTime;
                    if (DrawerCache.GetDrawer(typeof(PatternTime)).Draw("###Start", ref start, rc))
                    {
                        n.StartTime = (PatternTime)start;
                    }

                    ImGui.SameLine();
                    ImGui.Text(" - ");
                    ImGui.SameLine();

                    object duration = n.Duration;
                    if (DrawerCache.GetDrawer(typeof(PatternTime)).Draw("###Duration", ref duration, rc))
                    {
                        n.Duration = (PatternTime)duration;
                    }

                    ImGui.EndChildFrame();
                }
            }

            notes.Sort(StartTimeComparer);

            return false;
        }

        private int StartTimeComparer(Note x, Note y)
        {
            return x.StartTime.CompareTo(y.StartTime);
        }
    }
}
