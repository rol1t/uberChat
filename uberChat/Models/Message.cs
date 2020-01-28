using System;
using System.ComponentModel.DataAnnotations;

namespace uberChat.Models
{
    [Serializable]
    public class Message
    {
        [Required(AllowEmptyStrings = true)]
        public string GroupName { get; set; }

        //message in same group have unique id, but message in other group can have equal id
        public int Id { get; set; }

        public string Sender { get; set; }
        public string Content { get; set; }
    }
}