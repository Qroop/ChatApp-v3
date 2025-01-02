using System;
using System.Diagnostics;

namespace ChatApp.Model
{
    struct Message
    {
        public string Id { get; }
        public string Sender { get;}
        public string Receiver { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }
        public bool SentFromServer { get; }

        public Message(string sender, string reciever, string content, bool sentFromServer)
        {
            Sender = sender;
            Receiver = reciever;
            Content = content;
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            SentFromServer = sentFromServer;
        }

        public Message(string deconstructed)
        {
            // Split the input string by the delimiter '~'
            string[] parts = deconstructed.Split('~');

            if (parts.Length >= 6)
            {
                Id = parts[0];  
                Sender = parts[1];  
                Receiver = parts[2];  
                Content = parts[3];  
                Timestamp = DateTime.Parse(parts[4]);  
                SentFromServer = bool.Parse(parts[5]);
            }
            else
            {
                throw new ArgumentException("Invalid deconstructed string format.");
            }
        }

        public Message(Message other)
        {
            Id = other.Id;
            Sender = other.Sender;
            Receiver = other.Receiver;
            Content = other.Content;
            Timestamp = other.Timestamp;
            SentFromServer = other.SentFromServer;
        }

        public override string ToString()
        {
            string product = "";
            product += Id + "~";
            product += Sender + "~";
            product += Receiver + "~";
            product += Content + "~";
            product += Timestamp.ToString() + "~";
            product += SentFromServer.ToString();

            return product;
        }
    }
}