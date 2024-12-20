using System;
using System.Diagnostics;

namespace ChatApp.Model
{
    struct Message
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public Message(string sender, string reciever, string content)
        {
            Sender = sender;
            Receiver = reciever;
            Content = content;
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
        }

        public Message(string deconstructed)
        {
            // Split the input string by the delimiter '~'
            string[] parts = deconstructed.Split('~');

            if (parts.Length >= 5)
            {
                Id = parts[0];  // Assuming Id is a string
                Sender = parts[1];  // Assuming Sender is a string
                Receiver = parts[2];  // Assuming Receiver is a string
                Content = parts[3];  // Assuming Content is a string
                Timestamp = DateTime.Parse(parts[4]);  // Assuming Timestamp is a DateTime
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
        }

        public override string ToString()
        {
            string product = "";
            product += Id + "~";
            product += Sender + "~";
            product += Receiver + "~";
            product += Content += "~";
            product += Timestamp.ToString();
            //Debug.WriteLine("Product in Message constructor: " + product);
            return product;
        }
    }
}