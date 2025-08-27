using System;

namespace ICLPrinterServer
{
    public class FiscalReceiptItem : FiscalReceiptLine
    {
        public string Item { get; }

        public decimal Quantity { get; }

        public string MeasUnit { get; }

        public decimal Price { get; }

        public decimal ExtraPercent { get; }

        public FiscalPrinterTaxGroup TaxGroup { get; }

        public FiscalReceiptItem (string item, decimal quantity, string measUnit, decimal price,
            decimal extraPercent, FiscalPrinterTaxGroup taxGroup)
        {
            Item = item;
            Quantity = Math.Round (quantity, 3);
            MeasUnit = measUnit;
            Price = Math.Round (price, 2);
            ExtraPercent = extraPercent;
            TaxGroup = taxGroup;

            Total = Math.Round ((Price + Math.Round (Price * extraPercent / 100, 2)) * Quantity, 2);
        }

        public override void Print (int charsPerLine)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            if (Quantity != 1)
                Console.WriteLine ($"{Quantity} x {Price}");

            var itemName = Item;
            if (itemName.Length > 36) {
                Console.WriteLine (itemName.Substring (0, 36));
                itemName = itemName.Substring (36);
            }

            var totalString = $" {Total} {TaxGroupToString (TaxGroup)}";
            Console.WriteLine (itemName.PadRight (charsPerLine - totalString.Length, ' ') + totalString);
        }

        private string TaxGroupToString (FiscalPrinterTaxGroup group)
        {
            switch (group) {
                case FiscalPrinterTaxGroup.A:
                    return "А";
                case FiscalPrinterTaxGroup.B:
                    return "Б";
                case FiscalPrinterTaxGroup.C:
                    return "В";
                case FiscalPrinterTaxGroup.D:
                    return "Г";
                default:
                    return "Б";
            }
        }
    }
}
