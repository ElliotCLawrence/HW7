using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;

namespace CS422
{
    public class WebRequest
    {
        TcpClient client;
        private NetworkStream netStream;
        public Stream bodyStream;
        public List<Tuple<string, string>> headers;
        public string httpVersion;
        public string httpMethod;
        public string URI;
        public int bodyLength;


        public WebRequest(TcpClient myClient,  Stream originBodyStream, List<Tuple<string, string>> headerList, string httpVer, string httpMeth, string Destination)
        {
            client = myClient;
            netStream = client.GetStream();
            bodyStream = originBodyStream;
            headers = new List<Tuple<string, string>>(headerList);
            bodyLength = -1;
            foreach (Tuple<string, string> header in headers)
            {
                if (header.Item1 == "Content-Length")
                {
                    bodyLength = Int32.Parse(header.Item2);
                }
            }
            
            httpVersion = httpVer;
            httpMethod = httpMeth;
            URI = Destination;
        }

        

        


        public void WriteNotFoundResponse(string pageHTML)
        {
            string responseString = "HTTP / 1.1 404 Not Found\nContent-Type: text/html\nContent-Length: " + pageHTML.Length + "\r\n\r\n" + pageHTML;
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);
            netStream.Write(responseBytes, 0, responseBytes.Length); //write the status line 
        }

        public bool WriteHTMLResponse(string htmlString)
        {
            string responseString = "HTTP / 1.1 200 OK\r\nContent-Type: text/html\r\nContent-Length: " + htmlString.Length + "\r\n\r\n" + htmlString;
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);
            netStream.Write(responseBytes, 0, responseBytes.Length); //write the status line 
            return true;
        }
    }
}
