using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnaagdierenMarktGame.Server.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KnaagdierenMarktGame.Server
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        public List<string> GroupNames { get; set; } = new List<string>();

        private readonly IHubContext<GameHub> hubContext;
        public GameController(IHubContext<GameHub> context)
        {
            hubContext = context;
        }

        //[HttpPost]
        //public async Task SendMessage(string user, string message)
        //{
        //    await hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        //}

        //public async Task JoinGroup(string groupName)
        //{
        //    if (GroupNames.Contains(groupName))
        //    {
        //        GroupNames.Add(groupName);
        //    }
        //    await hubContext.Groups.AddToGroupAsync(((GameHub)hubContext).Context.ConnectionId, groupName);
        //}

        //public async Task SendMessageToGroup(string groupName, string user, string message)
        //{
        //    await hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        //}

        //[HttpGet]
        //public List<string> GetGroups()
        //{
        //    return Connections.Instance.Groups;
        //    //return Ok(GroupNames);
        //}

        //IActionResult
    }
}
