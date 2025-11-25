using System.Data;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;

namespace SGE.Application.Services.Readers;

public class ExcelReader
{
    /// <summary>
    ///     Read the Excel file and return the data in a list.
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    public List<Dictionary<string, string>> Read(IFormFile formFile)
    {
        using var reader = ExcelReaderFactory.CreateReader(formFile.OpenReadStream());
        var result = reader.AsDataSet();

        var allData = new List<Dictionary<string, string>>();

        foreach (DataTable table in result.Tables)
        {
            if (table.Rows.Count == 0) continue;

            // Retrieve column names
            var columnNames = new List<string>();
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var columnName = table.Rows[0][i].ToString() ?? $"Column{i}";
                columnNames.Add(columnName.ToLower());
            }

            // Retrieve data
            for (var rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
            {
                var row = table.Rows[rowIndex];
                var rowData = new Dictionary<string, string>();

                for (var colIndex = 0; colIndex < columnNames.Count; colIndex++)
                    rowData[columnNames[colIndex]] = row[colIndex].ToString() ?? string.Empty;

                allData.Add(rowData);
            }
        }

        return allData;
    }
}