using System;
using System.Threading;
using HidSharp;

class Program
{
    static void Main()
    {
        const int vendorId = 0x7C92;
        const int productId = 0x0001;
        start:
        HidDevice rawDevice = null;

        // Find the Raw HID interface with exactly 33-byte reports
        foreach (var dev in DeviceList.Local.GetHidDevices(vendorId, productId))
        {
            if (dev.GetMaxOutputReportLength() == 33 &&
                dev.GetMaxInputReportLength() == 33)
            {
                rawDevice = dev;
                break;
            }
        }

        if (rawDevice == null)
        {
            Console.WriteLine("Raw HID interface not found.");
            return;
        }

        try
        {
            using (var stream = rawDevice.Open())
            {
                stream.ReadTimeout = Timeout.Infinite;
                stream.WriteTimeout = 2000;

                // Start the reader thread
                Thread readerThread = new Thread(() => ReaderLoop(stream));
                readerThread.IsBackground = true; // Stops when main exits
                readerThread.Start();

                Console.WriteLine("Reader thread started. Press Enter to send a test message or Ctrl+C to quit.");

                while (true)
                {
                    Console.ReadLine(); // Wait for user input to send
                    byte[] outBuffer = new byte[33];
                    outBuffer[0] = 0x01; // Example command
                    outBuffer[1] = 0xAA; // Example payload

                    stream.Write(outBuffer);
                    Console.WriteLine("Sent command to keyboard.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HID error: {ex.Message}");
            goto start; 
        }
    }

    static void ReaderLoop(HidStream stream)
    {
        byte[] inBuffer = new byte[33];

        while (true)
        {
            try
            {
                int bytesRead = stream.Read(inBuffer, 0, inBuffer.Length);
                if (bytesRead > 0)
                {
                    // Print first few bytes for debugging
                    Console.WriteLine($"Received {bytesRead} bytes: " +
                        BitConverter.ToString(inBuffer, 0, bytesRead));
                }
            }
            catch (TimeoutException)
            {
                // Shouldn't happen with Timeout.Infinite
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reader error: {ex.Message}");
                break;
            }
        }
    }
}