using System;
using System.Collections.Generic;
using System.Linq;
using Knbn.Extension.Models;
using Knbn.Extension.UI.ViewModels;
using Xunit;

namespace Knbn.Tests
{
    public class KanbanViewModelTests
    {
        private WorkItem MakeCard(string title, bool hasActiveSessions, WorkItemStatus status = WorkItemStatus.Active)
        {
            return new WorkItem
            {
                Title = title,
                Status = status,
                Sessions = new List<Session>
                {
                    new Session
                    {
                        SessionId = "s1",
                        Cwd = "C:/projects/" + title.ToLower().Replace(" ", "-"),
                        Active = hasActiveSessions
                    }
                },
                Notes = new List<Note>
                {
                    new Note { Message = "Working on it", SessionId = "s1" }
                },
                UpdatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public void Refresh_SortsCardsIntoCorrectColumns()
        {
            var cards = new List<WorkItem>
            {
                MakeCard("Active Work", hasActiveSessions: true),
                MakeCard("Waiting Work", hasActiveSessions: false),
                MakeCard("Done Work", hasActiveSessions: false, status: WorkItemStatus.Done)
            };

            var vm = new KanbanViewModel();
            vm.Refresh(cards);

            Assert.Single(vm.ActiveCards);
            Assert.Equal("Active Work", vm.ActiveCards[0].Title);
            Assert.Single(vm.WaitingCards);
            Assert.Equal("Waiting Work", vm.WaitingCards[0].Title);
            Assert.Single(vm.DoneCards);
            Assert.Equal("Done Work", vm.DoneCards[0].Title);
        }

        [Fact]
        public void CardViewModel_ExposesExpectedProperties()
        {
            var card = MakeCard("Auth System", hasActiveSessions: true);
            var vm = new CardViewModel(card);

            Assert.Equal("Auth System", vm.Title);
            Assert.Equal("auth-system", vm.ProjectName);
            Assert.Equal(1, vm.SessionCount);
            Assert.Equal("Working on it", vm.LatestNote);
            Assert.False(vm.IsStale);
        }

        [Fact]
        public void CardViewModel_IsStale_WhenNoActivityFor24Hours()
        {
            var card = MakeCard("Old Work", hasActiveSessions: false);
            card.UpdatedAt = DateTime.UtcNow.AddHours(-25);
            var vm = new CardViewModel(card);

            Assert.True(vm.IsStale);
        }
    }
}
