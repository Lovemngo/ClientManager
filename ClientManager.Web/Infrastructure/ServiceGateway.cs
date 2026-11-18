using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Text;
using System.Web;
using ClientManager.Business;

namespace ClientManager.Web
{
    public static class ServiceGateway
    {
        private static readonly ConcurrentDictionary<string, ChannelFactory<IClientService>> Factories =
            new ConcurrentDictionary<string, ChannelFactory<IClientService>>();

        private static ChannelFactory<IClientService> GetFactory()
        {
            Uri address = new Uri(HttpContext.Current.Request.Url, VirtualPathUtility.ToAbsolute("~/ClientService.svc"));
            return Factories.GetOrAdd(address.AbsoluteUri, key =>
                new ChannelFactory<IClientService>("ClientServiceEndpoint", new EndpointAddress(key)));
        }

        public static T Call<T>(Func<IClientService, T> operation)
        {
            IClientService proxy = GetFactory().CreateChannel();
            var channel = (IClientChannel)proxy;
            try
            {
                T result = operation(proxy);
                channel.Close();
                return result;
            }
            catch
            {
                channel.Abort();
                throw;
            }
        }

        public static void Call(Action<IClientService> operation)
        {
            Call<object>(s => { operation(s); return null; });
        }
        public static string Describe(Exception ex)
        {
            var fault = ex as FaultException<ServiceFault>;
            if (fault != null)
            {
                var sb = new StringBuilder(fault.Detail.Message);
                foreach (string error in fault.Detail.ValidationErrors)
                    sb.Append(" ").Append(error);
                return sb.ToString();
            }

            return "An unexpected error occurred.";
        }
    }
}