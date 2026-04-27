using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RocketReps.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    public DbSet<School> Schools => Set<School>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<ClassroomMembership> ClassroomMemberships => Set<ClassroomMembership>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<DeckAssignment> DeckAssignments => Set<DeckAssignment>();
    public DbSet<StudentCardProgress> StudentCardProgress => Set<StudentCardProgress>();
    public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(160);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("now()");
            entity.HasOne(user => user.School)
                .WithMany()
                .HasForeignKey(user => user.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<School>(entity =>
        {
            entity.Property(school => school.Name).HasMaxLength(200).IsRequired();
            entity.Property(school => school.Mascot).HasMaxLength(80).IsRequired();
            entity.Property(school => school.CreatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(school => school.Name).IsUnique();
        });

        builder.Entity<Classroom>(entity =>
        {
            entity.Property(classroom => classroom.Name).HasMaxLength(160).IsRequired();
            entity.Property(classroom => classroom.JoinCode).HasMaxLength(16).IsRequired();
            entity.Property(classroom => classroom.CreatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(classroom => classroom.JoinCode).IsUnique();
            entity.HasIndex(classroom => new { classroom.SchoolId, classroom.Name });
            entity.HasOne(classroom => classroom.CreatedByTeacher)
                .WithMany()
                .HasForeignKey(classroom => classroom.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ClassroomMembership>(entity =>
        {
            entity.Property(membership => membership.UserId).HasMaxLength(450).IsRequired();
            entity.Property(membership => membership.RequestedAt).HasDefaultValueSql("now()");
            entity.HasIndex(membership => new { membership.ClassroomId, membership.UserId }).IsUnique();
            entity.HasOne(membership => membership.User)
                .WithMany(user => user.ClassroomMemberships)
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(membership => membership.ApprovedByTeacher)
                .WithMany()
                .HasForeignKey(membership => membership.ApprovedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Deck>(entity =>
        {
            entity.Property(deck => deck.Title).HasMaxLength(180).IsRequired();
            entity.Property(deck => deck.Description).HasMaxLength(600).IsRequired();
            entity.Property(deck => deck.Subject).HasMaxLength(80).IsRequired();
            entity.Property(deck => deck.GradeBand).HasMaxLength(80).IsRequired();
            entity.Property(deck => deck.CreatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(deck => new { deck.IsGlobalStock, deck.Subject });
            entity.HasOne(deck => deck.OwnerTeacher)
                .WithMany()
                .HasForeignKey(deck => deck.OwnerTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Card>(entity =>
        {
            entity.Property(card => card.Front).HasMaxLength(1_000).IsRequired();
            entity.Property(card => card.Back).HasMaxLength(1_000).IsRequired();
            entity.Property(card => card.ChoicesJson).HasMaxLength(2_000);
            entity.Property(card => card.CorrectAnswer).HasMaxLength(500);
            entity.HasIndex(card => new { card.DeckId, card.SortOrder });
        });

        builder.Entity<DeckAssignment>(entity =>
        {
            entity.Property(assignment => assignment.AssignedByTeacherId).HasMaxLength(450).IsRequired();
            entity.Property(assignment => assignment.CreatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(assignment => new { assignment.ClassroomId, assignment.DeckId });
            entity.HasOne(assignment => assignment.AssignedByTeacher)
                .WithMany()
                .HasForeignKey(assignment => assignment.AssignedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentCardProgress>(entity =>
        {
            entity.Property(progress => progress.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(progress => new { progress.UserId, progress.CardId }).IsUnique();
            entity.HasIndex(progress => progress.DueAt);
            entity.HasOne(progress => progress.User)
                .WithMany(user => user.CardProgress)
                .HasForeignKey(progress => progress.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewLog>(entity =>
        {
            entity.Property(review => review.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(review => new { review.UserId, review.ReviewedAt });
            entity.HasOne(review => review.User)
                .WithMany(user => user.ReviewLogs)
                .HasForeignKey(review => review.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
