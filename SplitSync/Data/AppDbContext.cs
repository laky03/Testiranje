using Microsoft.EntityFrameworkCore;
using SplitSync.Entities;

namespace SplitSync.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; } = null!;
        public DbSet<PasswordReset> PasswordResets { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupsUsers> GroupsUsers { get; set; } = null!;
        public DbSet<GroupInvitation> GroupInvitations { get; set; } = null!;

        public DbSet<Racun> Racuns { get; set; } = null!;
        public DbSet<RacunItem> RacunItems { get; set; } = null!;

        public DbSet<Anketa> Anketas { get; set; } = null!;
        public DbSet<AnketaOption> AnketaOptions { get; set; } = null!;
        public DbSet<AnketaAnswer> AnketaAnswers { get; set; } = null!;
        public DbSet<AnketaAnswerOption> AnketaAnswerOptions { get; set; } = null!;

        public DbSet<Dogadjaj> Dogadjaji { get; set; } = null!;
        public DbSet<DogadjajGlas> DogadjajGlasovi { get; set; } = null!;

        public DbSet<ShoppingListaItem> ShoppingListaItems { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);

                e.Property(x => x.Username).HasMaxLength(100).IsRequired();
                e.Property(x => x.Email).HasMaxLength(255).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(500);
                e.Property(x => x.FirstName).HasMaxLength(100);
                e.Property(x => x.LastName).HasMaxLength(100);
                e.Property(x => x.IsEmailVerified).HasDefaultValue(false);
                e.Property(x => x.GoogleSubject).HasMaxLength(200);
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
                e.Property(x => x.Slika);
                e.Property(x => x.SlikaExtension).HasMaxLength(10);

                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.Username).IsUnique();
                e.HasIndex(x => x.GoogleSubject).IsUnique();
            });

            b.Entity<EmailConfirmation>(e =>
            {
                e.ToTable("email_confirmations");
                e.HasKey(x => x.Id);

                e.Property(x => x.Email).HasMaxLength(255).IsRequired();
                e.Property(x => x.Username).HasMaxLength(100).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
                e.Property(x => x.FirstName).HasMaxLength(100);
                e.Property(x => x.LastName).HasMaxLength(100);
                e.Property(x => x.Code).HasMaxLength(12).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
                e.Property(x => x.ExpiresAtUtc).IsRequired();

                e.HasIndex(x => x.Email).IsUnique();
            });

            b.Entity<PasswordReset>(e =>
            {
                e.ToTable("password_resets");
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).HasMaxLength(255).IsRequired();
                e.Property(x => x.Code).HasMaxLength(12).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
                e.Property(x => x.ExpiresAtUtc).IsRequired();

                e.HasIndex(x => x.Email).IsUnique();
            });

            b.Entity<Group>(e =>
            {
                e.ToTable("groups");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Slika);
                e.Property(x => x.OwnerUserId).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
            });

            b.Entity<GroupsUsers>(e =>
            {
                e.ToTable("groups_users");
                e.HasKey(x => new { x.GroupId, x.UserId });
                e.Property(x => x.IsAdmin).HasDefaultValue(false);
                e.Property(x => x.Nickname);
                e.Property(x => x.JoinedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Group)
                    .WithMany(g => g.Members)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                    .WithMany(u => u.MemberGroups)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.GroupId);
                e.HasIndex(x => x.UserId);
            });

            b.Entity<GroupInvitation>(e =>
            {
                e.ToTable("group_invitations");
                e.HasKey(x => x.Id);

                e.Property(x => x.GroupId).IsRequired();
                e.Property(x => x.InvitedUserId).IsRequired();
                e.Property(x => x.InvitedByUserId).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Group)
                    .WithMany(g => g.GroupInvitations)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.InvitedUser)
                    .WithMany(u => u.GroupInvitations)
                    .HasForeignKey(x => x.InvitedUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.InvitedByUser)
                    .WithMany(u => u.GroupInvites)
                    .HasForeignKey(x => x.InvitedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Racun>(e =>
            {
                e.ToTable("racuni");
                e.HasKey(x => x.Id);

                e.Property(x => x.Naziv);
                e.Property(x => x.Iznos).IsRequired();
                e.Property(x => x.CreatorUserId);
                e.Property(x => x.GroupId).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Creator)
                    .WithMany(c => c.RacuniKaoKreator)
                    .HasForeignKey(x => x.CreatorUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Group)
                    .WithMany(g => g.Racuni)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<RacunItem>(e =>
            {
                e.ToTable("racun_items");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.RacunId).IsRequired();
                e.Property(x => x.Iznos).IsRequired();
                e.Property(x => x.DeoRacuna).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany(u => u.RacunItems)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Racun)
                    .WithMany(r => r.Items)
                    .HasForeignKey(x => x.RacunId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.RacunId);
            });

            b.Entity<Anketa>(e =>
            {
                e.ToTable("ankete");
                e.HasKey(x => x.Id);

                e.Property(x => x.HasStarted).IsRequired().HasDefaultValue(false);
                e.Property(x => x.IsFinished).IsRequired().HasDefaultValue(false);
                e.Property(x => x.CreatorId).IsRequired();
                e.Property(x => x.GroupId).IsRequired();
                e.Property(x => x.Naziv).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Creator)
                    .WithMany(u => u.KreiraneAnkete)
                    .HasForeignKey(x => x.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Group)
                    .WithMany(r => r.Ankete)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.GroupId);
            });

            b.Entity<AnketaOption>(e =>
            {
                e.ToTable("anketa_options");
                e.HasKey(x => x.Id);

                e.Property(x => x.AnketaId).IsRequired();
                e.Property(x => x.Naziv).IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Anketa)
                    .WithMany(a => a.AnketaOptions)
                    .HasForeignKey(x => x.AnketaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.AnketaId);
            });

            b.Entity<AnketaAnswer>(e =>
            {
                e.ToTable("anketa_answers");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.AnketaId).IsRequired();
                e.Property(x => x.SubmittedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.User)
                    .WithMany(u => u.AnketaAnswers)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Anketa)
                    .WithMany(u => u.AnketaAnswers)
                    .HasForeignKey(x => x.AnketaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.UserId, x.AnketaId }).IsUnique();
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.AnketaId);
            });

            b.Entity<AnketaAnswerOption>(e =>
            {
                e.ToTable("anketa_answer_options");
                e.HasKey(x => x.Id);

                e.Property(x => x.AnketaAnswerId).IsRequired();
                e.Property(x => x.AnketaOptionId).IsRequired();
                e.Property(x => x.Ocena).IsRequired();

                e.HasOne(x => x.AnketaAnswer)
                    .WithMany(a => a.AnketaAnswerOptions)
                    .HasForeignKey(x => x.AnketaAnswerId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.AnketaOption)
                    .WithMany(a => a.AnketaAnswerOptions)
                    .HasForeignKey(x => x.AnketaOptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.AnketaAnswerId, x.AnketaOptionId }).IsUnique();
                e.HasIndex(x => x.AnketaAnswerId);
                e.HasIndex(x => x.AnketaOptionId);
            });

            b.Entity<Dogadjaj>(e =>
            {
                e.ToTable("dogadjaji");
                e.HasKey(x => x.Id);

                e.Property(x => x.CreatorId).IsRequired();
                e.Property(x => x.GrupaId).IsRequired();
                e.Property(x => x.Slika);
                e.Property(x => x.VremeDogadjaja).HasColumnType("timestamptz").IsRequired();
                e.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
                e.Property(x => x.Opis);
                e.Property(x => x.Lokacija);
                e.Property(x => x.Naziv).IsRequired();

                e.HasOne(x => x.Creator)
                    .WithMany(u => u.CreatedDogadjaji)
                    .HasForeignKey(x => x.CreatorId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Grupa)
                    .WithMany(g => g.Dogadjaji)
                    .HasForeignKey(x => x.GrupaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.CreatorId);
                e.HasIndex(x => x.GrupaId);
            });

            b.Entity<DogadjajGlas>(e =>
            {
                e.ToTable("dogadjaji_glasovi");
                e.HasKey(x => x.Id);

                e.Property(x => x.UserId).IsRequired();
                e.Property(x => x.DogadjajId).IsRequired();
                e.Property(x => x.GlasOption).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany(u => u.Glasovi)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Dogadjaj)
                    .WithMany(d => d.Glasovi)
                    .HasForeignKey(x => x.DogadjajId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.UserId, x.DogadjajId }).IsUnique();
                e.HasIndex(x => x.DogadjajId);
                e.HasIndex(x => x.UserId);
            });

            b.Entity<ShoppingListaItem>(e =>
            {
                e.ToTable("shopping_lista_items");
                e.HasKey(x => x.Id);

                e.Property(x => x.GroupId).IsRequired();
                e.Property(x => x.TrazioUserId).IsRequired();
                e.Property(x => x.NabavioUserId);
                e.Property(x => x.Naziv).IsRequired();
                e.Property(x => x.TrazenoUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");
                e.Property(x => x.NabavljenoUtc).HasColumnType("timestamptz");

                e.HasOne(x => x.Grupa)
                    .WithMany(g => g.ShoppingListaItems)
                    .HasForeignKey(x => x.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Trazio)
                    .WithMany(d => d.TrazeniItemi)
                    .HasForeignKey(x => x.TrazioUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Nabavio)
                    .WithMany(d => d.NabavljeniItemi)
                    .HasForeignKey(x => x.NabavioUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(x => x.GroupId);
            });

            b.Entity<Chat>(e =>
            {
                e.ToTable("chat");
                e.HasKey(x => x.Id);

                e.Property(x => x.GrupaId).IsRequired();
                e.Property(x => x.SentById).IsRequired();
                e.Property(x => x.Poruka).IsRequired();
                e.Property(x => x.SentAtUtc).HasColumnType("timestamptz").HasDefaultValueSql("now()");

                e.HasOne(x => x.Grupa)
                    .WithMany(g => g.Chats)
                    .HasForeignKey(x => x.GrupaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SentBy)
                    .WithMany(d => d.Chats)
                    .HasForeignKey(x => x.SentById)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.GrupaId);
            });
        }
    }
}
