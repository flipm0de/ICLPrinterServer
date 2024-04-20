using System;

namespace ICLPrinterServer
{
    public class FiscalReceiptText : FiscalReceiptLine
    {
        public string Text { get; }

        public FiscalReceiptText(string text)
        {
            Text = text;
        }

        public virtual void Print(int charsPerLine)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(Text);
        }
    }
}
