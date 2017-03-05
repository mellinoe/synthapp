using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Veldrid.Graphics;

namespace SynthApp.Widgets
{
    public class FilePicker
    {
        private const string FilePickerID = "###FilePicker";
        private static readonly Dictionary<object, FilePicker> s_filePickers = new Dictionary<object, FilePicker>();
        private static readonly Vector2 DefaultFilePickerSize = new Vector2(600, 400);

        public string CurrentFolder { get; set; }
        public string SelectedFile { get; set; }

        public static FilePicker GetFilePicker(object o, string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Application.Instance.ProjectContext.GetAssetRootPath();
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            if (!s_filePickers.TryGetValue(o, out FilePicker fp))
            {
                fp = new FilePicker();
                fp.CurrentFolder = startingPath;
                s_filePickers.Add(o, fp);
            }

            return fp;
        }

        public bool Draw(ref string selected)
        {
            string label = null;
            if (selected != null)
            {
                if (Util.TryGetFileInfo(selected, out FileInfo realFile))
                {
                    label = realFile.Name;
                }
                else
                {
                    label = "<Select File>";
                }
            }
            if (ImGui.Button(label))
            {
                ImGui.OpenPopup(FilePickerID);
            }

            bool result = false;
            ImGui.SetNextWindowSize(DefaultFilePickerSize, SetCondition.FirstUseEver);
            if (ImGui.BeginPopupModal(FilePickerID, WindowFlags.NoTitleBar))
            {
                result = DrawFolder(ref selected, true);
                ImGui.EndPopup();
            }

            return result;
        }

        private bool DrawFolder(ref string selected, bool returnOnSelection  = false)
        {
            ImGui.Text("Current Folder: " + CurrentFolder);
            bool result = false;

            if (ImGui.BeginChildFrame(1, new Vector2(0, 600), WindowFlags.ShowBorders))
            {
                DirectoryInfo di = new DirectoryInfo(CurrentFolder);
                if (di.Exists)
                {
                    if (di.Parent != null)
                    {
                        ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Yellow.ToVector4());
                        if (ImGui.Selectable("../", false, SelectableFlags.DontClosePopups))
                        {
                            CurrentFolder = di.Parent.FullName;
                        }
                        ImGui.PopStyleColor();
                    }
                    foreach (var fse in Directory.EnumerateFileSystemEntries(di.FullName))
                    {
                        if (Directory.Exists(fse))
                        {
                            string name = Path.GetFileName(fse);
                            ImGui.PushStyleColor(ColorTarget.Text, RgbaFloat.Yellow.ToVector4());
                            if (ImGui.Selectable(name + "/", false, SelectableFlags.DontClosePopups))
                            {
                                CurrentFolder = fse;
                            }
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            string name = Path.GetFileName(fse);
                            bool isSelected = SelectedFile == fse;
                            if (ImGui.Selectable(name, isSelected, SelectableFlags.DontClosePopups))
                            {
                                SelectedFile = fse;
                                if (returnOnSelection)
                                {
                                    result = true;
                                    selected = SelectedFile;
                                }
                            }
                            if (ImGui.IsLastItemHovered() && ImGui.IsMouseDoubleClicked(0))
                            {
                                result = true;
                                selected = SelectedFile;
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                }

            }
            ImGui.EndChildFrame();


            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
            }

            if (SelectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    selected = SelectedFile;
                    ImGui.CloseCurrentPopup();
                }
            }

            return result;
        }
    }
}
