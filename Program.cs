using System;
using System.Net.Sockets;
using PacketDotNet;
using SharpPcap;

namespace IGMPPacketSniffer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get a list of available devices
            var devices = CaptureDeviceList.Instance;

            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine.");
                return;
            }

            // Display the list of devices
            Console.WriteLine("The following devices are available on this machine:");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine($"{i}) {devices[i].Description}");
            }

            Console.WriteLine("Select a device by index:");
            int index = int.Parse(Console.ReadLine());

            // Get the selected device
            var device = devices[index];

            // Open the device for capturing
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);
            device.Open(DeviceMode.Promiscuous, 1000);

            // Start capturing
            Console.WriteLine($"Capturing on {device.Description}...");
            device.StartCapture();

            Console.WriteLine("Press Enter to stop capturing...");
            Console.ReadLine();

            // Stop capturing when done
            device.StopCapture();
            device.Close();
        }

        private static void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            // Parse the packet as an Ethernet packet
            var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            // Check if the packet is an IP packet
            var ipPacket = packet.Extract<InternetPacket>();
            if (ipPacket != null)
            {
                // Check if the packet is an IGMP packet
                var igmpPacket = packet.Extract<IgmpV2Packet>();
                if (igmpPacket != null)
                {
                    Console.WriteLine($"IGMP Packet detected: {igmpPacket}");
                }
            }
        }
    }
}
