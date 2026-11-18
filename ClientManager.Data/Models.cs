using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ClientManager.Data
{
    [DataContract]
    public class LookupItem
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string Name { get; set; }
    }

    [DataContract]
    public class LookupData
    {
        [DataMember] public List<LookupItem> Genders { get; set; } = new List<LookupItem>();
        [DataMember] public List<LookupItem> AddressTypes { get; set; } = new List<LookupItem>();
        [DataMember] public List<LookupItem> ContactTypes { get; set; } = new List<LookupItem>();
    }

    [DataContract]
    public class AddressDto
    {
        [DataMember] public int AddressId { get; set; }
        [DataMember] public int AddressTypeId { get; set; }
        [DataMember] public string AddressTypeName { get; set; }
        [DataMember] public string Line1 { get; set; }
        [DataMember] public string Line2 { get; set; }
        [DataMember] public string Suburb { get; set; }
        [DataMember] public string City { get; set; }
        [DataMember] public string PostalCode { get; set; }
        [DataMember] public string Country { get; set; }
    }

    [DataContract]
    public class ContactDto
    {
        [DataMember] public int ContactId { get; set; }
        [DataMember] public int ContactTypeId { get; set; }
        [DataMember] public string ContactTypeName { get; set; }
        [DataMember] public string Value { get; set; }
    }

    [DataContract]
    public class ClientDto
    {
        [DataMember] public int ClientId { get; set; }         
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
        [DataMember] public int GenderId { get; set; }
        [DataMember] public DateTime? DateOfBirth { get; set; }
        [DataMember] public string IdNumber { get; set; }
        [DataMember] public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        [DataMember] public List<ContactDto> Contacts { get; set; } = new List<ContactDto>();
    }

    [DataContract]
    public class ClientSummaryDto
    {
        [DataMember] public int ClientId { get; set; }
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
        [DataMember] public string GenderName { get; set; }
        [DataMember] public DateTime? DateOfBirth { get; set; }
        [DataMember] public string IdNumber { get; set; }
    }

    [DataContract]
    public class ClientPage
    {
        [DataMember] public List<ClientSummaryDto> Items { get; set; } = new List<ClientSummaryDto>();
        [DataMember] public int TotalCount { get; set; }
        [DataMember] public int PageNumber { get; set; }
        [DataMember] public int PageSize { get; set; }
    }
    public class ExportRow
    {
        public int ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string IdNumber { get; set; }
        public string AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}