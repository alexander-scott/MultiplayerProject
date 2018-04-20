using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using ProtoBuf;

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
        Client_SendPlayerName,

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

    [ProtoContract]
    [ProtoInclude(51, typeof(StringPacket))]
    [ProtoInclude(52, typeof(IntPacket))]
    [ProtoInclude(53, typeof(RoomInformation))]
    [ProtoInclude(54, typeof(WaitingRoomInformation))]
    [ProtoInclude(55, typeof(GameInstanceInformation))]
    [ProtoInclude(56, typeof(PlayerUpdatePacket))]
    [ProtoInclude(57, typeof(PlayerFiredPacket))]
    [ProtoInclude(58, typeof(EnemySpawnedPacket))]
    [ProtoInclude(59, typeof(EnemyDefeatedPacket))]
    [ProtoInclude(60, typeof(PlayerDefeatedPacket))]
    [ProtoInclude(61, typeof(LeaderboardPacket))]
    [ProtoInclude(62, typeof(LeaderboardUpdatePacket))]
    public class BasePacket
    {
        [ProtoMember(1)]
        public DateTime SendDate { get; set; }
        [ProtoMember(2)]
        public int MessageType { get; set; }
    }

    [ProtoContract]
    public class StringPacket : BasePacket
    {
        [ProtoMember(1)]
        public string String { get; set; }
    }

    [ProtoContract]
    public class IntPacket : BasePacket
    {
        [ProtoMember(1)]
        public int Integer { get; set; }
    }

    [ProtoContract]
    public class RoomInformation : BasePacket
    {
        [ProtoMember(1)]
        public string RoomName { get; set; }
        [ProtoMember(2)]
        public string RoomID { get; set; }
        [ProtoMember(3)]
        public int ConnectionCount { get; set; }
        [ProtoMember(4)]
        public int ReadyCount { get; set; }
        [ProtoMember(5)]
        public string[] ConnectionIDs { get; set; }
        [ProtoMember(6)]
        public string[] ConnectionNames { get; set; }
        [ProtoMember(7)]
        public int RoomState { get; set; }
    }

    [ProtoContract]
    public class WaitingRoomInformation : BasePacket
    {
        [ProtoMember(1)]
        public RoomInformation[] Rooms { get; set; }
        [ProtoMember(2)]
        public int RoomCount { get; set; }
    }

    [ProtoContract]
    public class GameInstanceInformation : BasePacket
    {
        [ProtoMember(1)]
        public int PlayerCount { get; set; }
        [ProtoMember(2)]
        public string[] PlayerIDs { get; set; }
        [ProtoMember(3)]
        public string[] PlayerNames { get; set; }
        [ProtoMember(4)]
        public PlayerColour[] PlayerColours { get; set; }
        [ProtoMember(5)]
        public string LocalPlayerID { get; set; }
    }

    [ProtoContract]
    public class PlayerUpdatePacket : BasePacket
    {
        [ProtoMember(1)]
        public float DeltaTime { get; set; }
        [ProtoMember(2)]
        public float XPosition { get; set; }
        [ProtoMember(3)]
        public float YPosition { get; set; }
        [ProtoMember(4)]
        public float Speed { get; set; }
        [ProtoMember(5)]
        public float Rotation { get; set; }
        [ProtoMember(6)]
        public KeyboardMovementInput Input { get; set; }
        [ProtoMember(7)]
        public string PlayerID { get; set; }
        [ProtoMember(8)]
        public int SequenceNumber { get; set; }
    }

    [ProtoContract]
    public class KeyboardMovementInput
    {
        [ProtoMember(1)]
        public bool LeftPressed { get; set; }
        [ProtoMember(2)]
        public bool RightPressed { get; set; }
        [ProtoMember(3)]
        public bool UpPressed { get; set; }
        [ProtoMember(4)]
        public bool DownPressed { get; set; }
        [ProtoMember(5)]
        public bool FirePressed { get; set; }

        //public KeyboardMovementInput()
        //{
        //    LeftPressed = false;
        //    RightPressed = false;
        //    UpPressed = false;
        //    DownPressed = false;
        //    FirePressed = false;
        //}
    }

    [ProtoContract]
    public class PlayerColour
    {
        [ProtoMember(1)]
        public int R { get; set; }
        [ProtoMember(2)]
        public int G { get; set; }
        [ProtoMember(3)]
        public int B { get; set; }
    }

    [ProtoContract]
    public class PlayerFiredPacket : BasePacket
    {
        [ProtoMember(1)]
        public float TotalGameTime { get; set; }
        [ProtoMember(2)]
        public float XPosition { get; set; }
        [ProtoMember(3)]
        public float YPosition { get; set; }
        [ProtoMember(4)]
        public float Speed { get; set; }
        [ProtoMember(5)]
        public float Rotation { get; set; }
        [ProtoMember(6)]
        public string PlayerID { get; set; }
        [ProtoMember(7)]
        public string LaserID { get; set; }
    }

    [ProtoContract]
    public class EnemySpawnedPacket : BasePacket
    {
        [ProtoMember(1)]
        public float TotalGameTime { get; set; }
        [ProtoMember(2)]
        public float XPosition { get; set; }
        [ProtoMember(3)]
        public float YPosition { get; set; }
        [ProtoMember(4)]
        public string EnemyID { get; set; }
    }

    [ProtoContract]
    public class EnemyDefeatedPacket : BasePacket
    {
        [ProtoMember(1)]
        public string CollidedLaserID { get; set; }
        [ProtoMember(2)]
        public string CollidedEnemyID { get; set; }
        [ProtoMember(3)]
        public string AttackingPlayerID { get; set; }
        [ProtoMember(4)]
        public int AttackingPlayerNewScore { get; set; }
    }

    [ProtoContract]
    public class PlayerDefeatedPacket : BasePacket
    {
        [ProtoMember(1)]
        public string CollidedLaserID { get; set; }
        [ProtoMember(2)]
        public string CollidedPlayerID { get; set; }
        [ProtoMember(3)]
        public int CollidedPlayerNewScore { get; set; }
    }

    [ProtoContract]
    public class LeaderboardPacket : BasePacket
    {
        [ProtoMember(1)]
        public int PlayerCount { get; set; }
        [ProtoMember(2)]
        public string[] PlayerNames { get; set; }
        [ProtoMember(3)]
        public int[] PlayerScores { get; set; }
        [ProtoMember(4)]
        public PlayerColour[] PlayerColours { get; set; }
    }

    [ProtoContract]
    public class LeaderboardUpdatePacket : BasePacket
    {
        [ProtoMember(1)]
        public int PlayerCount { get; set; }
        [ProtoMember(2)]
        public int PlayerReadyCount { get; set; }
        [ProtoMember(3)]
        public bool IsClientReady { get; set; }
    }
}
