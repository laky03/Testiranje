namespace Complete.Models;

public class RestoraniContext : DbContext
{
    public required DbSet<Grad> Gradovi { get; set; }
    public required DbSet<Restoran> Restorani { get; set; }
    public required DbSet<JeloSastojak> Recept { get; set; }
    public required DbSet<Jelo> Jelo { get; set; }
    public required DbSet<Sastojak> Sastojci { get; set; }

    public RestoraniContext(DbContextOptions options) : base(options)
    {
        
    }
}