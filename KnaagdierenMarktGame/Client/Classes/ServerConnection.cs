using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class ServerConnection
    {
        public HubConnection HubConnection { get; set; }

        public delegate Task NewServerMessage(Message message);

        public event NewServerMessage OnServerMessage;

        public ServerConnection(Uri baseAdress)
        {
            HubConnection = new HubConnectionBuilder()
            .WithUrl((new Uri(baseAdress, "/gamehub")).AbsoluteUri)
            .Build();

            HubConnection.On<Message>("ReceiveMessage", (message) => OnServerMessage?.Invoke(message));
        }

        public async Task StartConnection()
        {
            await HubConnection.StartAsync();
        }
        public async Task EndConnection()
        {
            await HubConnection.StopAsync();
        }
    }
}
