using _4_Pizza_Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4_Pizza_Event
{
    internal class Program
    {
        // 이벤트 루프 기반 서버 생성 후 Start
        static void Main()
        {
            EventLoop eventLoop = new EventLoop();
            EventServer server = new EventServer(eventLoop);
            server.Start();
            eventLoop.RunForever();
        }
    }
}