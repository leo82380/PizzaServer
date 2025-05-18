using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

namespace _99_BusyClient
{
    internal class BusyClient
    {
        static void Main(string[] args)
        {
            // 1000번 반복
            for (int i = 0; i < 1000; i++)
            {
                // 스레드 생성
                new Thread(() =>
                {
                    try
                    {
                        // localhost 12345 포트로 클라이언트 생성
                        using var client = new TcpClient("127.0.0.1", 12345);
                        // 클라이언트에서 스트림 가져오기
                        var stream = client.GetStream();
                        // "5"를 byte[]로 변환
                        var msg = Encoding.UTF8.GetBytes("5\n");
                        // 서버 전송
                        stream.Write(msg, 0, msg.Length);

                        // 버퍼 생성
                        byte[] buffer = new byte[1024];
                        // 서버에서 받은 값 읽어오기
                        int read = stream.Read(buffer, 0, buffer.Length);
                        // string으로 변환
                        string response = Encoding.UTF8.GetString(buffer, 0, read);
                        // 로그
                        Console.WriteLine(response.Trim());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    // 스레드 시작
                }).Start();
            }
        }
    }
}