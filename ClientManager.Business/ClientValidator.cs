using System;
using System.Collections.Generic;
using ClientManager.Data;

namespace ClientManager.Business
{
    internal static class ClientValidator
    {
        public static void Normalise(ClientDto c)
        {
            c.FirstName = Clean(c.FirstName);
            c.LastName = Clean(c.LastName);
            c.IdNumber = Clean(c.IdNumber);
            if (c.DateOfBirth.HasValue) c.DateOfBirth = c.DateOfBirth.Value.Date;

            var addresses = new List<AddressDto>();
            if (c.Addresses != null)
            {
                foreach (AddressDto a in c.Addresses)
                {
                    if (a == null) continue;
                    a.Line1 = Clean(a.Line1);
                    a.Line2 = Clean(a.Line2);
                    a.Suburb = Clean(a.Suburb);
                    a.City = Clean(a.City);
                    a.PostalCode = Clean(a.PostalCode);
                    a.Country = Clean(a.Country);
                    bool blank = a.Line1 == null && a.Line2 == null && a.Suburb == null &&
                                 a.City == null && a.PostalCode == null && a.Country == null;
                    if (!blank) addresses.Add(a);
                }
            }
            c.Addresses = addresses;

            var contacts = new List<ContactDto>();
            if (c.Contacts != null)
            {
                foreach (ContactDto ct in c.Contacts)
                {
                    if (ct == null) continue;
                    ct.Value = Clean(ct.Value);
                    if (ct.Value != null) contacts.Add(ct);
                }
            }
            c.Contacts = contacts;
        }

        /// <summary>Returns every broken rule (empty list = valid).</summary>
        public static List<string> Validate(ClientDto c, LookupData lookups)
        {
            var errors = new List<string>();

            if (c.FirstName == null) errors.Add("First name is required.");
            else CheckLength(errors, "First name", c.FirstName, 100);

            if (c.LastName == null) errors.Add("Last name is required.");
            else CheckLength(errors, "Last name", c.LastName, 100);

            if (!ContainsId(lookups.Genders, c.GenderId)) errors.Add("Please select a gender.");

            if (c.DateOfBirth.HasValue)
            {
                if (c.DateOfBirth.Value.Date > DateTime.Today) errors.Add("Date of birth cannot be in the future.");
                else if (c.DateOfBirth.Value.Year < 1900) errors.Add("Date of birth is not valid.");
            }

            CheckLength(errors, "ID number", c.IdNumber, 20);

            int n = 0;
            foreach (AddressDto a in c.Addresses)
            {
                n++;
                string label = "Address " + n + ": ";
                if (!ContainsId(lookups.AddressTypes, a.AddressTypeId)) errors.Add(label + "select an address type.");
                if (a.Line1 == null) errors.Add(label + "address line 1 is required.");
                CheckLength(errors, label + "line 1", a.Line1, 150);
                CheckLength(errors, label + "line 2", a.Line2, 150);
                CheckLength(errors, label + "suburb", a.Suburb, 100);
                CheckLength(errors, label + "city", a.City, 100);
                CheckLength(errors, label + "postal code", a.PostalCode, 20);
                CheckLength(errors, label + "country", a.Country, 100);
            }

            n = 0;
            foreach (ContactDto ct in c.Contacts)
            {
                n++;
                string label = "Contact " + n + ": ";
                if (!ContainsId(lookups.ContactTypes, ct.ContactTypeId)) errors.Add(label + "select a contact type.");
                CheckLength(errors, label + "value", ct.Value, 150);
            }

            return errors;
        }

        private static void CheckLength(List<string> errors, string field, string value, int max)
        {
            if (value != null && value.Length > max)
                errors.Add(field + " may not exceed " + max + " characters.");
        }

        private static bool ContainsId(List<LookupItem> items, int id)
        {
            foreach (LookupItem item in items)
                if (item.Id == id) return true;
            return false;
        }

        private static string Clean(string value)
        {
            if (value == null) return null;
            value = value.Trim();
            return value.Length == 0 ? null : value;
        }
    }
}
