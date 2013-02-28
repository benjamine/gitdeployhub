using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace GitDeployHub.Web.Engine
{
    public class Instance
    {
        public string Name { get; private set; }

        public string Folder { get; set; }

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

                        var output = new StringBuilder();
                        if (!HasFolder(".git"))
                        {
                            _tags = new[] { "<not-a-git-repo>" };
                        }
                        else
                        {
                            ExecuteProcess("git", "tag --contains HEAD", output, false);
                            _tags = output.ToString().Split(new[] { '\r', '\n' })
                                          .Select(tag => tag.Trim())
                                          .Where(tag => tag.Length > 0).ToArray();
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

        private string _trackedBranch;

        public string TrackedBranch
        {
            get
            {
                if (_trackedBranch == null)
                {
                    var output = new StringBuilder();
                    ExecuteProcess("git", "for-each-ref --format='%(upstream:short)' $(git symbolic-ref -q HEAD)", output, false);
                    _trackedBranch = output.ToString().Trim(new[] { ' ', '\t', '\r', '\n' });
                }
                return _trackedBranch;
            }
        }

        public Instance(Hub hub, string name, string folder = null)
        {
            Hub = hub;
            Name = name;
            Folder = folder;
            if (string.IsNullOrWhiteSpace(Folder))
            {
                if (Name == "_self")
                {
                    Folder = Path.GetFullPath(Path.Combine(MappedApplicationPath));
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
            Hub.DeploymentQueue.Add(deployment);
            return deployment;
        }

        public void ExecuteProcess(string command, string arguments, StringBuilder output, bool echo = true)
        {
            if (echo)
            {
                output.AppendFormat(" & {0} {1}", command, arguments ?? "");
            }
            output.AppendLine();
            var processStartInfo = new ProcessStartInfo(command)
                {
                    UseShellExecute = false,
                    WorkingDirectory = Folder,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                processStartInfo.Arguments = arguments;
            }
            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            output.Append(stdout);
            output.Append(stderr);
            if (process.ExitCode != 0)
            {
                throw new Exception(string.Format("{0} {1} failed with exit code: {2}", command, arguments, process.ExitCode));
            }
        }

        public void Fetch(StringBuilder output)
        {
            ExecuteProcess("git", "fetch", output);
        }

        public void Status(StringBuilder output, out bool isBehind, out bool canFastForward)
        {
            var outputStart = output.Length;
            ExecuteProcess("git", "status -uno", output);
            var statusOutput = output.ToString(outputStart, output.Length - outputStart);
            isBehind = statusOutput.Contains("Your branch is behind");
            canFastForward = statusOutput.Contains("can be fast-forwarded");
        }

        public void Pull(StringBuilder output)
        {
            ExecuteProcess("git", "pull", output);
            _tags = null;
        }

        public bool HasFile(string fileName)
        {
            return File.Exists(Path.Combine(Folder, fileName));
        }

        public bool HasFolder(string path)
        {
            return Directory.Exists(Path.Combine(Folder, path));
        }

        public void ExecuteIfExists(string fileName, string command, string arguments, StringBuilder output)
        {
            if (!HasFile(fileName))
            {
                output.AppendFormat("({0} not present)", fileName);
                output.AppendLine();
                return;
            }
            ExecuteProcess(command, arguments, output);
        }

        public void ExecutePreDeploy(StringBuilder output)
        {
            string fileName = "Build\\PreDeploy.ps1";
            ExecuteIfExists(fileName, "powershell", fileName, output);
        }

        public void ExecutePostDeploy(StringBuilder output)
        {
            string fileName = "Build\\PostDeploy.ps1";
            ExecuteIfExists(fileName, "powershell", fileName, output);
        }
    }
}
