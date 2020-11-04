using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Server.Hubs
{
    public class GameHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("ReceiveMessage", MessageType.InitGroups, Connections.Instance.Groups);
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(Message message)
        {
            await Clients.All.SendAsync("ReceiveGameMessage", message);
        }

        public async Task JoinGroup(Group changedGroup)
        {
            //if (GetUserGroup(user) == null)
            //{
            await Groups.AddToGroupAsync(Context.ConnectionId, changedGroup.Name);
            if (Connections.Instance.Groups.FirstOrDefault(group => group.Name == changedGroup.Name.ToLower()) is Group group)
            {
                group = changedGroup;
            }
            else
            {
                Connections.Instance.Groups.Add(changedGroup);
            }

            await Clients.All.SendAsync("ReceiveMessage", MessageType.GroupChanged, changedGroup);

            if (changedGroup.Members.Count >= 2)
            {
                // temporary delay
                Thread.Sleep(1000);
                await StartGame(changedGroup.Name, changedGroup);
            }
            //}
        }

        private async Task StartGame(string groupName, Group changedGroup)
        {
            Random rnd = new Random();
            string randomlyChosenStartPlayer = changedGroup.Members[rnd.Next(0, changedGroup.Members.Count)];
            await Clients.Group(groupName).SendAsync("ReceiveMessage", MessageType.StartGame, randomlyChosenStartPlayer);
        }

        public async Task LeaveGroup(string user)
        {
            if (GetUserGroup(user) is Group userGroup)
            {
                userGroup.Members.Remove(user);
                await Clients.Others.SendAsync("ReceiveMessage", MessageType.LeavedGroup, user);
                if (userGroup.Members.Count < 1)
                {
                    await Clients.All.SendAsync("ReceiveMessage", MessageType.GroupDeleted, userGroup);
                    Connections.Instance.Groups.Remove(userGroup);
                }
            }
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public Group GetUserGroup(string userName)
        {
            foreach (var group in Connections.Instance.Groups)
            {
                if (group.Members.Any(user => user == userName))
                {
                    return group;
                }
            }
            return null;
        }
    }
}
