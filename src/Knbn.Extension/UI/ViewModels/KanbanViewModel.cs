using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Knbn.Extension.Models;

namespace Knbn.Extension.UI.ViewModels
{
    public class KanbanViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CardViewModel> ActiveCards { get; } = new ObservableCollection<CardViewModel>();
        public ObservableCollection<CardViewModel> WaitingCards { get; } = new ObservableCollection<CardViewModel>();
        public ObservableCollection<CardViewModel> DoneCards { get; } = new ObservableCollection<CardViewModel>();

        public int TotalActiveCount => ActiveCards.Count + WaitingCards.Count;

        public void Refresh(List<WorkItem> cards)
        {
            ActiveCards.Clear();
            WaitingCards.Clear();
            DoneCards.Clear();

            foreach (var card in cards)
            {
                var vm = new CardViewModel(card);

                if (card.Status == WorkItemStatus.Done)
                    DoneCards.Add(vm);
                else if (card.HasActiveSessions())
                    ActiveCards.Add(vm);
                else
                    WaitingCards.Add(vm);
            }

            OnPropertyChanged(nameof(TotalActiveCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
