using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICLPrinterServer
{
    public enum CommandCodes
    {
        UserDefined = 0,

        DisplayClear = 33,
        DisplaySetLowerText = 35,

        //Non Fiscal
        OpenNonFiscal = 38,
        CloseNonFiscal = 39,
        PrintTextNonFiscal = 42,

        //  
        OpenFiscalReturn = 43,
        PaperFeed = 44,
        PaperCut = 45,
        DisplaySetUpperText = 47,

        //Fiscal
        OpenFiscal = 48,
        AddItem = 49,
        AddDisplayItem = 52,
        SubTotal = 51,
        Payment = 53,
        PrintTextFiscal = 54,
        CloseFiscal = 56,
        PrintClientInfo = 57,
        Annul = 60,

        SetDateTime = 61,
        GetDateTime = 62,
        DisplayShowDateTime = 63,
        DailyReport = 69,
        RegisterCash = 70,
        DetailFMReportByNumbers = 73,
        GetStatus = 74,
        GetFiscalStatus = 76,
        ShortFMReportByDates = 79,
        GetDiagnosticData = 90,
        DetailFMReportByDates = 94,
        ShortFMReportByNumbers = 95,
        SetVATRates = 96,
        GetVATRates = 97,
        GetTaxNumber = 99,
        DisplaySetText = 100,
        GetCheckStatus = 103,
        OperatorsReport = 105,
        OpenCashDrawer = 106,

        PrintDuplicate = 109,
        GetLastDocumentNumber = 113,
        SetGraphicalLogoBitmap = 115,
        OpenRotatedNonFiscal = 122,
        PrintTextRotatedNonFiscal = 123,
        CloseRotatedNonFiscal = 124,
        CloseOperation = 130,
        SetPrintFonts = 145
    }

    public enum Alignment
    {
        Left,
        Center,
        Right
    }

    public enum FiscalPrinterTaxGroup
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        TaxExempt,
        Default,
        NotSet
    }

    public enum FiscalReceiptType
    {
        Sale,
        Annulment,
        Return
    }
}
