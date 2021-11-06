using System;

namespace PTHPlayer.HTSP.Models
{
    public class OpenFileResponse
    {
        public int Id { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
