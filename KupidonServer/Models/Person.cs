namespace KupidonServer.Models;

// jedna prijavljena osoba. promenljivo stanje ide pod lock jer mu pristupaju
// i hub niti i kupidon nit
public class Person
{
    private readonly object _gate = new();
    private readonly HashSet<string> _blocked = new(StringComparer.OrdinalIgnoreCase);
    private bool _awaitingConfirmation;

    public required string Username { get; init; }
    public required string City { get; init; }
    public required int Age { get; init; }
    public required string Phone { get; init; }

    // konekcija preko koje joj kupidon salje pisma
    public required string ConnectionId { get; init; }

    // ako ne ceka potvrdu, oznaci je i vrati true -> ne dobija dva pisma odjednom
    public bool TryBeginDelivery()
    {
        lock (_gate)
        {
            if (_awaitingConfirmation) return false;
            _awaitingConfirmation = true;
            return true;
        }
    }

    // potvrdila prijem (ili nije bilo kandidata) -> sme dalje
    public void Release()
    {
        lock (_gate) { _awaitingConfirmation = false; }
    }

    public void BlockUser(string username)
    {
        lock (_gate) { _blocked.Add(username); }
    }

    public bool IsBlocked(string username)
    {
        lock (_gate) { return _blocked.Contains(username); }
    }
}
