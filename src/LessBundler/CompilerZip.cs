using System.IO;
using System.Reflection;

namespace LessBundler
{
    public static class CompilerZip
    {
        public static void UnzipTo(string targetFolder)
        {
            const string resourceName = "LessBundler.Properties.Resources.LessCompiler.zip";
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var zipFile = Ionic.Zip.ZipFile.Read(resourceStream))
            {
                Directory.CreateDirectory(targetFolder);
                zipFile.ExtractAll(targetFolder);
            }
        }
    }
}
