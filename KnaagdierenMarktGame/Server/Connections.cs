using KnaagdierenMarktGame.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Server
{
    public sealed class Connections
    {
        private Connections()
        {

        }
        private static readonly Lazy<Connections> lazy = new Lazy<Connections>(() => new Connections());
        public static Connections Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public List<Group> Groups { get; set; } = new List<Group>();
    }
}
