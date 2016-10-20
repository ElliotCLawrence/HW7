//Elliot Lawrence 
//CS 422
//HW 7
//10/15/2016

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;


namespace CS422
{
    class WebServer
    {
        static ThreadPoolRouter threadPool;
        static DemoService demo;
        private static List<WebService> webServices = new List<WebService>();
        static Thread listenerThreadWorker;
        static TcpListener newListener;

        public static bool Start(int portNum, int threadCount)
        {

            AddService(new DemoService());
            threadPool = new ThreadPoolRouter(threadCount, portNum);
            threadPool.startWork();

            listenerThreadWorker = new Thread(new ParameterizedThreadStart(listenerThread));
            listenerThreadWorker.Start(portNum);


            return true;
        }

        public static void listenerThread(object port)
        {
            try
            {
                newListener = new TcpListener(IPAddress.Any, (int)port);
                newListener.Start();
                TcpClient client;
                while (true)
                {
                    client = newListener.AcceptTcpClient();

                    if (client == null)
                        break;

                    threadPool.addClient(client);
                }
            }
            catch
            {
                Console.WriteLine("Listener ending");
            }
        }

        public static void doThreadWork() //must cast to object in order to use parameterized thread start
        {
            TcpClient client;
            WebRequest request;

            while (true)
            {
                client = threadPool.takeClient();

                if (client == null) //if null client break this thread
                    break;

                request = BuildRequest(client);


                if (request == null)
                {
                    continue;
                }

                bool found = false;
                foreach (WebService services in webServices)
                {
                    if (services.ServiceURI == request.URI)
                    {
                        services.Handler(request);
                        found = true;
                        break;
                    }
                }

                if (!found)
                    request.WriteNotFoundResponse("404 Page Not Found");
            }
        }


        private static WebRequest BuildRequest(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
            int ammountRead = 0;
            clientStream.ReadTimeout = (int) new TimeSpan(0, 0, 2).TotalMilliseconds;
            int startingSeconds = DateTime.Now.Second;
            int startingMinute = DateTime.Now.Minute;
            string fullRequest = "";
            string destination = "/";
            byte[] streamBuff = new byte[1024]; //create a buffer for reading
            int x = clientStream.Read(streamBuff, 0, 1024);
            int y = 0;
            int i = 0; //index of streamBuff
            ammountRead += x;
            string validReq = "GET / HTTP/1.1\r\n"; //I'm using this string to check against the request.

            while (x > 0 && y < validReq.Length) //while the ammount read is greater than 0 bytes
            { 
                if (ammountRead > 2048)
                {
                    clientStream.Close();
                    client.Close();
                    return null;
                }
                if (DateTime.Now.Minute != startingMinute)//if it's a different minute, add 60 seconds when you check change in time
                {
                    if (DateTime.Now.Second + 60 - startingSeconds > 10) //if 10 seconds has past since started reading.
                    {
                        clientStream.Close();
                        client.Close();
                        return null;
                    }
                }
                else if (DateTime.Now.Second - startingSeconds > 10) //if 10 seconds has past since started reading.
                {
                    clientStream.Close();
                    client.Close();
                    return null;
                }

                i = 0;
                fullRequest += Encoding.Default.GetString(streamBuff);
                while (i < x && y < validReq.Length)
                {
                    if (y < 5 || y > 5)
                    {//first part 'GET /'
                        if (Convert.ToChar(streamBuff[i]) != validReq[y])
                        { //invalid request
                            client.Close(); //close stream and return false
                            return null;
                        }
                        //else
                        y++; //valid
                    }
                    else
                    { //the request is giving the requested web page (this comes right after 'GET /')
                        if (Convert.ToChar(streamBuff[i]) == ' ')
                            y++;
                        else //add this byte to the clientRequest
                            destination += Convert.ToChar(streamBuff[i]);
                    }
                    i++; //increment i
                }

                if (y < validReq.Length) //only read if you need to.
                {
                    x = clientStream.Read(streamBuff, 0, 1024);//read next bytes
                    ammountRead += x;
                }

                else //otherwise, break the loop
                    break;
            }

            //read in the headers
            while (true)
            {
                if (ammountRead > 102400)
                {
                    clientStream.Close();
                    client.Close();
                    return null;
                }

                if (fullRequest.Contains("\r\n\r\n"))
                {
                    break;
                }
                
                else
                {
                    //read more stuff
                    x = clientStream.Read(streamBuff, 0, 1024); //read in more
                    ammountRead += x;
                    fullRequest += Encoding.Default.GetString(streamBuff);

                    if (DateTime.Now.Minute != startingMinute) //if it's a different minute, add 60 seconds when you check change in time
                    {
                        if (DateTime.Now.Second + 60 - startingSeconds > 10) //if 10 seconds has past since started reading.
                        {
                            clientStream.Close();
                            client.Close();
                            return null;
                        }
                    }
                    else if (DateTime.Now.Second - startingSeconds > 10) //if 10 seconds has past since started reading.
                    {
                        clientStream.Close();
                        client.Close();
                        return null;
                    }
                }

                if (x <= 0)
                    return null; //never had headers, nor a second \r\n. invalid request!
            }

            string onlyHeaders;
            onlyHeaders = fullRequest.Substring(validReq.Length-2); //before the \r\n
            string endHeaders = "\r\n\r\n";
            int endHeadersCount = 0;

            while (endHeadersCount < onlyHeaders.Length)
            {
                if (onlyHeaders.Substring(endHeadersCount, 4) == endHeaders)
                    break;
                endHeadersCount++;
            }

            List<Tuple<string, string>> headerList = new List<Tuple<string, string>>();

            string headers = onlyHeaders.Substring(0, endHeadersCount);
            string[] splitters = new string[1];
            splitters[0] = "\r\n";

            string [] headerArray = headers.Split( splitters, StringSplitOptions.RemoveEmptyEntries);

            string[] headerSplitter;
            foreach (string headerCombo in headerArray)
            {
                headerCombo.Trim();
                headerSplitter = headerCombo.Split(':');
                
                if (headerSplitter.Length == 2)
                {
                    headerList.Add(new Tuple<string, string>( headerSplitter[0], headerSplitter[1]));
                }
                
            }

            //populate from fullRequest string the URI, Method, version, etc future HW
            MemoryStream streamOne = new MemoryStream();
            WebRequest request;
            int z = endHeadersCount + 4; //right after the last \r\n\r\n
            
            if (z < fullRequest.Length)
            {
                streamOne.Write( Encoding.ASCII.GetBytes( fullRequest), z, fullRequest.Length - z);
                ConcatStream jointStream = new ConcatStream(streamOne, client.GetStream());
                request = new WebRequest(client, jointStream , headerList, "1.1", "GET", destination); //PLACE HOLDER LINE
            }
            
            else
            {
                request = new WebRequest(client, client.GetStream(), headerList, "1.1", "GET", destination); //PLACE HOLDER LINE
            }
            return request;
        }

        public static void AddService(WebService service)
        {
            webServices.Add(service);

        }

        public static void Stop()
        {
            threadPool.Dispose();

            newListener.Stop();
            listenerThreadWorker.Join();

            return;
        }
    }
}


