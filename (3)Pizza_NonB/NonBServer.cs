using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace _3_Pizza_NonB
{
    internal class NonBServer
    {
        private const int BUFFER_SIZE = 1024;
        private const int PORT = 12345;
        private Socket serverSocket;
        private readonly List<Socket> clients = new();

        // 논블로킹 서버 생성자
        public NonBServer()
        {
            try
            {
                Console.WriteLine($"Starting up at: 127.0.0.1:{PORT}");

                // tcp로 소켓 생성
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // IP랑 연결
                serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, PORT));
                // 클라이언트 소켓 대기열 지정
                serverSocket.Listen(1000);      // 클라이언트 소켓 대기열 최대 1000 
                // 논블로킹을 위한 Blocking = false;
                serverSocket.Blocking = false;  // 서버 소켓 : 블로킹 off!
            }
            catch (SocketException)
            {
                // 소켓 에러가 난다면 소켓 닫기
                serverSocket?.Close();
                Console.WriteLine("\nServer failed to start.");
            }
        }

        private void Accept()
        {
            try
            {
                // 클라이언트 소켓 접속 허용
                Socket clientSocket = serverSocket.Accept();
                // 클라이언트도 논블로킹으로 변경
                clientSocket.Blocking = false;      // 클라이언트소켓 : 블로킹 off!
                // 현재 클라이언트 list에 등록
                clients.Add(clientSocket);
                // 연결 완료 표시
                Console.WriteLine($"Connected to {clientSocket.RemoteEndPoint}");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        private void Serve(Socket client)
        {
            // 소켓통신을 위한 버퍼 생성
            byte[] buffer = new byte[BUFFER_SIZE];

            try
            {
                // 클라이언트에게 받은 값 저장
                int bytesRead = client.Receive(buffer);
                // 받은 값이 없으면
                if (bytesRead == 0)
                {
                    // 지워버리고 닫기
                    clients.Remove(client);
                    client.Close();
                    return;
                }

                // 받은 값을 string으로 변결
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string response;

                // int값인지 판단해 결과 지정
                if (int.TryParse(received, out int order))
                {
                    response = $"Thank you for ordering {order} pizzas!\n";
                }
                else
                {
                    response = "Wrong number of pizzas, please try again\n";
                }

                // 클라이언트에게 byte 배열로 변경해 전송
                Console.WriteLine($"Sending message to {client.RemoteEndPoint}");
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                client.Send(responseData);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
            {
                // no client waiting to be accepted — just continue
            }
        }

        public void Start()
        {
            Console.WriteLine("Server listening for incoming connections");

            try
            {
                while (true)        // 폴링 반복문
                {                   // 입출력 연산(accept(), send(), read() 이 성공할 때까지 연산 반복
                    Accept();       // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감

                    // 모든 클라이언트랑 통신
                    foreach (var client in new List<Socket>(clients))
                    {
                        Serve(client);  // 만약 소켓에 데이터가 없으면 대기하지 않고 넘어감
                    }

                    Thread.Sleep(1); // avoid 100% CPU usage
                }
            }
            finally
            {
                serverSocket.Close();
                Console.WriteLine("\nServer stopped.");
            }
        }

        static void Main()
        {
            // 논블로킹 서버 생성 및 실행
            NonBServer server = new NonBServer();
            server.Start();
        }
    }
}