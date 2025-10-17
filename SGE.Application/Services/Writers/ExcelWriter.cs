using ClosedXML.Excel;

namespace SGE.Application.Services.Writers;

public class ExcelWriter
{
    /// <summary>
    /// Write the data to an Excel file.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sheetName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Write<T>(List<T> data, string sheetName = "Sheet1")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties();

        // Add header row
        for (var i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Add data rows
        for (var rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var item = data[rowIndex];
            for (var colIndex = 0; colIndex < properties.Length; colIndex++)
            {
                var value = properties[colIndex].GetValue(item);
                worksheet.Cell(rowIndex + 2, colIndex + 1).Value = value?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();

        // Save to memory
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return stream.ToArray();
    }
}