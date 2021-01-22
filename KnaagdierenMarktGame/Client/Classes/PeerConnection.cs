using KnaagdierenMarktGame.Shared;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class PeerConnection
    {
        public delegate Task NewGameMessage(Message message);

        public event NewGameMessage OnNewGameMessage;

        private static Action<Message> OnNewMessage;

        private static Action<string> OnSetPeerID;

        public string PeerID { get; set; }

        private readonly IJSRuntime JSRuntime;

        public PeerConnection(IJSRuntime jSRuntime)
        {
            JSRuntime = jSRuntime;
            OnNewMessage = (message) => OnNewGameMessage?.Invoke(message);
            OnSetPeerID = (peerID) => PeerID = peerID;
        }

        public void StartConnections(List<Player> players)
        {
            foreach (Player player in players)
            {
                JSRuntime.InvokeVoidAsync("join", player.PeerID);
            }
        }

        public async Task SendMessageToPeers(Message message, bool IsAlsoInternalMessage = true)
        {
            //System.Diagnostics.Debug.WriteLine("message: " + message.MessageType + "from SENDMESSAGETOPEER");
            await JSRuntime.InvokeVoidAsync("sendMessageToAllPeers", message);
            if (IsAlsoInternalMessage)
            {
                OnNewGameMessage?.Invoke(message);
            }
        }

        [JSInvokable]
        public static void UpdateMessageCaller(Message message) => OnNewMessage.Invoke(message);

        [JSInvokable]
        public static void SetPeerID(string peerID) => OnSetPeerID.Invoke(peerID);

    }
}
