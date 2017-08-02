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

        public ResponseHeader Read(RequestHeader requestHeader, double maxAge, TimestampsToReturn timestampsToReturn, ReadValueIdCollection nodesToRead, out DataValueCollection results, out DiagnosticInfoCollection diagnosticInfos)
        {
            return _session.Read(requestHeader, maxAge, timestampsToReturn, nodesToRead, out results, out diagnosticInfos);
        }

        public ResponseHeader Browse(RequestHeader requestHeader, ViewDescription view, NodeId nodeToBrowse, uint maxResultsToReturn, BrowseDirection browseDirection, NodeId referenceTypeId, bool includeSubtypes, uint nodeClassMask, out byte[] continuationPoint, out ReferenceDescriptionCollection references)
        {
            return _session.Browse(requestHeader, view, nodeToBrowse, maxResultsToReturn, browseDirection, referenceTypeId, includeSubtypes, nodeClassMask, out continuationPoint, out references);
        }

        public NamespaceTable GetNamespaceUris()
        {
            return _session.NamespaceUris;
        }

        public INode Find(ExpandedNodeId nodeId)
        {
            return _session.NodeCache.Find(nodeId);
        }
    }
}