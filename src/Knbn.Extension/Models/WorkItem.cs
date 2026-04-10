using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Knbn.Extension.Models
{
    public enum WorkItemStatus
    {
        Active,
        Done
    }

    public class WorkItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public WorkItemStatus Status { get; set; } = WorkItemStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<Session> Sessions { get; set; } = new List<Session>();
        public List<Note> Notes { get; set; } = new List<Note>();

        public bool HasActiveSessions()
        {
            foreach (var session in Sessions)
            {
                if (session.Active) return true;
            }
            return false;
        }

        public string ProjectName()
        {
            if (Sessions.Count == 0) return "";
            var cwd = Sessions[Sessions.Count - 1].Cwd;
            if (string.IsNullOrEmpty(cwd)) return "";
            var parts = cwd.Replace('\\', '/').TrimEnd('/').Split('/');
            return parts[parts.Length - 1];
        }
    }

    public class Session
    {
        public string SessionId { get; set; }
        public string Cwd { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        public bool Active { get; set; } = true;
    }

    public class Note
    {
        public string Message { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
