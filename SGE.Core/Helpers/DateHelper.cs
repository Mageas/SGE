using System.Globalization;

namespace SGE.Core.Helpers;

public static class DateHelper
{
    public static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;

        string[] formats =
        {
            // --- Formats date seule ---
            "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd-MM-yyyy",

            // --- Formats avec Heure:Minute (ex: 14:30) ---
            "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm", "MM/dd/yyyy HH:mm", "dd-MM-yyyy HH:mm",
            "yyyy-MM-ddTHH:mm", // Support ISO avec 'T'

            // --- Formats avec Heure:Minute:Seconde (ex: 14:30:59) ---
            "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss" // Support ISO avec 'T'
        };

        if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);

        return null;
    }
}
