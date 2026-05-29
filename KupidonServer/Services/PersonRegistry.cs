using System.Collections.Concurrent;
using KupidonServer.Models;

namespace KupidonServer.Services;

// registar prijavljenih, kljuc je username + pomocna mapa connectionId -> username
public class PersonRegistry
{
    private readonly ConcurrentDictionary<string, Person> _byUsername =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _connToUser = new();

    // false ako je username vec zauzet
    public bool TryAdd(Person person)
    {
        if (!_byUsername.TryAdd(person.Username, person))
            return false;
        _connToUser[person.ConnectionId] = person.Username;
        return true;
    }

    public Person? GetByConnection(string connectionId)
        => _connToUser.TryGetValue(connectionId, out var user)
           && _byUsername.TryGetValue(user, out var p)
            ? p : null;

    public Person? GetByUsername(string username)
        => _byUsername.TryGetValue(username, out var p) ? p : null;

    // pri diskonekciji izbaci osobu
    public void RemoveByConnection(string connectionId)
    {
        if (_connToUser.TryRemove(connectionId, out var user))
            _byUsername.TryRemove(user, out _);
    }

    // kopija da kupidon ne drzi registar zakljucan dok salje
    public IReadOnlyList<Person> Snapshot() => _byUsername.Values.ToList();
}
