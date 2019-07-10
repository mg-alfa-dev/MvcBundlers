using System.IO.Compression;

namespace CoffeeBundler
{
    public static class CompilerZip
    {
        public static void UnzipTo(string targetFolder)
        {
            const string resourceName = "CoffeeBundler.Properties.Resources.CoffeeCompiler.zip";
            ZipFile.ExtractToDirectory(resourceName, targetFolder);
        }
    }
}
