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
        private static List<Group> _groups = new List<Group>();
        private static Dictionary<string, string> _usernameConnectionIds = new Dictionary<string, string>();
        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("ReceiveMessage", MessageType.InitGroups, _groups);
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(Message message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task CreateGroup(string groupName, string userName)
        {
            if (GetGroupByName(groupName) is null)
            {
                await AddReferencesToConnectionId(groupName, userName);
                Group newGroup = new Group() { Name = groupName, Members = { userName } };
                _groups.Add(newGroup);

                await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.NewGroup, Objects = { newGroup } });
            }
        }

        public async Task JoinGroup(string groupName, string userName)
        {
            if (GetGroupByName(groupName) is Group group)
            {
                await AddReferencesToConnectionId(groupName, userName);
                group.Members.Add(userName);

                await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.JoinedGroup, Objects = { group } });

                if (group.Members.Count >= 3)
                {
                    // temporary delay
                    Thread.Sleep(1000);
                    await StartGame(group.Name, group);
                }
            }
        }

        private async Task AddReferencesToConnectionId(string groupName, string userName)
        {
            if (!_usernameConnectionIds.ContainsKey(Context.ConnectionId))
            {
                _usernameConnectionIds.Add(Context.ConnectionId, userName);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        private static Group GetGroupByName(string groupName)
        {
            if (_groups.FirstOrDefault(group => group.Name == groupName) is Group group)
            {
                return group;
            }
            else
            {
                return null;
            }
        }

        private async Task StartGame(string groupName, Group changedGroup)
        {
            Random rnd = new Random();
            string randomlyChosenStartPlayer = changedGroup.Members[rnd.Next(0, changedGroup.Members.Count)];
            _groups.Remove(changedGroup);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.StartGame, Objects = { randomlyChosenStartPlayer } });
        }

        public async Task LeaveGroup(string user)
        {
            if (GetUserGroup(user) is Group userGroup)
            {
                userGroup.Members.Remove(user);
                await Clients.Others.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.LeftGroup, Objects = { user } } );
                if (userGroup.Members.Count < 1)
                {
                    await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.GroupDeleted, Objects = { userGroup } });
                    _groups.Remove(userGroup);
                }
            }
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public Group GetUserGroup(string userName)
        {
            foreach (var group in _groups)
            {
                if (group.Members.Any(user => user == userName))
                {
                    return group;
                }
            }
            return null;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_usernameConnectionIds.ContainsKey(Context.ConnectionId))
            {
                string leftUser = _usernameConnectionIds[Context.ConnectionId];
                Group leftUserGroup = GetUserGroup(leftUser);
                if (leftUserGroup != null)
                {
                    LeaveGroup(leftUser);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
