namespace Complete.Models;

public class Jelo
{
    [Key]
    public int ID { get; set; }
    public required string Naziv { get; set; }
    public string? Slika { get; set; }
    public int KalorijskaVrednost { get; set; }
    public string? Tip { get; set; }
    public bool DaLiJeJelo { get; set; }
    // Ukoliko isto Jelo može da se pravi u više restorana
    // public List<Restoran>? Restorani { get; set; }
    // Veza više na više gde se tabela kreira za nas od strane EF
    [JsonIgnore]
    public Restoran? Restoran { get; set; }
    public List<JeloSastojak>? Sastojci { get; set; }
}