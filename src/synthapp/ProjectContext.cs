using System.IO;

namespace SynthApp
{
    public class ProjectContext
    {
        public string FullPath { get; set; }

        public string GetAssetRootPath()
        {
            return new FileInfo(FullPath).DirectoryName;
        }
    }
}
