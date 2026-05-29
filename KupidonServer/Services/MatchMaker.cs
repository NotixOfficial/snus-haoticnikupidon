using KupidonContracts;
using KupidonServer.Models;

namespace KupidonServer.Services;

// racuna score i bira od koga stize pismo, pa sklopi pismo
public class MatchMaker
{
    private readonly SecureRng _rng;

    private static readonly string[] Messages =
    {
        "Radujem se nasem susretu!",
        "Zelim da se upoznamo.",
        "Nisam zainteresovan/a za upoznavanje."
    };
    private const string NotInterested = "Nisam zainteresovan/a za upoznavanje.";

    public MatchMaker(SecureRng rng) => _rng = rng;

    // null ako nema kandidata
    public Person? ChooseSender(Person recipient, IReadOnlyList<Person> all)
    {
        Person? best = null;
        int bestScore = int.MinValue;

        foreach (var c in all)
        {
            if (c.Username.Equals(recipient.Username, StringComparison.OrdinalIgnoreCase))
                continue;                          // ne sebi
            if (recipient.IsBlocked(c.Username))
                continue;                          // blokiran

            int score = 0;
            if (c.City.Equals(recipient.City, StringComparison.OrdinalIgnoreCase))
                score += 30;                       // isti grad
            if (Math.Abs(c.Age - recipient.Age) <= 2)
                score += 20;                       // bliske godine
            score += _rng.Next(0, 101);            // random 0-100

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }
        return best;
    }

    public LetterDto CreateLetter(Person sender)
    {
        string message = Messages[_rng.Next(0, Messages.Length)];
        bool showPhone = message != NotInterested;  // bez telefona ako nije zainteresovan

        return new LetterDto
        {
            FromUsername = sender.Username,
            FromCity = sender.City,
            FromAge = sender.Age,
            FromPhone = showPhone ? sender.Phone : null,
            Message = message,
            ShowPhone = showPhone
        };
    }
}
