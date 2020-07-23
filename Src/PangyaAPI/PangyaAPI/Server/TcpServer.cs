﻿using PangyaAPI.Auth;
using PangyaAPI.BinaryModels;
using PangyaAPI.PangyaClient;
using PangyaAPI.PangyaPacket;
using PangyaAPI.Tools;
using Py_Connector.DataBase;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
namespace PangyaAPI.Server
{
    public abstract partial class TcpServer
    {
        #region Delegates
        public delegate void ConnectedEvent(Player player);
        public delegate void PacketReceivedEvent(Player player, Packet packet);
        #endregion

        #region Events
        /// <summary>
        /// Este evento ocorre quando o ProjectG se conecta ao Servidor
        /// </summary>
        public event ConnectedEvent OnClientConnected;

        /// <summary>
        /// Este evento ocorre quando o Servidor Recebe um Packet do ProjectG
        /// </summary>
        public event PacketReceivedEvent OnPacketReceived;

        #endregion

        #region Fields

        /// <summary>
        /// Lista de Players conectados
        /// </summary>
        public GenericDisposableCollection<Player> Players = new GenericDisposableCollection<Player>();

        public uint NextConnectionId { get; set; } = 1;

        public TcpListener _server;      

        public bool _isRunning;

        public AuthClient AuthServer;

        public ServerSettings Data;

       public bool ShowLog { get; set; }

        public DateTime EndTime { get; set; }

        public bool OpenServer = false;

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Envia chave para o player
        /// </summary>
        protected abstract void SendKey(Player player);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcp"></param>
        protected abstract Player OnConnectPlayer(TcpClient tcp);
        protected abstract void ServerExpection(Player Client,Exception Ex);

        public abstract void DisconnectPlayer(Player Player);

        public abstract void ServerStart();

        public abstract Player GetClientByConnectionId(UInt32 ConnectionId);

        public abstract Player GetPlayerByNickname(string Nickname);

        public abstract Player GetPlayerByUsername(string Username);

        public abstract Player GetPlayerByUID(UInt32 UID);

        public abstract bool GetPlayerDuplicate(UInt32 UID);
        public abstract bool PlayerDuplicateDisconnect(UInt32 UID);

        public abstract void RunCommand(string[] Command);
        #endregion

        #region Constructor
        public TcpServer()
        {
            EndTime = new DateTime(2563, 07, 30, 0, 17, 0);
        }

        #endregion

        #region Private Methods

        #region AuthServer

        protected abstract AuthClient AuthServerConstructor();

        protected abstract void OnAuthServerPacketReceive(AuthClient client, AuthPacket packet);


        /// <summary>
        /// Conecta-se ao AuthServer
        /// </summary>
        public bool ConnectToAuthServer(AuthClient client)
        {
            AuthServer = client;
            AuthServer.OnDisconnect += OnAuthServerDisconnected;
            AuthServer.OnPacketReceived += AuthServer_OnPacketReceived;
            return AuthServer.Connect();
        }

        /// <summary>
        /// É Disparado quando um packet é recebido do AuthServer
        /// </summary>
        private void AuthServer_OnPacketReceived(AuthClient authClient, AuthPacket packet)
        {
            OnAuthServerPacketReceive(authClient, packet);
        }

        /// <summary>
        /// É disparado quando não há conexão com o AuthServer
        /// </summary>
        private void OnAuthServerDisconnected()
        {
            Console.WriteLine("Servidor parado.");
            Console.WriteLine("Não foi possível estabelecer conexão com o authServer!");
            Console.ReadKey();
            Environment.Exit(1);
        }

        #endregion

        public void UpdateServer()
        {
            var db = new PangyaEntities();
            var query = $"UPDATE [dbo].[Pangya_Server] Set UsersOnline = '{Players.Count}' where ServerID = '{Data.UID}'";

            db.Database.SqlQuery<PangyaEntities>(query).FirstOrDefault();
        }

