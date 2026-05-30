using KupidonContracts;
using Microsoft.AspNetCore.SignalR.Client;

const string Url = "http://localhost:5050/cupid";

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Haoticni kupidon - klijent ===");

var connection = new HubConnectionBuilder()
    .WithUrl(Url)
    .Build();

bool awaitingConfirm = false;   // imamo li pismo koje ceka /ok
bool quitting = false;          // namerno izlazimo (da Closed ne javi gresku)

// stiglo pismo
connection.On<LetterDto>(nameof(ICupidClient.ReceiveLetter), letter =>
{
    Console.WriteLine();
    Console.WriteLine("========== NOVO PISMO ==========");
    Console.WriteLine($"  Od:      {letter.FromUsername}");
    Console.WriteLine($"  Grad:    {letter.FromCity}");
    Console.WriteLine($"  Godine:  {letter.FromAge}");
    if (letter.ShowPhone && letter.FromPhone is not null)
        Console.WriteLine($"  Telefon: {letter.FromPhone}");
    Console.WriteLine($"  Poruka:  \"{letter.Message}\"");
    Console.WriteLine("--------------------------------");
    Console.WriteLine("  Ukucaj /ok da potvrdis prijem (dok ne potvrdis, ne primas nova pisma).");
    Console.WriteLine("================================");
    awaitingConfirm = true;
});

// poruka sa servera
connection.On<string>(nameof(ICupidClient.ReceiveMessage), msg =>
{
    Console.WriteLine($"[SERVER] {msg}");
});

// ne koristim auto-reconnect: posle reconnecta dobijes nov ConnectionId pa bi server
// slao na stari -> izgubljena pisma. radije javim da je veza pala i izadjem
connection.Closed += _ =>
{
    if (!quitting)
    {
        Console.WriteLine("\n[VEZA] Izgubljena veza sa serverom. Pokreni klijenta ponovo da se vratis.");
        Environment.Exit(1);
    }
    return Task.CompletedTask;
};

// povezi se
try
{
    await connection.StartAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Ne mogu da se povezem na server ({Url}).");
    Console.WriteLine($"Pokreni prvo KupidonServer, pa onda klijenta. Detalj: {ex.Message}");
    return;
}
Console.WriteLine("Povezan na server.\n");

// prijava (sa validacijom unosa)
while (true)
{
    var info = new PersonInfo
    {
        Username = ReadNonEmpty("Username: "),
        City = ReadNonEmpty("Grad: "),
        Age = ReadPositiveInt("Godine: "),
        Phone = ReadPhone("Broj telefona: ")
    };

    var result = await connection.InvokeAsync<RegistrationResult>(
        nameof(IPersonService.InitSinglePerson), info);

    Console.WriteLine(result.Message);
    if (result.Success) break;
    Console.WriteLine("Probaj ponovo.\n");
}

// komande
Console.WriteLine();
Console.WriteLine("Komande:");
Console.WriteLine("  /ok                potvrdi prijem poslednjeg pisma");
Console.WriteLine("  /block <username>  blokiraj korisnika (ne primas pisma od njega)");
Console.WriteLine("  /quit              izlaz");
Console.WriteLine();

while (true)
{
    string? line = Console.ReadLine();
    if (line is null) break;
    line = line.Trim();
    if (line.Length == 0) continue;

    if (line.Equals("/quit", StringComparison.OrdinalIgnoreCase))
    {
        quitting = true;
        break;
    }

    if (line.Equals("/ok", StringComparison.OrdinalIgnoreCase))
    {
        if (!awaitingConfirm)
        {
            Console.WriteLine("Nema pisma koje ceka potvrdu.");
            continue;
        }
        await connection.InvokeAsync(nameof(IPersonService.ConfirmReceived));
        awaitingConfirm = false;
        Console.WriteLine("Prijem potvrdjen. Sada mozes da primis novo pismo.");
        continue;
    }

    if (line.StartsWith("/block ", StringComparison.OrdinalIgnoreCase))
    {
        string target = line["/block ".Length..].Trim();
        if (target.Length == 0)
        {
            Console.WriteLine("Upotreba: /block <username>");
            continue;
        }
        await connection.InvokeAsync<bool>(nameof(IPersonService.Block), target);
        continue;
    }

    Console.WriteLine("Nepoznata komanda. Dostupno: /ok, /block <username>, /quit");
}

quitting = true;
await connection.DisposeAsync();
Console.WriteLine("Dovidjenja!");


// helperi za unos
static string ReadNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(s))
            return s.Trim();
        Console.WriteLine("  Greska: polje ne sme biti prazno.");
    }
}

static int ReadPositiveInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s))
        {
            Console.WriteLine("  Greska: nisi nista uneo/la.");
            continue;
        }
        if (!int.TryParse(s.Trim(), out int value))
        {
            Console.WriteLine("  Greska: dozvoljeni su samo brojevi (ne karakteri).");
            continue;
        }
        if (value <= 0)
        {
            Console.WriteLine("  Greska: broj mora biti pozitivan.");
            continue;
        }
        return value;
    }
}

static string ReadPhone(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string? s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s))
        {
            Console.WriteLine("  Greska: nisi nista uneo/la.");
            continue;
        }
        s = s.Trim();
        if (s.StartsWith('-'))
        {
            Console.WriteLine("  Greska: broj telefona ne sme biti negativan.");
            continue;
        }
        // dozvoli + na pocetku i cifre
        string digits = s.StartsWith('+') ? s[1..] : s;
        if (digits.Length == 0 || !digits.All(char.IsDigit))
        {
            Console.WriteLine("  Greska: telefon sme da sadrzi samo cifre (uz opciono vodece +).");
            continue;
        }
        return s;
    }
}
