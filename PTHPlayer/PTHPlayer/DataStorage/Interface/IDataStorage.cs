using PTHPlayer.Models;

namespace PTHPlayer.DataStorage
{
    public interface IDataStorage
    {
        CredentialsModel GetCredentials();
        void SaveCredentials(CredentialsModel credentials);
        void ClearCredentials();
        string GetField(string name);
        void SaveField(string name, string value);
    }
}
