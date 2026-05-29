namespace KupidonContracts;

// podaci koje osoba unese pri prijavi
public class PersonInfo
{
    public string Username { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Phone { get; set; } = string.Empty;
}

// pismo koje kupidon salje osobi
public class LetterDto
{
    public string FromUsername { get; set; } = string.Empty;
    public string FromCity { get; set; } = string.Empty;
    public int FromAge { get; set; }
    public string? FromPhone { get; set; }   // null kad se telefon ne prikazuje
    public string Message { get; set; } = string.Empty;
    public bool ShowPhone { get; set; }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
