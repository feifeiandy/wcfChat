
// Copyright (C) 2006 by Nikola Paljetak

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Description;

namespace NikeSoftChat
{
    class Program
    {
        static void Main(string[] args)
        {

            //Uri uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            //ServiceHost host = new ServiceHost(typeof(NikeSoftChat.ChatService), uri);
            //host.Open();
            //Console.WriteLine("Chat service listen on endpoint {0}", uri.ToString());
            //Console.WriteLine("Press ENTER to stop chat service...");
            //Console.ReadLine();
            //host.Abort();
            //host.Close(); 
            Uri uri = new Uri(ConfigurationManager.AppSettings["addr"]);
            using (ServiceHost host = new ServiceHost(typeof(NikeSoftChat.ChatService), uri))
            {
                ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (smb == null)
                {
                    host.Description.Behaviors.Add(new ServiceMetadataBehavior());
                }

                host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
                host.Open();

                Console.WriteLine("Chat service listen on endpoint {0}", uri.ToString());
                Console.WriteLine("Press ENTER to stop chat service...");
                Console.ReadLine();
                host.Abort();
                host.Close();
            }
        }

       
    }
}
