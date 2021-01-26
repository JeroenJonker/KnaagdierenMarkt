using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Server.Hubs
{
    public class GatheringHub : Hub
    {
        private static List<Group> _groups = new List<Group>();
        private static Dictionary<string, KeyValuePair<string, Group>> _connectionIdToUsernamesAndGroups = new Dictionary<string, KeyValuePair<string, Group>>();
        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.InitGroups, Objects = { _groups } });
            return base.OnConnectedAsync();
        }

        public async Task CreateGroup(string groupName, string userName, string peerID)
        {
            if (GetGroupByName(groupName) is null)
            {
                Group newGroup = new Group() { Name = groupName, Members = { new Player(userName, peerID) } };
                _groups.Add(newGroup);
                await AddReferencesToConnectionId(newGroup, userName);

                await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.NewGroup, Objects = { newGroup } });
            }
        }

        public async Task JoinGroup(string groupName, string userName, string peerID)
        {
            if (GetGroupByName(groupName) is Group group)
            {
                group.Members.Add(new Player(userName, peerID ));
                await AddReferencesToConnectionId(group, userName);

                await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.JoinedGroup, Objects = { group } });

                if (group.Members.Count >= Constants.MaxPlayers)
                {
                    // temporary delay
                    Thread.Sleep(1000);
                    await StartGame(group.Name, group);
                }
            }
        }

        private async Task AddReferencesToConnectionId(Group group, string userName)
        {
            if (!_connectionIdToUsernamesAndGroups.ContainsKey(Context.ConnectionId))
            {
                _connectionIdToUsernamesAndGroups.Add(Context.ConnectionId, new KeyValuePair<string, Group>(userName,group));
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, group.Name);
        }

        private static Group GetGroupByName(string groupName) => _groups.Find(group => group.Name == groupName);

        private async Task StartGame(string groupName, Group changedGroup)
        {
            List<Player> playerOrder = GetPlayerOrder(new List<Player>(changedGroup.Members)).ToList();
            await Clients.Group(groupName).SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.StartGame, Objects = { playerOrder } });
        }

        public IEnumerable<Player> GetPlayerOrder(List<Player> remainingPlayers)
        {
            Random rnd = new Random();
            while (remainingPlayers.Count() > 0)
            {
                Player chosenPlayer = remainingPlayers[rnd.Next(0, remainingPlayers.Count)];
                remainingPlayers.Remove(chosenPlayer);
                yield return chosenPlayer;
            }
        }
        public async Task LeaveGroup(string user, string groupname)
        {
            Group userGroup = _groups.Find(group => group.Name == groupname);
            await RemoveUserFromGroup(user, userGroup);
        }

        public async Task RemoveUserFromGroup(string user, Group userGroup)
        {
            userGroup = _groups.Find(group => group.Name == userGroup.Name); 
            Player leftplayer = userGroup.Members.Find(player => player.Name == user);
            userGroup.Members.Remove(leftplayer);
            await Clients.Others.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.LeftGroup, Objects = { user } } );
            if (userGroup.Members.Count < 1)
            {
                await Clients.All.SendAsync("ReceiveMessage", new Message() { MessageType = MessageType.GroupDeleted, Objects = { userGroup } });
                _groups.Remove(userGroup);
            }
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connectionIdToUsernamesAndGroups.ContainsKey(Context.ConnectionId))
            {
                KeyValuePair<string, Group> leftUser = _connectionIdToUsernamesAndGroups[Context.ConnectionId];
                await RemoveUserFromGroup(leftUser.Key, leftUser.Value);
                _connectionIdToUsernamesAndGroups.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
