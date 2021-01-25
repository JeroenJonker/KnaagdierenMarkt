using System;
using System.Collections.Generic;

namespace KnaagdierenMarktGame.Shared
{
    public class Group : IEquatable<Group>
    {
        public string Name { get; set; }
        public List<Player> Members { get; set; } = new List<Player>();

        public bool Equals(Group other)
        {
            if (other == null) return false;
            return (Name.Equals(other.Name));
        }
    }
}
