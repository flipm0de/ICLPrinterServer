using System;
using System.Collections.Generic;

namespace ICLPrinterServer
{
    public class NonFiscalReceipt
    {
        public string[] Header { get; set; }

        public int Number { get; set; }

        public DateTime TimeStamp { get; set; }

        private List<FiscalReceiptLine> lines;
        public List<FiscalReceiptLine> Lines
        {
            get => lines ?? (lines = new List<FiscalReceiptLine> ());
            set => lines = value;
        }

        public NonFiscalReceipt()
        {
            TimeStamp = DateTime.Now;
        }

        public void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var str in Header)
                Console.WriteLine(str);

            Console.WriteLine();
        }
    }
}
