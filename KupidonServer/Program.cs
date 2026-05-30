using KupidonServer.Hubs;
using KupidonServer.Services;

var builder = WebApplication.CreateBuilder(args);

// fiksiraj port 5050 (da ne zavisi od launchSettings/env/podrazumevanog 5000)
builder.WebHost.ConfigureKestrel(options => options.ListenLocalhost(5050));

// signalr + servisi
builder.Services.AddSignalR();
builder.Services.AddSingleton<PersonRegistry>();
builder.Services.AddSingleton<SecureRng>();
builder.Services.AddSingleton<MatchMaker>();
builder.Services.AddHostedService<CupidService>();

var app = builder.Build();

app.MapHub<CupidHub>("/cupid");
app.MapGet("/", () => "Haoticni kupidon server radi. SignalR hub: /cupid");

app.Run();
