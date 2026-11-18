using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using ClientManager.Data;

namespace ClientManager.Business
{
    [ServiceContract(Namespace = "http://clientmanager/2026")]
    public interface IClientService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        LookupData GetLookups();

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ClientPage SearchClients(string searchText, int pageNumber, int pageSize);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ClientDto GetClient(int clientId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        int SaveClient(ClientDto client);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DeleteClient(int clientId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Stream ExportClientsCsv();
    }

    [DataContract]
    public class ServiceFault
    {
        [DataMember] public string Message { get; set; }
        [DataMember] public System.Collections.Generic.List<string> ValidationErrors { get; set; } =
            new System.Collections.Generic.List<string>();
    }
}
