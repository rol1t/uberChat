using System;

namespace uberChat.Models
{
    [Serializable]
    public class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}