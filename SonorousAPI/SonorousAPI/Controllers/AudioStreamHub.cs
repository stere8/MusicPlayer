using Microsoft.AspNetCore.SignalR;
using System.IO;

namespace SonorousAPI.Controllers
{
    public class AudioStreamHub : Hub
    {
        private string _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        public async Task StreamAudio(string filename)
        {
            var filePath = Path.Combine(_uploadsFolderPath, filename);

            if (!File.Exists(filePath))
            {
                await Clients.Caller.SendAsync("ReceiverError", "File Not Found");
                return;
            }

            byte[] buffer = new byte[65536]; // 64KB buffer size for streaming

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await Clients.Caller.SendAsync("ReceiveAudio", buffer.Take(bytesRead).ToArray());
                    }
                }
                await Clients.Caller.SendAsync("StreamComplete");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            }
        }
    }
}