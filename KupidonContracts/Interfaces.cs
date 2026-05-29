namespace KupidonContracts;

// dva interfejsa: jedan za osobe (klijent -> server), jedan za kupidona (server -> klijent)
// u sustini wcf duplex (servicecontract + callbackcontract) prebacen na signalr

// sta osoba poziva na serveru
public interface IPersonService
{
    Task<RegistrationResult> InitSinglePerson(PersonInfo info);
    Task ConfirmReceived();              // potvrda da je primljeno prethodno pismo
    Task<bool> Block(string username);
}

// kako kupidon (server) dostavlja osobi
public interface ICupidClient
{
    Task ReceiveLetter(LetterDto letter);
    Task ReceiveMessage(string message); // sistemske poruke
}
