using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class Friendship
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int friend_id { get; set; }
        public string status { get; set; } = "pending";
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }

}
