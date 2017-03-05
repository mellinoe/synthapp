using System;
using System.Collections.Generic;
using System.IO;

namespace SynthApp
{
    public class ProjectContext
    {
        public string FullPath { get; set; }

        public string GetAssetRootPath()
        {
            if (!string.IsNullOrEmpty(FullPath))
            {
                return new FileInfo(FullPath).DirectoryName;
            }
            else
            {
                return string.Empty;
            }
        }

        public string NormalizeAssetPath(string file)
        {
            if (!Util.IsValidPath(file))
            {
                return file;
            }
            if (!Path.IsPathRooted(file) || string.IsNullOrEmpty(FullPath))
            {
                return file;
            }
            else
            {
                if (IsParentFolder(file, GetAssetRootPath()))
                {
                    return GetRelativeSegment(file, GetAssetRootPath());
                }
                else
                {
                    return file;
                }
            }
        }

        public bool IsParentFolder(string path, string parentFolder)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            while ((di = di.Parent) != null)
            {
                if (di.FullName == parentFolder)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetRelativeSegment(string path, string parentFolder)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            List<string> sections = new List<string>();
            sections.Add(Path.GetFileName(path));
            while ((di = di.Parent) != null)
            {
                if (di.FullName == parentFolder)
                {
                    break;
                }
                else
                {
                    sections.Add(di.Name);
                }
            }

            sections.Reverse();
            return string.Join("/", sections);
        }
    }
}
