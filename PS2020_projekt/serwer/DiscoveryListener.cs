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
    class DiscoveryListener
    {
        public bool verbose = true;
        private static string MulticastAddress = "239.0.0.222";

        public int Port = 7;
        

        private string offer;
        private string offeredIp;
        private int offeredPort;

        private bool loopFlag;

        private Thread discoveryThread;

        private UdpClient client;
        private IPEndPoint localEp;




        public DiscoveryListener(string ip, int port)
        {
            offeredIp = ip;
            offeredPort = port;
            offer = String.Format("OFFER {0} {1}", ip, port);
        }
        public DiscoveryListener(string address)
        {
            offeredIp = address.Split(':')[0];
            offeredPort = Int32.Parse(address.Split(':')[1]);
            offer = String.Format("OFFER {0} {1}", address.Split(':')[0], address.Split(':')[1]);
        }
        public void Run()
        {
            
            discoveryThread = new Thread(() => {
                StartServer();
                try
                {
                    while (loopFlag)
                    {
                        Loop();
                    }
                }catch(Exception e)
                {
                    Log("Exception occured");
                }
                
                CloseServer();
            });

            discoveryThread.Start();
            
        }
        public void Stop()
        {
            loopFlag = false;
            discoveryThread.Interrupt();
            discoveryThread.Abort();
        }

        private void StartServer()
        {
            client = new UdpClient();

            client.ExclusiveAddressUse = false;
            localEp = new IPEndPoint(IPAddress.Any, Port);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            client.Client.Bind(localEp);//?

            IPAddress multicastaddress = IPAddress.Parse(MulticastAddress);
            try
            {
                client.JoinMulticastGroup(multicastaddress);
            }
            catch (Exception e)
            {
                Log("error: cant join multicast group");
                return;
            }
            Log("now waiting for incoming messages...");
            loopFlag = true;
        }
        private void Loop()
        {
            Byte[] data = client.Receive(ref localEp);
            string strData = Encoding.Unicode.GetString(data);
            if (strData.StartsWith("DISCOVERY"))
            {
                //Log("'" + strData + "', from: " + localEp.ToString());

                //sender:
                IPAddress multicastaddress = IPAddress.Parse(MulticastAddress);
                UdpClient udpclient = new UdpClient();
                try
                {
                    udpclient.JoinMulticastGroup(multicastaddress);
                }
                catch (Exception e)
                {
                    Log("error: cant join multicast group");
                    return;
                }
                IPEndPoint remoteep = new IPEndPoint(multicastaddress, Port);

                Byte[] buffer = null;
                buffer = Encoding.Unicode.GetBytes(offer);
                Log("sending offer : " + offer);
                udpclient.Send(buffer, buffer.Length, remoteep);
            }
            
            
        }

        public void CloseServer()
        {
            Log("closing server");
            loopFlag = false;
            discoveryThread.Interrupt();
            discoveryThread.Abort();
        }

        private void Log(string text)
        {
            if (verbose)
            {
                Console.Out.WriteLine(DateTime.Now.TimeOfDay.ToString() + " DiscoveryListener[" + offeredIp + ":" + offeredPort + "]> " + text);
            }
        }
        
    }
}
