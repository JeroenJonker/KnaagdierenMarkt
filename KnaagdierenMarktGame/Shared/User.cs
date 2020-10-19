using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public class User : IEquatable<User>
    {
        public string Name { get; set; }

        public string Group { get; set; }

        public bool Equals(User other)
        {
            if (other == null) return false;
            return (Name.Equals(other.Name));
        }
    }
}
