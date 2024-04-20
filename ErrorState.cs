using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICLPrinterServer
{
    public class ErrorState
    {
        public const string SyntaxError = "SyntaxError";
        public const string InvalidOperationCode = "InvalidOperationCode";
        public const string ClockNotSet = "ClockNotSet";
        public const string DateNotSet = "DateNotSet";
        public const string ClockHardwareError = "ClockHardwareError";
        public const string PrintHeadFault = "PrintHeadFault";
        public const string PrintHeadOverheat = "PrintHeadOverheat";
        public const string GeneralError = "GeneralError";

        public const string SumOverflow = "SumOverflow";
        public const string CmdNotAllowed = "CmdNotAllowed";
        public const string RAM_IsReset = "RAM_IsReset";
        public const string RAM_Corrupted = "RAM_Corrupted";
        public const string LidOpen = "LidOpen";

        public const string NoPaper = "NoPaper";
        public const string NoPaperNoReprint = "NoPaperNoReprint";
        public const string LittlePaper = "LittlePaper";
        public const string EKL_NoPaper = "EKL_NoPaper";
        public const string FiscalCheckOpen = "FiscalCheckOpen";
        public const string EKL_LittlePaper = "EKL_LittlePaper";
        public const string EKL_MemoryLow = "EKL_MemoryLow";
        public const string EKL_Overflow = "EKL_Overflow";
        public const string EKL_CompactFormat = "EKL_CompactFormat";
        public const string EKL_DetailedFormat = "EKL_DetailedFormat";
        public const string EKL_Error = "EKL_Error";
        public const string NonFiscalCheckOpen = "NonFiscalCheckOpen";
        public const string RotatedNonFiscalCheckOpen = "RotatedNonFiscalCheckOpen";
        public const string AnnulCheckOpen = "AnnulCheckOpen";

        public const string Switch1IsOn = "Switch1IsOn";
        public const string Switch2IsOn = "Switch2IsOn";
        public const string Switch3IsOn = "Switch3IsOn";
        public const string Switch4IsOn = "Switch4IsOn";
        public const string Switch5IsOn = "Switch5IsOn";
        public const string Switch6IsOn = "Switch6IsOn";
        public const string Switch7IsOn = "Switch7IsOn";
        public const string Switch8IsOn = "Switch8IsOn";

        public const string FiscalMemReadError = "FiscalMemReadError";
        public const string FiscalMemWriteError = "FiscalMemWriteError";
        public const string NoFiscalMemInstalled = "NoFiscalMemInstalled";
        public const string FiscalMemLowSpace = "FiscalMemLowSpace";
        public const string FiscalMemFull = "FiscalMemFull";
        public const string FiscalMemGeneralError = "FiscalMemGeneralError";

        public const string FiscalMemIsReadOnly = "FiscalMemIsReadOnly";
        public const string FiscalMemIsFormatted = "FiscalMemIsFormatted";
        public const string FiscalMode = "FiscalMode";
        public const string TaxValuesAreSet = "TaxValuesAreSet";
        public const string SerialNumbersAreSet = "SerialNumbersAreSet";

        public const string Report24HRequired = "Report24HRequired";
        public const string WaitingPaperReplaceConfirmation = "WaitingPaperReplaceConfirmation";
        public const string IllegalTaxGroup = "IllegalTaxGroup";
        public const string NoValidTaxGroup = "NoValidTaxGroup";
        public const string LoginFailed3Times = "LoginFailed3Times";
        public const string BadPrinterStatusReceived = "BadPrinterStatusReceived";
        public const string BadSerialPort = "BadSerialPort";
        public const string BadConnectionParameters = "BadCommunicationParameters";
        public const string NotEnoughCashInTheRegister = "NotEnoughCashInTheRegister";
        public const string FiscalPrinterOverflow = "FiscalPrinterOverflow";
        public const string FiscalPrinterNotReady = "FiscalPrinterNotReady";
        public const string TooManyTransactionsInReceipt = "TooManyTransactionsInReceipt";
        public const string VATGroupsMismatch = "VATGroupsMismatch";
        public const string InvalidInputData = "InvalidInputData";
        public const string FreePriceLimitation = "EvalPriceLimitation";
        public const string FreeQttyLimitation = "EvalQttyLimitation";
        public const string FreeItemsLimitation = "EvalItemsLimitation";
        public const string FreeLimitation = "EvalLimitation";
        public const string CommandNotAcknoledged = "CommandNotAcknoledged";
        public const string CommandNotSupported = "CommandNotSupported";
        public const string PowerInterruption = "PowerInterruption";
        public const string DailyReportNotEmpty = "DailyReportNotEmpty";
        public const string ItemsReportNotEmpty = "ItemsReportNotEmpty";
        public const string OperatorsReportNotEmpty = "OperatorsReportNotEmpty";
        public const string ReportsOverflow = "ReportsOverflow";
        public const string DuplicateNotPrinted = "DuplicateNotPrinted";
        public const string RegularReceipt = "RegularReceipt";
        public const string PrintVATInReceipt = "PrintVATInReceipt";
        public const string ReceiptIsInvoice = "ReceiptIsInvoice";
        public const string UsingNonIntegerNumbers = "UsingNonIntegerNumbers";
        public const string UsingIntegerNumbers = "UsingIntegerNumbers";
        public const string AutoCutPaper = "AutoCutPaper";
        public const string TransparentDisplay = "TransparentDisplay";
        public const string CommunicationSpeed9600 = "CommunicationSpeed9600";
        public const string CommunicationSpeed19200 = "CommunicationSpeed19200";
        public const string AutoOpenSafe = "AutoOpenSafe";
        public const string PrintLogoOnReceipts = "PrintLogoOnReceipts";
        public const string PrintTotalInForeignCurrency = "PrintTotalInForeignCurrency";
        public const string BadPassword = "BadPassword";
        public const string BadLogicalAddress = "BadLogicalAddress";
        public const string TaxTerminalError = "TaxTerminalError";
        public const string IncorrectItemInformation = "IncorrectItemInformation";
        public const string OldProtocolUsed = "OldProtocolUsed";
        public const string NewProtocolUsed = "NewProtocolUsed";
        public const string NRAConnectionError = "NRAConnectionError";
        public const string ModemConnectionError = "ModemConnectionError";
        public const string SIMCardError = "SIMCardError";
        public const string BatteryLow = "BatteryLow";

        public const string KitchenPrinterError = "KitchenPrinterError";
        public const string KitchenPrinterNoPaper = "KitchenPrinterNoPaper";

        public const string CashReceiptPrinterDisconnected = "CashReceiptPrinterDisconnected";
        public const string CashDrawerDisconnected = "CashDrawerDisconnected";
        public const string FiscalPrinterBusy = "FiscalPrinterBusy";
        public const string NonFiscalPrinterDisconnected = "NonFiscalPrinterDisconnected";
        public const string ExternalDisplayDisconnected = "ExternalDisplayDisconnected";
        public const string KitchenPrinterDisconnected = "KitchenPrinterDisconnected";
        public const string CardReaderDisconnected = "CardReaderDisconnected";
        public const string ElectronicScaleDisconnected = "ElectronicScaleDisconnected";
        public const string ElectronicScaleNotEnabled = "ElectronicScaleNotEnabled";
        public const string SalesDataControllerDisconnected = "SalesDataControllerDisconnected";
        public const string BarcodeScannerDisconnected = "BarcodeScannerDisconnected";
        public const string AccessControllerDisconnected = "AccessControllerDisconnected";
        public const string DriverNotFound = "DriverNotFound";
        public const string CertificateNotFound = "CertificateNotFound";

        public const string IncorrectWorkflowManager = "IncorrectWorkflowManager";
        public const string UnableToCreateUniqueSaleNumber = "UnableToCreateUniqueSaleNumber";
        public const string ErrorHandled = "ErrorHandled";

        public static readonly string [] AllLicenseLimitations = { FreeLimitation, FreeItemsLimitation, FreePriceLimitation, FreeQttyLimitation };

        private readonly HashSet<string> informations = new HashSet<string> ();
        public HashSet<string> Informations
        {
            get { return informations; }
        }

        private string command;
        public string Command
        {
            get { return command; }
            set { command = value; }
        }

        public ErrorState ()
        {
            informations.Add (FiscalMemIsFormatted);
            informations.Add (FiscalMode);
            informations.Add (NewProtocolUsed);
        }

        public ErrorState (string error)
            : this ()
        {
            Set (error);
        }

        public ErrorState Set (string error)
        {
            informations.Add (error);

            return this;
        }

        public ErrorState SetInformation (string error)
        {
            informations.Add (error);

            return this;
        }

        public bool Check (params string [] errorCodes)
        {
            return errorCodes.Any (ec => informations.Contains (ec));
        }

        public bool Unset (string error)
        {
            if (informations.Contains (error)) {
                informations.Remove (error);
                return true;
            }

            return false;
        }

        public void Clear ()
        {
            informations.Clear ();
            command = null;
        }

        public void Load (ErrorState state)
        {
            Clear ();
            if (state == null)
                return;

            foreach (var item in state.informations)
                informations.Add (item);

            command = state.command;
        }

        public override string ToString ()
        {
            var ret = new StringBuilder ();
            if (command != null)
                ret.Append ("Command: " + command);

            var info = string.Join (", ", informations);
            if (info.Length > 0) {
                if (ret.Length > 0)
                    ret.Append ("\n");

                ret.Append ($"Information: {info}");
            }

            return ret.ToString ();
        }
    }
}
