using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public delegate void EmptyDelegate();
    public delegate void StringDelegate(string str);
    public delegate void IntDelegate(int i);
    public delegate void WaitingRoomDelegate(WaitingRoomInformation waitingRoom);

    public enum ApplicationType
    {
        None,
        Client,
        Server
    }

    public enum MessageableComponent
    {
        BaseServer,
        WaitingRoom,
        Lobby,
        Game
    }

    public enum MessageType : byte
    {
        // Base
        Client_Disconnect,
        Server_Disconnect,

        // Waiting room
        WR_ServerSend_FullInfo, // Test
        WR_ServerSend_SingleUpdate,
        WR_ServerSend_NewRoom,
        WR_ServerSend_DeleteRoom,

        WR_ClientRequest_JoinRoom,
        WR_ClientRequest_LeaveRoom,
        WR_ClientRequest_CreateRoom,

        WR_ServerResponse_SuccessJoinRoom,
        WR_ServerResponse_FailJoinRoom,
        WR_ServerResponse_SuccessCreateRoom,
        WR_ServerResponse_FailCreateRoom,
    }

    public struct InputInformation
    {
        public KeyboardState CurrentKeyboardState;
        public KeyboardState PreviousKeyboardState;

        public GamePadState CurrentGamePadState;
        public GamePadState PreviousGamePadState;

        public MouseState CurrentMouseState;
        public MouseState PreviousMouseState;
    }

    [Serializable]
    public class BasePacket
    {
        public DateTime SendDate { get; set; }

        public BasePacket()
        {
            SendDate = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class NetworkPacket : BasePacket
    {
        public byte[] SomeArbitaryBytes { get; set; }
        public int SomeArbitaryInt { get; set; }
        public double SomeArbitaryDouble { get; set; }
        public string SomeArbitaryString { get; set; }

        public NetworkPacket() : base()
        {
            SomeArbitaryInt = 7;
            SomeArbitaryDouble = 98.1;
            SomeArbitaryBytes = new byte[10];
            for (var i = 0; i < SomeArbitaryBytes.Length; i++)
            {
                SomeArbitaryBytes[i] = (byte)i;
            }
            SomeArbitaryString = "Test string";
        }
    }

    [Serializable]
    public class LobbyInformation : BasePacket
    {
        public string LobbyName { get; set; }
        public string LobbyID { get; set; }
        public int ConnectionCount { get; set; }
        public string[] ConnectionIDs { get; set; }
        public string[] ConnectionNames { get; set; }

        public LobbyInformation(string lobbyName, string lobbyID, List<ServerConnection> connections) : base()
        {
            LobbyName = lobbyName;
            LobbyID = lobbyID;
            ConnectionCount = connections.Count;

            ConnectionIDs = new string[ConnectionCount];
            ConnectionNames = new string[ConnectionCount];
            for (int i = 0; i < ConnectionCount; i++)
            {
                ConnectionIDs[i] = connections[i].ID;
                ConnectionNames[i] = connections[i].Name;
            }
        }
    }

    [Serializable]
    public class WaitingRoomInformation : BasePacket
    {
        public LobbyInformation[] Lobbies;
        public int LobbyCount;

        public WaitingRoomInformation() : base()
        {

        }
    }
}
