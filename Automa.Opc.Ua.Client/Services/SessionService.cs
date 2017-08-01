using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Automa.Opc.Ua.Client.Interfaces;
using Opc.Ua;
using Opc.Ua.Client;

namespace Automa.Opc.Ua.Client.Services
{
    internal class SessionService : ISession
    {
        Session _session;

        public void AddSubscription(Subscription subscription)
        {
            _session.AddSubscription(subscription);
            subscription.Create();
        }

        public async Task<ISession> Create(ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, bool updateBeforeConnect, string sessionName, uint sessionTimeout, IUserIdentity identity, IList<string> preferredLocales)
        {
            _session = await Session.Create(configuration, endpoint, updateBeforeConnect, sessionName, sessionTimeout, identity, preferredLocales);
            return this;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public Subscription GetDefaultSubscription()
        {
            return _session.DefaultSubscription;
        }

        public void RemoveSubscription(Subscription subscription)
        {
            _session.RemoveSubscription(subscription);
        }
    }
}