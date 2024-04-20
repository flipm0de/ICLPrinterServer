# ICLPrinterServer

A simple console application which is emulating in part **Datecs FP-700X** fiscal printer over LAN.

The idea is to use this app to develop and test software that integrates with a such a printer without actually owning the physical device or moving around with one.

Currently, this project supports the minimum set of commands needed to print a fiscal receipt, non-fiscal receipt and a storno (cash receipt return).

Over time support for more commands may be added as needed or support for other fiscal devices. Currently, device manufacturers like **Datecs**, **Daisy**, **Incotex**, **Eltrade** are using the same protocol with small variations so it will be easy to adapt the code to support them too if the future proves this to be an actual need.
