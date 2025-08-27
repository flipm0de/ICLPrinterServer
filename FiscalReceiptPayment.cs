using System;

namespace ICLPrinterServer
{
    public class FiscalReceiptPayment : FiscalReceiptLine
    {
        public int PaymentTypeId { get; }

        public decimal Amount { get; }

        public FiscalReceiptPayment (int paymentTypeId, decimal amount)
        {
            PaymentTypeId = paymentTypeId;
            Amount = amount;
            Total = -amount;
        }

        public override void Print (int charsPerLine)
        {
            var value = Amount + "  ";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine (GetPaymentType ().PadRight (charsPerLine - value.Length) + value);
        }

        private string GetPaymentType ()
        {
            switch (PaymentTypeId) {
                case 1:
                    return "КРЕДИТНА/ДЕБИТНА КАРТА";

                case 2:
                    return "БАНКА";

                case 3:
                    return "КУПОН";

                case 4:
                    return "ДПЪЛНИТЕЛНО 1";

                case 5:
                    return "ДПЪЛНИТЕЛНО 2";

                default:
                    return "В БРОЙ";
            }
        }

    }
}
