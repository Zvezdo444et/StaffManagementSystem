using ClosedXML.Excel;
using Inspector.Models;
using Inspector.ViewModels.Search;
using System.Data;
using System.Globalization;

namespace Inspector.Services
{
    public interface IExportService
    {
        Task ExportEmployeesToExcelAsync(IEnumerable<Employee> employees, string filePath);
        Task ExportSearchResultsToExcelAsync(DataTable resultsTable, List<SearchParameterViewModel> selectedParameters, string filePath);
        Task ExportContractsToExcelAsync(IEnumerable<Employee> employees, string filePath);
        Task ExportVacationsToExcelAsync(IEnumerable<Employee> employees, string filePath);

        Task ExportCategoryesToExcelAsync(IEnumerable<Employee> employees, string filePath);
    }

    public class ExportService : IExportService
    {
        public async Task ExportEmployeesToExcelAsync(IEnumerable<Employee> employees, string filePath)
        {
            await Task.Run(() =>
            {

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Список работников");

                string reportDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));

                string title = $"Список работников учреждения образования\n" +
                               $"\"Полоцкий государственный экономический колледж\"\n" +
                               $"(по состоянию на {reportDate})";

                worksheet.Cell(1, 1).SetValue(title);

                worksheet.Range(1, 1, 1, 7).Merge();

                var titleRange = worksheet.Range(1, 1, 1, 7);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                titleRange.Style.Alignment.WrapText = true;           
                titleRange.Style.Alignment.ShrinkToFit = false;    
                worksheet.Row(1).Height = 70; 

                int headerRow = 2;

                worksheet.Cell(headerRow, 1).SetValue("№п/п");
                worksheet.Cell(headerRow, 2).SetValue("Фамилия И.О.");
                worksheet.Cell(headerRow, 3).SetValue("Должность");
                worksheet.Cell(headerRow, 4).SetValue("Дата рождения");
                worksheet.Cell(headerRow, 5).SetValue("Домашний телефон");
                worksheet.Cell(headerRow, 6).SetValue("Мобильный телефон");
                worksheet.Cell(headerRow, 7).SetValue("Адрес");

                var headerRange = worksheet.Range(headerRow, 1, headerRow, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = headerRow + 1;
                int serialNumber = 1;

                foreach (var emp in employees.OrderBy(e => e.LastName))
                {
                    worksheet.Cell(row, 1).SetValue(serialNumber++);          
                    string fullFio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}".Trim();
                    worksheet.Cell(row, 2).SetValue(fullFio);
                    worksheet.Cell(row, 3).SetValue(emp.CurrentProfession ?? emp.Category ?? "");

                    var birthCell = worksheet.Cell(row, 4);
                    birthCell.SetValue(emp.BirthDate);
                    birthCell.Style.DateFormat.Format = "dd.MM.yyyy";
                    birthCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 5).SetValue(emp.HomePhone ?? "").Style.NumberFormat.Format = "@";
                    worksheet.Cell(row, 6).SetValue(emp.Phone ?? "").Style.NumberFormat.Format = "@";
                    worksheet.Cell(row, 7).SetValue(emp.Address ?? "");

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                worksheet.SheetView.FreezeRows(headerRow);

                var dataRange = worksheet.Range(headerRow, 1, row - 1, 7);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workbook.SaveAs(filePath);
            });
        }

        public async Task ExportCategoryesToExcelAsync(IEnumerable<Employee> employees, string filePath)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Категории работников");

                string reportDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));

                string title = $"Список категорий работников учреждения образования\n" +
                               $"\"Полоцкий государственный экономический колледж\"\n" +
                               $"(по состоянию на {reportDate})";

                worksheet.Cell(1, 1).SetValue(title);
                worksheet.Range(1, 1, 1, 4).Merge();

                var titleRange = worksheet.Range(1, 1, 1, 4);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 11;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                titleRange.Style.Alignment.WrapText = true;
                titleRange.Style.Alignment.ShrinkToFit = false;
                worksheet.Row(1).Height = 60;

                int headerRow = 2;

                worksheet.Cell(headerRow, 1).SetValue("№п/п");
                worksheet.Cell(headerRow, 2).SetValue("Фамилия И.О.");
                worksheet.Cell(headerRow, 3).SetValue("Должность");
                worksheet.Cell(headerRow, 4).SetValue("Категория"); 
                
                var headerRange = worksheet.Range(headerRow, 1, headerRow, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Range(headerRow, 3, headerRow, 4).SetAutoFilter();

                int row = headerRow + 1;
                int serialNumber = 1;

                foreach (var emp in employees.OrderBy(e => e.LastName))
                {
                    worksheet.Cell(row, 1).SetValue(serialNumber++);
                    string fullFio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}".Trim();
                    worksheet.Cell(row, 2).SetValue(fullFio);
                    worksheet.Cell(row, 3).SetValue(emp.CurrentProfession ?? ""); 
                    worksheet.Cell(row, 4).SetValue(emp.Category ?? "");         

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(headerRow);

                var dataRange = worksheet.Range(headerRow, 1, row - 1, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workbook.SaveAs(filePath);
            });
        }

        public async Task ExportContractsToExcelAsync(IEnumerable<Employee> employees, string filePath)
        {
            await Task.Run(() =>
            {

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Контракты работников");

                string reportDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));

                string title = $"Список контрактов работников учреждения образования\n" +
                               $"\"Полоцкий государственный экономический колледж\"\n" +
                               $"(по состоянию на {reportDate})";

                worksheet.Cell(1, 1).SetValue(title);
                worksheet.Range(1, 1, 1, 5).Merge();

                var titleRange = worksheet.Range(1, 1, 1, 5);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                titleRange.Style.Alignment.WrapText = true;
                titleRange.Style.Alignment.ShrinkToFit = false;
                worksheet.Row(1).Height = 70;

                int headerRow = 2;

                worksheet.Cell(headerRow, 1).SetValue("№п/п");
                worksheet.Cell(headerRow, 2).SetValue("Фамилия И.О.");
                worksheet.Cell(headerRow, 3).SetValue("Должность");
                worksheet.Cell(headerRow, 4).SetValue("Дата рождения");
                worksheet.Cell(headerRow, 5).SetValue("Срок действия контракта");

                var headerRange = worksheet.Range(headerRow, 1, headerRow, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = headerRow + 1;
                int serialNumber = 1;

                foreach (var emp in employees.OrderBy(e => e.LastName))
                {
                    worksheet.Cell(row, 1).SetValue(serialNumber++);
                    string fullFio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}".Trim();
                    worksheet.Cell(row, 2).SetValue(fullFio);
                    worksheet.Cell(row, 3).SetValue(emp.CurrentProfession ?? emp.Category ?? "");

                    var birthCell = worksheet.Cell(row, 4);
                    birthCell.SetValue(emp.BirthDate);
                    birthCell.Style.DateFormat.Format = "dd.MM.yyyy";
                    birthCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var contractCell = worksheet.Cell(row, 5);
                    contractCell.SetValue(emp.ContractEndDate);
                    contractCell.Style.DateFormat.Format = "dd.MM.yyyy";
                    contractCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                worksheet.SheetView.FreezeRows(headerRow);

                var dataRange = worksheet.Range(headerRow, 1, row - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workbook.SaveAs(filePath);
            });
        }

        public async Task ExportVacationsToExcelAsync(IEnumerable<Employee> employees, string filePath)
        {
            await Task.Run(() =>
            {

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Отпуска работников");

                string reportDate = DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("ru-RU"));

                string title = $"Список отпусков работников учреждения образования\n" +
                               $"\"Полоцкий государственный экономический колледж\"\n" +
                               $"(по состоянию на {reportDate})";

                worksheet.Cell(1, 1).SetValue(title);
                worksheet.Range(1, 1, 1, 9).Merge();

                var titleRange = worksheet.Range(1, 1, 1, 9);
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                titleRange.Style.Alignment.WrapText = true;
                titleRange.Style.Alignment.ShrinkToFit = false;
                worksheet.Row(1).Height = 70;

                int headerRow = 2;

                worksheet.Cell(headerRow, 1).SetValue("№п/п");
                worksheet.Cell(headerRow, 2).SetValue("Фамилия Имя Отчество");
                worksheet.Cell(headerRow, 3).SetValue("Должность");
                worksheet.Cell(headerRow, 4).SetValue("Дата начала отпуска");
                worksheet.Cell(headerRow, 5).SetValue("Дата окончания отпуска");
                worksheet.Cell(headerRow, 6).SetValue("Дней");
                worksheet.Cell(headerRow, 7).SetValue("Период");
                worksheet.Cell(headerRow, 8).SetValue("Вид отпуска");
                worksheet.Cell(headerRow, 9).SetValue("Основание");

                var headerRange = worksheet.Range(headerRow, 1, headerRow, 9);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = headerRow + 1;
                int serialNumber = 1;

                var today = DateTime.Today; 

        foreach (var emp in employees.OrderBy(e => e.LastName))
        {
            var activeVacations = emp.VacationRecords
                .Where(v => v.EndDate.HasValue && v.EndDate.Value.Date >= today && v.StartDate.Value.Date <= today)
                .OrderBy(v => v.StartDate)
                .ToList();

            if (!activeVacations.Any())
                continue;   

            foreach (var vac in activeVacations)
            {
                worksheet.Cell(row, 1).SetValue(serialNumber);
                string fullFio = $"{emp.LastName} {emp.FirstName} {emp.MiddleName}".Trim();
                worksheet.Cell(row, 2).SetValue(fullFio);
                worksheet.Cell(row, 3).SetValue(emp.CurrentProfession ?? emp.Category ?? "");

                if (vac.StartDate.HasValue)
                {
                    var startCell = worksheet.Cell(row, 4);
                    startCell.SetValue(vac.StartDate.Value);
                    startCell.Style.DateFormat.Format = "dd.MM.yyyy";
                    startCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                if (vac.EndDate.HasValue)
                {
                    var endCell = worksheet.Cell(row, 5);
                    endCell.SetValue(vac.EndDate.Value);
                    endCell.Style.DateFormat.Format = "dd.MM.yyyy";
                    endCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                worksheet.Cell(row, 6).SetValue(vac.WorkingDays?.ToString() ?? "");
                worksheet.Cell(row, 7).SetValue(vac.Period ?? "");
                worksheet.Cell(row, 8).SetValue(vac.VacationKind ?? "");
                worksheet.Cell(row, 9).SetValue(vac.Basis ?? "");

                row++;
            }

            serialNumber++; 
        }

                worksheet.Columns().AdjustToContents();

                worksheet.SheetView.FreezeRows(headerRow);

                var dataRange = worksheet.Range(headerRow, 1, row - 1, 9);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workbook.SaveAs(filePath);
            });
        }

        public async Task ExportSearchResultsToExcelAsync(DataTable resultsTable,
    List<SearchParameterViewModel> selectedParameters, string filePath)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Результаты поиска");

                int headerRow = 1;

                for (int i = 0; i < selectedParameters.Count; i++)
                {
                    string columnName = selectedParameters[i].Key switch
                    {
                        "Fio" => "Фамилия Имя Отчество",
                        "BirthDate" => "Дата рождения",
                        "Phone" => "Мобильный телефон",
                        "PassportSeries" => "Серия паспорта",
                        "EduLevel" => "Уровень образования",
                        _ => selectedParameters[i].DisplayName
                    };

                    worksheet.Cell(headerRow, i + 1).SetValue(columnName);
                }

                var headerRange = worksheet.Range(headerRow, 1, headerRow, selectedParameters.Count);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Один вызов на весь диапазон — фильтры на всех столбцах
                headerRange.SetAutoFilter();

                int row = headerRow + 1;

                foreach (DataRow dataRow in resultsTable.Rows)
                {
                    for (int col = 0; col < selectedParameters.Count; col++)
                    {
                        string value = dataRow[col]?.ToString() ?? "";
                        worksheet.Cell(row, col + 1).SetValue(value);
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(headerRow);

                var dataRange = worksheet.Range(headerRow, 1, row - 1, selectedParameters.Count);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                workbook.SaveAs(filePath);
            });
        }
    }
}
