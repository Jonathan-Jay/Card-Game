using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
        void Start() 
        {
            byte[] buffer = new byte[512];

            //Setup our end point (server)
            try
            {
                //IPAddress ip = Dns.GetHostAddresses("mail.bigpond.com")[0];
                IPAddress ip = IPAddress.Parse("35.169.14.57");
                IPEndPoint server = new IPEndPoint(ip, 42069);

                //create out client socket 
                Socket client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //attempted a connection
                try
                {
                    Debug.Log("Attempting Connection to server...");
                    client.Connect(server);
                    //release the resource
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                catch (ArgumentNullException argExc)
                {
                    
                }
                catch (SocketException SockExc)
                {
                    
                }
                catch (Exception e)
                {
                    
                }
            }
            catch (Exception e)
            { 
                   
            }

        }
    [SerializaField] TMPro.text 
    // Update is called once per frame
    void SendMessage(string message)
    {
        
    }
}
