using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Perifericos.Server.Hubs;

[Authorize]
public class AlertsHub : Hub
{
}



