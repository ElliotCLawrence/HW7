using System;
using System.Threading; //I'm only including this so I don't have to say system.threading the whole time.
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
//Elliot Lawrence
//11349302
//HW6
namespace CS422
{
    public class ThreadPoolRouter : IDisposable
    {
        Thread[] threadList;
        BlockingCollection<TcpClient> clients = new BlockingCollection<TcpClient>();
        int numThreads;
        int port;

        public ThreadPoolRouter(int threadCount, int portNum)
        {
            if (threadCount <= 0)
            { //if 0 threads, default to 64
                threadCount = 64;
            }
            numThreads = threadCount;
            port = portNum;
            threadList = new Thread[numThreads];
            for (int x = 0; x < numThreads; x++)
            {
                threadList[x] = new Thread(WebServer.doThreadWork);
            }
        }

        public void startWork()
        {
            for (int x = 0; x < numThreads; x++)
            {
                threadList[x].Start();
            }
        }

        public void addClient(TcpClient clientToAdd)
        {
            clients.Add(clientToAdd);
        }
        
        public int getPort
        {
            get
            {
                return port;
            }
        }
        public int threadCount
        {
            get
            {
                return numThreads;
            }
        }

        public TcpClient takeClient()
        {
           return clients.Take();
        }

        public void Dispose()
        {
            for (int x = 0; x < numThreads; x++)
            { //need to add num of thread null cases (null is a check) so that each thread grabs a null flag
                clients.Add(null);
                threadList[x].Join();
            }
        }
    }
}