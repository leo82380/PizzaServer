using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _4_Pizza_Event
{
    internal class EventLoop
    {
        // Reader, Writer 저장을 위한 딕셔너리 선언
        private readonly Dictionary<Socket, Action<Socket>> readers = new();
        private readonly Dictionary<Socket, Action<Socket>> writers = new();
        // "소켓을 읽을 수 있을 때 알려줘" 목록에 등록
        public void RegisterRead(Socket socket, Action<Socket> callback)
        {
            // 매개변수로 받아온 콜백함수 등록
            readers[socket] = callback;
        }
        // "소켓을 쓸 수 있을 때 알려줘" 목록에 등록
        public void RegisterWrite(Socket socket, Action<Socket> callback)
        {
            // 매개변수로 받아온 콜백함수 등록
            writers[socket] = callback;
        }
        // 	소켓이 종료되면 등록 해제
        public void Unregister(Socket socket)
        {
            // 딕셔너리에서 전부 해제
            readers.Remove(socket);
            writers.Remove(socket);
        }
        // 이벤트 루프 돌며 Select()로 가능한 작업 감지 후 콜백 실행
        public void RunForever()
        {
            while (true)
            {
                var readList = new List<Socket>(readers.Keys);  // 읽기 가능 소켓 목록
                var writeList = new List<Socket>(writers.Keys); // 쓰기 가능 소켓 목록
                Socket.Select(readList, writeList, null, 1000); // 읽/쓸 수 있는 상태가 된 소켓 선별해 list에 반환

                // readList를 돌며 호출할 수 있으면 호출
                foreach (var sock in readList)
                {
                    if (readers.TryGetValue(sock, out var callback))
                    {
                        callback(sock); // 예: OnRead 호출
                    }
                }

                // writeList를 돌며 호출할 수 있으면 호출
                foreach (var sock in writeList)
                {
                    if (writers.TryGetValue(sock, out var callback))
                    {
                        callback(sock); // 예: OnWrite 호출
                    }
                }
            }
        }
    }
}