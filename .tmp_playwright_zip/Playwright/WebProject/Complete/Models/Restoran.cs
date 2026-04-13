namespace Complete.Models;

public class Restoran
{
    [Key]
    public int ID { get; set; }
    // Lokacija
    public double X { get; set; }
    public double Y { get; set; }
    public required string Naziv { get; set; }
    //public string? TipHrane { get; set; }
    public double ZbirOcena { get; set; }
    public int BrojOcena { get; set; }
    [NotMapped]
    public double ProsecnaOcena
    {
        get
        {
            /*if (BrojOcena == 0)
            {
                return 0;
            }
            else
            {
                return ZbirOcena / BrojOcena;
            }*/
            return ZbirOcena / (BrojOcena == 0 ? 1 : BrojOcena);
        }
    }
    public double Prihodi { get; set; }
    public double Rashodi { get; set; }
    [NotMapped]
    public double Zarada
    {
        get
        {
            return Prihodi - Rashodi;
        }
    }
    public TipHrane? TipHrane { get; set; }
    [JsonIgnore]
    public Grad? Grad { get; set; }
    public List<Jelo>? Meni { get; set; }
    public List<Magacin>? Magacin { get; set; }
}