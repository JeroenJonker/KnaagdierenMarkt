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
    }
}
