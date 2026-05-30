using KupidonContracts;
using KupidonServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KupidonServer.Services;

// kupidon - svakih minut posalje po jedno pismo svakom ko je potvrdio prethodno
public class CupidService : BackgroundService
{
    private readonly PersonRegistry _registry;
    private readonly MatchMaker _matchMaker;
    private readonly IHubContext<CupidHub, ICupidClient> _hub;
    private readonly ILogger<CupidService> _logger;
    private readonly TimeSpan _interval;

    public CupidService(
        PersonRegistry registry,
        MatchMaker matchMaker,
        IHubContext<CupidHub, ICupidClient> hub,
        ILogger<CupidService> logger,
        IConfiguration config)
    {
        _registry = registry;
        _matchMaker = matchMaker;
        _hub = hub;
        _logger = logger;

        // default 60s, moze da se smanji preko Cupid:IntervalSeconds za demo
        int seconds = config.GetValue("Cupid:IntervalSeconds", 60);
        _interval = TimeSpan.FromSeconds(seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kupidon krenuo, interval = {Interval}s", _interval.TotalSeconds);
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SendRoundAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greska u kupidon rundi");
            }
        }
    }

    private async Task SendRoundAsync()
    {
        var all = _registry.Snapshot();
        if (all.Count == 0) return;

        _logger.LogInformation("Kupidon runda: {Count} prijavljenih", all.Count);

        foreach (var recipient in all)
        {
            // preskoci ako jos nije potvrdio prethodno pismo
            if (!recipient.TryBeginDelivery())
                continue;

            var sender = _matchMaker.ChooseSender(recipient, all);
            if (sender == null)
            {
                recipient.Release(); // nema kandidata
                continue;
            }

            var letter = _matchMaker.CreateLetter(sender);
            try
            {
                await _hub.Clients.Client(recipient.ConnectionId).ReceiveLetter(letter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Slanje pisma osobi {User} nije uspelo", recipient.Username);
                recipient.Release(); // da ne ostane zaglavljeno
            }
        }
    }
}
