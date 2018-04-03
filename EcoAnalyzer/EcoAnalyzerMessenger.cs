using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Net.Security;
using System.IO;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Linq;
using Eco.Gameplay.Systems.Chat;
using System.Security.Authentication;

namespace EcoAnalyzer
{
    public class NoCommonSupportedVersionAvailable : Exception
    {
        public NoCommonSupportedVersionAvailable() : base("No common EcoAnalyzer protocol version available")
        {
        }
    }

    public class EcoAnalyzerMessenger : IDisposable
    {
        readonly private string host;
        readonly private int port;
        readonly private int[] supportedVersions = new int[] { 1 };
        readonly private TcpClient client;
        readonly private ConcurrentQueue<string> messagesToSend = new ConcurrentQueue<string>();

        private SslStream stream;

        private byte[] writeBuffer;

        public EcoAnalyzerMessenger(string host, int port)
        {
            this.host = host;
            this.port = port;
            client = new TcpClient(AddressFamily.InterNetwork);
        }
         
        public async Task Start()
        {
            await client.ConnectAsync(host, port);
            var certificate = new X509Certificate(await LoadServerCertificate());
            stream = new SslStream(client.GetStream());
            await stream.AuthenticateAsClientAsync(host, new X509CertificateCollection(new X509Certificate[] { certificate }), SslProtocols.Default, true);
            if (!stream.RemoteCertificate.Equals(certificate)) throw new Exception("Connection is not authenticated");
            // Perform versioning handshake
            var tempBuffer = new byte[1024];
            var read = await stream.ReadAsync(tempBuffer, 0, tempBuffer.Length);
            var message = Encoding.UTF8.GetString(tempBuffer, 0, read);
            var version = EcoAnalyzerProtocolHandshake.Handshake(message, supportedVersions);
            // Throw if version is 0 and thus no supported value available
            if (version == 0) throw new NoCommonSupportedVersionAvailable();
            // Communicate the version to use
            tempBuffer = Encoding.UTF8.GetBytes(EcoAnalyzerProtocolHandshake.UseVersionMessage(version));
            await stream.WriteAsync(tempBuffer, 0, tempBuffer.Length);
            // Start the message loop
            RunMessageLoop();
        }

        public void Stop()
        {
            messagesToSend.Enqueue("STOP");
        }

        public void Dispose()
        {
            client.Close();
        }

        private async Task<X509Certificate> LoadServerCertificate()
        {
            var stream =  Assembly.GetExecutingAssembly().GetManifestResourceStream("EcoAnalyzer.certificate.der");
            var buffer = new byte[2044];
            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            return new X509Certificate2(buffer.Take(read).ToArray());
        }

        private void RunMessageLoop()
        {
            Task.Run(async () => {
                while (true)
                {
                    string messageToSend;
                    var available = messagesToSend.TryDequeue(out messageToSend);
                    if (available)
                    {
                        if (messageToSend == "STOP") break;
                        await Handle(messageToSend);
                    } else
                    {
                        ChatManager.ServerMessageToAllLoc("EcoAnalyzer tickzors", true);
                        await Task.Delay(2000);
                    }
                }
            });
        }

        private async Task Handle(string message)
        {
            writeBuffer = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
        }
    }
}
