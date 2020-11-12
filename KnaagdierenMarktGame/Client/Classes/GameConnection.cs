using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class GameConnection
    {
        public HubConnection HubConnection { get; set; }

        public delegate Task NewMessage(MessageType messageType, object message);

        public event NewMessage OnNewMessage;

        //temporary
        public delegate Task NewGameMessage(Message message);

        public event NewGameMessage OnNewGameMessage;

        public GameConnection(Uri baseAdress)
        {
            HubConnection = new HubConnectionBuilder()
            .WithUrl((new Uri(baseAdress, "/gamehub")).AbsoluteUri)
            .Build();

            HubConnection.On<MessageType, object>("ReceiveMessage", (messageType, message) => OnNewMessage?.Invoke(messageType, message));
            //temporary
            HubConnection.On<Message>("ReceiveGameMessage", (message) => OnNewGameMessage?.Invoke(message));
        }

        public async Task StartConnection()
        {
            await HubConnection.StartAsync();
        }
    }
}
