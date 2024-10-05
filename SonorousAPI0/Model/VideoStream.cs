using System.Net;

namespace SonorousAPI.Model;

public class VideoStream
{
    private readonly string _filename;
    private string _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");


    public VideoStream(string filename)
    {
        var _filename = Path.Combine(_uploadsFolderPath, filename);
    }

    public async void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
    {
        try
        {
            var buffer = new byte[65536];

            using (var video = File.Open(_filename, FileMode.Open, FileAccess.Read))
            {
                var length = (int)video.Length;
                var bytesRead = 1;

                while (length > 0 && bytesRead > 0)
                {
                    bytesRead = video.Read(buffer, 0, Math.Min(length, buffer.Length));
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                    length -= bytesRead;
                }
            }
        }
        catch (Exception ex)
        {
            return;
        }
        finally
        {
            outputStream.Close();
        }
    }
}