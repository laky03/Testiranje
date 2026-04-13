namespace Complete.Models;

public class Grad
{
    [Key]
    public int ID { get; set; }
    [MaxLength(50)]
    public required string Naziv { get; set; }
    [Range(1, 100000)]
    public double Povrsina { get; set; }
    [Range(1, 1000000)]
    public int BrojStanovnika { get; set; }

    public List<Restoran>? Restorani { get; set; }
}