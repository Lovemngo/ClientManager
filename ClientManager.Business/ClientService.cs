using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using ClientManager.Data;

namespace ClientManager.Business
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ClientService : IClientService
    {
        public const int MaxPageSize = 200;
        private const int DefaultPageSize = 25;

        private readonly ClientRepository _repository;

        public ClientService()
            : this(new ClientRepository(ConfigurationManager.ConnectionStrings["ClientDb"].ConnectionString))
        {
        }

        public ClientService(ClientRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            _repository = repository;
        }

        public LookupData GetLookups()
        {
            return Execute(() => _repository.GetLookups());
        }

        public ClientPage SearchClients(string searchText, int pageNumber, int pageSize)
        {
            return Execute(() =>
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = DefaultPageSize;
                if (pageSize > MaxPageSize) pageSize = MaxPageSize;   
                return _repository.Search(searchText, pageNumber, pageSize);
            });
        }

        public ClientDto GetClient(int clientId)
        {
            return Execute(() => _repository.GetClient(clientId));
        }

        public int SaveClient(ClientDto client)
        {
            return Execute(() =>
            {
                if (client == null)
                    throw new BusinessValidationException("No client was supplied.");

                ClientValidator.Normalise(client);

                List<string> errors = ClientValidator.Validate(client, _repository.GetLookups());
                if (errors.Count == 0 && client.IdNumber != null &&
                    _repository.IdNumberExists(client.IdNumber, client.ClientId))
                {
                    errors.Add("Another client already has this ID number.");
                }
                if (errors.Count > 0)
                    throw new BusinessValidationException(errors);

                try
                {
                    return _repository.Save(client);
                }
                catch (KeyNotFoundException)
                {
                    throw new BusinessValidationException("This client no longer exists. Refresh the list and try again.");
                }
            });
        }

        public void DeleteClient(int clientId)
        {
            Execute<object>(() =>
            {
                if (!_repository.Delete(clientId))
                    throw new BusinessValidationException("This client no longer exists.");
                return null;
            });
        }

        public Stream ExportClientsCsv()
        {
            return Execute<Stream>(() =>
            {
                string path = CsvExporter.WriteToTempFile(_repository.StreamExportRows());
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                      bufferSize: 65536, options: FileOptions.DeleteOnClose);
            });
        }

        // -- error handling

        private static T Execute<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (BusinessValidationException ex)
            {
                var fault = new ServiceFault
                {
                    Message = "Please correct the following:",
                    ValidationErrors = new List<string>(ex.Errors)
                };
                throw new FaultException<ServiceFault>(fault, new FaultReason(fault.Message));
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Trace.TraceError("ClientService error: " + ex);
                var fault = new ServiceFault { Message = "An unexpected error occurred. Please try again or contact support." };
#if DEBUG
               
                fault.Message += " [" + ex.GetType().Name + ": " + ex.Message + "]";
#endif
                throw new FaultException<ServiceFault>(fault, new FaultReason(fault.Message));
            }
        }
    }
}