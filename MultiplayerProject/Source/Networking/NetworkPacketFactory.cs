using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public sealed class NetworkPacketFactory
    {
        private static NetworkPacketFactory instance = null;
        private static readonly object padlock = new object();

        NetworkPacketFactory()
        {
        }

        public static NetworkPacketFactory Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new NetworkPacketFactory();
                    }
                    return instance;
                }
            }
        }

        public BasePacket MakeBasePacket()
        {
            BasePacket packet = new BasePacket
            {
                SendDate = DateTime.UtcNow
            };
            return packet;
        }

        public StringPacket MakeStringPacket(string s)
        {
            StringPacket packet = new StringPacket
            {
                SendDate = DateTime.UtcNow,
                String = s
            };
            return packet;
        }

        public IntPacket MakeIntPacket(int i)
        {
            IntPacket packet = new IntPacket
            {
                SendDate = DateTime.UtcNow,
                Integer = i
            };
            return packet;
        }

        public RoomInformation MakeRoomInformationPacket(string roomName, string roomID, List<ServerConnection> connections, int readyCount, GameRoomState roomState)
        {
            RoomInformation packet = new RoomInformation
            {
                SendDate = DateTime.UtcNow,
                RoomName = roomName,
                RoomID = roomID,
                ReadyCount = readyCount,
                ConnectionCount = connections.Count,
                RoomState = (int)roomState
            };

            packet.ConnectionIDs = new string[packet.ConnectionCount];
            packet.ConnectionNames = new string[packet.ConnectionCount];
            for (int i = 0; i < packet.ConnectionCount; i++)
            {
                packet.ConnectionIDs[i] = connections[i].ID;
                packet.ConnectionNames[i] = connections[i].Name;
            }

            return packet;
        }

        public WaitingRoomInformation MakeWaitingRoomPacket()
        {
            WaitingRoomInformation packet = new WaitingRoomInformation
            {
                SendDate = DateTime.UtcNow
            };
            return packet;
        }

        public GameInstanceInformation MakeGameInstanceInformationPacket(int playerCount, List<ServerConnection> players, List<Color> playerColours, string localPlayerID)
        {
            GameInstanceInformation packet = new GameInstanceInformation
            {
                SendDate = DateTime.UtcNow,
                PlayerCount = playerCount,
                LocalPlayerID = localPlayerID
            };

            packet.PlayerIDs = new string[packet.PlayerCount];
            packet.PlayerNames = new string[packet.PlayerCount];
            packet.PlayerColours = new PlayerColour[packet.PlayerCount];
            for (int i = 0; i < packet.PlayerCount; i++)
            {
                packet.PlayerIDs[i] = players[i].ID;
                packet.PlayerNames[i] = players[i].Name;
                packet.PlayerColours[i] = MakePlayerColour(playerColours[i].R, playerColours[i].G, playerColours[i].B);
            }
            return packet;
        }

        public PlayerUpdatePacket MakePlayerUpdatePacket(float xPosition, float yPosition, float speed, float rotation)
        {
            PlayerUpdatePacket packet = new PlayerUpdatePacket
            {
                SendDate = DateTime.UtcNow,
                XPosition = xPosition,
                YPosition = yPosition,
                Speed = speed,
                Rotation = rotation
            };
            return packet;
        }

        public KeyboardMovementInput MakeKeyboardMovementInput()
        {
            KeyboardMovementInput input = new KeyboardMovementInput
            {
                LeftPressed = false,
                RightPressed = false,
                UpPressed = false,
                DownPressed = false,
                FirePressed = false
            };
            return input;
        }

        public PlayerColour MakePlayerColour(int r, int g, int b)
        {
            PlayerColour colour = new PlayerColour
            {
                R = r,
                G = g,
                B = b
            };
            return colour;
        }

        public PlayerFiredPacket MakePlayerFiredPacket(float xPosition, float yPosition, float speed, float rotation)
        {
            PlayerFiredPacket packet = new PlayerFiredPacket
            {
                XPosition = xPosition,
                YPosition = yPosition,
                Speed = speed,
                Rotation = rotation
            };
            return packet;
        }

        public EnemySpawnedPacket MakeEnemySpawnedPacket(float xPos, float yPos, string enemyID)
        {
            EnemySpawnedPacket packet = new EnemySpawnedPacket
            {
                XPosition = xPos,
                YPosition = yPos,
                EnemyID = enemyID
            };
            return packet;
        }

        public EnemyDefeatedPacket MakeEnemyDefeatedPacket(string laserID, string enemyID, string attackingPlayerID, int attackingPlayerNewScore)
        {
            EnemyDefeatedPacket packet = new EnemyDefeatedPacket
            {
                CollidedLaserID = laserID,
                CollidedEnemyID = enemyID,
                AttackingPlayerID = attackingPlayerID,
                AttackingPlayerNewScore = attackingPlayerNewScore
            };
            return packet;
        }

        public PlayerDefeatedPacket MakePlayerDefeatedPacket(string laserID, string playerID, int collidedPlayerNewScore)
        {
            PlayerDefeatedPacket packet = new PlayerDefeatedPacket
            {
                CollidedLaserID = laserID,
                CollidedPlayerID = playerID,
                CollidedPlayerNewScore = collidedPlayerNewScore
            };
            return packet;
        }

        public LeaderboardPacket MakeLeaderboardPacket(int playerCount, string[] playerNames, int[] playerScores, PlayerColour[] playerColours)
        {
            LeaderboardPacket packet = new LeaderboardPacket
            {
                PlayerCount = playerCount,
                PlayerNames = playerNames,
                PlayerScores = playerScores,
                PlayerColours = playerColours
            };
            return packet;
        }

        public LeaderboardUpdatePacket MakeLeaderboardUpdatePacket(int playerCount, int playerReadyCount, bool clientReady)
        {
            LeaderboardUpdatePacket packet = new LeaderboardUpdatePacket
            {
                PlayerCount = playerCount,
                PlayerReadyCount = playerReadyCount,
                IsClientReady = clientReady
            };
            return packet;
        }
    }
}
