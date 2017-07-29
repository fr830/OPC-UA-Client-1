using System.Security.Cryptography.X509Certificates;

namespace Automa.Opc.Ua.Client
{
    public class ClientOptions
    {
        /// <summary>
        /// The name of the application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The endpoint URL of the OPC UA Server to connect to
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        /// The certificate of the application
        /// </summary>
        public X509Certificate2 ApplicationCertificate { get; set; }

        /// <summary>
        /// The default session timeout for the client
        /// </summary>
        public uint SessionTimeout { get; set; }

        /// <summary>
        /// The default publishing interval when monitoring a node
        /// </summary>
        public int DefaultPublishingInterval { get; set; }

        /// <summary>
        /// Determines if untrusted certificates are automatically accepted
        /// </summary>
        public bool AutoAcceptUntrustedCertificates { get; set; }

        public ClientOptions()
        {
            AutoAcceptUntrustedCertificates = true;
            SessionTimeout = 60000;
            DefaultPublishingInterval = 1000;
        }
    }
}
