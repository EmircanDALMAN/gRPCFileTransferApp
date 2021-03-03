using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using grpcFileTransportDownloadClient;

namespace grpcDownloadClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new FileService.FileServiceClient(channel);
            string downloadPath = @"C:\Users\emiq1\OneDrive\Masaüstü\GRPCFileStreamingExample\grpcDownloadClient\DownloadFiles";

            var fileInfo = new grpcFileTransportDownloadClient.FileInfo
            {
                FileExtension = ".m4v",
                FileName = "ornekvideo"
            };
            FileStream fileStream = null;
            var request = client.FileDownload(fileInfo);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int count = 0;
            decimal chunkSize = 0;
            while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                if (count++ == 0)
                {
                    fileStream = new FileStream(@$"{downloadPath}\{request.ResponseStream.Current.Info.FileName}{request.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);
                    fileStream.SetLength(request.ResponseStream.Current.FileSize);
                }
                var buffer = request.ResponseStream.Current.Buffer.ToByteArray();
                await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);
                System.Console.WriteLine($"{Math.Round((chunkSize += request.ResponseStream.Current.ReadedByte) * 100) / request.ResponseStream.Current.FileSize}%");
            }
            await fileStream.DisposeAsync();
            fileStream.Close();
        }
    }
}
