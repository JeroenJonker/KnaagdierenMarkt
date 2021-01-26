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
    public class GatheringController : ControllerBase
    {
        private readonly IHubContext<GatheringHub> hubContext;
        public GatheringController(IHubContext<GatheringHub> context)
        {
            hubContext = context;
        }
    }
}
