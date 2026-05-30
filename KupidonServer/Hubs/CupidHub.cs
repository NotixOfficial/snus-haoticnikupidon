using KupidonContracts;
using KupidonServer.Models;
using KupidonServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace KupidonServer.Hubs;

// hub = servis za osobe (IPersonService). Hub<ICupidClient> da server moze type-safe da zove klijenta
public class CupidHub : Hub<ICupidClient>, IPersonService
{
    private readonly PersonRegistry _registry;

    public CupidHub(PersonRegistry registry) => _registry = registry;

    // prijava osobe (klijent vec validira, ovo je jos jedna provera)
    public async Task<RegistrationResult> InitSinglePerson(PersonInfo info)
    {
        string? error = Validate(info);
        if (error != null)
            return new RegistrationResult { Success = false, Message = error };

        var person = new Person
        {
            Username = info.Username.Trim(),
            City = info.City.Trim(),
            Age = info.Age,
            Phone = info.Phone.Trim(),
            ConnectionId = Context.ConnectionId
        };

        if (!_registry.TryAdd(person))
            return new RegistrationResult
            {
                Success = false,
                Message = $"Username '{person.Username}' je vec zauzet, probaj drugi."
            };

        await Clients.Caller.ReceiveMessage(
            $"Prijava uspesna. Cekaj pisma od kupidona (stizu svakih minut).");

        return new RegistrationResult
        {
            Success = true,
            Message = $"Dobrodosao/la, {person.Username}!"
        };
    }

    // potvrda prijema -> sme da primi novo
    public Task ConfirmReceived()
    {
        _registry.GetByConnection(Context.ConnectionId)?.Release();
        return Task.CompletedTask;
    }

    // blokiranje korisnika
    public async Task<bool> Block(string username)
    {
        var person = _registry.GetByConnection(Context.ConnectionId);
        if (person == null || string.IsNullOrWhiteSpace(username))
            return false;

        person.BlockUser(username.Trim());
        await Clients.Caller.ReceiveMessage($"Blokirao/la si korisnika '{username.Trim()}'.");
        return true;
    }

    // kad se klijent diskonektuje, izbaci ga iz registra
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _registry.RemoveByConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    private static string? Validate(PersonInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.Username)) return "Username ne sme biti prazan.";
        if (string.IsNullOrWhiteSpace(info.City)) return "Grad ne sme biti prazan.";
        if (info.Age <= 0) return "Godine moraju biti pozitivan broj.";
        if (string.IsNullOrWhiteSpace(info.Phone)) return "Broj telefona ne sme biti prazan.";
        return null;
    }
}
