using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uberChat.Models
{
    [Serializable]
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        //public string Password { get; set; }

        [Required(AllowEmptyStrings = true)]

        public string CurrentGroup { get; set; }
        public List<string> GroupId { get; set; } = new List<string>();
    }
}
