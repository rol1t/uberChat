using System;
using System.ComponentModel.DataAnnotations;

namespace uberChat.Models
{
    [Serializable]
    public class Message
    {
        [Required(AllowEmptyStrings = true)]
        public string GroupName { get; set; }

        public string Sender { get; set; }
        public string Content { get; set; }
    }
}