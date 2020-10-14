using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public class Group
    {
        public string Name { get; set; }
        public List<User> Members { get; set; } = new List<User>();
    }
}
