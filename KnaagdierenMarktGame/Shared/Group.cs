using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public class Group
    {
        public string Name { get; set; }
        public List<string> Members { get; set; } = new List<string>();
    }
}
