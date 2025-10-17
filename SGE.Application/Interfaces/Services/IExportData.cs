namespace SGE.Application.Interfaces.Services;

public interface IExportData
{
    /// <summary>
    /// Export Departments to Excel
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ExportToExcelAsync(CancellationToken cancellationToken);
}