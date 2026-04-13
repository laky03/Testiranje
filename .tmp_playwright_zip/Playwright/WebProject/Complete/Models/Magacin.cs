namespace Complete.Models;

public class Magacin
{
    [Key]
    public int ID { get; set; }
    public double Kolicina { get; set; }
    public Sastojak? Sastojak { get; set; }
    public Restoran? Restoran { get; set; }
}