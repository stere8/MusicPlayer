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

            // Read the entire MP3 file into a byte array
            byte[] audioData;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                audioData = new byte[fs.Length];
                await fs.ReadAsync(audioData, 0, (int)fs.Length);
            }

            // Send the entire audio file as a Base64 string (or as binary data)
            // You can choose either method based on your client-side handling
            var base64Audio = Convert.ToBase64String(audioData);
            await Clients.Caller.SendAsync("ReceiveAudio", base64Audio);

            // Notify the client that streaming is complete
            await Clients.Caller.SendAsync("StreamComplete");
        }
    }
}