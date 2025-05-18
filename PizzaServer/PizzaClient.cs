using System;
using System.Net.Sockets;
using System.Text;

namespace _0_SimpleClient
{
    internal class PizzaClient
    {
        private const int BUFFER_SIZE = 1024;   // 데이터 송수신 최대 길이
        private const string SERVER_ADDRESS = "127.0.0.1"; // 서버 IP주소
        private const int SERVER_PORT = 12345; // 서버 포트

        static void Main()
        {
            try
            {   // 클라이언트 소켓 생성 및 스트립 가져오기
                using TcpClient client = new TcpClient(SERVER_ADDRESS, SERVER_PORT);
                NetworkStream stream = client.GetStream();

                // 서버가 계속 돌아가기 위한 반복문
                while (true)
                {
                    Console.Write("How many pizzas do you want? ");
                    string? order = Console.ReadLine();                     // 피자 주문 입력

                    // 주문이 비어있으면 종료
                    if (string.IsNullOrEmpty(order))
                        break;

                    // ReadLine으로 받아온 주문을 byte 배열로 변환 후 전송
                    byte[] dataToSend = Encoding.UTF8.GetBytes(order);      // 바이트 변환
                    stream.Write(dataToSend, 0, dataToSend.Length);         // 데이터 전송

                    // 서버의 응답을 받기 위한 byte 배열 버퍼 생성
                    byte[] buffer = new byte[BUFFER_SIZE];
                    // 서버의 응답을 버퍼를 활용해 bytesRead에 저장
                    int bytesRead = stream.Read(buffer, 0, BUFFER_SIZE);    // 데이터 수신
                    // 수신한 바이트를 UTF8로 변환 후 공백 제거
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).TrimEnd();      // 바이트 변환

                    // 서버의 응답을 출력
                    Console.WriteLine($"Server replied '{response}'");      // 주문 결과 출력
                }

                // 반복문이 종료되면 클라이언트 소켓 종료
                Console.WriteLine("Client closing");
            }
            // 소켓 예외 처리
            catch (SocketException ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        }
    }
}