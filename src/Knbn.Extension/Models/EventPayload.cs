using System.Text.Json.Serialization;

namespace Knbn.Extension.Models
{
    public class EventPayload
    {
        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("cwd")]
        public string Cwd { get; set; }

        [JsonPropertyName("card_id")]
        public string CardId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
