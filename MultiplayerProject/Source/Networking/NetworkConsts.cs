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
        GameRoom,
        Game
    }

    public enum MessageType : byte
    {
        TestPacket,

        // Base
        Client_Disconnect,
        Server_Disconnect,

        // Waiting room
        WR_ServerSend_WaitingRoomFullInfo,
        WR_ServerSend_SingleUpdate,
        WR_ServerSend_NewRoom,
        WR_ServerSend_DeleteRoom,

        WR_ClientRequest_JoinRoom,
        WR_ClientRequest_LeaveRoom,
        WR_ClientRequest_CreateRoom,

        WR_ServerResponse_SuccessJoinRoom,
        WR_ServerResponse_SuccessLeaveRoom,
        WR_ServerResponse_FailJoinRoom,
        WR_ServerResponse_FailCreateRoom,

        // Game room
        GR_ServerSend_GameRoomFullInfo,
        GR_ServerSend_AllClientsReady,
        GR_ServerSend_GameStart3,
        GR_ServerSend_GameStart2,
        GR_ServerSend_GameStart1,
        GR_ServerSend_CountdownAborted,
        GR_ServerSendGameStart,

        GR_ClientRequest_Ready,
        GR_ClientRequest_Unready,

        GR_ServerResponse_SuccessReady,
        GR_ServerResponse_SuccessUnready,
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
    public class StringPacket : BasePacket
    {
        public string String { get; set; }

        public StringPacket(string s) : base()
        {
            String = s;
        }
    }

    [Serializable]
    public class IntPacket : BasePacket
    {
        public int Integer { get; set; }

        public IntPacket(int i) : base()
        {
            Integer = i;
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
    public class RoomInformation : BasePacket
    {
        public string RoomName { get; set; }
        public string RoomID { get; set; }
        public int ConnectionCount { get; set; }
        public int ReadyCount { get; set; }
        public string[] ConnectionIDs { get; set; }
        public string[] ConnectionNames { get; set; }

        public RoomInformation(string roomName, string roomID, List<ServerConnection> connections, int readyCount) : base()
        {
            RoomName = roomName;
            RoomID = roomID;
            ReadyCount = readyCount;
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
        public RoomInformation[] Rooms;
        public int RoomCount;

        public WaitingRoomInformation() : base()
        {

        }
    }
}
