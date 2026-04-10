using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Knbn.Extension.Services;
using Knbn.Extension.UI;
using Knbn.Extension.UI.ViewModels;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Knbn.Extension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(KanbanToolWindow), Style = VsDockStyle.Tabbed,
        Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    public sealed class KnbnPackage : ToolkitPackage
    {
        public static KnbnPackage Instance { get; private set; }

        private AggregatorService _aggregator;
        private PersistenceService _persistence;
        private HttpListenerService _httpServer;
        private KanbanViewModel _viewModel;

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            await base.InitializeAsync(cancellationToken, progress);

            var eventsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".knbn", "events.jsonl");

            _aggregator = new AggregatorService();
            _persistence = new PersistenceService(eventsPath);
            _viewModel = new KanbanViewModel();

            // Replay persisted events
            foreach (var evt in _persistence.Replay())
            {
                _aggregator.HandleEvent(evt);
            }
            _viewModel.Refresh(_aggregator.GetCards());

            // Subscribe to changes
            _aggregator.CardsChanged += () =>
            {
                _viewModel.Refresh(_aggregator.GetCards());
            };

            // Start HTTP server
            _httpServer = new HttpListenerService(_aggregator, _persistence);
            _httpServer.Start();

            await this.RegisterCommandsAsync();
        }

        public async Task ShowToolWindowAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var window = await FindToolWindowAsync(
                typeof(KanbanToolWindow), 0, true, DisposalToken)
                as KanbanToolWindow;
            if (window?.Frame == null)
                throw new NotSupportedException("Cannot create tool window");

            window.SetContent(new KanbanToolWindowControl(_viewModel));

            var windowFrame = (Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpServer?.Stop();
            }
            base.Dispose(disposing);
        }
    }
}
