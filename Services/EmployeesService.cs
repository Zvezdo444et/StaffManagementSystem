using Inspector.Data;
using Inspector.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Inspector.Services;

public sealed class EmployeesService : IEmployeesService
{
    private readonly IDbContextFactory<InspectorDbContext> _dbFactory;
    private List<Employee>? _allEmployeesCache;
    private readonly Dictionary<int, Employee> _fullEmployeeCache = new();
    private List<RelationType>? _relationTypesCache;
    private List<EducationLevel>? _educationLevelsCache;

    public EmployeesService(IDbContextFactory<InspectorDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    private void InvalidateEmployeeCache(int? employeeId = null)
    {
        _allEmployeesCache = null;
        if (employeeId.HasValue)
            _fullEmployeeCache.Remove(employeeId.Value);
        else
            _fullEmployeeCache.Clear();
    }

    private void InvalidateRelationCache() => _relationTypesCache = null;
    private void InvalidateEducationCache() => _educationLevelsCache = null;

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
    {
        if (_allEmployeesCache != null)
            return _allEmployeesCache;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        _allEmployeesCache = await db.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.Passport)
            .Include(e => e.MilitaryRegistration)
            .Include(e => e.FamilyMembers).ThenInclude(f => f.RelationType)
            .Include(e => e.EducationRecords).ThenInclude(r => r.EducationLevel)
            .Include(e => e.AppointmentTransfers)
            .Include(e => e.QualificationUpgrades)
            .Include(e => e.RetrainingRecords)
            .Include(e => e.VacationRecords)
            .Include(e => e.AttestationRecords)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);
        return _allEmployeesCache;
    }

    public async Task<Employee?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default)
    {
        if (_fullEmployeeCache.TryGetValue(id, out var cached))
            return cached;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var employee = await db.Employees
            .AsNoTracking()
            .Include(e => e.Passport)
            .Include(e => e.MilitaryRegistration)
            .Include(e => e.FamilyMembers).ThenInclude(f => f.RelationType)
            .Include(e => e.EducationRecords).ThenInclude(r => r.EducationLevel)
            .Include(e => e.AppointmentTransfers)
            .Include(e => e.QualificationUpgrades)
            .Include(e => e.RetrainingRecords)
            .Include(e => e.VacationRecords)
            .Include(e => e.AttestationRecords)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee != null)
            _fullEmployeeCache[id] = employee;
        return employee;
    }

    public async Task<IReadOnlyList<RelationType>> GetRelationTypesAsync(CancellationToken ct = default)
    {
        if (_relationTypesCache != null)
            return _relationTypesCache;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        _relationTypesCache = await db.RelationTypes
            .AsNoTracking()
            .OrderBy(rt => rt.Id)
            .ToListAsync(ct);
        return _relationTypesCache;
    }

    public async Task<IReadOnlyList<EducationLevel>> GetEducationLevelsAsync(CancellationToken ct = default)
    {
        if (_educationLevelsCache != null)
            return _educationLevelsCache;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        _educationLevelsCache = await db.EducationLevels
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(ct);
        return _educationLevelsCache;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var employee = await db.Employees.FindAsync(new object[] { id }, ct);
        if (employee == null) return;
        db.Employees.Remove(employee);
        await db.SaveChangesAsync(ct);
        InvalidateEmployeeCache(id);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (employee.FamilyMembers?.Count > 0)
        {
            var existingTypes = await db.RelationTypes.ToListAsync(ct);
            var relationTypeByName = existingTypes.ToDictionary(t => t.Name);
            foreach (var member in employee.FamilyMembers)
            {
                if (member.RelationType == null || string.IsNullOrWhiteSpace(member.RelationType.Name))
                    continue;
                string name = member.RelationType.Name.Trim();
                if (relationTypeByName.TryGetValue(name, out var existing))
                {
                    member.RelationType = existing;
                    member.RelationTypeId = existing.Id;
                    db.Entry(existing).State = EntityState.Unchanged;
                }
                else
                {
                    var newType = new RelationType { Name = name };
                    db.RelationTypes.Add(newType);
                    relationTypeByName[name] = newType;
                    member.RelationType = newType;
                    member.RelationTypeId = 0;
                }
            }
        }
        if (employee.EducationRecords?.Count > 0)
        {
            var existingLevels = await db.EducationLevels.ToListAsync(ct);
            var levelByName = existingLevels.ToDictionary(l => l.Name);
            foreach (var record in employee.EducationRecords)
            {
                if (string.IsNullOrWhiteSpace(record.CustomEducationLevel)) continue;
                string name = record.CustomEducationLevel.Trim();
                if (levelByName.TryGetValue(name, out var existing))
                {
                    record.EducationLevel = existing;
                }
                else
                {
                    var newLevel = new EducationLevel { Name = name };
                    db.EducationLevels.Add(newLevel);
                    levelByName[name] = newLevel;
                    record.EducationLevel = newLevel;
                }
            }
        }
        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);
        InvalidateEmployeeCache();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var existing = await db.Employees
            .Include(e => e.Passport)
            .Include(e => e.MilitaryRegistration)
            .Include(e => e.FamilyMembers)
            .Include(e => e.EducationRecords)
            .Include(e => e.AppointmentTransfers)
            .Include(e => e.QualificationUpgrades)
            .Include(e => e.RetrainingRecords)
            .Include(e => e.VacationRecords)
            .Include(e => e.AttestationRecords)
            .FirstOrDefaultAsync(e => e.Id == employee.Id, ct);
        if (existing == null)
            throw new InvalidOperationException($"Сотрудник с Id {employee.Id} не найден");
        db.Entry(existing).CurrentValues.SetValues(employee);
        if (employee.Passport != null)
        {
            if (existing.Passport == null)
                existing.Passport = new Passport { EmployeeId = existing.Id };
            existing.Passport.Series = employee.Passport.Series;
            existing.Passport.Number = employee.Passport.Number;
            existing.Passport.IdentificationNumber = employee.Passport.IdentificationNumber;
            existing.Passport.IssuedBy = employee.Passport.IssuedBy;
            existing.Passport.IssueDate = employee.Passport.IssueDate;
            existing.Passport.ExpiryDate = employee.Passport.ExpiryDate;
        }
        else if (existing.Passport != null)
        {
            db.Passports.Remove(existing.Passport);
        }
        if (employee.MilitaryRegistration != null)
        {
            if (existing.MilitaryRegistration == null)
                existing.MilitaryRegistration = new MilitaryRegistration { EmployeeId = existing.Id };
            existing.MilitaryRegistration.AccountingGroup = employee.MilitaryRegistration.AccountingGroup;
            existing.MilitaryRegistration.AccountingCategory = employee.MilitaryRegistration.AccountingCategory;
            existing.MilitaryRegistration.Composition = employee.MilitaryRegistration.Composition;
            existing.MilitaryRegistration.MilitaryRank = employee.MilitaryRegistration.MilitaryRank;
            existing.MilitaryRegistration.MilitarySpecialty = employee.MilitaryRegistration.MilitarySpecialty;
            existing.MilitaryRegistration.FitnessCategory = employee.MilitaryRegistration.FitnessCategory;
            existing.MilitaryRegistration.CommissariatName = employee.MilitaryRegistration.CommissariatName;
            existing.MilitaryRegistration.SpecialAccountingNumber = employee.MilitaryRegistration.SpecialAccountingNumber;
        }
        else if (existing.MilitaryRegistration != null)
        {
            db.MilitaryRegistrations.Remove(existing.MilitaryRegistration);
        }
        existing.FamilyMembers.Clear();
        if (employee.FamilyMembers?.Count > 0)
        {
            var existingTypes = await db.RelationTypes.ToListAsync(ct);
            var typeDict = existingTypes.ToDictionary(t => t.Id);
            foreach (var item in employee.FamilyMembers)
            {
                if (item.RelationType != null && typeDict.TryGetValue(item.RelationType.Id, out var trackedType))
                {
                    item.RelationType = trackedType;
                    item.RelationTypeId = trackedType.Id;
                }
                existing.FamilyMembers.Add(item);
            }
        }
        existing.EducationRecords.Clear();
        if (employee.EducationRecords?.Count > 0)
        {
            var existingLevels = await db.EducationLevels.ToListAsync(ct);
            var levelDict = existingLevels.ToDictionary(l => l.Id);
            foreach (var record in employee.EducationRecords)
            {
                if (record.EducationLevel != null && levelDict.TryGetValue(record.EducationLevel.Id, out var trackedLevel))
                    record.EducationLevel = trackedLevel;
                existing.EducationRecords.Add(record);
            }
        }
        existing.AppointmentTransfers.Clear();
        if (employee.AppointmentTransfers != null)
            foreach (var a in employee.AppointmentTransfers) existing.AppointmentTransfers.Add(a);
        existing.QualificationUpgrades.Clear();
        if (employee.QualificationUpgrades != null)
            foreach (var q in employee.QualificationUpgrades) existing.QualificationUpgrades.Add(q);
        existing.RetrainingRecords.Clear();
        if (employee.RetrainingRecords != null)
            foreach (var r in employee.RetrainingRecords) existing.RetrainingRecords.Add(r);
        existing.VacationRecords.Clear();
        if (employee.VacationRecords != null)
            foreach (var v in employee.VacationRecords) existing.VacationRecords.Add(v);
        existing.AttestationRecords.Clear();
        if (employee.AttestationRecords != null)
            foreach (var a in employee.AttestationRecords) existing.AttestationRecords.Add(a);
        await db.SaveChangesAsync(ct);
        InvalidateEmployeeCache(employee.Id);
        return existing;
    }

    public async Task AddRelationTypeAsync(string name, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.RelationTypes.AnyAsync(rt => rt.Name == name, ct))
            return;
        db.RelationTypes.Add(new RelationType { Name = name });
        await db.SaveChangesAsync(ct);
        InvalidateRelationCache();
        InvalidateEmployeeCache();
    }

    public async Task DeleteRelationTypeAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var type = await db.RelationTypes.FindAsync(new object[] { id }, ct);
        if (type == null) return;
        db.RelationTypes.Remove(type);
        await db.SaveChangesAsync(ct);
        InvalidateRelationCache();
        InvalidateEmployeeCache();
    }

    public async Task AddEducationLevelAsync(string name, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        if (await db.EducationLevels.AnyAsync(l => l.Name == name, ct))
            return;
        db.EducationLevels.Add(new EducationLevel { Name = name });
        await db.SaveChangesAsync(ct);
        InvalidateEducationCache();
        InvalidateEmployeeCache();
    }

    public async Task DeleteEducationLevelAsync(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var level = await db.EducationLevels.FindAsync(new object[] { id }, ct);
        if (level == null) return;
        db.EducationLevels.Remove(level);
        await db.SaveChangesAsync(ct);
        InvalidateEducationCache();
        InvalidateEmployeeCache();
    }

    public async Task<bool> IsRelationTypeInUseAsync(int relationTypeId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.FamilyMembers.AnyAsync(f => f.RelationTypeId == relationTypeId, ct);
    }

    public async Task<bool> IsEducationLevelInUseAsync(int educationLevelId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.EducationRecords.AnyAsync(e => e.EducationLevel != null && e.EducationLevel.Id == educationLevelId, ct);
    }

    public async Task<InspectorDbContext> GetDbContextAsync()
    {
        return await _dbFactory.CreateDbContextAsync();
    }
}