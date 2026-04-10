using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Knbn.Extension.Models;

namespace Knbn.Extension.UI.ViewModels
{
    public class CardViewModel : INotifyPropertyChanged
    {
        private readonly WorkItem _card;

        public CardViewModel(WorkItem card)
        {
            _card = card;
        }

        public string Id => _card.Id;
        public string Title => _card.Title;
        public string ProjectName => _card.ProjectName();
        public int SessionCount => _card.Sessions.Count;
        public WorkItemStatus Status => _card.Status;
        public bool HasActiveSessions => _card.HasActiveSessions();

        public string LatestNote =>
            _card.Notes.Count > 0
                ? _card.Notes[_card.Notes.Count - 1].Message
                : null;

        public string TimeAgo
        {
            get
            {
                var diff = DateTime.UtcNow - _card.UpdatedAt;
                if (diff.TotalMinutes < 1) return "just now";
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
                return $"{(int)diff.TotalDays}d ago";
            }
        }

        public bool IsStale => (DateTime.UtcNow - _card.UpdatedAt).TotalHours > 24;

        public System.Windows.Media.SolidColorBrush BorderColor
        {
            get
            {
                if (_card.Status == WorkItemStatus.Done)
                    return new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x60, 0x8B, 0x4E));
                if (HasActiveSessions)
                    return new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x4E, 0xC9, 0xB0));
                return new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xDC, 0xDC, 0xAA));
            }
        }

        public double CardOpacity => IsStale ? 0.5 : 1.0;

        public bool HasNote => LatestNote != null;

        public DateTime UpdatedAt => _card.UpdatedAt;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
