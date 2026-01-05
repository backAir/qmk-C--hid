using System;
using HidSharp;

class Program
{
    static void Main()
    {
        const int vendorId = 0x7C92;
        const int productId = 0x0001;

        HidDevice rawDevice = null;

        foreach (var dev in DeviceList.Local.GetHidDevices(vendorId, productId))
        {
            // QMK Raw HID always uses 32-byte OUT reports
            if (dev.GetMaxOutputReportLength() == 33)
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
                byte[] buffer = new byte[32];
                buffer[0] = 0x01; // command
                buffer[1] = 0xFF;

                stream.Write(buffer);
            }

            Console.WriteLine("Raw HID message sent.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HID error: {ex.Message}");
        }
    }
}
