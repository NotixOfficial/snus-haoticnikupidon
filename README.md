# Haoticni kupidon

SNUS kolokvijum 2 - PubSub aplikacija u **ASP.NET Core (SignalR)** koja simulira
"haoticnog kupidona": server svakih minut salje prijavljenim osobama ljubavna
pisma na osnovu poklapanja (lokacija, godine) i nasumicnog faktora.

## Struktura (3 projekta)

- **KupidonContracts** - deljeni ugovori: 2 interfejsa (`IPersonService`,
  `ICupidClient`) i DTO-ovi (`PersonInfo`, `LetterDto`, `RegistrationResult`).
- **KupidonServer** - ASP.NET Core + SignalR hub + kupidon (pozadinska nit).
- **KupidonClient** - konzolni klijent (svaka instanca = jedna osoba).

## Pokretanje

Prvo server (sluzi na `http://localhost:5050`, hub `/cupid`):

```
dotnet run --project KupidonServer
```

Pa **jedan ili vise** klijenata (svaki u svom terminalu = po jedna osoba):

```
dotnet run --project KupidonClient
```

Za demo bez cekanja celog minuta, smanji interval kupidona:

```
Cupid__IntervalSeconds=5 dotnet run --project KupidonServer
```

## Kako se koristi (klijent)

1. Unesi `username`, `grad`, `godine`, `telefon` (uz validaciju unosa).
2. Cekaj pisma od kupidona.
3. Komande:
   - `/ok` - potvrdi prijem poslednjeg pisma (dok ne potvrdis, ne primas novo)
   - `/block <username>` - blokiraj korisnika (od njega vise ne primas pismo)
   - `/quit` - izlaz

## Algoritam kupidona (skoring)

Za svakog primaoca, za svakog kandidata (osim sebe i blokiranih):

| Uslov | Poeni |
|-------|-------|
| ista lokacija | +30 |
| slicne godine (±2) | +20 |
| nasumicni faktor (`RNGCryptoServiceProvider`) | +0-100 |

Pismo se salje od kandidata sa **najvecim** score-om. Poruka je nasumicna; ako je
"Nisam zainteresovan/a za upoznavanje." - **telefon se ne prikazuje**.

## Mapiranje na zahteve zadatka

| Zahtev | Gde |
|--------|-----|
| 2 interfejsa (osobe / kupidon) | `KupidonContracts/Interfaces.cs` |
| PubSub framework (ASP.NET Core) | SignalR hub `CupidHub` |
| `InitSinglePerson` + validacija | `CupidHub.InitSinglePerson`, klijentski helperi |
| kupidon svakih minut | `CupidService` (BackgroundService + PeriodicTimer) |
| skoring + nasumicni faktor | `MatchMaker` + `SecureRng` (RNGCryptoServiceProvider) |
| poruka, sakrivanje telefona | `MatchMaker.CreateLetter` |
| potvrda pre novog pisma | `Person.TryBeginDelivery/Release`, `/ok` |
| `/block username` | `CupidHub.Block`, `Person.BlockUser` |

Detaljnije u [OBJASNJENJE.md](OBJASNJENJE.md).
