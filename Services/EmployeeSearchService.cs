using Inspector.Models;
using Inspector.Models.Search;
using Inspector.Services;
using System.Data;

namespace Inspector.Services;

public sealed class EmployeeSearchService : IEmployeeSearchService
{
    private readonly IEmployeesService _employeesService;

    public EmployeeSearchService(IEmployeesService employeesService)
    {
        _employeesService = employeesService;
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesForSearchAsync(CancellationToken ct = default)
    {
        return await _employeesService.GetAllAsync(ct);
    }

    public async Task<DataTable> SearchAsync(SearchTemplate t)
    {
        var keys = new HashSet<string>(t.SelectedKeys);
        var tbl = CreateTable();

        foreach (var emp in await GetEmployeesForSearchAsync())
        {
            var r = tbl.NewRow();

            Set(r, "FIO", keys, "Fio", emp.FullName);
            Set(r, "POL", keys, "IsMale", emp.IsMale ? "Муж" : "Жен");
            if (keys.Contains("BirthDate")) r["DATA_ROZHD"] = emp.BirthDate.ToString("dd.MM.yyyy");
            Set(r, "MESTO_ROZHD", keys, "BirthPlace", emp.BirthPlace);
            Set(r, "KVALIF", keys, "DiplomaQualification", emp.DiplomaQualification);
            Set(r, "DATE_PRIEM", keys, "HireDate", emp.HireDate?.ToString("dd.MM.yyyy"));
            Set(r, "CHL_PROF", keys, "IsUnionMember", emp.IsUnionMember ? "Да" : "Нет");
            Set(r, "KATEGORIA", keys, "Category", emp.Category);
            Set(r, "DOP_SVED", keys, "AdditionalInfo", emp.AdditionalInfo);
            Set(r, "SPEC_DIPLOM", keys, "DiplomaSpecialty", emp.DiplomaSpecialty);
            Set(r, "ADRES", keys, "Address", emp.Address);
            Set(r, "DOM_TELEF", keys, "HomePhone", emp.HomePhone);
            Set(r, "OSN_PROF", keys, "MainProfession", emp.MainProfession);
            Set(r, "TELEFON", keys, "Phone", emp.Phone);
            Set(r, "STAZH_SPEC", keys, "SpecialtyExperienceYears", emp.SpecialtyExperienceYears?.ToString());
            Set(r, "STAZH_OBSH", keys, "TotalExperienceYears", emp.TotalExperienceYears?.ToString());
            Set(r, "KONTRAKT", keys, "ContractEndDate", emp.ContractEndDate?.ToString("dd.MM.yyyy"));
            Set(r, "POSL_MESTO", keys, "LastWorkPlace", emp.LastWorkPlace);
            Set(r, "POSL_DOLZH", keys, "LastWorkPosition", emp.LastWorkPosition);
            Set(r, "DATE_UVOL_POSL", keys, "LastWorkDismissalDate", emp.LastWorkDismissalDate?.ToString("dd.MM.yyyy"));
            Set(r, "PRICH_UVOL_POSL", keys, "LastWorkDismissalReason", emp.LastWorkDismissalReason);

            Set(r, "EDU_SPECIALTY", keys, "EduSpecialty",
                string.Join("; ", emp.EducationRecords.Select(er => er.Specialty).Where(s => !string.IsNullOrWhiteSpace(s))));
            Set(r, "EDU_QUALIFICATION", keys, "EduQualification",
                string.Join("; ", emp.EducationRecords.Select(er => er.Qualification).Where(q => !string.IsNullOrWhiteSpace(q))));

            if (emp.Passport != null)
            {
                Set(r, "PASS_SER", keys, "PassportSeries", emp.Passport.Series);
                Set(r, "PASS_NOM", keys, "PassportNumber", emp.Passport.Number);
                Set(r, "PASS_IDENT", keys, "PassportIdentificationNumber", emp.Passport.IdentificationNumber);
                Set(r, "PASS_KEM", keys, "PassportIssuedBy", emp.Passport.IssuedBy);
                Set(r, "PASS_DATE", keys, "PassportIssueDate", emp.Passport.IssueDate?.ToString("dd.MM.yyyy"));
            }

            if (emp.MilitaryRegistration != null)
            {
                Set(r, "MIL_GRUPPA", keys, "MilAccountingGroup", emp.MilitaryRegistration.AccountingGroup);
                Set(r, "MIL_KATEG", keys, "MilAccountingCategory", emp.MilitaryRegistration.AccountingCategory);
                Set(r, "MIL_SOSTAV", keys, "MilComposition", emp.MilitaryRegistration.Composition);
                Set(r, "MIL_ZNANIE", keys, "MilRank", emp.MilitaryRegistration.MilitaryRank);
                Set(r, "MIL_VUS", keys, "MilSpecialty", emp.MilitaryRegistration.MilitarySpecialty);
                Set(r, "MIL_GODN", keys, "MilFitnessCategory", emp.MilitaryRegistration.FitnessCategory);
                Set(r, "MIL_KOMIS", keys, "MilCommissariatName", emp.MilitaryRegistration.CommissariatName);
                Set(r, "MIL_NOM_SP", keys, "MilSpecialNumber", emp.MilitaryRegistration.SpecialAccountingNumber);
            }

            tbl.Rows.Add(r);
        }
        return tbl;
    }

    private static void Set(DataRow r, string col, HashSet<string> keys, string key, string? val)
    {
        if (keys.Contains(key) && r.Table.Columns.Contains(col))
            r[col] = string.IsNullOrWhiteSpace(val) ? DBNull.Value : (object)val;
    }

    private static DataTable CreateTable()
    {
        var t = new DataTable();
        t.Columns.Add("FIO");
        t.Columns.Add("POL");
        t.Columns.Add("DATA_ROZHD");
        t.Columns.Add("MESTO_ROZHD");
        t.Columns.Add("KVALIF");
        t.Columns.Add("DATE_PRIEM");
        t.Columns.Add("CHL_PROF");
        t.Columns.Add("KATEGORIA");
        t.Columns.Add("DOP_SVED");
        t.Columns.Add("SPEC_DIPLOM");
        t.Columns.Add("ADRES");
        t.Columns.Add("DOM_TELEF");
        t.Columns.Add("OSN_PROF");
        t.Columns.Add("TELEFON");
        t.Columns.Add("STAZH_SPEC");
        t.Columns.Add("STAZH_OBSH");
        t.Columns.Add("KONTRAKT");
        t.Columns.Add("POSL_MESTO");
        t.Columns.Add("POSL_DOLZH");
        t.Columns.Add("DATE_UVOL_POSL");
        t.Columns.Add("PRICH_UVOL_POSL");
        t.Columns.Add("EDU_SPECIALTY");
        t.Columns.Add("EDU_QUALIFICATION");
        t.Columns.Add("PASS_SER");
        t.Columns.Add("PASS_NOM");
        t.Columns.Add("PASS_IDENT");
        t.Columns.Add("PASS_KEM");
        t.Columns.Add("PASS_DATE");
        t.Columns.Add("MIL_GRUPPA");
        t.Columns.Add("MIL_KATEG");
        t.Columns.Add("MIL_SOSTAV");
        t.Columns.Add("MIL_ZNANIE");
        t.Columns.Add("MIL_VUS");
        t.Columns.Add("MIL_GODN");
        t.Columns.Add("MIL_KOMIS");
        t.Columns.Add("MIL_NOM_SP");
        return t;
    }
}