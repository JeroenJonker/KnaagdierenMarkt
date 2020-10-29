using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using KnaagdierenMarktGame.Client.Classes;

namespace KnaagdierenMarktGame.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            Uri baseAdress = new Uri(builder.HostEnvironment.BaseAddress);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAdress });

            builder.Services.AddSingleton(sp => new GameConnection(baseAdress));
            builder.Services.AddSingleton<GameState>();
            //builder.Services.AddSingleton(sp => new GameConnection(baseAdress));
            //builder.Services.AddSingleton(sp => new GameState());

            await builder.Build().RunAsync();
        }
    }
}
