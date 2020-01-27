using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uberChat.Models
{
    [Serializable]
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public int? ChatId { get; set; }
        public List<Message> MessageBox { get; set; } = new List<Message>();
    }
}
