using System;
using System.Collections.Generic;
using System.Linq;
using Knbn.Extension.Models;

namespace Knbn.Extension.Services
{
    public class AggregatorService : IAggregatorService
    {
        private readonly List<WorkItem> _cards = new List<WorkItem>();
        private readonly object _lock = new object();

        public event Action CardsChanged;

        public EventResult HandleEvent(EventPayload payload)
        {
            lock (_lock)
            {
                if (payload.Action == "register")
                    return HandleRegister(payload);
                if (payload.Action == "join")
                    return HandleJoin(payload);
                if (payload.Action == "status")
                    return HandleStatus(payload);
                if (payload.Event == "SessionEnd")
                    return HandleSessionEnd(payload);

                return new EventResult { Outcome = "unknown" };
            }
        }

        public List<WorkItem> GetCards()
        {
            lock (_lock)
            {
                return _cards.ToList();
            }
        }

        public void SetCardStatus(string cardId, WorkItemStatus status)
        {
            lock (_lock)
            {
                var card = _cards.FirstOrDefault(c => c.Id == cardId);
                if (card != null)
                {
                    card.Status = status;
                    card.UpdatedAt = DateTime.UtcNow;
                    CardsChanged?.Invoke();
                }
            }
        }

        private EventResult HandleRegister(EventPayload payload)
        {
            var existing = _cards.FirstOrDefault(c =>
                string.Equals(c.Title, payload.Title, StringComparison.OrdinalIgnoreCase)
                && c.Status == WorkItemStatus.Active);

            if (existing != null)
            {
                existing.Sessions.Add(new Session
                {
                    SessionId = payload.SessionId,
                    Cwd = payload.Cwd
                });
                existing.UpdatedAt = DateTime.UtcNow;
                CardsChanged?.Invoke();
                return new EventResult { Outcome = "joined", CardId = existing.Id, Title = existing.Title };
            }

            var card = new WorkItem { Title = payload.Title };
            card.Sessions.Add(new Session
            {
                SessionId = payload.SessionId,
                Cwd = payload.Cwd
            });
            _cards.Add(card);
            CardsChanged?.Invoke();
            return new EventResult { Outcome = "created", CardId = card.Id, Title = card.Title };
        }

        private EventResult HandleJoin(EventPayload payload)
        {
            var card = _cards.FirstOrDefault(c => c.Id == payload.CardId);
            if (card == null)
                return new EventResult { Outcome = "not_found" };

            card.Sessions.Add(new Session
            {
                SessionId = payload.SessionId,
                Cwd = payload.Cwd
            });
            card.UpdatedAt = DateTime.UtcNow;
            CardsChanged?.Invoke();
            return new EventResult { Outcome = "joined", CardId = card.Id, Title = card.Title };
        }

        private EventResult HandleStatus(EventPayload payload)
        {
            var card = _cards.FirstOrDefault(c =>
                c.Sessions.Any(s => s.SessionId == payload.SessionId && s.Active));

            if (card == null)
                return new EventResult { Outcome = "not_found" };

            card.Notes.Add(new Note
            {
                Message = payload.Message,
                SessionId = payload.SessionId
            });
            card.UpdatedAt = DateTime.UtcNow;
            CardsChanged?.Invoke();
            return new EventResult { Outcome = "updated", CardId = card.Id, Title = card.Title };
        }

        private EventResult HandleSessionEnd(EventPayload payload)
        {
            var affected = new List<string>();
            foreach (var card in _cards)
            {
                foreach (var session in card.Sessions)
                {
                    if (session.SessionId == payload.SessionId && session.Active)
                    {
                        session.Active = false;
                        session.EndedAt = DateTime.UtcNow;
                        card.UpdatedAt = DateTime.UtcNow;
                        affected.Add(card.Id);
                    }
                }
            }

            if (affected.Count > 0)
                CardsChanged?.Invoke();

            return new EventResult { Outcome = affected.Count > 0 ? "ended" : "not_found" };
        }
    }
}
