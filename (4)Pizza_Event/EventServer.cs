using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _4_Pizza_Event
{
    internal class EventServer
    {
        private const int BUFFER_SIZE = 1024;
        private readonly Socket serverSocket;
        private readonly EventLoop loop;
        private readonly Dictionary<Socket, string> pendingMessages = new();

        //
        public EventServer(EventLoop loop)
        {
            // 이벤트 서버에 이벤트 등록
            this.loop = loop;

            // 서버 소켓 초기화
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 12345));
            serverSocket.Listen(100);
            serverSocket.Blocking = false; // 논블로킹

            Console.WriteLine("Server started on 127.0.0.1:12345");
        }

        public void Start()
        {
            // OnAccept 등록
            loop.RegisterRead(serverSocket, OnAccept);  // 새로운 클라이언트 연결을 수락할 수 있을 때 실행
            Console.WriteLine("Waiting for clients...");
        }

        private void OnAccept(Socket listener)
        {
            try
            {
                // 클라이언트 연결 수락
                Socket client = listener.Accept();
                client.Blocking = false;
                Console.WriteLine($"Connected: {client.RemoteEndPoint}");

                // 메시지 읽기 함수 등록
                loop.RegisterRead(client, OnRead);  // 특정 클라이언트가 보낸 메시지를 읽을 수 있을 때 실행
            }
            catch (SocketException)
            {
                // Non-blocking accept failed — ignore
            }

            // 논블로킹 서버가 클라이언트를 계속 받으려면 매번 등록해야함
            loop.RegisterRead(serverSocket, OnAccept);  // 소켓은 비동기(non-blocking)이므로, 한번 Select() 이후엔 등록이 해제됨
                                                        // 계속해서 새로운 접속을 받으려면 매번 다시 등록해야 함
        }

        // 클라이언트가 전송한 메시지 읽기
        private void OnRead(Socket client)
        {
            var buffer = new byte[BUFFER_SIZE];
            try
            {
                // 클라이언트가 전송한 내용 받기
                int bytes = client.Receive(buffer);

                // 없으면 Close
                if (bytes == 0)
                {
                    CloseClient(client);
                    return;
                }

                // 메시지 변환
                string message = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();
                pendingMessages[client] = message;      // 클라이언트가 보낸 메시지 [클라이언트 0~n]별로 저장(아직 미응답 상태)

                // 쓰기 등록
                loop.RegisterWrite(client, OnWrite);    // 클라이언트 소켓으로 쓰기 상태 가능하면 OnWrite() 호출
            }
            catch (SocketException)
            {
                CloseClient(client);
            }
        }

        // 클라이언트 송신용 함수
        private void OnWrite(Socket client) // 클라이언트로 소켓으로 메시지 송신
        {
            // 응답할 메시지 없다면 패스, 있다면 message 변수로 반환
            if (!pendingMessages.TryGetValue(client, out string message))
            {
                return;
            }

            // int값인지 판단 후 결과 지정
            string response;
            if (int.TryParse(message, out int pizzas))
            {
                response = $"Thank you for ordering {pizzas} pizzas!\n";
            }
            else
            {
                response = "Wrong number of pizzas, please try again\n";
            }

            try
            {
                // 클라이언트에게 전송
                Console.WriteLine($"Sending to {client.RemoteEndPoint}");
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                client.Send(responseBytes);
            }
            catch (SocketException)
            {
                // 소켓 에러가 나면 close
                CloseClient(client);
                return;
            }

            // 메시지 지우기
            pendingMessages.Remove(client);

            // 읽기 다시 등록
            loop.RegisterRead(client, OnRead);
        }

        // 클라이언트 Close
        private void CloseClient(Socket client)
        {
            // loop에서 client 등록 해제 후 닫기
            Console.WriteLine($"Disconnected: {client.RemoteEndPoint}");
            loop.Unregister(client);
            client.Close();
        }
    }

}