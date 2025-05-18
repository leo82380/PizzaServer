using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

namespace _1_Pizza_ServerClient
{
    internal class Server
    {
        private const int BUFFER_SIZE = 1024;
        // IP주소, 포트번호
        private static readonly IPEndPoint ADDRESS = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
        private TcpListener serverSocket;

        // 서버 생성자를 활용해 서버 소켓 생성 및 시작
        public Server()     // 서버 소켓 생성
        {
            try
            {
                Console.WriteLine($"Starting up at: {ADDRESS}");
                serverSocket = new TcpListener(ADDRESS);
                serverSocket.Start();
            }
            catch (SocketException)
            {
                Console.WriteLine("\nServer failed to start.");
                serverSocket?.Stop();
            }
        }

        public TcpClient Accept()   // 클라이언트 서버 접속 대기 및 클라이언트 소켓 반환
        {
            // 서버 소켓에서 클라이언트 소켓 연결
            TcpClient client = serverSocket.AcceptTcpClient();
            // 클라이언트 소켓의 IP주소와 포트번호를 IPEndPoint로 변환
            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            // 클라이언트 소켓의 IP주소와 포트번호를 출력
            Console.WriteLine($"Connected to {clientEndPoint}");
            // Client 소켓을 반환
            return client;
        }

        public void Serve(TcpClient client)     // 클라가 보내는 데이터 수신 및 서버의 응답 송신
        {
            // 클라이언트 소켓의 Stream을 가져옴
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                // 클라이언트 소켓에게 받은걸 전송하는 반복문
                while (true)
                {
                    // 클라이언트에게 받은 응답을 읽어옴
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);
                    // 없으면 멈춤
                    if (bytesRead == 0) break;

                    // 받은 응답을 string으로 변환
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string response;

                    // 응답이 int값 안에 있으면 결과 설정. 아니면 잘못되었다고 설정
                    if (int.TryParse(receivedData, out int order))
                    {
                        response = $"Thank you for ordering {order} pizzas!\n";
                    }
                    else
                    {
                        response = "Wrong number of pizzas, please try again\n";
                    }

                    // 클라이언트에게 결과를 전송
                    Console.WriteLine($"Sending message to {client.Client.RemoteEndPoint}");
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            finally
            {
                // 클라이언트 연결 끊어짐 예외처리
                Console.WriteLine($"Connection with {client.Client.RemoteEndPoint} has been closed");
                client.Close();
            }
        }

        public void Start() // 클라 연결 대기를 위한 loop문
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                // 클라이언트 연결용 반복분
                while (true)
                {
                    // 클라이언트 접속을 허용하고 서로 응답함.
                    TcpClient client = Accept();
                    Serve(client);
                }
            }
            finally
            {
                // 서버 닫힘 예외처리
                serverSocket.Stop();
                Console.WriteLine("\nServer stopped.");
            }
        }

        static void Main(string[] args) // Main문 서버 시작
        {
            // Server 생성자로 서버 시작.
            Server server = new Server();
            server.Start();
        }
    }
}