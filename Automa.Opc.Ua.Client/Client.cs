using Opc.Ua;
using Opc.Ua.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automa.Opc.Ua.Client.Services;
using Automa.Opc.Ua.Client.Interfaces;
// ReSharper disable AccessToDisposedClosure

namespace Automa.Opc.Ua.Client
{
    public class Client : IDisposable
    {
        internal static ISession SessionStub { get; set; }
        internal static IDiscoveryClient DiscoveryClientStub { get; set; }
        private ISession _session;
        private ClientOptions _options;
        private readonly Dictionary<string, Subscription> _subscriptions = new Dictionary<string, Subscription>();

        /// <summary>
        /// Creates a new instance of Client class
        /// </summary>
        /// <param name="options">The options for creating a new instance of Client class</param>
        /// <returns>A new instance of Client class</returns>
        public static async Task<Client> Create(ClientOptions options)
        {
            var config = new ApplicationConfiguration()
            {
                ApplicationName = options.ApplicationName,
                ApplicationType = ApplicationType.Client,
                ApplicationUri = $"urn:{Utils.GetHostName()}:OPCFoundation:HelloWorld",
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "X509Store",
                        StorePath = "CurrentUser\\UA_MachineDefault",
                        SubjectName = options.ApplicationName
                    },
                    TrustedPeerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "Directory",
                    },
                    TrustedIssuerCertificates = new CertificateTrustList
                    {
                        StoreType = "Directory",
                        StorePath = "Directory",
                    },
                    NonceLength = 32,
                    AutoAcceptUntrustedCertificates = options.AutoAcceptUntrustedCertificates
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
            };

            await config.Validate(ApplicationType.Client);

            config.SecurityConfiguration.ApplicationCertificate.Certificate = options.ApplicationCertificate;

            if (config.SecurityConfiguration.ApplicationCertificate.Certificate != null)
            {
                config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
            }


