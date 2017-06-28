using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LessBundler
{
    public static class Compiler
    {
        private static readonly object _InitializationLock = new object();

        private static string _CompilerFolder;
        private static bool _Initialized;

        private static void _Initialize()
        {
            if (_Initialized)
                return;
            lock (_InitializationLock)
            {
                if (_Initialized)
                    return;
                _CompilerFolder = Path.GetTempFileName();
                File.Delete(_CompilerFolder);
                Directory.CreateDirectory(_CompilerFolder);
                CompilerZip.UnzipTo(_CompilerFolder);
                _Initialized = true;
            }
        }

        public static void CompilePath(string pathToCompile, TimeSpan timeout)
        {
            _Initialize();
            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                ErrorDialog = false,
                LoadUserProfile = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,

                WorkingDirectory = _CompilerFolder,
                FileName = Path.Combine(_CompilerFolder, "node.exe"),
                Arguments = "bin/lessc " + pathToCompile + " " + Path.ChangeExtension(pathToCompile, ".css"),
            };

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            var processOutput = new StringBuilder();
            process.OutputDataReceived += (s, e) => processOutput.AppendLine(e.Data);
            process.ErrorDataReceived += (s, e) => processOutput.AppendLine(e.Data);

            process.Start();
            using (process)
            {
                process.StandardInput.Close();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                if (!process.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
                {
                    // process took too long?
                    process.Kill();
                    throw new InvalidOperationException(string.Format("Less compiler took too long:\r\nOutput:\r\n{0}", processOutput));
                }

                if (process.ExitCode != 0)
                {
                    // process exited with weird code?
                    throw new InvalidOperationException(string.Format("Less compiler exited with code {0}:\r\nOutput:\r\n{1}", process.ExitCode, processOutput));
                }
            }
        }
    }
}