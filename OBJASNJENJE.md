# Haoticni kupidon - SNUS kolokvijum 2

pubsub aplikacija u asp.net core-u (signalr). server svaki minut salje prijavljenim
osobama ljubavna pisma, na osnovu poklapanja + malo srece.

## pokretanje
server:
```
dotnet run --project KupidonServer
```
klijent (pokreni vise puta, svaka instanca je jedna osoba):
```
dotnet run --project KupidonClient
```
server slusa na http://localhost:5050. za demo da ne cekas ceo minut:
```
Cupid__IntervalSeconds=5 dotnet run --project KupidonServer
```

## struktura
- **KupidonContracts** - 2 interfejsa: `IPersonService` (osobe -> server) i `ICupidClient`
  (kupidon -> osobe), plus dto-ovi
- **KupidonServer** - signalr hub (`CupidHub`) + kupidon kao background servis + registar osoba
- **KupidonClient** - konzola

## kako radi
- prijava preko `InitSinglePerson`: username, grad, godine, telefon (uz validaciju unosa)
- kupidon svaki minut svakome bira posiljaoca: isti grad +30, bliske godine (±2) +20,
  random 0-100 (`RNGCryptoServiceProvider`); salje onom sa najvecim skorom (ne sebi, ne blokiranom)
- poruka je nasumicna od tri; ako je "nisam zainteresovan/a" telefon se ne prikazuje
- ne stize novo pismo dok prethodno ne potvrdis sa `/ok`
- `/block username` da od nekog ne dobijas pisma

## par napomena
- registar i stanje osoba su thread-safe (`ConcurrentDictionary` + lock) jer mu pristupaju
  i hub niti i kupidon nit
- `RNGCryptoServiceProvider` je deprecated od .net 6 ali ga zadatak trazi (zamena bi bio
  `RandomNumberGenerator`)
