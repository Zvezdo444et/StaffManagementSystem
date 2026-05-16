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
        // ===================== User =====================
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

        // ===================== Employee =====================
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.LastName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.MiddleName)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(e => e.BirthPlace)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(e => e.Address)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(e => e.MainProfession)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(e => e.CurrentProfession)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(e => e.DiplomaSpecialty)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(e => e.DiplomaQualification)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(e => e.Phone)
                  .HasMaxLength(20)
                  .IsRequired(false);

            entity.Property(e => e.HomePhone)
                  .HasMaxLength(20)
                  .IsRequired(false);

            entity.Property(e => e.LastWorkPlace)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(e => e.LastWorkPosition)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(e => e.LastWorkDismissalReason)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(e => e.AdditionalInfo)
                  .HasMaxLength(1000)
                  .IsRequired(false);

            entity.Property(e => e.Category)
                  .HasMaxLength(100)
                  .IsRequired(false);
        });

        // ===================== Passport =====================
        modelBuilder.Entity<Passport>(entity =>
        {
            entity.HasKey(p => p.EmployeeId);

            entity.HasOne(p => p.Employee)
                  .WithOne(e => e.Passport)
                  .HasForeignKey<Passport>(p => p.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Series)
                  .HasMaxLength(10)
                  .IsRequired(false);

            entity.Property(p => p.Number)
                  .HasMaxLength(20)
                  .IsRequired(false);

            entity.Property(p => p.IdentificationNumber)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(p => p.IssuedBy)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(p => p.ExpiryDate)
                  .HasColumnType("datetime2(7)")
                  .IsRequired(false);
        });

        // ===================== MilitaryRegistration =====================
        modelBuilder.Entity<MilitaryRegistration>(entity =>
        {
            entity.HasKey(m => m.EmployeeId);

            entity.HasOne(m => m.Employee)
                  .WithOne(e => e.MilitaryRegistration)
                  .HasForeignKey<MilitaryRegistration>(m => m.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(m => m.AccountingGroup)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(m => m.AccountingCategory)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(m => m.Composition)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(m => m.MilitaryRank)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(m => m.MilitarySpecialty)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(m => m.FitnessCategory)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(m => m.CommissariatName)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(m => m.SpecialAccountingNumber)
                  .HasMaxLength(100)
                  .IsRequired(false);
        });

        // ===================== FamilyMember =====================
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

            entity.Property(f => f.LastName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(f => f.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(f => f.MiddleName)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(f => f.BirthDate)
                  .HasColumnType("date")
                  .IsRequired(false);

            entity.Ignore(f => f.RelationTypeStr);
            entity.Ignore(f => f.CustomRelationType);
        });

        // ===================== EducationRecord =====================
        modelBuilder.Entity<EducationRecord>(entity =>
        {
            entity.HasOne(er => er.Employee)
                  .WithMany(e => e.EducationRecords)
                  .HasForeignKey(er => er.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(er => er.EducationLevel)
                  .WithMany()
                  .HasForeignKey("EducationLevelId")
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Ignore(er => er.CustomEducationLevel);

            entity.Property(er => er.InstitutionName)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(er => er.StudyType)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(er => er.Specialty)
                  .HasMaxLength(500)
                  .IsRequired(false);

            entity.Property(er => er.Qualification)
                  .HasMaxLength(500)
                  .IsRequired(false);
        });

        // ===================== AppointmentTransfer =====================
        modelBuilder.Entity<AppointmentTransfer>(entity =>
        {
            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.AppointmentTransfers)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(a => a.BasisDocumentName)
                  .HasMaxLength(300)
                  .IsRequired(false);

            entity.Property(a => a.BasisDocumentNumber)
                  .HasMaxLength(50)
                  .IsRequired(false);

            entity.Property(a => a.Position)
                  .HasMaxLength(200)
                  .IsRequired(false);

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

        // ===================== QualificationUpgrade =====================
        modelBuilder.Entity<QualificationUpgrade>(entity =>
        {
            entity.HasOne(u => u.Employee)
                  .WithMany(e => e.QualificationUpgrades)
                  .HasForeignKey(u => u.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(u => u.UpgradeType)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(u => u.CertificateNumber)
                  .HasMaxLength(100)
                  .IsRequired(false);
        });

        // ===================== RetrainingRecord =====================
        modelBuilder.Entity<RetrainingRecord>(entity =>
        {
            entity.HasOne(r => r.Employee)
                  .WithMany(e => e.RetrainingRecords)
                  .HasForeignKey(r => r.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(r => r.Specialty)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(r => r.DiplomaNumber)
                  .HasMaxLength(100)
                  .IsRequired(false);
        });

        // ===================== AttestationRecord =====================
        modelBuilder.Entity<AttestationRecord>(entity =>
        {
            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.AttestationRecords)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(a => a.CommissionDecision)
                  .HasMaxLength(500)
                  .IsRequired(false);
        });

        // ===================== VacationRecord =====================
        modelBuilder.Entity<VacationRecord>(entity =>
        {
            entity.ToTable("VacationRecord");

            entity.HasOne<Employee>()
                  .WithMany(e => e.VacationRecords)
                  .HasForeignKey(v => v.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(v => v.VacationKind)
                  .HasMaxLength(100)
                  .IsRequired(false);

            entity.Property(v => v.Basis)
                  .HasMaxLength(300)
                  .IsRequired(false);
        });

        // ===================== RelationType =====================
        modelBuilder.Entity<RelationType>(entity =>
        {
            entity.Property(r => r.Name)
                  .HasMaxLength(100)
                  .IsRequired();
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

        // ===================== EducationLevel =====================
        modelBuilder.Entity<EducationLevel>(entity =>
        {
            entity.Property(e => e.Name)
                  .HasMaxLength(100)
                  .IsRequired();
        });

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