using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string user, string groupName)
        {
            if (!IsUserInAGroup(user))
            {
                User newUser = new User() { Name = user, Group = groupName.ToLower() };
                Group changedGroup;
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                if (Connections.Instance.Groups.FirstOrDefault(group => group.Name == groupName.ToLower()) is Group group)
                {
                    group.Members.Add(newUser);
                    changedGroup = group;
                }
                else
                {
                    Group newGroup = new Group() { Name = groupName.ToLower() };
                    newGroup.Members.Add(newUser);
                    Connections.Instance.Groups.Add(newGroup);
                    changedGroup = newGroup;
                }
                await Clients.All.SendAsync("ReceiveMessage", MessageType.GroupChanged, changedGroup);
            }
        }
        public async Task LeaveGroup(User user)
        {
            if (IsUserInAGroup(user.Name))
            {
                Group group = Connections.Instance.Groups.First(groupname => user.Group == groupname.Name);
                group.Members.Remove(user);
                await Clients.Others.SendAsync("ReceiveMessage", MessageType.LeavedGroup, user);
                if (group.Members.Count < 1)
                {
                    await Clients.All.SendAsync("ReceiveMessage", MessageType.GroupDeleted, group);
                    Connections.Instance.Groups.Remove(group);
                }
            }
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public bool IsUserInAGroup(string userName)
        {
            foreach (var group in Connections.Instance.Groups)
            {
                if (group.Members.Any(user => user.Name == userName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
