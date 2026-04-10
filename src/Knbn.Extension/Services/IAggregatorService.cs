using System.Collections.Generic;
using Knbn.Extension.Models;

namespace Knbn.Extension.Services
{
    public class EventResult
    {
        public string Outcome { get; set; }
        public string CardId { get; set; }
        public string Title { get; set; }
    }

    public interface IAggregatorService
    {
        EventResult HandleEvent(EventPayload payload);
        List<WorkItem> GetCards();
    }
}
