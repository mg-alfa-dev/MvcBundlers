using System;
using System.Diagnostics;
using System.IO;
using System.Web.Optimization;

namespace LessBundler
{
    public class LessTransform : IBundleTransform
    {
        private readonly string _BaseFileName;
        private readonly string _VirtualRoot;

        public TimeSpan Timeout { get; set; }

        public LessTransform(string baseFileName, string virtualRoot)
            : this(baseFileName, virtualRoot, TimeSpan.FromMinutes(1))
        {
        }

        public LessTransform(string baseFileName, string virtualRoot, TimeSpan timeout)
        {
            _BaseFileName = baseFileName;
            _VirtualRoot = virtualRoot.EndsWith("/") ? virtualRoot : virtualRoot + "/";
            Timeout = timeout;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            lock (this)
            {
                var timer = Stopwatch.StartNew();
                var compilerTime = new Stopwatch();
                var fileGroup = response.Files;

                var tempFolder = Path.GetTempPath();
                var ourTempFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("n"));
                Directory.CreateDirectory(ourTempFolder);
                try
                {
                    foreach (var file in fileGroup)
                    {
                        var pathToFile = context.HttpContext.Server.MapPath(file.VirtualFile.VirtualPath);
                        if (!file.VirtualFile.VirtualPath.StartsWith(_VirtualRoot))
                            throw new InvalidOperationException(
                                string.Format(
                                    "Expected virtual path of file {0} to start with {1}, but instead the path was {2}",
                                    file.VirtualFile.Name,
                                    _VirtualRoot,
                                    file.VirtualFile.VirtualPath));
                        var relativePath = file.VirtualFile.VirtualPath.Remove(0, _VirtualRoot.Length);
                        var targetPathToFile = Path.Combine(ourTempFolder, relativePath);
                        var targetFolder = Path.GetDirectoryName(targetPathToFile);
                        Directory.CreateDirectory(targetFolder);
                        File.Copy(pathToFile, targetPathToFile, true);
                    }

                    // compile the root file from .less to .css
                    compilerTime.Start();
                    Compiler.CompilePath(Path.Combine(ourTempFolder, _BaseFileName), Timeout);
                    compilerTime.Stop();

                    // add the file to the bundle
                    var cssFileName = Path.ChangeExtension(Path.Combine(ourTempFolder, _BaseFileName), ".css");
                    response.Content = File.ReadAllText(cssFileName);
                    response.ContentType = "text/css";
                }
                finally
                {
                    Directory.Delete(ourTempFolder, true);
                }

                Debug.WriteLine("Bundle {0} built in {1} ms, compile time: {2} ms", context.BundleVirtualPath, timer.ElapsedMilliseconds, compilerTime.ElapsedMilliseconds);
            }
        }
    }
}