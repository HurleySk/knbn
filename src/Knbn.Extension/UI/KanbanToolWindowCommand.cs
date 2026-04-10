using System;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Knbn.Extension.UI
{
    [Command("b2c3d4e5-f6a7-8901-bcde-f12345678901", 0x0100)]
    internal sealed class KanbanToolWindowCommand : BaseCommand<KanbanToolWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await KnbnPackage.Instance.ShowToolWindowAsync();
        }
    }
}
