using PTHPlayer.DataStorage.Service;
using PTHPlayer.HTSP;
using PTHPlayer.HTSP.Listeners;

namespace PTHPlayer.Controllers
{
    public class HTSPController
    {
        private HTSPService HTSPClient { get; }
        private DataService DataStorageClient { get; }
        private HTSPListener HTSListener { get; }

        public HTSPController(HTSPService hTSPClient, DataService dataStorageClient, HTSPListener hTSListener)
        {
            HTSPClient = hTSPClient;
            DataStorageClient = dataStorageClient;
            HTSListener = hTSListener;
        }

        public void Connect()
        {
            if (DataStorageClient.IsCredentialsExist())
            {

                var credentials = DataStorageClient.GetCredentials();

                if (HTSPClient.NeedRestart())
                {

                    HTSPClient.Open(credentials.Server, credentials.Port, HTSListener);

                    HTSPClient.Login(credentials.UserName, credentials.Password);

                    HTSPClient.EnableAsyncMetadata();
                }
            }
        }

        public void Close()
        {
            HTSPClient.Close();
        }
    }
}
