namespace ICLPrinterServer
{
    internal class Program
    {
        private static void Main (string [] args)
        {
            new PrinterServer(4999).Run();
        }
    }
}
