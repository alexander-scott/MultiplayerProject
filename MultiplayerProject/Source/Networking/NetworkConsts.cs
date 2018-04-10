using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public delegate void EmptyDelegate();
    public delegate void StringDelegate(string str);
    public delegate void IntDelegate(int i);
    public delegate void BasePacketDelegate(BasePacket packet);
    public delegate void ServerConnectionDelegate(ServerConnection client, string roomID);

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

        WR_ClientRequest_WaitingRoomInfo,
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
        GI_ServerSend_UpdateRemotePlayer,
        
        GI_ClientSend_PlayerUpdate,
        GI_ClientSend_PlayerFired,

        GI_ServerSend_RemotePlayerFired,
        GI_ServerSend_EnemySpawn,
        GI_ServerSend_EnemyDefeated,
        GI_ServerSend_PlayerDefeated,

        GI_ServerSend_GameOver,

        // Leaderboard
        LB_ClientSend_RematchReady,
        LB_ClientSend_RematchUnready,
        LB_ClientSend_ReturnToWaitingRoom,

        LB_ServerSend_UpdateLeaderboard
    }

    public enum GameRoomState : byte
    {
        Waiting,
        InSession,
        Leaderboards
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
        public MessageType MessageType { get; set; }

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
        public GameRoomState RoomState { get; set; }

        public RoomInformation(string roomName, string roomID, List<ServerConnection> connections, int readyCount, GameRoomState roomState) : base()
        {
            RoomName = roomName;
            RoomID = roomID;
            ReadyCount = readyCount;
            ConnectionCount = connections.Count;
            RoomState = roomState;

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
        public List<RoomInformation> Rooms { get; set; }
        public int RoomCount { get; set; }

        public WaitingRoomInformation() : base()
        {

        }
    }

    [Serializable]
    public class GameInstanceInformation : BasePacket
    {
        public int PlayerCount { get; set; }
        public string[] PlayerIDs { get; set; }
        public PlayerColour[] PlayerColours { get; set; }
        public string LocalPlayerID { get; set; }

        public GameInstanceInformation(int playerCount, List<ServerConnection> players, List<Color> playerColours, string localPlayerID) : base()
        {
            PlayerCount = playerCount;
            LocalPlayerID = localPlayerID;

            PlayerIDs = new string[PlayerCount];
            PlayerColours = new PlayerColour[PlayerCount];
            for (int i = 0; i < PlayerCount; i++)
            {
                PlayerIDs[i] = players[i].ID;
                PlayerColours[i] = new PlayerColour(playerColours[i].R, playerColours[i].G, playerColours[i].B);
            }
        }
    }

    [Serializable]
    public class PlayerUpdatePacket : BasePacket
    {
        public float DeltaTime { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public float Speed { get; set; }
        public float Rotation { get; set; }
        public KeyboardMovementInput Input { get; set; }
        public string PlayerID { get; set; }
        public int SequenceNumber { get; set; }

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

    [Serializable]
    public class PlayerColour
    {
        public int R;
        public int G;
        public int B;

        public PlayerColour(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    [Serializable]
    public class PlayerFiredPacket : BasePacket
    {
        public float TotalGameTime { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public float Speed { get; set; }
        public float Rotation { get; set; }
        public string PlayerID { get; set; }
        public string LaserID { get; set; }

        public PlayerFiredPacket(float xPosition, float yPosition, float speed,
            float rotation) : base()
        {
            XPosition = xPosition;
            YPosition = yPosition;
            Speed = speed;
            Rotation = rotation;
        }
    }

    [Serializable]
    public class EnemySpawnedPacket : BasePacket
    {
        public float TotalGameTime { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public string EnemyID { get; set; }

        public EnemySpawnedPacket(float xPos, float yPos, string enemyID) :base()
        {
            XPosition = xPos;
            YPosition = yPos;
            EnemyID = enemyID;
        }
    }

    [Serializable]
    public class EnemyDefeatedPacket : BasePacket
    {
        public string CollidedLaserID { get; set; }
        public string CollidedEnemyID { get; set; }

        public string AttackingPlayerID { get; set; }
        public int AttackingPlayerNewScore { get; set; }

        public EnemyDefeatedPacket(string laserID, string enemyID, string attackingPlayerID, int attackingPlayerNewScore)
        {
            CollidedLaserID = laserID;
            CollidedEnemyID = enemyID;
            AttackingPlayerID = attackingPlayerID;
            AttackingPlayerNewScore = attackingPlayerNewScore;
        }
    }

    [Serializable]
    public class PlayerDefeatedPacket : BasePacket
    {
        public string CollidedLaserID { get; set; }
        public string CollidedPlayerID { get; set; }
        public int CollidedPlayerNewScore { get; set; }

        public PlayerDefeatedPacket(string laserID, string playerID, int collidedPlayerNewScore)
        {
            CollidedLaserID = laserID;
            CollidedPlayerID = playerID;
            CollidedPlayerNewScore = collidedPlayerNewScore;
        }
    }

    [Serializable]
    public class LeaderboardPacket : BasePacket
    {
        public int PlayerCount { get; set; }
        public string[] PlayerNames { get; set; }
        public int[] PlayerScores { get; set; }
        public PlayerColour[] PlayerColours { get; set; }

        public LeaderboardPacket(int playerCount, string[] playerNames, int[] playerScores, PlayerColour[] playerColours)
        {
            PlayerCount = playerCount;
            PlayerNames = playerNames;
            PlayerScores = playerScores;
            PlayerColours = playerColours;
        }
    }

    [Serializable]
    public class LeaderboardUpdatePacket : BasePacket
    {
        public int PlayerCount { get; set; }
        public int PlayerReadyCount { get; set; }
        public bool IsClientReady { get; set; }

        public LeaderboardUpdatePacket(int playerCount, int playerReadyCount, bool clientReady)
        {
            PlayerCount = playerCount;
            PlayerReadyCount = playerReadyCount;
            IsClientReady = clientReady;
        }
    }
}
