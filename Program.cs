using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CafeteriaApp
{
    class CafeteriaServer
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(5, 5); // Máximo de 5 estudiantes en la cafetería

        static async Task Main(string[] args)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:9090/");
            httpListener.Start();

            Console.WriteLine("Servidor de Cafetería iniciado. Esperando conexiones...");

            while (true)
            {
                var context = await httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleWebSocketAsync(webSocketContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        static async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            while (true)
            {
                await semaphore.WaitAsync();

                try
                {
                    // Entra un estudiante a la cafetería
                    string message = "Bienvenido a la cafetería. Puede ordenar su café.";
                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                    // Simula el tiempo que un estudiante pasa en la cafetería
                    await Task.Delay(5000); // 5 segundos

                    // Sale un estudiante de la cafetería
                    semaphore.Release();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}