using System.ComponentModel.DataAnnotations;

namespace KupidonContracts;

// podaci koje osoba unese pri prijavi (anotacije sluze za server-side proveru)
public class PersonInfo
{
    [Required(ErrorMessage = "Username ne sme biti prazan.")]
    [RegularExpression(@"^[\p{L}0-9_]+$", ErrorMessage = "Username sme da sadrzi samo slova, cifre i _.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad ne sme biti prazan.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Grad sme da sadrzi samo slova i razmake.")]
    public string City { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Godine moraju biti broj izmedju 1 i 120.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Broj telefona ne sme biti prazan.")]
    [RegularExpression(@"^\+?[0-9]{6,15}$", ErrorMessage = "Telefon: 6-15 cifara, opciono vodece +.")]
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