        /// <summary>
        /// Aguarda Conexões
        /// </summary>
        public void HandleWaitConnections()
        {
            while (_isRunning)
            {
                // Inicia Escuta de novas conexões (Quando player se conecta).
                TcpClient newClient = _server.AcceptTcpClient();

                // Cliente conectado
                // Cria uma Thread para manusear a comunicação (uma thread por cliente)
                Thread t = new Thread(new ParameterizedThreadStart(HandlePlayer));
                t.Start(newClient);
            }
        }

        /// <summary>
        /// Manuseia Comunicação do Cliente
        /// </summary>
        private void HandlePlayer(object obj)
        {
            //Recebe cliente a partir do parâmetro
            TcpClient tcpClient = (TcpClient)obj;

            var Player = OnConnectPlayer(tcpClient);

            var thread = new Thread(new ThreadStart(Player.RefreshData));
            thread.Start();

            //Chama evento OnClientConnected
            this.OnClientConnected?.Invoke(Player);

            while (Player.Connected)
            {
                try
                {
                    byte[] message = ReceivePacket(tcpClient.GetStream());

                    if (message.Length >= 5)
                    {
                        if (Player.Connected)
                        {
                            var packet = new Packet(message, Player.GetKey);
                            if (ShowLog)
                            {
                                packet.Log();
                            }
                            //Dispara evento OnPacketReceived
                            OnPacketReceived?.Invoke(Player, packet: packet);
                        }
                    }
                    else
                    {
                        if (Player.Connected)
                        {
                            DisconnectPlayer(Player);
                        }
                    }
                }
                catch(Exception ex)
                {
                    ServerExpection(Player, ex);
                }
            }
            if (Player.Connected)
                DisconnectPlayer(Player);
        }

       protected byte[] ReceivePacket(NetworkStream Stream)
        {
            int bytesRead = 0;
            byte[] message, messageBufferRead = new byte[500000]; //Tamanho do BUFFER á ler             
            try
            {
                if (Stream != null && Stream.CanRead)
                {
                    //Lê mensagem do cliente
                    bytesRead = Stream.Read(messageBufferRead, 0, messageBufferRead.Length);
                }
                //variável para armazenar a mensagem recebida
                message = new byte[bytesRead];

                //Copia mensagem recebida
                Buffer.BlockCopy(messageBufferRead, 0, message, 0, bytesRead);

                return message;
            }
            catch
            {
                return new byte[0];
            }
        }

        #endregion

        #region Public Methods

        public void SendToAll(byte[] Data)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].SendResponse(Data);
            }
        }

        public void Notice(string message)
        {
            var response = new PangyaBinaryWriter(new MemoryStream());

            response.Write(new byte[] { 0x42, 0x00 });
            response.WritePStr("Aviso: " + message);
            SendToAll(response.GetBytes());
            Console.WriteLine("BroadCast enviado com sucesso");
        }

        public void ShowHelp()
        {
            Console.WriteLine(Environment.NewLine);
            WriteConsole.WriteLine("Welcome To Py-Server!" + Environment.NewLine);

            WriteConsole.WriteLine("See available console commands:" + Environment.NewLine);

            WriteConsole.WriteLine("help      | Displays console commands");
            WriteConsole.WriteLine("topnotice | Displays message to users who are playing Game");
            WriteConsole.WriteLine("kickuser  | Disconnect by UserName");
            WriteConsole.WriteLine("kicknick  | Disconnect by Nick");
            WriteConsole.WriteLine("kickuid   | Disconnect by UID");
            WriteConsole.WriteLine("clear     | Clear Console");
            WriteConsole.WriteLine("cls       | Clear console");
            WriteConsole.WriteLine("quit      | Close By Server");
            WriteConsole.WriteLine("Start     | Open Server");
            WriteConsole.WriteLine("Stop      | Close Server");

            Console.WriteLine(Environment.NewLine);
        }
        #endregion      
    }
}
