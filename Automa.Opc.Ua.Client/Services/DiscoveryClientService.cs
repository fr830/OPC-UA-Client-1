using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Automa.Opc.Ua.Client.Interfaces;
using Opc.Ua;
using Opc.Ua.Client;

namespace Automa.Opc.Ua.Client.Services
{
    internal class DiscoveryClientService : IDiscoveryClient
    {
        private DiscoveryClient _discoveryClient;

        public ResponseHeader GetEndpoints(RequestHeader requestHeader, string endpointUrl, StringCollection localeIds, StringCollection profileUris, out EndpointDescriptionCollection endpoints)
        {
            return _discoveryClient.GetEndpoints(requestHeader, endpointUrl, localeIds, profileUris, out endpoints);
        }

        public IDiscoveryClient Create(Uri discoveryUrl, EndpointConfiguration configuration)
        {
            _discoveryClient = DiscoveryClient.Create(discoveryUrl, configuration);
            return this;
        }

        public void Dispose()
        {
            _discoveryClient.Dispose();
        }
    }
}