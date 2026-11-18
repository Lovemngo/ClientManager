using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClientManager.Data;

namespace ClientManager.Business
{
    internal static class CsvExporter
    {
        private const string Header =
            "ClientId,FirstName,LastName,Gender,DateOfBirth,IdNumber," +
            "AddressType,Line1,Line2,Suburb,City,PostalCode,Country";
        public static string WriteToTempFile(IEnumerable<ExportRow> rows)
        {
            string path = Path.Combine(Path.GetTempPath(), "clients_" + Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
                {
                    writer.WriteLine(Header);
                    foreach (ExportRow r in rows)
                    {
                        writer.WriteLine(string.Join(",", new[]
                        {
                            r.ClientId.ToString(),
                            Escape(r.FirstName),
                            Escape(r.LastName),
                            Escape(r.Gender),
                            r.DateOfBirth.HasValue ? r.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                            Escape(r.IdNumber),
                            Escape(r.AddressType),
                            Escape(r.Line1),
                            Escape(r.Line2),
                            Escape(r.Suburb),
                            Escape(r.City),
                            Escape(r.PostalCode),
                            Escape(r.Country)
                        }));
                    }
                }
                return path;
            }
            catch
            {
                try { if (File.Exists(path)) File.Delete(path); } catch { }
                throw;
            }
        }
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            bool needsQuotes = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            return needsQuotes ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }
    }
}
