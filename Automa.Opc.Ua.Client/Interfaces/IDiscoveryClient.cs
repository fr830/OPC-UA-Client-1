using System;
using Opc.Ua.Client;
using System.Threading.Tasks;
using Opc.Ua;
using System.Collections.Generic;

namespace Automa.Opc.Ua.Client.Interfaces
{
    public interface IDiscoveryClient : IDisposable
    {
        IDiscoveryClient Create(Uri discoveryUrl, EndpointConfiguration configuration);

        ResponseHeader GetEndpoints(RequestHeader requestHeader, string endpointUrl, StringCollection localeIds, StringCollection profileUris, out EndpointDescriptionCollection endpoints);
    }
}