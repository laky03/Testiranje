# SplitSync

SplitSync je ASP.NET Core MVC web aplikacija za organizaciju troškova i aktivnosti unutar grupa. Projekat omogućava korisnicima da kreiraju grupe, pozivaju članove, dele račune, dobiju predlog uplata, prave ankete, organizuju događaje, vode shopping listu i komuniciraju kroz grupni chat.

## Tehnologije

- ASP.NET Core MVC (`net8.0`)
- Entity Framework Core
- PostgreSQL
- Cookie autentifikacija
- Google OAuth prijava
- SMTP slanje email poruka

## Glavne funkcionalnosti

- Registracija i prijava korisnika
- Verifikacija email adrese kodom
- Reset lozinke putem email koda
- Google login
- Izmena profila i profilne slike
- Kreiranje grupa i slanje pozivnica
- Upravljanje članovima, admin privilegijama i nadimcima u grupi
- Dodavanje i pregled računa po grupama
- Automatski obračun i predlog uplata između članova
- Kreiranje anketa i glasanje
- Kreiranje događaja i RSVP glasanje
- Grupna shopping lista
- Grupni chat
- Home feed sa najnovijim aktivnostima iz svih korisnikovih grupa
- API rute za dohvat novih poruka, događaja, računa i pozivnica

## Struktura projekta

- `SplitSync/Controllers` - MVC kontroleri i API endpointi
- `SplitSync/Views` - Razor view fajlovi
- `SplitSync/Entities` - entiteti baze
- `SplitSync/Models` - view modeli i DTO klase
- `SplitSync/Data` - `AppDbContext` i EF konfiguracija
- `SplitSync/Services` - email servis, hashovanje lozinke i logika za predlog uplata
- `SplitSync/Migrations` - EF Core migracije
- `SplitSync/wwwroot` - statički resursi

## Zahtevi

Pre pokretanja potrebno je da imaš:

- .NET 8 SDK
- PostgreSQL server
- validan SMTP nalog za slanje email poruka
- Google OAuth kredencijale ako želiš Google prijavu

## Konfiguracija

Podešavanja se nalaze u `SplitSync/appsettings.json`.

Obavezno izmeni sledeće vrednosti:

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=splitsync;Username=postgres;Password=YOUR_DB_PASSWORD"
},
"GoogleAuth": {
  "ClientId": "YOUR_GOOGLE_CLIENT_ID",
  "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
  "RedirectUri": "https://localhost:7118/GoogleAuth/Callback"
},
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "YOUR_EMAIL",
  "Password": "YOUR_EMAIL_PASSWORD",
  "From": "FSL SplitSync <your@email.com>"
}
```

Ako ne koristiš Google prijavu, aplikacija će raditi i bez nje, ali `GoogleAuth` sekcija treba da bude pravilno podešena pre korišćenja te opcije.

## Baza podataka

Projekat koristi Entity Framework Core migracije koje se automatski primenjuju pri pokretanju aplikacije:

```csharp
db.Database.Migrate();
```

To znači da je dovoljno da baza postoji i da connection string bude ispravan. Tabele će biti kreirane ili ažurirane pri startu aplikacije.

## Pokretanje projekta

Iz root foldera projekta pokreni:

```bash
dotnet restore
dotnet run --project SplitSync
```

Development profil koristi sledeće adrese:

- `https://localhost:7118`
- `http://localhost:5118`

## Autentifikacija

Aplikacija koristi cookie autentifikaciju. Neautorizovani korisnici se preusmeravaju na:

```text
/Account/Login
```

Podržani tokovi:

- klasična registracija i prijava
- email verifikacija
- zaboravljena lozinka / reset lozinke
- Google OAuth prijava

## Napomene

- Profilne slike i slike grupa/događaja čuvaju se u bazi kao `byte[]`.
- Aplikacija koristi session state za deo Google login toka.
- Veći deo interfejsa i validacionih poruka je na srpskom jeziku.
- U repozitorijumu se već nalaze migracije, pa nije potrebno praviti nove za inicijalno pokretanje.

## Rešenje

Solution fajl:

- `SplitSync.sln`

Glavni web projekat:

- `SplitSync/SplitSync.csproj`
