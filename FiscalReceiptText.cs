using System;

namespace ICLPrinterServer
{
    public class FiscalReceiptText : FiscalReceiptLine
    {
        public string Text { get; }

        public FiscalReceiptText (string text)
        {
            Text = text;
        }

        public override void Print (int charsPerLine)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine (Text);
        }
    }
}
