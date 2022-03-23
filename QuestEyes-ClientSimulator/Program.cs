using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

bool connecting = false;
bool connected = false;

//print info
Console.Title = "QuestEyes Client Simulator";
Console.WriteLine("QuestEyes Client Simulator");
Console.WriteLine("This program simulates a QuestEyes device for development purposes.");

//generate a device identifier prefaced with SIM
int _min = 0001;
int _max = 9999;
Random _rdm = new Random();
int identifier = _rdm.Next(_min, _max);
string simDeviceName = "QuestEyes-SIM" + identifier.ToString();
Console.WriteLine("\nSimulating as: " + simDeviceName);
Console.Title = "QuestEyes Client Simulator - " + simDeviceName;

//Get local IP
string localIP = string.Empty;
string strHostName = Dns.GetHostName();

IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
IPAddress[] addr = ipEntry.AddressList;
for (int i = 0; i < addr.Length; i++)
{
    if (!addr[i].ToString().Contains(":"))
    {
         localIP = addr[i].ToString();
         Console.WriteLine("Local IP: " + localIP + "\n");
    }
}
Console.Title = "QuestEyes Client Simulator - " + simDeviceName + " - " + localIP;

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
} else
{
    Console.WriteLine("Found " + detectedAmount + " image(s) to loop over connection.");
}

//Begin the connection to the server, like a actual QuestEyes device would
try
{
    Console.WriteLine("Broadcasting towards listening server applications...");
    Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    IPAddress serverAddr = IPAddress.Parse("255.255.255.255");
    IPEndPoint endPoint = new IPEndPoint(serverAddr, 7579);
    Byte[] broadcastString = Encoding.ASCII.GetBytes("QUESTEYE_REQ_CONN:" + simDeviceName + ":" + localIP);
    udp.EnableBroadcast = true;

    Timer udpTimer = new();
    udpTimer.Elapsed += new ElapsedEventHandler(udpSend(udp, broadcastString, endPoint));
    udpTimer.Interval = 10000;
    udpTimer.Start();

} catch (Exception e) {
    Console.WriteLine(e);
}

ElapsedEventHandler? udpSend(Socket udp, Byte[] broadcastString, IPEndPoint endPoint)
{
    while (connecting == false && connected == false)
    {
        udp.SendTo(broadcastString, endPoint);
    }
    return null;
}

