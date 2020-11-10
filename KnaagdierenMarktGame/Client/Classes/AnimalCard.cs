using KnaagdierenMarktGame.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class AnimalCard : IEquatable<AnimalCard>
    {
        public AnimalTypes AnimalType { get; set; }

        public int Value { get; set; }

        public AnimalCard(AnimalTypes animal, int value = 0)
        {
            AnimalType = animal;
            Value = value;
        }
        public bool Equals(AnimalCard other)
        {
            if (other == null) return false;
            return (AnimalType.Equals(other.AnimalType));
        }
    }
}
