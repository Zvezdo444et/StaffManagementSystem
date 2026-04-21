using Microsoft.EntityFrameworkCore;
using Inspector.Models;

namespace Inspector.Data;

public sealed class InspectorDbContext : DbContext
{
    public InspectorDbContext(DbContextOptions<InspectorDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Passport> Passports => Set<Passport>();
    public DbSet<MilitaryRegistration> MilitaryRegistrations => Set<MilitaryRegistration>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<EducationRecord> EducationRecords => Set<EducationRecord>();
    public DbSet<AppointmentTransfer> AppointmentTransfers => Set<AppointmentTransfer>();
    public DbSet<QualificationUpgrade> QualificationUpgrades => Set<QualificationUpgrade>();
    public DbSet<RetrainingRecord> RetrainingRecords => Set<RetrainingRecord>();
    public DbSet<AttestationRecord> AttestationRecords => Set<AttestationRecord>();
    public DbSet<PensionAgeSetting> PensionAgeSettings => Set<PensionAgeSetting>();
    public DbSet<RelationType> RelationTypes => Set<RelationType>();
    public DbSet<VacationRecord> VacationRecords => Set<VacationRecord>();
    public DbSet<EducationLevel> EducationLevels => Set<EducationLevel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VacationRecord>(entity =>
        {
            entity.ToTable("VacationRecord");
            entity.HasOne<Employee>()
                .WithMany(e => e.VacationRecords)
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Passport>().HasKey(p => p.EmployeeId);
        modelBuilder.Entity<Passport>()
            .HasOne(p => p.Employee)
            .WithOne(e => e.Passport)
            .HasForeignKey<Passport>(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Passport>()
        .Property(p => p.ExpiryDate)
        .HasColumnType("datetime2(7)")  
        .IsRequired(false);

        modelBuilder.Entity<MilitaryRegistration>().HasKey(m => m.EmployeeId);
        modelBuilder.Entity<MilitaryRegistration>()
            .HasOne(m => m.Employee)
            .WithOne(e => e.MilitaryRegistration)
            .HasForeignKey<MilitaryRegistration>(m => m.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FamilyMember>(entity =>
        {
            entity.HasOne(f => f.Employee)
                  .WithMany(e => e.FamilyMembers)
                  .HasForeignKey(f => f.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.RelationType)
                  .WithMany()
                  .HasForeignKey(f => f.RelationTypeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(f => f.BirthDate)
                  .HasColumnType("date")
                  .IsRequired(false);

            entity.Ignore(f => f.RelationTypeStr);
            entity.Ignore(f => f.CustomRelationType);
        });

        modelBuilder.Entity<EducationRecord>(entity =>
        {
            entity.HasOne(er => er.Employee)
                  .WithMany(e => e.EducationRecords)
                  .HasForeignKey(er => er.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(er => er.CustomEducationLevel);

            entity.HasOne(er => er.EducationLevel)
                  .WithMany()
                  .HasForeignKey("EducationLevelId")
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(er => er.Specialty)
                  .HasMaxLength(500)      
                  .IsRequired(false);

            entity.Property(er => er.Qualification)
                  .HasMaxLength(500)
                  .IsRequired(false);
        });

        modelBuilder.Entity<AppointmentTransfer>(entity =>
        {
            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.AppointmentTransfers)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(a => a.ContractType)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(a => a.ContractTerm)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(a => a.DocumentDate)     
                  .HasColumnName("DocumentDate")
                  .HasColumnType("datetime2(7)")
                  .IsRequired(false);
        });

        modelBuilder.Entity<QualificationUpgrade>()
            .HasOne(u => u.Employee)
            .WithMany(e => e.QualificationUpgrades)
            .HasForeignKey(u => u.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RetrainingRecord>()
            .HasOne(r => r.Employee)
            .WithMany(e => e.RetrainingRecords)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttestationRecord>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.AttestationRecords)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Login)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(u => u.PasswordHash)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.HasIndex(u => u.Login)
                  .IsUnique();


        });

        modelBuilder.Entity<RelationType>().HasData(
            new RelationType { Id = 1, Name = "Жена" },
            new RelationType { Id = 2, Name = "Муж" },
            new RelationType { Id = 3, Name = "Сын" },
            new RelationType { Id = 4, Name = "Дочь" },
            new RelationType { Id = 5, Name = "Отец" },
            new RelationType { Id = 6, Name = "Мать" },
            new RelationType { Id = 7, Name = "Брат" },
            new RelationType { Id = 8, Name = "Сестра" });

        modelBuilder.Entity<EducationLevel>().HasData(
            new EducationLevel { Id = 1, Name = "Среднее общее" },
            new EducationLevel { Id = 2, Name = "Среднее профессиональное" },
            new EducationLevel { Id = 3, Name = "Высшее" },
            new EducationLevel { Id = 4, Name = "Бакалавр" },
            new EducationLevel { Id = 5, Name = "Магистр" },
            new EducationLevel { Id = 6, Name = "Кандидат наук" },
            new EducationLevel { Id = 7, Name = "Доктор наук" });
    }
}