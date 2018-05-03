using System.Net;

namespace WPFS4Launcher
{
    internal class Constants
    {
        public static bool KRClient = false;

        public static MainWindow LoginWindow;
        public static IPEndPoint ConnectEndPoint = new IPEndPoint(Dns.GetHostAddresses("80.82.215.36")[0], 28001);
    }
}
