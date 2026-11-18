using System.Collections.Generic;
using ClientManager.Data;

namespace ClientManager.Web.Models
{
    public class ClientListViewModel
    {
        public string Search { get; set; }
        public ClientPage Page { get; set; }
        public int TotalPages { get; set; }
    }
    public class AddressRowModel
    {
        public string Index { get; set; }
        public AddressDto Address { get; set; }
        public List<LookupItem> Types { get; set; }
    }
    public class ContactRowModel
    {
        public string Index { get; set; }
        public ContactDto Contact { get; set; }
        public List<LookupItem> Types { get; set; }
    }
}
