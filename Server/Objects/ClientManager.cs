﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

using CommonObjects.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace Server.Objects
{
    public class ClientManager : IDisposable
    {
        private List<Socket> _clients;
        private ViewModel.MainWindowViewModel _mainWindowViewModel;

        private Dictionary<Type, String> _requestTypes;

        public ClientManager(ViewModel.MainWindowViewModel mainWindowViewModel)
        {
            _clients = new List<Socket>();
            _mainWindowViewModel = mainWindowViewModel;
            InitTypes();
        }

        private void InitTypes()
        {
            _requestTypes = new Dictionary<Type, string>();
            _requestTypes.Add(typeof(ConnectRequest),"connect");
            _requestTypes.Add(typeof(DisconnectRequest),"disconnect");
            _requestTypes.Add(typeof(MessageRequest),"message");
        }

        public void AddClient(Socket client)
        {
            _clients.Add(client);
            Thread clientLoop_Thread = new Thread(ClientLoop);
            clientLoop_Thread.IsBackground = true;
            clientLoop_Thread.Start(client);
        }

        public void Dispose()
        {
            foreach (Socket client in _clients)
            {
                client?.Close();
            }

            _clients.Clear();
        }

        public void SendResponseToAllClients(Response response)
        {
            foreach (Socket client in _clients)
            {
                SendResponseToClient(client,response);
            }
        }

        private void SendResponseToClient(Socket client, Response response)
        {
            String send_data_str = JsonConvert.SerializeObject(response);
            byte[] send_data_bytes = Encoding.UTF8.GetBytes(send_data_str);
            client.Send(send_data_bytes);
        }

        private Request GetRequestFromClient(Socket client)
        {
            Request value = null;

            byte[] recv_data_bytes = new byte[1024];
            client.Receive(recv_data_bytes);
            String recv_data_str = Encoding.UTF8.GetString(recv_data_bytes);

            dynamic temp = JsonConvert.DeserializeObject(recv_data_str);

            for (int i = 0; i < _requestTypes.Count; i++)
            {
                if(temp.request_type == _requestTypes.Values.ElementAt(i))
                {
                    value = JsonConvert.DeserializeObject(recv_data_str,_requestTypes.Keys.ElementAt(i)) as Request;
                    break;
                }
            }

            return value;
        }

        private void ClientLoop(object client)
        {
            Socket temp_client = client as Socket;
            User client_user = null;

            try
            {
                while (true)
                {
                    Request client_request = GetRequestFromClient(temp_client);

                    switch (client_request.RequestType)
                    {
                        case "connect":
                            {
                                Response connectResponse = new ConnectResponse((client_request as ConnectRequest).User);
                                SendResponseToClient(temp_client,connectResponse);
                                Application.Current.Dispatcher.Invoke( () => _mainWindowViewModel.AddUser((client_request as ConnectRequest).User));
                                client_user = (client_request as ConnectRequest).User;
                                break;
                            }

                        case "disconnect":
                            {
                                Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.RemoveUserByID((client_request as DisconnectRequest).User.ID));
                                throw new Exception();
                            }

                        case "message":
                            {
                                Response messageResponse = new MessageResponse((client_request as MessageRequest).Message);
                                SendResponseToAllClients(messageResponse);
                                break;
                            }

                        default:
                            break;
                    }
                }
            }
            catch(Exception e)
            {

            }
            finally
            {
                temp_client?.Close();
                _clients.Remove(temp_client);
                if(client_user != null) Application.Current.Dispatcher.Invoke( () => _mainWindowViewModel.RemoveUserByID(client_user.ID));
            }
        }
    }
}
