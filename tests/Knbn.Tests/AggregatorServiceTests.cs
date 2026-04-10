using System.Linq;
using Knbn.Extension.Models;
using Knbn.Extension.Services;
using Xunit;

namespace Knbn.Tests
{
    public class AggregatorServiceTests
    {
        [Fact]
        public void Register_CreatesNewCard_WhenNoMatchingTitle()
        {
            var svc = new AggregatorService();
            var result = svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "My Task",
                SessionId = "s1",
                Cwd = "/repo"
            });

            Assert.Equal("created", result.Outcome);
            Assert.NotNull(result.CardId);
            Assert.Equal("My Task", result.Title);

            var cards = svc.GetCards();
            Assert.Single(cards);
            Assert.Equal("My Task", cards[0].Title);
            Assert.Single(cards[0].Sessions);
            Assert.Equal("s1", cards[0].Sessions[0].SessionId);
        }

        [Fact]
        public void Register_JoinsExistingCard_WhenTitleMatchesCaseInsensitive()
        {
            var svc = new AggregatorService();

            var first = svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "My Task",
                SessionId = "s1",
                Cwd = "/repo"
            });

            var second = svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "MY TASK",
                SessionId = "s2",
                Cwd = "/repo2"
            });

            Assert.Equal("created", first.Outcome);
            Assert.Equal("joined", second.Outcome);
            Assert.Equal(first.CardId, second.CardId);

            var cards = svc.GetCards();
            Assert.Single(cards);
            Assert.Equal(2, cards[0].Sessions.Count);
        }

        [Fact]
        public void Register_CreatesNewCard_WhenTitleIsDifferent()
        {
            var svc = new AggregatorService();

            svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Task A",
                SessionId = "s1",
                Cwd = "/repo"
            });

            var result = svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Task B",
                SessionId = "s2",
                Cwd = "/repo"
            });

            Assert.Equal("created", result.Outcome);
            Assert.Equal(2, svc.GetCards().Count);
        }

        [Fact]
        public void Join_LinksSessionToExistingCard()
        {
            var svc = new AggregatorService();

            var created = svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Task A",
                SessionId = "s1",
                Cwd = "/repo"
            });

            var joined = svc.HandleEvent(new EventPayload
            {
                Action = "join",
                CardId = created.CardId,
                SessionId = "s2",
                Cwd = "/repo2"
            });

            Assert.Equal("joined", joined.Outcome);
            Assert.Equal(created.CardId, joined.CardId);

            var cards = svc.GetCards();
            Assert.Single(cards);
            Assert.Equal(2, cards[0].Sessions.Count);
            Assert.Equal("s2", cards[0].Sessions[1].SessionId);
        }

        [Fact]
        public void Join_ReturnsNotFound_WhenCardIdInvalid()
        {
            var svc = new AggregatorService();

            var result = svc.HandleEvent(new EventPayload
            {
                Action = "join",
                CardId = "nonexistent-id",
                SessionId = "s1",
                Cwd = "/repo"
            });

            Assert.Equal("not_found", result.Outcome);
        }

        [Fact]
        public void Status_AddsNoteToCard()
        {
            var svc = new AggregatorService();

            svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Task A",
                SessionId = "s1",
                Cwd = "/repo"
            });

            var result = svc.HandleEvent(new EventPayload
            {
                Action = "status",
                SessionId = "s1",
                Message = "Working on it"
            });

            Assert.Equal("updated", result.Outcome);

            var cards = svc.GetCards();
            Assert.Single(cards[0].Notes);
            Assert.Equal("Working on it", cards[0].Notes[0].Message);
            Assert.Equal("s1", cards[0].Notes[0].SessionId);
        }

        [Fact]
        public void SessionEnd_MarksSessionInactive()
        {
            var svc = new AggregatorService();

            svc.HandleEvent(new EventPayload
            {
                Action = "register",
                Title = "Task A",
                SessionId = "s1",
                Cwd = "/repo"
            });

            var result = svc.HandleEvent(new EventPayload
            {
                Event = "SessionEnd",
                SessionId = "s1"
            });

            Assert.Equal("ended", result.Outcome);

            var cards = svc.GetCards();
            Assert.False(cards[0].Sessions[0].Active);
            Assert.NotNull(cards[0].Sessions[0].EndedAt);
        }

        [Fact]
        public void SessionEnd_ReturnsNotFound_WhenSessionUnknown()
        {
            var svc = new AggregatorService();

            var result = svc.HandleEvent(new EventPayload
            {
                Event = "SessionEnd",
                SessionId = "unknown-session"
            });

            Assert.Equal("not_found", result.Outcome);
        }
    }
}
