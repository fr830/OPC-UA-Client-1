using System;
using Opc.Ua.Client;
using System.Threading.Tasks;
using Opc.Ua;
using System.Collections.Generic;

namespace Automa.Opc.Ua.Client.Interfaces
{
    public interface ISession : IDisposable
    {
        Subscription GetDefaultSubscription();

        Task<ISession> Create(ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, bool updateBeforeConnect, string sessionName, uint sessionTimeout, IUserIdentity identity, IList<string> preferredLocales);

        void AddSubscription(Subscription subscription);

        void RemoveSubscription(Subscription subscription);
    }
}