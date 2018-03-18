using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public delegate void EmptyDelegate();
    public delegate void StringDelegate(string str);
    public delegate void IntDelegate(int i);
    public delegate void WaitingRoomDelegate(WaitingRoomInformation waitingRoom);
    public delegate void GameRoomDelegate(GameInstanceInformation gameRoom);

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

        // Game instance
        GI_ServerSend_LoadNewGame,
        
        GI_ClientSend_PlayerUpdatePacket,
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
        public bool IsPlaying { get; set; }

        public RoomInformation(string roomName, string roomID, List<ServerConnection> connections, int readyCount, bool isPlaying) : base()
        {
            RoomName = roomName;
            RoomID = roomID;
            ReadyCount = readyCount;
            ConnectionCount = connections.Count;
            IsPlaying = isPlaying;

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

    [Serializable]
    public class GameInstanceInformation : BasePacket
    {
        public int PlayerCount { get; set; }
        public string[] PlayerIDs { get; set; }
        public string LocalPlayerID { get; set; }

        public GameInstanceInformation(int playerCount, List<ServerConnection> players, string localPlayerID) : base()
        {
            PlayerCount = playerCount;
            LocalPlayerID = localPlayerID;

            PlayerIDs = new string[PlayerCount];
            for (int i = 0; i < PlayerCount; i++)
            {
                PlayerIDs[i] = players[i].ID;
            }
        }
    }

    [Serializable]
    public class PlayerUpdatePacket : BasePacket
    {
        public float TotalGameTime { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public float Speed { get; set; }
        public float Rotation { get; set; }
        public KeyboardMovementInput Input { get; set; }

        public PlayerUpdatePacket(float xPosition, float yPosition, float speed,
            float rotation) :base()
        {
            XPosition = xPosition;
            YPosition = yPosition;
            Speed = speed;
            Rotation = rotation;
        }
    }

    [Serializable]
    public class KeyboardMovementInput
    {
        public bool LeftPressed { get; set; }
        public bool RightPressed { get; set; }
        public bool UpPressed { get; set; }
        public bool DownPressed { get; set; }
        public bool FirePressed { get; set; }

        public KeyboardMovementInput()
        {
            LeftPressed = false;
            RightPressed = false;
            UpPressed = false;
            DownPressed = false;
            FirePressed = false;
        }
    }
}
