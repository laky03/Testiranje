namespace Complete.Code;

public record SastojakSaKolicinom(int SastojakID, int Kolicina);
public record JeloSaSastojcima(Jelo Jelo, List<SastojakSaKolicinom> Sastojci);