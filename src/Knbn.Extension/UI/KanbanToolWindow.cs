using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Knbn.Extension.UI
{
    [Guid("c3d4e5f6-a7b8-9012-cdef-123456789012")]
    public class KanbanToolWindow : ToolWindowPane
    {
        public KanbanToolWindow() : base(null)
        {
            Caption = "knbn Board";
        }

        public void SetContent(KanbanToolWindowControl control)
        {
            Content = control;
        }
    }
}
