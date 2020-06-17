﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace serwer
{
    class TimeListener
    {
        public bool verbose = true;
        public int port = 69;
        public int backlog = 10;

        private Socket listeningSocket = null;
        private Socket connectionSocket = null;

        private bool loopFlag;



        public TimeListener()
        {
            //randomize port
        }
        public void Run()
        {
            StartServer();
            loopFlag = true;
            Thread listener = new Thread(() =>
            {
                while (loopFlag)
                {
                    WaitForClient();
                }
            });
            listener.Start();

        }

        public string GetAddress()
        {
            return "";
        }

        public void StartServer()
        {
            Log("starting server...");
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //bind

            IPAddress hostIP = Dns.Resolve(IPAddress.Any.ToString()).AddressList[0];
            IPEndPoint ep = new IPEndPoint(hostIP, 0);
            listeningSocket.Bind(ep);

            listeningSocket.Listen(backlog);
            Log("server started, ip=" + hostIP.ToString() + ", port=" + ep.Port);
        }
        public void WaitForClient()
        {
            Log("waiting for next client to connect ... ");

            connectionSocket = listeningSocket.Accept();
            Log("new client has connected, start TimerServer for him");

            // Or lambda expressions if you are using C# 3.0
            //ClientThread clientThread = new ClientThread(this, connectionSocket);
            //clientList.Add(clientThread);
            //clientThread.Start();
        }

        public void CloseServer()
        {
            Log("closing server");
            try
            {
                listeningSocket.Close();
                //foreach (ClientThread ct in clientList)
                //{
                //    ct.Stop();
                //}
            }
            catch (Exception ignored) { }

        }



        public void Log(string text)
        {
            if (verbose)
            {
                Console.Out.WriteLine("EchoServer> " + text);
            }
        }
    }
}