using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System;

namespace _2_Pizza_Thread
{
    internal class Handler
    {
        private const int BUFFER_SIZE = 1024;
        private readonly TcpClient client;

        public Handler(TcpClient client)    // 각 스레드마다 클라이언트 소켓을 부여받는다.
        {
            this.client = client;
        }

        public void Run()   // 스레드마다 서버의 역할 수행
        {
            // 클라이언트의 Client를 IPEndPoint로 변환
            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"Connected to {clientEndPoint}");

            // 클라이언트의 스트림을 가져옴
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                while (true)
                {
                    // 클라이언트 정보 수신
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead == 0) break;

                    // 클라이언트가 보낸 메시지 저장
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string response;

                    if (int.TryParse(received, out int order))
                    {
                        response = $"Thank you for ordering {order} pizzas!\n";
                    }
                    else
                    {
                        response = "Wrong number of pizzas, please try again\n";
                    }

                    // 받은 메시지를 서버에 전송
                    Console.WriteLine($"Sending message to {clientEndPoint}");
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            finally
            {
                // 서버 연결 종료
                Console.WriteLine($"Connection with {clientEndPoint} has been closed");
                client.Close();
            }
        }
    }

    internal class ThreadServer
    {
        private const int PORT = 12345;
        private readonly TcpListener server;

        // 생성자를 활용해 서버 소켓 세팅
        public ThreadServer()   // 서버 소켓 세팅
        {
            try
            {
                IPEndPoint localAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);
                Console.WriteLine($"Starting up at: {localAddress}");
                server = new TcpListener(localAddress);
                server.Start();
            }
            catch (SocketException)
            {
                server?.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        public void Start()
        {
            // 서버 시작
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                // 클라리언트가 들어올 때까지 대기하가 들어오면 스레드 생성 및 스레드 시작 및 연결
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Client connection request from {client.Client.RemoteEndPoint}");

                    Handler handler = new Handler(client);
                    Thread thread = new Thread(new ThreadStart(handler.Run));
                    thread.Start();
                } // 클라이언트 요청이 들어올 때마다 새로운 스레드를 생성한다.
            }
            finally
            {
                // 서버 정지
                server.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }
        static void Main()
        {
            // 스레드기반 서버 시작
            ThreadServer server = new ThreadServer();
            server.Start();
        }
    }
}