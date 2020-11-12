using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public class Message
    {
        public MessageType MessageType { get; set; }
        public string Sender { get; set; }
        public string Target { get; set; }
        public List<object> Objects { get; set; } = new List<object>();

    }
}
