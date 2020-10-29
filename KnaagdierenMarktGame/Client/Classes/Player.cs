using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class Player : IEquatable<Player>
    {
        public string Name { get; set; }

        public List<int> MoneyCards { get; set; } = new List<int>();

        public List<AnimalCard> AnimalCards { get; set; } = new List<AnimalCard>();

        public Player(string name)
        {
            Name = name;
        }

        public bool Equals(Player other)
        {
            if (other == null) return false;
            return (Name.Equals(other.Name));
        }
    }
}
