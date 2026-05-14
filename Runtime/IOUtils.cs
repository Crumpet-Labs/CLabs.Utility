using System.IO;

namespace CLabs.Utility {
    public static class IOUtils {
        public static void ForceDirectory(string path) {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void DeleteFile(string path) {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}