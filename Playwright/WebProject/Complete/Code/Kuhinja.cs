namespace Complete.Code;

public static class Kuhinja
{
    public static async Task<bool> ProveriDaLiMozeDaSePripremi(RestoraniContext context, Jelo jelo)
    {
        bool uspesno = true;

        if (jelo == null || jelo.Restoran == null || jelo.Restoran.Magacin == null || jelo.Sastojci == null)
        {
            return false;
        }

        var magacin = jelo.Restoran.Magacin.ToList();

        foreach (var s in jelo.Sastojci)
        {
            var m = magacin.Where(p => p.Sastojak?.ID == s.Sastojak?.ID).FirstOrDefault();
            
            if (s.Sastojak == null || m == null)
            {
                return false;
            }

            if (m.Sastojak?.RokTrajanja < DateTime.Now ||
                m.Kolicina < s.Kolicina)
            {
                // Naručiti, odmah smo dobili sastojak, pa ćemo da ga povećamo...
                
                if (m.Sastojak != null)
                {
                    m.Sastojak.RokTrajanja = DateTime.Now.AddYears(5);
                }

                m.Kolicina += 10 * s.Kolicina;
                uspesno = false;
            }
        }

        await context.SaveChangesAsync();

        return uspesno;
    }

    public static async Task<bool> PripremiJelo(RestoraniContext context, Jelo jelo, int brojRadnika, double cenaRadnika)
    {
        try
        {
            double cenaJela = 0;

            if (jelo == null || jelo.Restoran == null || jelo.Restoran.Magacin == null || jelo.Sastojci == null)
            {
                return false;
            }

            foreach (var sastojak in jelo.Sastojci)
            {
                var magacin = jelo.Restoran.Magacin.Where(p => p.Sastojak == sastojak.Sastojak).FirstOrDefault();
                
                if (magacin != null && sastojak != null && sastojak.Sastojak != null)
                {
                    cenaJela += sastojak.Kolicina * sastojak.Sastojak.Cena;
                    magacin.Kolicina -= sastojak.Kolicina;
                }
                else
                {
                    return false;
                }
            }

            jelo.Restoran.Rashodi += brojRadnika * cenaRadnika + cenaJela;
            // Cena jela se množi sa 1.2, zato što je 20% veća cena od sume sastojaka,
            // a na to se dodaje cena radnika koji su radili na pripremi i posluženju
            jelo.Restoran.Prihodi += brojRadnika * cenaRadnika + cenaJela * 1.2;

            context.Jelo.Update(jelo);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
