using System.IO;
using System.Reflection;

namespace CoffeeBundler
{
    public static class CompilerZip
    {
        public static void UnzipTo(string targetFolder)
        {
            const string resourceName = "CoffeeBundler.Properties.Resources.CoffeeCompiler.zip";
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var zipFile = Ionic.Zip.ZipFile.Read(resourceStream))
            {
                Directory.CreateDirectory(targetFolder);
                zipFile.ExtractAll(targetFolder);
            }
        }
    }
}
