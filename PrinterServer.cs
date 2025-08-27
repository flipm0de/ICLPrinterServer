using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ICLPrinterServer
{
    internal class PrinterServer
    {
        private const byte MARKER_START = 0x01;
        private const byte MARKER_ARGS_SEPARATOR = 0x04;
        private const byte MARKER_ARGS_END = 0x05;
        private const byte MARKER_END = 0x03;
        private const byte MARKER_ACK = 0x06;
        private const byte MARKER_NACK = 0x15;
        private const byte MARKER_SYNC = 0x16;
        private const int MAX_SYNC_COUNT = 300;
        private const byte TAB = 0x09;
        private const char TAB_C = '\t';

        private readonly TcpClient client;
        private readonly int charsPerLine = 48;
        private readonly int port;
        private readonly string deviceName = "Vladster virtual test printer";
        private readonly string fwRevision = "000001";
        private readonly string fwDate = "13APR24";
        private readonly string fwTime = "0000";
        private readonly string fwCRC = "9999";
        private readonly string deviceSerialNumber = "DT123456";
        private readonly string fiscalMemoryNumber = "01234567";

        private Encoding encoding = Encoding.GetEncoding (1251);
        private ErrorState currentErrorState;
        private byte currentSequence;
        private FiscalReceipt lastFiscalReceipt;
        private FiscalReceipt fiscalReceipt;
        private NonFiscalReceipt nonFiscalReceipt;
        private int nonFiscalReceiptCounter;
        private int fiscalReceiptCounter;
        private string [] fiscalHeader = new [] { "Тестова Компания ООД", "София, бул. Цар Борис III", "ЕИК: 111222333", "Магазин \"Центъра\"", "София, бул. Цар Борис III", "ЗДДС № BG111222333" };

        public PrinterServer (TcpClient client)
        {
            this.client = client;
            currentErrorState = new ErrorState ();
        }

        public void Run ()
        {
            var stream = client.GetStream ();
            stream.ReadTimeout = 100;
            var lastDataReceived = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine ("Accepted connection...");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine ();

            var commandBuffer = new List<byte> ();
            var commandReady = false;

            while (true) {
                var buffer = new byte [1024];
                int received;
                try {
                    received = stream.Read (buffer, 0, buffer.Length);
                } catch (Exception) {
                    received = 0;
                }

                if (received == 0) {
                    if (lastDataReceived.AddSeconds (20) < DateTime.Now) {
                        stream.Dispose ();
                        client.Close ();
                    }

                    if (!client.Connected) {
                        client.Dispose ();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine ("Disconnected.");
                        return;
                    }

                    Thread.Sleep (500);
                    continue;
                }

                lastDataReceived = DateTime.Now;

                int i;
                for (i = 0; i < received; i++) {
                    var inByte = buffer [i];
                    if (inByte == MARKER_ACK)
                        continue;

                    if (commandBuffer.Count != 0 ||
                        inByte == MARKER_START)
                        commandBuffer.Add (inByte);

                    if (inByte == MARKER_END) {
                        commandReady = true;
                        break;
                    }
                }

                if (!commandReady)
                    continue;

                int len = (commandBuffer [1] - '0') << 0xC;
                len += (commandBuffer [2] - '0') << 0x8;
                len += (commandBuffer [3] - '0') << 0x4;
                len += (commandBuffer [4] - '0');

                currentSequence = commandBuffer [5];

                int value = (commandBuffer [6] - '0') << 0xC;
                value += (commandBuffer [7] - '0') << 0x8;
                value += (commandBuffer [8] - '0') << 0x4;
                value += (commandBuffer [9] - '0');
                var command = (CommandCodes) value;

                var argsBuffer = new List<byte> ();
                i = 10;
                for (; i < commandBuffer.Count; i++) {
                    if (commandBuffer [i] == MARKER_ARGS_END)
                        break;

                    argsBuffer.Add (commandBuffer [i]);
                }

                int crc = (commandBuffer [i + 1] - '0') << 0xC;
                crc += (commandBuffer [i + 2] - '0') << 0x8;
                crc += (commandBuffer [i + 3] - '0') << 0x4;
                crc += (commandBuffer [i + 4] - '0');

                try {
                    ProcessCommand (stream, command, argsBuffer.ToArray ());

                    commandReady = false;
                    commandBuffer.Clear ();
                } catch (IOException e) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine ("IOException: " + e.Message);
                }
            }
        }

        private void ProcessCommand (NetworkStream stream, CommandCodes command, byte [] commandArgs)
        {
            byte [] message;
            List<byte> args;
            string [] strings;
            string text;
            FiscalReceiptText fiscalReceiptText;
            int itemsCount;
            switch (command) {
                case CommandCodes.GetStatus:
                    message = PackMessage ((int) command);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.GetFiscalStatus:
                    byte isOpen = 0;
                    if (fiscalReceipt != null) {
                        switch (fiscalReceipt.ReceiptType) {
                            case FiscalReceiptType.Sale:
                                isOpen = 1;
                                break;
                            case FiscalReceiptType.Annulment:
                                isOpen = 2;
                                break;
                            case FiscalReceiptType.Return:
                                isOpen = 3;
                                break;
                        }
                    } else if (nonFiscalReceipt != null)
                        isOpen = 5;

                    args = new List<byte> { 0, TAB, isOpen, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);
                    args.AddRange (encoding.GetBytes (fiscalReceipt?.Details.Count (d => d is FiscalReceiptItem).ToString (CultureInfo.InvariantCulture) ?? "0"));
                    args.Add (TAB);
                    args.AddRange (encoding.GetBytes (fiscalReceipt?.Details.OfType<FiscalReceiptItem> ().Sum (d => d.Total).ToString ("N2", CultureInfo.InvariantCulture) ?? "0.00"));
                    args.Add (TAB);
                    args.AddRange (encoding.GetBytes (fiscalReceipt?.Details.OfType<FiscalReceiptPayment> ().Sum (p => p.Amount).ToString ("N2", CultureInfo.InvariantCulture) ?? "0.00"));

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.GetDateTime:
                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (DateTime.Now.ToString ("dd-MM-yy HH:mm:ss")));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.GetDiagnosticData:
                    message = PackMessage ((int) command, encoding.GetBytes ($"{deviceName},{fwRevision} {fwDate} {fwTime},{fwCRC},00000000,{deviceSerialNumber},{fiscalMemoryNumber}"));
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.CloseRotatedNonFiscal:
                    if (nonFiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (nonFiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.Annul:
                    if (fiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    fiscalReceipt = null;
                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.OpenFiscal:
                    if (fiscalReceipt != null || nonFiscalReceipt != null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    strings = encoding.GetString (commandArgs).Split (TAB_C);
                    if (strings.Length < 4) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    fiscalReceipt = new FiscalReceipt
                    {
                        Header = fiscalHeader.Select (value => value.PadLeft ((charsPerLine - value.Length) / 2 + value.Length)).ToArray (),
                        Number = ++fiscalReceiptCounter
                    };
                    if (strings.Length > 4 && Regex.IsMatch (strings [2], @"^\w{8}-\w{4}-\d{7}$"))
                        fiscalReceipt.USN = strings [2];

                    fiscalReceipt.PrintHeader (charsPerLine);
                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.OpenFiscalReturn:
                    if (fiscalReceipt != null || nonFiscalReceipt != null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    strings = encoding.GetString (commandArgs).Split (TAB_C);
                    if (strings.Length < 7) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    fiscalReceipt = new FiscalReceipt
                    {
                        ReceiptType = strings [3] == "0" ? FiscalReceiptType.Annulment : FiscalReceiptType.Return,
                        Header = fiscalHeader.Select (value => value.PadLeft ((charsPerLine - value.Length) / 2 + value.Length)).ToArray (),
                        Number = ++fiscalReceiptCounter,
                        SourceDocumentNumber = strings [4],
                        SourceDocumentDateTime = DateTime.TryParseExact (strings [5], "dd-MM-yy HH:mm:ss", null, DateTimeStyles.AssumeLocal, out var dateTime) ? dateTime : DateTime.Now,
                        SourceFiscalMemory = strings [6]
                    };
                    if (strings.Length > 10 && Regex.IsMatch (strings [10], @"^\w{8}-\w{4}-\d{7}$"))
                        fiscalReceipt.USN = strings [10];

                    fiscalReceipt.PrintHeader (charsPerLine);
                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.PrintTextFiscal:
                    if (fiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    strings = encoding.GetString (commandArgs).Split (TAB_C);
                    text = strings [0];
                    if (text.Length > charsPerLine - 2)
                        text = text.Substring (0, charsPerLine - 2);
                    else if (text.Length < charsPerLine - 2)
                        text = text.PadRight (charsPerLine - 2);

                    fiscalReceiptText = new FiscalReceiptText ($"#{text}#");
                    fiscalReceiptText.Print (charsPerLine);
                    fiscalReceipt.Details.Add (fiscalReceiptText);

                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.AddItem:
                    if (fiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    strings = encoding.GetString (commandArgs).Split (TAB_C);
                    if (strings.Length < 6) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    var itemName = strings [0];
                    var taxGroup = TaxGroupFromString (strings [1]);
                    var price = decimal.TryParse (strings [2], NumberStyles.Any, CultureInfo.InvariantCulture, out var decValue) ? decValue : 0;
                    var qtty = decimal.TryParse (strings [3], NumberStyles.Any, CultureInfo.InvariantCulture, out decValue) ? decValue : 0;
                    var isDiscount = strings [4] == "2";
                    var discount = decimal.TryParse (strings [5], NumberStyles.Any, CultureInfo.InvariantCulture, out decValue) ? decValue : 0;
                    var measUnit = strings.Length > 7 ? strings [7] : "бр.";

                    var fiscalReceiptAddItem = new FiscalReceiptItem (itemName, qtty, measUnit, price,
                        isDiscount ? -discount : discount, taxGroup);
                    fiscalReceiptAddItem.Print (charsPerLine);
                    fiscalReceipt.Details.Add (fiscalReceiptAddItem);

                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.Payment:
                    if (fiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    strings = encoding.GetString (commandArgs).Split (TAB_C);
                    if (strings.Length != 3 && strings.Length != 2) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    if (!int.TryParse (strings [0], NumberStyles.Any, CultureInfo.InvariantCulture, out var paymentType) || paymentType < 0 || paymentType > 5) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    if (!decimal.TryParse (strings [1], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)) {
                        ReturnError (stream, command, ErrorState.SyntaxError);
                        return;
                    }

                    fiscalReceipt.PrintTotal (charsPerLine);

                    var fiscalReceiptPayment = new FiscalReceiptPayment (paymentType, amount);
                    fiscalReceiptPayment.Print (charsPerLine);
                    fiscalReceipt.Details.Add (fiscalReceiptPayment);
                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.CloseFiscal:
                    if (fiscalReceipt == null || fiscalReceipt.Details.Sum (d => d.Total) > 0) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    itemsCount = fiscalReceipt.Details.Count (d => d is FiscalReceiptItem);
                    PrintKeyValuePair (fiscalReceipt.Number.ToString ().PadLeft (7, '0'), itemsCount.ToString (CultureInfo.InvariantCulture) + " " + (itemsCount > 1 ? "АРТИКУЛА" : "АРТИКУЛ"));
                    PrintCentered ("ДАТА " + fiscalReceipt.TimeStamp.ToString ("dd.MM.yyyy") + "  ЧАС " + fiscalReceipt.TimeStamp.ToString ("HH:mm:ss"));
                    Console.ForegroundColor = ConsoleColor.White;
                    PrintCentered ("ФИСКАЛЕН БОН");
                    PrintKeyValuePair (deviceSerialNumber, fiscalMemoryNumber);
                    lastFiscalReceipt = fiscalReceipt;
                    fiscalReceipt = null;

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    PrintCentered ("<cut>", '-');

                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.PrintDuplicate:
                    if (lastFiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    Console.WriteLine ();

                    lastFiscalReceipt.PrintHeader (charsPerLine);
                    Console.ForegroundColor = ConsoleColor.White;
                    PrintCentered ("ДУБЛИКАТ");
                    Console.ResetColor ();
                    Console.WriteLine ();

                    lastFiscalReceipt.TotalIsPrinted = false;
                    foreach (var line in lastFiscalReceipt.Details) {
                        if (line is FiscalReceiptPayment)
                            lastFiscalReceipt.PrintTotal (charsPerLine);

                        line.Print (charsPerLine);
                    }

                    itemsCount = lastFiscalReceipt.Details.Count (d => d is FiscalReceiptItem);
                    PrintKeyValuePair (lastFiscalReceipt.Number.ToString ().PadLeft (7, '0'), itemsCount.ToString (CultureInfo.InvariantCulture) + " " + (itemsCount > 1 ? "АРТИКУЛА" : "АРТИКУЛ"));
                    PrintCentered ("ДАТА " + lastFiscalReceipt.TimeStamp.ToString ("dd.MM.yyyy") + "  ЧАС " + lastFiscalReceipt.TimeStamp.ToString ("HH:mm:ss"));
                    Console.ForegroundColor = ConsoleColor.White;
                    PrintCentered ("ФИСКАЛЕН БОН");
                    PrintKeyValuePair (deviceSerialNumber, fiscalMemoryNumber);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    PrintCentered ("<cut>", '-');

                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.OpenNonFiscal:
                    if (fiscalReceipt != null || nonFiscalReceipt != null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    nonFiscalReceipt = new NonFiscalReceipt
                    {
                        Header = fiscalHeader.Select (value => value.PadLeft ((charsPerLine - value.Length) / 2 + value.Length)).ToArray (),
                        Number = ++fiscalReceiptCounter
                    };

                    nonFiscalReceipt.PrintHeader ();
                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.PrintTextNonFiscal:
                    if (nonFiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    text = encoding.GetString (commandArgs);
                    if (text.Length > charsPerLine - 2)
                        text = text.Substring (0, charsPerLine - 2);
                    else if (text.Length < charsPerLine - 2)
                        text = text.PadRight (charsPerLine - 2);

                    fiscalReceiptText = new FiscalReceiptText ($"#{text}#");
                    fiscalReceiptText.Print (charsPerLine);
                    nonFiscalReceipt.Lines.Add (fiscalReceiptText);

                    message = PackMessage ((int) command, 0, TAB);
                    stream.Write (message, 0, message.Length);
                    return;

                case CommandCodes.CloseNonFiscal:
                    if (nonFiscalReceipt == null) {
                        ReturnError (stream, command, ErrorState.GeneralError);
                        return;
                    }

                    PrintCentered ("ДАТА " + nonFiscalReceipt.TimeStamp.ToString ("dd.MM.yyyy") + "  ЧАС " + nonFiscalReceipt.TimeStamp.ToString ("HH:mm:ss"));
                    Console.ForegroundColor = ConsoleColor.White;
                    PrintCentered ("* СЛУЖЕБЕН БОН *");
                    PrintKeyValuePair (deviceSerialNumber, fiscalMemoryNumber);
                    nonFiscalReceipt = null;

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    PrintCentered ("<cut>", '-');

                    args = new List<byte> { 0, TAB };
                    args.AddRange (encoding.GetBytes (fiscalReceiptCounter.ToString (CultureInfo.InvariantCulture)));
                    args.Add (TAB);

                    message = PackMessage ((int) command, args.ToArray ());
                    stream.Write (message, 0, message.Length);
                    return;

                default:
                    ReturnError (stream, command, ErrorState.GeneralError);
                    return;
            }
        }

        private FiscalPrinterTaxGroup TaxGroupFromString (string group)
        {
            switch (group) {
                case "1":
                    return FiscalPrinterTaxGroup.A;
                case "2":
                    return FiscalPrinterTaxGroup.B;
                case "3":
                    return FiscalPrinterTaxGroup.C;
                case "4":
                    return FiscalPrinterTaxGroup.D;
                default:
                    return FiscalPrinterTaxGroup.B;
            }
        }

        private void PrintKeyValuePair (string key, string value, char padding = ' ')
        {
            Console.WriteLine (key.PadRight (charsPerLine - value.Length, padding) + value);
        }

        private void PrintCentered (string value, char padding = ' ')
        {
            Console.WriteLine (value.PadLeft ((charsPerLine - value.Length) / 2 + value.Length, padding).PadRight (charsPerLine, padding));
        }

        private void ReturnError (NetworkStream client, CommandCodes command, string errorState)
        {
            currentErrorState.Set (errorState);
            var message = PackMessage ((int) command);
            client.Write (message, 0, message.Length);
        }

        private byte [] PackMessage (int command, params byte [] commandArgs)
        {
            List<byte> res = new List<byte> ();

            #region Write header data

            int argsLen = commandArgs?.Length ?? 0;

            res.Add (MARKER_START);

            var len = argsLen + 10 + 0x20; //10 = (4 <LEN> + 1 <SEQ> + 4 <CMD> + 1 <END>) + 0x20 <OFFSET>
            res.Add ((byte) (((len >> 0xC) & 0xf) + '0'));
            res.Add ((byte) (((len >> 0x8) & 0xf) + '0'));
            res.Add ((byte) (((len >> 0x4) & 0xf) + '0'));
            res.Add ((byte) (((len >> 0x0) & 0xf) + '0'));

            res.Add (currentSequence);

            res.Add ((byte) (((command >> 0xC) & 0xf) + '0'));
            res.Add ((byte) (((command >> 0x8) & 0xf) + '0'));
            res.Add ((byte) (((command >> 0x4) & 0xf) + '0'));
            res.Add ((byte) (((command >> 0x0) & 0xf) + '0'));

            #endregion

            // Write operation data
            if (commandArgs != null)
                res.AddRange (commandArgs);

            res.Add (MARKER_ARGS_SEPARATOR);

            res.AddRange (PackStatus (currentErrorState));

            res.Add (MARKER_ARGS_END);

            // Calculate checksum
            int checkSum = 0;
            for (int i = 1; i < res.Count; i++)
                checkSum += res [i];

            // Write the check sum
            res.Add ((byte) (((checkSum >> 0xC) & 0xf) + '0'));
            res.Add ((byte) (((checkSum >> 0x8) & 0xf) + '0'));
            res.Add ((byte) (((checkSum >> 0x4) & 0xf) + '0'));
            res.Add ((byte) (((checkSum >> 0x0) & 0xf) + '0'));

            // Write terminator
            res.Add (MARKER_END);

            return res.ToArray ();
        }

        private byte [] PackStatus (ErrorState errorState)
        {
            byte b0 = 0;
            if (errorState.Check (ErrorState.SyntaxError))
                b0 |= 0x01;
            if (errorState.Check (ErrorState.InvalidOperationCode))
                b0 |= 0x02;
            if (errorState.Check (ErrorState.PrintHeadFault))
                b0 |= 0x10;
            if (errorState.Check (ErrorState.GeneralError))
                b0 |= 0x20;
            if (errorState.Check (ErrorState.LidOpen))
                b0 |= 0x40;

            byte b1 = 0;
            if (errorState.Check (ErrorState.SumOverflow))
                b1 |= 0x01;
            if (errorState.Check (ErrorState.CmdNotAllowed))
                b1 |= 0x02;

            byte b2 = 0;
            if (errorState.Check (ErrorState.NoPaper))
                b2 |= 0x01;
            if (errorState.Check (ErrorState.LittlePaper))
                b2 |= 0x02;
            if (errorState.Check (ErrorState.EKL_NoPaper))
                b2 |= 0x04;
            if (errorState.Check (ErrorState.FiscalCheckOpen))
                b2 |= 0x08;
            if (errorState.Check (ErrorState.EKL_LittlePaper))
                b2 |= 0x10;
            if (errorState.Check (ErrorState.NonFiscalCheckOpen))
                b2 |= 0x20;

            byte b4 = 0;
            if (errorState.Check (ErrorState.FiscalMemWriteError))
                b4 |= 0x01;
            if (errorState.Check (ErrorState.TaxValuesAreSet))
                b4 |= 0x02;
            if (errorState.Check (ErrorState.SerialNumbersAreSet))
                b4 |= 0x04;
            if (errorState.Check (ErrorState.FiscalMemLowSpace))
                b4 |= 0x08;
            if (errorState.Check (ErrorState.FiscalMemFull))
                b4 |= 0x10;
            if (errorState.Check (ErrorState.FiscalMemGeneralError))
                b4 |= 0x20;

            byte b5 = 0;
            if (errorState.Check (ErrorState.FiscalMemIsFormatted))
                b5 |= 0x02;
            if (errorState.Check (ErrorState.FiscalMode))
                b5 |= 0x08;

            byte b6 = 0;
            if (errorState.Check (ErrorState.NewProtocolUsed))
                b6 |= 0x80;

            return new byte [] { b0, b1, b2, 0, b4, b5, b6, 0, 0 };
        }
    }
}
