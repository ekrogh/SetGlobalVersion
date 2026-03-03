using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Community.VisualStudio.Toolkit
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public int Id { get; }
        public CommandAttribute(int id) => Id = id;
    }

    public abstract class BaseCommand<T> where T : BaseCommand<T>, new()
    {
        private AsyncPackage _package;

        internal static async Task RegisterAsync(AsyncPackage package)
        {
            var instance = new T();
            await instance.InitializeAsync(package);
        }

        private async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _package = package;

            var commandAttribute = typeof(T).GetCustomAttribute<CommandAttribute>();
            if (commandAttribute == null)
            {
                return;
            }

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;
            if (commandService == null)
            {
                return;
            }

            var menuCommandId = new CommandID(SetGlobalVersion.PackageGuids.SetGlobalVersion, commandAttribute.Id);
            var menuCommand = new OleMenuCommand(async (_, e) =>
            {
                await ExecuteAsync(e as Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs ?? new Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs(null, IntPtr.Zero));
            }, menuCommandId);

            commandService.AddCommand(menuCommand);
        }

        protected AsyncPackage Package => _package;
        protected abstract Task ExecuteAsync(Microsoft.VisualStudio.Shell.OleMenuCmdEventArgs e);
    }

    public abstract class BaseToolWindow<T> where T : BaseToolWindow<T>, new()
    {
        public abstract string GetTitle(int toolWindowId);
        public abstract Type PaneType { get; }
        public abstract Task<System.Windows.FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken);

        public static async Task ShowAsync()
        {
            var package = ToolkitPackage.Current;
            if (package == null)
            {
                return;
            }

            var instance = new T();
            var window = await package.ShowToolWindowAsync(instance.PaneType, 0, true, package.DisposalToken);
            if (window?.Content == null)
            {
                window.Content = await instance.CreateAsync(0, package.DisposalToken);
            }
        }
    }

    public abstract class ToolkitPackage : AsyncPackage
    {
        internal static ToolkitPackage Current { get; private set; }

        private static readonly MethodInfo RegisterCommandMethod = typeof(ToolkitPackage)
            .GetMethod(nameof(RegisterCommandAsync), BindingFlags.Instance | BindingFlags.NonPublic);

        protected override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Current = this;
            return base.InitializeAsync(cancellationToken, progress);
        }

        public async Task RegisterCommandsAsync()
        {
            var commandTypes = GetType().Assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null)
                .ToList();

            foreach (var commandType in commandTypes)
            {
                var method = RegisterCommandMethod?.MakeGenericMethod(commandType);
                if (method == null)
                {
                    continue;
                }

                var task = method.Invoke(this, null) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }

        private Task RegisterCommandAsync<TCommand>() where TCommand : BaseCommand<TCommand>, new()
            => BaseCommand<TCommand>.RegisterAsync(this);

        public void RegisterToolWindows()
        {
        }
    }

    public sealed class Solution
    {
        private readonly DTE2 _dte;

        internal Solution(DTE2 dte)
        {
            _dte = dte;
        }

        public string Name => _dte?.Solution?.FileName;
        public string FullPath => _dte?.Solution?.FullName;

        public IEnumerable<SolutionItem> Children
        {
            get
            {
                var result = new List<SolutionItem>();
                if (_dte?.Solution?.Projects == null)
                {
                    return result;
                }

                foreach (Project project in _dte.Solution.Projects)
                {
                    if (project == null)
                    {
                        continue;
                    }

                    result.Add(SolutionItem.FromProject(project));
                }

                return result;
            }
        }

        public Task<SolutionFolder> AddSolutionFolderAsync(string name)
        {
            var folderProject = ((EnvDTE100.Solution4)_dte.Solution).AddSolutionFolder(name);
            return Task.FromResult(new SolutionFolder(folderProject));
        }
    }

    public sealed class SolutionItem
    {
        public string Name { get; init; }
        public string FullPath { get; init; }
        public IEnumerable<SolutionItem> Children { get; init; } = Enumerable.Empty<SolutionItem>();

        internal static SolutionItem FromProject(Project project)
        {            var children = new List<SolutionItem>();
            if (project.ProjectItems != null)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    children.Add(FromProjectItem(item));
                }
            }

            return new SolutionItem
            {
                Name = project.Name,
                FullPath = project.FullName,
                Children = children
            };
        }

        internal static SolutionItem FromProjectItem(ProjectItem item)
        {            var children = new List<SolutionItem>();
            if (item.ProjectItems != null)
            {
                foreach (ProjectItem child in item.ProjectItems)
                {
                    if (child == null)
                    {
                        continue;
                    }
                    children.Add(FromProjectItem(child));
                }
            }

            string fullPath = string.Empty;
            try
            {
                if (item.FileCount > 0)
                {
                    fullPath = item.FileNames[1];
                }
            }
            catch
            {
                fullPath = string.Empty;
            }

            return new SolutionItem
            {
                Name = item.Name,
                FullPath = fullPath,
                Children = children
            };
        }
    }

    public sealed class SolutionFolder
    {
        private readonly Project _project;

        internal SolutionFolder(Project project)
        {
            _project = project;
        }

        public Task<IEnumerable<PhysicalFile>> AddExistingFilesAsync(string path)
        {
            _project?.ProjectItems?.AddFromFile(path);
            IEnumerable<PhysicalFile> files = new[] { new PhysicalFile { FullPath = path } };
            return Task.FromResult(files);
        }
    }

    public sealed class PhysicalFile
    {
        public string FullPath { get; set; }
    }

    public static class VS
    {
        public static class Solutions
        {
            public static Task<bool> IsOpenAsync()
            {                var dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
                return Task.FromResult(dte?.Solution?.IsOpen == true);
            }

            public static Task<Solution> GetCurrentSolutionAsync()
            {                var dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
                return Task.FromResult(dte?.Solution == null ? null : new Solution(dte));
            }
        }

        public static class Events
        {
            public static class SolutionEvents
            {
                private static EnvDTE.SolutionEvents _events;
                private static bool _initialized;
                private static Action _onAfterCloseSolution;
                private static Action _onAfterBackgroundSolutionLoadComplete;

                public static event Action OnAfterCloseSolution
                {
                    add
                    {                        EnsureSubscribed();
                        _onAfterCloseSolution += value;
                    }
                    remove => _onAfterCloseSolution -= value;
                }

                public static event Action OnAfterBackgroundSolutionLoadComplete
                {
                    add
                    {                        EnsureSubscribed();
                        _onAfterBackgroundSolutionLoadComplete += value;
                    }
                    remove => _onAfterBackgroundSolutionLoadComplete -= value;
                }

                private static void EnsureSubscribed()
                {
                    if (_initialized)
                    {
                        return;
                    }

                    var dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
                    if (dte == null)
                    {
                        return;
                    }

                    _events = ((Events2)dte.Events).SolutionEvents;
                    _events.AfterClosing += () => _onAfterCloseSolution?.Invoke();
                    _events.Opened += () => _onAfterBackgroundSolutionLoadComplete?.Invoke();
                    _initialized = true;
                }
            }
        }

        public static class MessageBox
        {
            public static Task<int> ShowAsync(string title, string message, OLEMSGICON icon, OLEMSGBUTTON buttons)
            {
                var sp = Package.GetGlobalService(typeof(SVsServiceProvider)) as IServiceProvider;
                var result = VsShellUtilities.ShowMessageBox(
                    sp,
                    message,
                    title,
                    icon,
                    buttons,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return Task.FromResult(result);
            }
        }
    }
}

namespace SetGlobalVersion
{
    internal static class WindowGuids
    {
        public const string MainWindow = "{00000000-0000-0000-0000-000000000000}";
    }
}