namespace Complete.Models;

public class JeloSastojak
{
    [Key]
    public int ID { get; set; }
    public double Kolicina { get; set; }
    public Jelo? Jelo { get; set; }
    public Sastojak? Sastojak { get; set; }
}