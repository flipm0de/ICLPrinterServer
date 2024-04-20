using System;
using System.Collections.Generic;
using System.Linq;

namespace ICLPrinterServer
{
    public class FiscalReceipt
    {
        public FiscalReceiptType ReceiptType { get; set; }

        public string[] Header { get; set; }

        public string USN { get; set; }

        public int Number { get; set; }

        public DateTime TimeStamp { get; set; }

        public bool TotalIsPrinted { get; set; }

        public string SourceDocumentNumber { get; set; }

        public DateTime SourceDocumentDateTime { get; set; }

        public string SourceFiscalMemory { get; set; }

        private List<FiscalReceiptLine> details;
        public List<FiscalReceiptLine> Details
        {
            get => details ?? (details = new List<FiscalReceiptLine> ());
            set => details = value;
        }

        public FiscalReceipt()
        {
            TimeStamp = DateTime.Now;
        }

        public void PrintHeader(int charsPerLine)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var str in Header)
                Console.WriteLine(str);

            if (!string.IsNullOrWhiteSpace(USN))
                PrintCentered(charsPerLine, $"УНП: {USN}");

            switch (ReceiptType)
            {
                case FiscalReceiptType.Annulment:
                    PrintStornoSubHeader(charsPerLine, "Операторска грешка");
                    break;
                case FiscalReceiptType.Return:
                    PrintStornoSubHeader(charsPerLine, "Връщане/рекламация");
                    break;
            }

            Console.WriteLine();
        }

        private void PrintStornoSubHeader(int charsPerLine, string subtitle)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            PrintCentered(charsPerLine, "СТОРНО");
            PrintCentered(charsPerLine, subtitle);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Документ: {SourceDocumentNumber}/{SourceDocumentDateTime:dd-MM-yyyy HH:mm:ss}");
            Console.WriteLine($"ФП: {SourceFiscalMemory}");
        }

        public void PrintTotal(int charsPerLine)
        {
            if (TotalIsPrinted)
                return;

            Console.WriteLine("----------  ".PadLeft(charsPerLine));

            Console.ForegroundColor = ConsoleColor.White;
            var total = details.Sum(d => d.Total) + "  ";
            Console.WriteLine("ОБЩА СУМА".PadRight(charsPerLine - total.Length) + total);
            Console.WriteLine();
            
            TotalIsPrinted = true;
        }

        private void PrintCentered(int charsPerLine, string value, char padding = ' ')
        {
            Console.WriteLine(value.PadLeft((charsPerLine - value.Length) / 2 + value.Length, padding).PadRight(charsPerLine, padding));
        }
    }
}
