namespace Complete.Models;

public class TipHrane
{
    public int ID { get; set; }
    public string? Tip { get; set; }
    [ForeignKey("RestoranFK")]
    public Restoran? Restoran { get; set; }
}
