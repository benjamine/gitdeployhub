using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using GitDeployHub.Web.Engine.Processes;

namespace GitDeployHub.Web.Engine
{
    public class Instance
    {
        public string Name { get; private set; }

        public string Folder { get; set; }

        public IDictionary<string, string> EnvironmentVariables { get; set; }

        public string Treeish { get; set; }

        private static string _mappedApplicationPath;

        public static string MappedApplicationPath
        {
            get
            {
                if (_mappedApplicationPath == null)
                {
                    var appPath = HttpContext.Current.Request.ApplicationPath.ToLower();
                    if (appPath == "/")
                    {
                        //a site
                        appPath = "/";
                    }
                    else if (!appPath.EndsWith(@"/"))
                    {
                        //a virtual
                        appPath += @"/";
                    }
                    var mappedPath = HttpContext.Current.Server.MapPath(appPath);
                    if (!mappedPath.EndsWith(@"\"))
                        mappedPath += @"\";
                    _mappedApplicationPath = mappedPath;
                }
                return _mappedApplicationPath;
            }
        }

        public Deployment LastDeployment { get; set; }

        public Deployment CurrentDeployment { get; set; }

        public Process CurrentProcess { get; set; }

        public Hub Hub { get; set; }

        private string[] _tags;

        public string[] Tags
        {
            get
            {
                if (_tags == null)
                {
                    try
                    {
                        var log = new StringLog();
                        if (!HasFolder(".git"))
                        {
                            _tags = new[] { "<not-a-git-repo>" };
                        }
                        else
                        {
                            ExecuteProcess("git", "log -n1 --pretty=format:%d", log, false);
                            _tags = log.Output.Trim(new[] { ' ', '(', ')', '\r', '\n' })
                                       .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(t => t.Trim())
                                       .Where(t => t != "HEAD" && t != "master" && !t.StartsWith("origin/"))
                                       .ToArray();
                        }
                    }
                    catch
                    {
                        _tags = new[] { "<error-getting-tags>" };
                    }
                }
                return _tags;
            }
        }

        private string[] _filesChangedToTreeish;

        public string[] FilesChangedToTreeish
        {
            get
            {
                if (_filesChangedToTreeish == null)
                {
                    var log = new StringLog();
                    Diff(log, Treeish, out _filesChangedToTreeish, false);
                }
                return _filesChangedToTreeish;
            }
        }

        private string _trackedBranch;

        public string TrackedBranch
        {
            get
            {
                if (_trackedBranch == null)
                {
                    var log = new StringLog();
                    ExecuteProcess("git", "for-each-ref --format='%(upstream:short)' $(git symbolic-ref -q HEAD)", log, false);
                    _trackedBranch = log.Output.Trim(new[] { ' ', '\t', '\r', '\n' });
                }
                return _trackedBranch;
            }
        }

        public Instance(Hub hub, string name, string treeish = null, string folder = null)
        {
            Hub = hub;
            Name = name;
            Treeish = treeish;
            Folder = folder;
            EnvironmentVariables = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(Folder))
            {
                if (Name == "_self")
                {
                    Folder = Path.GetFullPath(MappedApplicationPath);
                    if (!HasFile(".git"))
                    {
                        var gitFolder = Folder;
                        while (!string.IsNullOrEmpty(gitFolder))
                        {
                            gitFolder = Directory.GetParent(gitFolder).FullName;
                            if (Directory.Exists(Path.Combine(gitFolder, ".git")))
                            {
                                Folder = gitFolder;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Folder = Path.GetFullPath(Path.Combine(MappedApplicationPath, Path.Combine("../../", name)));
                }
            }
        }

        public Deployment CreateDeployment(IDictionary<string, string> parameters = null)
        {
            var deployment = new Deployment(Hub, this)
            {
                Parameters = parameters
            };
            Hub.Queue.Add(deployment);
            return deployment;
        }

        public void ExecuteProcess(string command, string arguments, ILog log, bool echo = true)
        {
            if (echo)
            {
                log.Log(string.Format(" & {0} {1}", command, arguments ?? ""));
            }
            var processStartInfo = new ProcessStartInfo(command)
                {
                    UseShellExecute = false,
                    WorkingDirectory = Folder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            if (EnvironmentVariables != null)
            {
                foreach (var envVar in EnvironmentVariables)
                {
                    processStartInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
                }
            }
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                processStartInfo.Arguments = arguments;
            }

            var process = new System.Diagnostics.Process
                {
                    StartInfo = processStartInfo
                };
            process.OutputDataReceived += (sender, args) => log.Log(args.Data);
            process.ErrorDataReceived += (sender, args) => log.Log(args.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            var exited = process.WaitForExit((int)TimeSpan.FromMinutes(15).TotalMilliseconds);
            if (!exited)
            {
                throw new Exception(string.Format("{0} {1} timed out.", command, arguments));
            }
            if (process.ExitCode != 0)
            {
                throw new Exception(string.Format("{0} {1} failed with exit code: {2}", command, arguments, process.ExitCode));
            }
        }

        public void Fetch(ILog log)
        {
            ExecuteProcess("git", "fetch", log);
            ExecuteProcess("git", "status -uno", log);
            _tags = null;
            _filesChangedToTreeish = null;
        }

        public void Status(ILog log, out bool isBehind, out bool canFastForward)
        {
            var commandLog = new StringLog(log);
            ExecuteProcess("git", "status -uno", commandLog);
            var statusOutput = commandLog.Output;
            isBehind = statusOutput.Contains("Your branch is behind");
            canFastForward = statusOutput.Contains("can be fast-forwarded");
        }

        public void Diff(ILog log, string treeish, out string[] changes, bool echo = true)
        {
            var commandLog = new StringLog(log);
            ExecuteProcess("git", "diff --name-only HEAD.." + treeish, commandLog, echo);
            var output = commandLog.Output;
            if (string.IsNullOrWhiteSpace(output))
            {
                changes = new string[0];
            }
            else
            {
                changes = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public void Pull(ILog log)
        {
            ExecuteProcess("git", "pull", log);
            _tags = null;
            _filesChangedToTreeish = null;
        }

        public void Stash(ILog log)
        {
            ExecuteProcess("git", "stash", log);
        }

        public void Checkout(string branchOrTag, ILog log)
        {
            ExecuteProcess("git", "checkout " + branchOrTag, log);
            _tags = null;
            _filesChangedToTreeish = null;
        }

        public bool HasFile(string fileName)
        {
            return File.Exists(Path.Combine(Folder, fileName));
        }

        public bool HasFolder(string path)
        {
            return Directory.Exists(Path.Combine(Folder, path));
        }

        public void ExecuteIfExists(string fileName, string command, string arguments, ILog log)
        {
            if (!HasFile(fileName))
            {
                log.Log(string.Format("({0} not present)", fileName));
                return;
            }
            ExecuteProcess(command, arguments, log);
        }

        public void ExecutePreDeploy(ILog log)
        {
            var fileName = "BuildScripts\\PreDeploy.ps1";
            ExecuteIfExists(fileName, "powershell", fileName, log);
        }

        public void ExecutePostDeploy(ILog log)
        {
            var fileName = "BuildScripts\\PostDeploy.ps1";
            ExecuteIfExists(fileName, "powershell", fileName, log);
        }

        internal void Log(string message, Process process)
        {
            Hub.Log(message, this, process);
        }
    }
}
