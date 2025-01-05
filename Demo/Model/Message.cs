using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp.Model
{
    public class Message
    {
        [JsonPropertyName("Request")]
        public string Request { get; set; }

        [JsonPropertyName("Sender")]
        public string Sender { get; set; }

        [JsonPropertyName("Receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("Content")]
        public string Content { get; set; }

        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("SentFromServer")]
        public bool SentFromServer { get; set; }

        public Message() { }

        public Message(string requestType, string sender, string reciever, string content, bool sentFromServer)
        {
            Request = requestType;
            Sender = sender;
            Receiver = reciever;
            Content = content;
            Timestamp = DateTime.Now;
            SentFromServer = sentFromServer;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }

        public static Message FromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }
    }
}