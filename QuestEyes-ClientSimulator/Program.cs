using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Timer = System.Timers.Timer;

bool connecting = true;
bool connected = false;
Timer udpTimer = new();

//print info
Console.Title = "QuestEyes Device Simulator";
Console.WriteLine("QuestEyes Device Simulator by Steven Wheeler");
Console.WriteLine("This program simulates a QuestEyes device for development purposes.");

//prep the UDP client and communication socket
UdpClient discoveryUDP = new();
TcpListener communicationSocket = new(IPAddress.Parse("127.0.0.1"), 7580);

//generate a device identifier prefaced with SIM
int _min = 0001;
int _max = 9999;
Random _rdm = new();
int identifier = _rdm.Next(_min, _max);
string simDeviceName = "SIM" + identifier.ToString();
Console.WriteLine("\nSimulating as: " + simDeviceName);
Console.Title = "QuestEyes Device Simulator - " + simDeviceName;

//identify the images to be looped over the feed
string exePath = Assembly.GetExecutingAssembly().Location;
var fileDirectory = Path.GetDirectoryName(exePath) + "/Images";

//check if folder does not exist
if (!Directory.Exists(fileDirectory))
{
    Directory.CreateDirectory(fileDirectory);
}

//check if folder contains .jpgs
int detectedAmount = Directory.GetFiles(fileDirectory, "*.jpg").Length;
if (detectedAmount == 0)
{
    Console.WriteLine("No .JPG images found in Images folder. Please note that subfolders are ignored. Exiting...");
    Environment.Exit(0);
}
else
{
    Console.WriteLine("Found " + detectedAmount + " image(s) to loop over connection.");
    //Begin broadcasting presence for the server, like a actual QuestEyes device would
    udpTimer.Elapsed += (sender, e) => udpSend(discoveryUDP, connected, simDeviceName);
    udpTimer.Interval = 1000;
    communicationSocket.Start();
    udpTimer.Start();
}

Console.WriteLine("Broadcasting towards listening server applications on local device...");
Console.WriteLine("Communication socket is open and waiting for connection.");

while (connecting) {
    Console.WriteLine("Waiting for connection...");
    TcpClient client = communicationSocket.AcceptTcpClient();
    Console.WriteLine("Connection received.");
    connecting = true;
    NetworkStream stream = client.GetStream();
    string sendName = "NAME QuestEyes-" + simDeviceName;
    string sendFirmware = "FIRMWARE_VER SIMULATOR";
    byte[] msg = System.Text.Encoding.ASCII.GetBytes(sendName);
    stream.Write(msg, 0, msg.Length);
    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg)));
    msg = System.Text.Encoding.ASCII.GetBytes(sendFirmware);
    stream.Write(msg, 0, msg.Length);
    Console.WriteLine(String.Format("Sent: {0}", System.Text.Encoding.UTF8.GetString(msg)));
    connected = true;
    connecting = false;
    while (connected) { };
}

//timer for sending presence packets
static void udpSend(UdpClient udpClient, bool connected, string simDeviceName)
{
    string broadcastString = "QUESTEYE_REQ_CONN:" + ("QuestEyes-" + simDeviceName) + ":127.0.0.1";
    byte[] broadcastBytes = Encoding.ASCII.GetBytes(broadcastString);
    try
    {
        if (!connected)
        {
            udpClient.Send(broadcastBytes, broadcastBytes.Length, "127.0.0.1", 7579);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}