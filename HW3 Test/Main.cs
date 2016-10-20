using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace CS422
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread clientThread = new Thread(master);


            const string DefaultTemplate = "GET / HTTP/1.1\r\n" +
                                           "Content-Length: 412\r\n" +
                                           "Pizza: beach\r\n\r\n" +
                                           "This is the body";
            clientThread.Start();


            TcpClient client = new TcpClient();

            client.Connect("localhost", 1337);

            client.GetStream().Write(Encoding.ASCII.GetBytes(DefaultTemplate), 0, DefaultTemplate.Length);

            byte[] response = new byte[1024];
            client.GetStream().Read(response, 0, 1024);
            string responseString = Encoding.Default.GetString(response);
            Console.WriteLine(responseString);
            

            
           
        }

        static void master()
        {

            const string DefaultTemplate = "HTTP/1.1 200 OK\r\n" +
                                         "Content-Type:text/html\r\n" +
                                         "\r\n\r\n" +
                                         "<html>ID Number: {0}<br>" +
                                         "DateTime.Now: {1}<br>" +
                                         "Requested URL: {2}</html>";

            WebServer.Start(1337, 64);

        }
    }
}
