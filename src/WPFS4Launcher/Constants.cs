using System.Net;

namespace WPFS4Launcher
{
    internal class Constants
    {
        public static bool KRClient = false;

        public static MainWindow LoginWindow;
        public static IPEndPoint ConnectEndPoint = new IPEndPoint(Dns.GetHostAddresses("127.0.0.1")[0], 28001);
    }
}
