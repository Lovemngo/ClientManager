using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ClientManager.Data
{
    public class ClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A connection string is required.", "connectionString");
            _connectionString = connectionString;
        }

        private SqlConnection OpenConnection()
        {
            var con = new SqlConnection(_connectionString);
            con.Open();
            return con;
        }
        public LookupData GetLookups()
        {
            const string sql =
                "SELECT GenderId, Name FROM Gender ORDER BY GenderId; " +
                "SELECT AddressTypeId, Name FROM AddressType ORDER BY AddressTypeId; " +
                "SELECT ContactTypeId, Name FROM ContactType ORDER BY ContactTypeId;";

            var data = new LookupData();
            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con))
            using (var r = cmd.ExecuteReader())
            {
                data.Genders = ReadLookup(r);
                r.NextResult();
                data.AddressTypes = ReadLookup(r);
                r.NextResult();
                data.ContactTypes = ReadLookup(r);
            }
            return data;
        }

        private static List<LookupItem> ReadLookup(IDataReader r)
        {
            var list = new List<LookupItem>();
            while (r.Read())
                list.Add(new LookupItem { Id = r.GetInt32(0), Name = r.GetString(1) });
            return list;
        }

        // --search
        public ClientPage Search(string searchText, int pageNumber, int pageSize)
        {
            const string filter =
                " WHERE (@Like IS NULL OR c.LastName LIKE @Like OR c.FirstName LIKE @Like OR c.IdNumber LIKE @Like) ";

            const string sql =
                "SELECT COUNT(*) FROM Client c" + filter + "OPTION (RECOMPILE); " +
                "SELECT c.ClientId, c.FirstName, c.LastName, g.Name, c.DateOfBirth, c.IdNumber " +
                "FROM Client c INNER JOIN Gender g ON g.GenderId = c.GenderId" + filter +
                "ORDER BY c.LastName, c.FirstName, c.ClientId " +
                "OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY OPTION (RECOMPILE);";

            var page = new ClientPage { PageNumber = pageNumber, PageSize = pageSize };

            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                DbHelper.AddParam(cmd, "@Like", SqlDbType.NVarChar, 210, BuildLikePattern(searchText));
                DbHelper.AddParam(cmd, "@Offset", SqlDbType.Int, 0, (pageNumber - 1) * pageSize);
                DbHelper.AddParam(cmd, "@PageSize", SqlDbType.Int, 0, pageSize);

                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read()) page.TotalCount = r.GetInt32(0);
                    r.NextResult();
                    while (r.Read())
                    {
                        page.Items.Add(new ClientSummaryDto
                        {
                            ClientId = r.GetInt32(0),
                            FirstName = r.GetString(1),
                            LastName = r.GetString(2),
                            GenderName = r.GetString(3),
                            DateOfBirth = DbHelper.GetDateOrNull(r, 4),
                            IdNumber = DbHelper.GetStringOrNull(r, 5)
                        });
                    }
                }
            }
            return page;
        }

        private static string BuildLikePattern(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return null;
            string escaped = searchText.Trim()
                .Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
            return escaped + "%";
        }

        public ClientDto GetClient(int clientId)
        {
            const string sql =
                "SELECT ClientId, FirstName, LastName, GenderId, DateOfBirth, IdNumber " +
                "FROM Client WHERE ClientId = @Id; " +

                "SELECT a.AddressId, a.AddressTypeId, t.Name, a.Line1, a.Line2, a.Suburb, a.City, a.PostalCode, a.Country " +
                "FROM ClientAddress a INNER JOIN AddressType t ON t.AddressTypeId = a.AddressTypeId " +
                "WHERE a.ClientId = @Id ORDER BY a.AddressId; " +

                "SELECT c.ContactId, c.ContactTypeId, t.Name, c.Value " +
                "FROM ClientContact c INNER JOIN ContactType t ON t.ContactTypeId = c.ContactTypeId " +
                "WHERE c.ClientId = @Id ORDER BY c.ContactId;";

            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                DbHelper.AddParam(cmd, "@Id", SqlDbType.Int, 0, clientId);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;

                    var client = new ClientDto
                    {
                        ClientId = r.GetInt32(0),
                        FirstName = r.GetString(1),
                        LastName = r.GetString(2),
                        GenderId = r.GetInt32(3),
                        DateOfBirth = DbHelper.GetDateOrNull(r, 4),
                        IdNumber = DbHelper.GetStringOrNull(r, 5)
                    };

                    r.NextResult();
                    while (r.Read())
                    {
                        client.Addresses.Add(new AddressDto
                        {
                            AddressId = r.GetInt32(0),
                            AddressTypeId = r.GetInt32(1),
                            AddressTypeName = r.GetString(2),
                            Line1 = r.GetString(3),
                            Line2 = DbHelper.GetStringOrNull(r, 4),
                            Suburb = DbHelper.GetStringOrNull(r, 5),
                            City = DbHelper.GetStringOrNull(r, 6),
                            PostalCode = DbHelper.GetStringOrNull(r, 7),
                            Country = DbHelper.GetStringOrNull(r, 8)
                        });
                    }

                    r.NextResult();
                    while (r.Read())
                    {
                        client.Contacts.Add(new ContactDto
                        {
                            ContactId = r.GetInt32(0),
                            ContactTypeId = r.GetInt32(1),
                            ContactTypeName = r.GetString(2),
                            Value = r.GetString(3)
                        });
                    }
                    return client;
                }
            }
        }

        public bool IdNumberExists(string idNumber, int excludeClientId)
        {
            const string sql = "SELECT COUNT(1) FROM Client WHERE IdNumber = @IdNumber AND ClientId <> @Exclude";
            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                DbHelper.AddParam(cmd, "@IdNumber", SqlDbType.NVarChar, 20, idNumber);
                DbHelper.AddParam(cmd, "@Exclude", SqlDbType.Int, 0, excludeClientId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // --save
        public int Save(ClientDto client)
        {
            using (var con = OpenConnection())
            using (var tx = con.BeginTransaction())
            {
                int clientId = client.ClientId;

                if (clientId == 0)
                {
                    const string insert =
                        "INSERT INTO Client (FirstName, LastName, GenderId, DateOfBirth, IdNumber) " +
                        "OUTPUT INSERTED.ClientId " +
                        "VALUES (@FirstName, @LastName, @GenderId, @DateOfBirth, @IdNumber)";
                    using (var cmd = new SqlCommand(insert, con, tx))
                    {
                        AddClientParams(cmd, client);
                        clientId = (int)cmd.ExecuteScalar();
                    }
                }
                else
                {
                    const string update =
                        "UPDATE Client SET FirstName = @FirstName, LastName = @LastName, GenderId = @GenderId, " +
                        "DateOfBirth = @DateOfBirth, IdNumber = @IdNumber, ModifiedOn = SYSUTCDATETIME() " +
                        "WHERE ClientId = @ClientId";
                    using (var cmd = new SqlCommand(update, con, tx))
                    {
                        AddClientParams(cmd, client);
                        DbHelper.AddParam(cmd, "@ClientId", SqlDbType.Int, 0, clientId);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new KeyNotFoundException("Client " + clientId + " was not found.");
                    }

                    // Child rows are replaced wholesale: simple and correct for a handful of rows per client.
                    ExecuteDeleteChildren(con, tx, "DELETE FROM ClientAddress WHERE ClientId = @ClientId", clientId);
                    ExecuteDeleteChildren(con, tx, "DELETE FROM ClientContact WHERE ClientId = @ClientId", clientId);
                }

                foreach (AddressDto a in client.Addresses)
                    InsertAddress(con, tx, clientId, a);

                foreach (ContactDto c in client.Contacts)
                    InsertContact(con, tx, clientId, c);

                tx.Commit();
                return clientId;
            }
        }

        private static void AddClientParams(SqlCommand cmd, ClientDto c)
        {
            DbHelper.AddParam(cmd, "@FirstName", SqlDbType.NVarChar, 100, c.FirstName);
            DbHelper.AddParam(cmd, "@LastName", SqlDbType.NVarChar, 100, c.LastName);
            DbHelper.AddParam(cmd, "@GenderId", SqlDbType.Int, 0, c.GenderId);
            DbHelper.AddParam(cmd, "@DateOfBirth", SqlDbType.Date, 0, c.DateOfBirth);
            DbHelper.AddParam(cmd, "@IdNumber", SqlDbType.NVarChar, 20, c.IdNumber);
        }

        private static void ExecuteDeleteChildren(SqlConnection con, SqlTransaction tx, string sql, int clientId)
        {
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                DbHelper.AddParam(cmd, "@ClientId", SqlDbType.Int, 0, clientId);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertAddress(SqlConnection con, SqlTransaction tx, int clientId, AddressDto a)
        {
            const string sql =
                "INSERT INTO ClientAddress (ClientId, AddressTypeId, Line1, Line2, Suburb, City, PostalCode, Country) " +
                "VALUES (@ClientId, @TypeId, @Line1, @Line2, @Suburb, @City, @PostalCode, @Country)";
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                DbHelper.AddParam(cmd, "@ClientId", SqlDbType.Int, 0, clientId);
                DbHelper.AddParam(cmd, "@TypeId", SqlDbType.Int, 0, a.AddressTypeId);
                DbHelper.AddParam(cmd, "@Line1", SqlDbType.NVarChar, 150, a.Line1);
                DbHelper.AddParam(cmd, "@Line2", SqlDbType.NVarChar, 150, a.Line2);
                DbHelper.AddParam(cmd, "@Suburb", SqlDbType.NVarChar, 100, a.Suburb);
                DbHelper.AddParam(cmd, "@City", SqlDbType.NVarChar, 100, a.City);
                DbHelper.AddParam(cmd, "@PostalCode", SqlDbType.NVarChar, 20, a.PostalCode);
                DbHelper.AddParam(cmd, "@Country", SqlDbType.NVarChar, 100, a.Country);
                cmd.ExecuteNonQuery();
            }
        }

        private static void InsertContact(SqlConnection con, SqlTransaction tx, int clientId, ContactDto c)
        {
            const string sql =
                "INSERT INTO ClientContact (ClientId, ContactTypeId, Value) VALUES (@ClientId, @TypeId, @Value)";
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                DbHelper.AddParam(cmd, "@ClientId", SqlDbType.Int, 0, clientId);
                DbHelper.AddParam(cmd, "@TypeId", SqlDbType.Int, 0, c.ContactTypeId);
                DbHelper.AddParam(cmd, "@Value", SqlDbType.NVarChar, 150, c.Value);
                cmd.ExecuteNonQuery();
            }
        }

        // --delete
        public bool Delete(int clientId)
        {
            const string sql = "DELETE FROM Client WHERE ClientId = @ClientId";
            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                DbHelper.AddParam(cmd, "@ClientId", SqlDbType.Int, 0, clientId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // --export
        public IEnumerable<ExportRow> StreamExportRows()
        {
            const string sql =
                "SELECT c.ClientId, c.FirstName, c.LastName, g.Name, c.DateOfBirth, c.IdNumber, " +
                "       t.Name, a.Line1, a.Line2, a.Suburb, a.City, a.PostalCode, a.Country " +
                "FROM Client c " +
                "INNER JOIN Gender g ON g.GenderId = c.GenderId " +
                "LEFT JOIN ClientAddress a ON a.ClientId = c.ClientId " +
                "LEFT JOIN AddressType t ON t.AddressTypeId = a.AddressTypeId " +
                "ORDER BY c.ClientId, a.AddressId";

            using (var con = OpenConnection())
            using (var cmd = new SqlCommand(sql, con) { CommandTimeout = 0 })
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    yield return new ExportRow
                    {
                        ClientId = r.GetInt32(0),
                        FirstName = r.GetString(1),
                        LastName = r.GetString(2),
                        Gender = r.GetString(3),
                        DateOfBirth = DbHelper.GetDateOrNull(r, 4),
                        IdNumber = DbHelper.GetStringOrNull(r, 5),
                        AddressType = DbHelper.GetStringOrNull(r, 6),
                        Line1 = DbHelper.GetStringOrNull(r, 7),
                        Line2 = DbHelper.GetStringOrNull(r, 8),
                        Suburb = DbHelper.GetStringOrNull(r, 9),
                        City = DbHelper.GetStringOrNull(r, 10),
                        PostalCode = DbHelper.GetStringOrNull(r, 11),
                        Country = DbHelper.GetStringOrNull(r, 12)
                    };
                }
            }
        }
    }
}
