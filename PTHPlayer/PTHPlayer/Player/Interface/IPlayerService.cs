using PTHPlayer.Player.Models;
using PTHPlayer.VideoPlayer.Models;
using System;
using System.Threading.Tasks;

namespace PTHPlayer.Player.Interfaces
{
    public interface IPlayerService
    {
        void Init(EventHandler<PlayerErrorEventArgs> errorEvent);
        void SetConfig(VideoConfigModel videoConfig, AudioConfigModel audioConfig);
        Task PacketReady();
        Task PreparePlayer();
        int VideoPacket(PacketModel response);
        int AudioPacket(PacketModel response);
        void Close();
        int GetPlayerState();
        void Play();
        TimeSpan GetPlayerTime();
    }
}
