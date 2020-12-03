using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public class Group : IEquatable<Group>
    {
        public string Name { get; set; }
        public List<string> Members { get; set; } = new List<string>();

        public bool Equals(Group other)
        {
            if (other == null) return false;
            return (Name.Equals(other.Name));
        }
    }
}
