using System.Windows.Controls;
using Knbn.Extension.UI.ViewModels;

namespace Knbn.Extension.UI
{
    public partial class KanbanToolWindowControl : UserControl
    {
        public KanbanToolWindowControl(KanbanViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
