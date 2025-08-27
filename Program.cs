using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ICLPrinterServer
{
    internal class Program
    {
        private const int PORT_NUMBER = 4999;

        private static async Task Main (string [] args)
        {
            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);

            var listener = new TcpListener (IPAddress.Any, PORT_NUMBER);
            listener.Start ();
            var cts = new CancellationTokenSource ();

            Console.WriteLine ($"Listening on port {PORT_NUMBER}");
            Console.CancelKeyPress += (sender, e) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine ("Ctrl+C pressed, stopping the listener...");
                    cts.Cancel ();
                    e.Cancel = true; // Prevent the application from terminating immediately
                };

            try {
                while (!cts.IsCancellationRequested) {
                    var client = await listener.AcceptTcpClientAsync (cts.Token);
                    // Run each client connection asynchronously so we can accept new connections
                    _ = Task.Run (() => {
                        try {
                            new PrinterServer (client).Run ();
                        } catch (Exception ex) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine ($"Error handling client: {ex.Message}");
                            Console.ResetColor ();
                        }
                    });
                }
            } catch (OperationCanceledException) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine ("Operation cancelled.");
            } finally {
                listener.Stop ();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine ("Listener stopped.");
            }
        }
    }
}
