using System.IO.Compression;

namespace LessBundler
{
    public static class CompilerZip
    {
        public static void UnzipTo(string targetFolder)
        {
            const string resourceName = "LessBundler.Properties.Resources.LessCompiler.zip";
            ZipFile.ExtractToDirectory(resourceName, targetFolder);
        }
    }
}
