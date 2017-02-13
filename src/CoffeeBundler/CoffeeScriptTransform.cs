using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Optimization;

namespace CoffeeBundler
{
    public class CoffeeScriptTransform : IBundleTransform
    {
        public CoffeeScriptTransform()
        {
            Timeout = TimeSpan.FromMinutes(1);
        }

        public CoffeeScriptTransform(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; set; }
        public void Process(BundleContext context, BundleResponse response)
        {
            lock (this)
            {
                var timer = Stopwatch.StartNew();
                var compilerTime = new Stopwatch();
                var javascriptText = new StringBuilder();
                var fileGroup = response.Files;

                var tempFolder = Path.GetTempPath();
                var ourTempFolder = Path.Combine(tempFolder, Guid.NewGuid().ToString("n"));
                Directory.CreateDirectory(ourTempFolder);
                try
                {
                    var filePaths = new Dictionary<string, string>();

                    // copy the source files to the temp folder with names like 0.coffee, 1.coffee, etc.
                    var fileNumber = 0;
                    foreach (var file in fileGroup)
                    {
                        var fileName = string.Format("{0}-{1}", fileNumber++, file.VirtualFile.Name);
                        var fileNameWithPath = filePaths[file.VirtualFile.VirtualPath] = Path.Combine(ourTempFolder, fileName);
                        var actualFilePath = context.HttpContext.Server.MapPath(file.VirtualFile.VirtualPath);
                        File.Copy(actualFilePath, fileNameWithPath);
                    }

                    // compile the files from .coffee to .js
                    compilerTime.Start();
                    Compiler.CompilePath(ourTempFolder, Timeout);
                    compilerTime.Stop();

                    // add the files to the bundle
                    foreach (var file in fileGroup)
                    {
                        var fileName = Path.ChangeExtension(filePaths[file.VirtualFile.VirtualPath], ".js");
                        javascriptText.AppendFormat("{2}/* Filename: {0} */{2}{1}{2}", file.IncludedVirtualPath, File.ReadAllText(fileName), Environment.NewLine);
                    }
                }
                finally
                {
                    Directory.Delete(ourTempFolder, true);
                }

                Debug.WriteLine("Bundle {0} built in {1} ms, compile time: {2} ms", context.BundleVirtualPath, timer.ElapsedMilliseconds, compilerTime.ElapsedMilliseconds);

                response.Content = javascriptText.ToString();
                response.ContentType = "text/javascript";
            }
        }
    }
}