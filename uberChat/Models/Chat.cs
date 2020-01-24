using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uberChat.Models
{
    [Serializable]
    public class Chat
    {
        public int ChatId { get; set; }
        public string Name { get; set; }
        public List<User> ConnectedUsers { get; set; } = new List<User>();
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