            if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                config.CertificateValidator.CertificateValidation += (validator, e) =>
                {
                    e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted);
                };
            }

            var endpointUri = new Uri(options.EndpointUrl);
            var configuration = EndpointConfiguration.Create(config);
            configuration.OperationTimeout = 10;
            EndpointDescriptionCollection endpoints = null;
            using (var client = DiscoveryClientStub != null ? DiscoveryClientStub.Create(
                endpointUri,
                EndpointConfiguration.Create(config)) : (new DiscoveryClientService()).Create(
                endpointUri,
                EndpointConfiguration.Create(config)))
            {
                await Task.Run(() =>
               {
                   client.GetEndpoints(null, null, null, null, out endpoints);
               });
            }
            endpoints = new EndpointDescriptionCollection(endpoints.Select(e =>
            {
                e.EndpointUrl = Utils.ReplaceLocalhost(e.EndpointUrl, endpointUri.DnsSafeHost);
                e.Server.DiscoveryUrls = StringCollection.ToStringCollection(e.Server.DiscoveryUrls
                    .Select(x => Utils.ReplaceLocalhost(x, endpointUri.DnsSafeHost))
                    .ToArray());
                return e;
            }));
            var endpointDescription = config.SecurityConfiguration.ApplicationCertificate.Certificate != null ?
                endpoints.Where(e => e.TransportProfileUri == Profiles.UaTcpTransport).OrderByDescending(e => e.SecurityLevel).FirstOrDefault() :
                endpoints.Where(e => e.TransportProfileUri == Profiles.UaTcpTransport).OrderBy(e => e.SecurityLevel).FirstOrDefault();
            endpointDescription = endpointDescription ?? new EndpointDescription(options.EndpointUrl);
            var ep = new ConfiguredEndpoint(endpointDescription.Server, EndpointConfiguration.Create(config));
            ep.Update(endpointDescription);
            return new Client
            {
                _session = SessionStub != null ?
                    await SessionStub.Create(config, ep, true, options.ApplicationName, options.SessionTimeout, new UserIdentity(new AnonymousIdentityToken()), null) :
                    await (new SessionService()).Create(config, ep, true, options.ApplicationName, options.SessionTimeout, new UserIdentity(new AnonymousIdentityToken()), null),
                _options = options
            };
        }

        /// <summary>
        /// Disposes the instance of Client class
        /// </summary>
        public void Dispose()
        {
            _session?.Dispose();
        }

        /// <summary>
        /// Watches a node for value changes
        /// </summary>
        /// <param name="tag">The tag of the node to be watched</param>
        /// <param name="notify">The function that handles the change</param>
        /// <param name="interval">The polling watch interval. If not specified, the DefaultPublishingInterval is used</param>
        /// <returns></returns>
        public async Task Watch(string tag, ChangeHandler notify, int interval = -1)
        {
            NodeId nodeId = tag;
            await Task.Run(() =>
            {
                var subscription = new Subscription(_session.GetDefaultSubscription()) { PublishingInterval = interval == -1 ? _options.DefaultPublishingInterval : interval };
                var list = new List<MonitoredItem> {
                    new MonitoredItem(subscription.DefaultItem)
                    {
                        DisplayName = tag,
                        StartNodeId = nodeId
                    }
                };
                list.ForEach(i => i.Notification += (item, e) =>
                {
                    notify?.Invoke(this, new ChangeEventArgs
                    {
                        Values = item.DequeueValues().Select(x => x.Value)
                    });
                });
                subscription.AddItems(list);
                _session.AddSubscription(subscription);
                _subscriptions.Add(tag, subscription);
            });
        }

        /// <summary>
        /// Stops watching a node for value changes
        /// </summary>
        /// <param name="tag">The tag of the node to be unwatched</param>
        public async Task Unwatch(string tag)
        {
            await Task.Run(() =>
            {
                if (!_subscriptions.Keys.Contains(tag)) return;
                var subscription = _subscriptions[tag];
                subscription.RemoveItems(subscription.MonitoredItems);
                subscription.Delete(true);
                _session.RemoveSubscription(subscription);
                subscription.Dispose();
                _subscriptions.Remove(tag);
            });
        }

        internal static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        internal static string ConvertToCamelCase(string phrase)
        {
            var splittedPhrase = phrase.Split(' ', '-', '.');
            var sb = new StringBuilder();

            foreach (var s in splittedPhrase)
            {
                var splittedPhraseChars = s.ToCharArray();
                if (splittedPhraseChars.Length > 0)
                {
                    splittedPhraseChars[0] = ((new string(splittedPhraseChars[0], 1)).ToUpper().ToCharArray())[0];
                }
                sb.Append(new string(splittedPhraseChars));
            }
            return sb.ToString();
        }

        ///// <summary>
        ///// Raises when the connection to the OPC server is lost.
        ///// </summary>
        //public event EventHandler LostConnection;

        ///// <summary>
        ///// Raises when the connection to the OPC server is restored.
        ///// </summary>
        //public event EventHandler RestoredConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ChangeHandler(object sender, ChangeEventArgs e);

        /// <summary>
        /// Reads a node, given its tag
        /// </summary>
        /// <param name="tag">The tag of the node to be read</param>
        /// <returns>the the values for the node corresponding to given tag</returns>
        public async Task<IEnumerable<object>> ReadNode(string tag)
        {
            NodeId nodeId = tag;
            var results = new DataValueCollection();
            DiagnosticInfoCollection diagnosticInfos;
            var nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value
                }
            };
            await Task.Run(() =>
               {
                   _session.Read(null, 0, TimestampsToReturn.Neither, nodesToRead, out results, out diagnosticInfos);
               });
            return results.Select(x => x.Value);
        }

        /// <summary>
        /// Reads a node, given its tag
        /// </summary>
        /// <param name="tag">The tag of the node to be read</param>
        /// <returns>the the values for the node corresponding to given tag</returns>
        public async Task<IEnumerable<Models.Node>> BrowseNode(string tag = null)
        {
            var nodeId = tag == null ? ObjectIds.ObjectsFolder : ExpandedNodeId.ToNodeId(tag, _session.GetNamespaceUris());
            var references = new ReferenceDescriptionCollection();
            byte[] continuationPoint;

            await Task.Run(() =>
               {
                   _session.Browse(
                       null,
                       null,
                       nodeId,
                       0u,
                       BrowseDirection.Forward,
                       ReferenceTypeIds.HierarchicalReferences,
                       true,
                       (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                       out continuationPoint,
                       out references);
               });
            return references.Select(x => new Models.Node
            {
                Tag = x.NodeId.ToString(),
                DisplayName = x.DisplayName.Text
            });
        }
    }
}
