using System;
using System.Collections.Generic;

namespace KnaagdierenMarktGame.Shared
{
    public class Player : IEquatable<Player>
    {
        public string Name { get; set; }

        public List<int> MoneyCards { get; set; } = new List<int>();

        public List<AnimalCard> AnimalCards { get; set; } = new List<AnimalCard>();

        public int AmountOfConnectionWarnings { get; set; } = 0;

        public string PeerID { get; set; }

        public Player(string name, string peerID)
        {
            Name = name;
            PeerID = peerID;
        }

        public bool Equals(Player other)
        {
            if (other == null) return false;
            return (Name.Equals(other.Name));
        }
    }
}
