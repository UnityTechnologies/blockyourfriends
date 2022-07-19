namespace BlockYourFriends.Multiplayer
{
    /// <summary>
    /// Just for displaying the anonymous Relay IP.
    /// </summary>
    public class ServerAddress
    {
        private string ip;
        private int port;

        public string IP => ip;
        public int Port => port;

        public ServerAddress(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public override string ToString()
        {
            return $"{ip}:{port}";
        }
    }
}
