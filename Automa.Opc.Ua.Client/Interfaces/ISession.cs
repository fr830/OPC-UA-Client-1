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

        ResponseHeader Read(RequestHeader requestHeader, double maxAge, TimestampsToReturn timestampsToReturn,
            ReadValueIdCollection nodesToRead, out DataValueCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ResponseHeader Browse(RequestHeader requestHeader, ViewDescription view, NodeId nodeToBrowse, uint maxResultsToReturn, BrowseDirection browseDirection, NodeId referenceTypeId, bool includeSubtypes,
            uint nodeClassMask, out byte[] continuationPoint,
            out ReferenceDescriptionCollection references);

        NamespaceTable GetNamespaceUris();

        INode Find(ExpandedNodeId nodeId);
    }
}