using PTHPlayer.DataStorage;
using PTHPlayer.Models;
using PTHPlayer.Tizen.TV.Services;
using System;
using System.Buffers.Binary;
using System.Text;
using Tizen.Security.SecureRepository;

[assembly: Xamarin.Forms.Dependency(typeof(StorageService))]
namespace PTHPlayer.Tizen.TV.Services
{
    public class StorageService : IDataStorage
    {
        public CredentialsModel GetCredentials()
        {
            var credentials = new CredentialsModel();
            try
            {
                var serverBytes = DataManager.Get("Server", "");
                credentials.Server = Encoding.UTF8.GetString(serverBytes);
                var portBytes = DataManager.Get("Port", "");
                credentials.Port = BinaryPrimitives.ReadInt32LittleEndian(portBytes);
                var userNameBytes = DataManager.Get("UserName", "");
                credentials.UserName = Encoding.UTF8.GetString(userNameBytes);
                var passwordBytes = DataManager.Get("Password", "");
                credentials.Password = Encoding.UTF8.GetString(passwordBytes);
            }
            catch
            {
                //No stored values do exception
            }
            return credentials;
        }
        public void SaveCredentials(CredentialsModel credentials)
        {
            DataManager.Save("Server", Encoding.UTF8.GetBytes(credentials.Server), new Policy());
            DataManager.Save("Port", BitConverter.GetBytes(credentials.Port), new Policy());
            DataManager.Save("UserName", Encoding.UTF8.GetBytes(credentials.UserName), new Policy());
            DataManager.Save("Password", Encoding.UTF8.GetBytes(credentials.Password), new Policy());
        }
    }
}
