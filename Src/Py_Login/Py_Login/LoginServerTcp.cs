﻿using PangyaAPI.Auth;
using PangyaAPI.PangyaClient;
using PangyaAPI.Server;
using PangyaAPI.Tools;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Py_Login.MainServer
{
    public class LoginServer : TcpServer
    {
        public LoginServer()
        {
            try
            {
                var Ini = new IniFile(ConfigurationManager.AppSettings["Config"]);
                Data = new ServerSettings
                {
                    Name = Ini.ReadString("Config", "Name", "LoginServer"),
                    Version = Ini.ReadString("Config", "Version", "SV_LS_Release_2.0"),
                    UID = Ini.ReadUInt32("Config", "UID", 10103),
                    MaxPlayers = Ini.ReadUInt32("Config", "MaxPlayers", 3000),
                    Port = Ini.ReadUInt32("Config", "Port", 10103),
                    IP = Ini.ReadString("Config", "IP", "127.0.0.1"),                    
                    Type = AuthClientTypeEnum.LoginServer,
                    AuthServer_Ip = Ini.ReadString("Config", "AuthServer_IP", "127.0.0.1"),
                    AuthServer_Port = Ini.ReadInt32("Config", "AuthServer_Port", 7997),
                    Key = "3493ef7ca4d69f54de682bee58be4f93"
                };
                ShowLog = Ini.ReadBool("Config", "PacketLog", false);
                if (ConnectToAuthServer(AuthServerConstructor()) == false)
                {
                    WriteConsole.WriteLine("[ERROR_START_AUTH]: Não foi possível se conectar ao AuthServer");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                _server = new TcpListener(IPAddress.Parse(Data.IP), (int)Data.Port);

                OpenServer = true;
            }
            catch (Exception erro)
            {
                WriteConsole.WriteLine($"[ERROR_START]: {erro.Message}");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        public override void ServerStart()
        {
            try
            {
                Data.InsertServer();
                this._isRunning = true;
                _server.Start((int)Data.MaxPlayers);
                WriteConsole.WriteLine($"[SERVER_START]: PORT {Data.Port}", ConsoleColor.Green);

                if (DateTime.Now == EndTime || (DateTime.Now.Month == EndTime.Month && DateTime.Now.Day == EndTime.Day))
                {
                    _isRunning = false;
                }
                //Inicia Thread para escuta de clientes
                var WaitConnectionsThread = new Thread(new ThreadStart(HandleWaitConnections));
                WaitConnectionsThread.Start();
            }
            catch (Exception erro)
            {
                WriteConsole.WriteLine($"[ERROR_START]: {erro.Message}");
            }
        }
        protected override Player OnConnectPlayer(TcpClient tcp)
        {
            var player = new LPlayer(tcp)
            {
                Server = this,
                ConnectionID = NextConnectionId
            };

            NextConnectionId += 1;

            SendKey(player);

            WriteConsole.WriteLine($"[PLAYER_CONNECT]: {player.GetAddress}:{player.GetPort} | KEY: {player.GetKey}", ConsoleColor.Green);
            
            Players.Add(player);
            UpdateServer();
           
            return player;
        }

        protected override AuthClient AuthServerConstructor()
        {
            return new AuthClient(Data);
        }

        protected override void SendKey(Player player)
        {
            var US = new byte[] { 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, player.GetKey, 0x00, 0x00, 0x00, 0x75, 0x27, 0x00, 0x00 };

            if (player.Tcp.Connected)
                //Envia packet com a chave
                player.SendBytes(US);
        }

        public override void DisconnectPlayer(Player Player)
        {
            var player = (LPlayer)Player;
            if (player.Connected)
            {
                player.Connected = false;
                player.Dispose();
                player.Tcp.Close();
                Players.Remove(player);
            }
            WriteConsole.WriteLine($"[PLAYER_DISCONNECT]: User {player?.GetLogin}");
            UpdateServer();
        }

        protected override void ServerExpection(Player Client, Exception Ex)
        {
            var player = (LPlayer)Client;
            
            if (player.Connected)
            {
                //AuthServer.Send(new AuthPacket() { ID = AuthPacketEnum.DISCONNECT_PLAYER_ALL_ON_SERVERS, Message = new { ID = player.GetUID } });
            }
        }

        public override Player GetClientByConnectionId(uint ConnectionId)
        {
            var Client = (LPlayer)Players.Model.Where(c => c.ConnectionID == ConnectionId).FirstOrDefault();

            return Client;
        }

        public override Player GetPlayerByNickname(string Nickname)
        {
            var Client = (LPlayer)Players.Model.Where(c => c.GetNickname == Nickname).FirstOrDefault();

            return Client;
        }

        public override Player GetPlayerByUsername(string Username)
        {
            var Client = (LPlayer)Players.Model.Where(c => c.GetLogin == Username).FirstOrDefault();

            return Client;
        }

        public override Player GetPlayerByUID(uint UID)
        {
            var Client = (LPlayer)Players.Model.Where(c => c.GetUID == UID).FirstOrDefault();

            return Client;
        }

        public override bool GetPlayerDuplicate(uint UID)
        {
            throw new NotImplementedException();
        }

        public override bool PlayerDuplicateDisconnect(uint UID)
        {
            throw new NotImplementedException();
        }

        protected override void OnAuthServerPacketReceive(AuthClient client, AuthPacket packet)
        {
            switch (packet.ID)
            {
                case AuthPacketEnum.SERVER_KEEPALIVE:
                    break;
                case AuthPacketEnum.SERVER_CONNECT:
                    break;
                case AuthPacketEnum.RECEIVES_USER_UID:
                    break;
                case AuthPacketEnum.SEND_DISCONNECT_PLAYER:
                    {
                        uint UID = packet.Message.ID;

                        var player = GetPlayerByUID(UID);

                        if (player != null)
                        {
                            DisconnectPlayer(player);
                        }
                    }
                    break;
                case AuthPacketEnum.SERVER_RELEASE_TICKET:
                    break;
                case AuthPacketEnum.SERVER_RELEASE_BOXRANDOM:
                    break;
                case AuthPacketEnum.SERVER_RELEASE_NOTICE:
                    break;
                case AuthPacketEnum.SERVER_COMMAND:
                default:
                    WriteConsole.WriteLine("[AUTH_PACKET]:  " + packet.ID);
                    break;
            }
        }

        public override void RunCommand(string[] Command)
        {
            string ReadCommand;
            LPlayer P;

            if (Command.Length > 1)
            {
                ReadCommand = Command[1];
            }
            else
            {
                ReadCommand = "";
            }
            switch (Command[0])
            {
                case "cls":
                case "limpar":
                case "clear":
                    {
                        Console.Clear();
                    }
                    break;
                case "kickuid":
                    {
                        P = (LPlayer)GetPlayerByUID(uint.Parse(ReadCommand.ToString()));
                        if (P == null)
                        {
                            WriteConsole.WriteLine("[SYSTEM_COMMAND]: THIS UID IS NOT ONLINE!", ConsoleColor.Red);
                            break;
                        }
                        DisconnectPlayer(P);
                    }
                    break;
                case "kickname":
                    {
                        P = (LPlayer)GetPlayerByNickname(ReadCommand);
                        if (P == null)
                        {
                            WriteConsole.WriteLine("[SYSTEM_COMMAND]: THIS NICKNAME IS NOT ONLINE!", ConsoleColor.Red);
                            return;
                        }
                        DisconnectPlayer(P);
                    }
                    break;
                case "kickuser":
                    {
                        P = (LPlayer)GetPlayerByUsername(ReadCommand);
                        if (P == null)
                        {
                            WriteConsole.WriteLine("[SYSTEM_COMMAND]: THIS USERNAME IS NOT ONLINE!", ConsoleColor.Red);
                            return;
                        }
                        DisconnectPlayer(P);
                    }
                    break;
                case "topnotice":
                    {
                        if (ReadCommand.Length > 0)
                            Notice(ReadCommand);
                    }
                    break;
                case "stop":
                    OpenServer = false;
                    WriteConsole.WriteLine("[SYSTEM_COMMAND]: Server is Close");
                    break;
                case "start":
                    OpenServer = true;
                    WriteConsole.WriteLine("[SYSTEM_COMMAND]: Server is Open");
                    break;
                case "comandos":
                case "commands":
                case "ajuda":
                case "help":
                    {
                        ShowHelp();
                    }
                    break;
                default:
                    {
                        WriteConsole.WriteLine("[SYSTEM_COMMAND]: Sorry Unknown Command, type 'help' to get the list of commands", ConsoleColor.Red);
                    }
                    break;
            }
        }
    }
}
