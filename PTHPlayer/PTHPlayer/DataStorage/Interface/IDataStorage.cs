using PTHPlayer.Models;

namespace PTHPlayer.DataStorage
{
    public interface IDataStorage
    {
        CredentialsModel GetCredentials();
        void SaveCredentials(CredentialsModel credentials);
    }
}
