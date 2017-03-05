using System.Collections.Generic;
using System.Linq;

namespace SynthApp
{
    public class SynthAppPreferences : PersistentStorage<SynthAppPreferences, SynthAppPreferences.Info>
    {
        private const int OpenedProjectHistoryLimit = 10;

        public List<string> OpenedProjectHistory { get; } = new List<string>();

        public string GetLastOpenedProject()
        {
            return OpenedProjectHistory.LastOrDefault();
        }

        public void SetLatestProject(string path)
        {
            OpenedProjectHistory.Remove(path);
            OpenedProjectHistory.Add(path);

            if (OpenedProjectHistory.Count > OpenedProjectHistoryLimit)
            {
                OpenedProjectHistory.RemoveAt(0);
            }

            Save();
        }

        public class Info : PersistentStorageInfo
        {
            public string StoragePath => "SynthApp";
        }
    }
}
