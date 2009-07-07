using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework;
using NetGore;
using NetGore.Collections;
using NetGore.IO;
using NetGore.Network;

namespace DemoGame.Server
{
    /// <summary>
    /// Contains the information about a single map instance and all of the Entities it contains.
    /// </summary>
    public class Map : MapBase, IDisposable
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly SafeEnumerator<NPC> _npcEnumerator;
        readonly List<NPC> _npcs;
        readonly SafeEnumerator<User> _userEnumerator;
        readonly TSList<User> _users;
        readonly World _world;
        bool _disposed;

        /// <summary>
        /// Gets the DBController used by this Map.
        /// </summary>
        public DBController DBController
        {
            get { return World.DBController; }
        }

        /// <summary>
        /// Gets an IEnumerable of NPCs on the Map.
        /// </summary>
        public IEnumerable<NPC> NPCs
        {
            get { return _npcEnumerator; }
        }

        /// <summary>
        /// Gets the list of users on the Map.
        /// </summary>
        public IEnumerable<User> Users
        {
            get { return _userEnumerator; }
        }

        /// <summary>
        /// Gets the World the Map belongs to.
        /// </summary>
        public World World
        {
            get { return _world; }
        }

        public Map(MapIndex mapIndex, World world) : base(mapIndex, world)
        {
            _world = world;

            _npcs = new List<NPC>();
            _npcEnumerator = new SafeEnumerator<NPC>(_npcs);

            _users = new TSList<User>();
            _userEnumerator = new SafeEnumerator<User>(_users);
        }

        public override void AddEntity(Entity entity)
        {
            IRespawnable respawnable = entity as IRespawnable;
            if (respawnable != null && !respawnable.ReadyToRespawn(GetTime()))
            {
                AddToRespawn(respawnable);
                return;
            }

            base.AddEntity(entity);
        }

        /// <summary>
        /// Adds an IRespawnable to the list of objects that need to respawn.
        /// </summary>
        /// <param name="respawnable">The object to respawn.</param>
        public void AddToRespawn(IRespawnable respawnable)
        {
            World.AddToRespawn(respawnable);
        }

        void CharAdded(Entity entity)
        {
            Character character = entity as Character;
            if (character == null)
                return;

            // If the character was already on a map, so remove them from the old map
            if (character.Map != null)
            {
                const string errmsg = "Character `{0}` [{1}] added to new map, but is already on a map!";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, character, character.MapEntityIndex);
                Debug.Fail(string.Format(errmsg, character, character.MapEntityIndex));
                character.Map.RemoveEntity(character);
            }

            // Set the new map
            character.Map = this;

            // Added character is a User
            User user = character as User;
            if (user != null)
            {
                Debug.Assert(!Users.Contains(user), string.Format("Users list already contains `{0}`!", user));
                _users.Add(user);
                SendMapData(user);
                return;
            }

            // Added character is a NPC
            NPC npc = character as NPC;
            if (npc != null)
            {
                Debug.Assert(!NPCs.Contains(npc), string.Format("NPCs list already contains `{0}`!", npc));
                _npcs.Add(npc);
                return;
            }

            // Unknown added character type - not actually an error, but it is likely an oversight
            throw new Exception("Unknown Character type - not a NPC or User...?");
        }

        void CharRemoved(Entity entity)
        {
            Character character = entity as Character;
            if (character == null)
                return;

            User user;
            NPC npc;

            if ((user = character as User) != null)
                _users.Remove(user);
            else if ((npc = character as NPC) != null)
                _npcs.Remove(npc);

            return;
        }

        /// <summary>
        /// Creates an ItemEntity on the map.
        /// </summary>
        /// <param name="template">ItemTemplate to create the item from.</param>
        /// <param name="pos">Position to create the item at.</param>
        /// <param name="amount">Amount of the item to create. Must be greater than 0.</param>
        /// <returns>Reference to the new ItemEntity created.</returns>
        public ItemEntity CreateItem(ItemTemplate template, Vector2 pos, byte amount)
        {
            // Check for a valid amount
            if (amount < 1)
            {
                const string errmsg = "Invalid item amount `{0}`! Amount must be > 0.";
                Debug.Fail(string.Format(errmsg, amount));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, amount);
                return null;
            }

            // Check for a valid template
            if (template == null)
            {
                const string errmsg = "Parameter `template` may not be null!";
                Debug.Fail(errmsg);
                if (log.IsErrorEnabled)
                    log.Error(errmsg);
                return null;
            }

            // Create the item, add it to the map, and return the reference
            ItemEntity item = new ItemEntity(template, pos, amount);
            AddEntity(item);
            return item;
        }

        /// <summary>
        /// When overridden in the derived class, creates a new WallEntityBase instance.
        /// </summary>
        /// <returns>WallEntityBase that is to be used on the map.</returns>
        protected override WallEntityBase CreateWall(IValueReader r)
        {
            return new WallEntity(r);
        }

        /// <summary>
        /// When overridden in the derived class, allows for additional processing on Entities added to the map.
        /// This is called after the Entity has finished being added to the map.
        /// </summary>
        /// <param name="entity">Entity that was added to the map.</param>
        protected override void EntityAdded(Entity entity)
        {
            base.EntityAdded(entity);

            // When a User enters the Map, reset the inactivity counter
            if (entity is User)
                _inactiveCounter = _emptyMapNoUpdateDelay;

            // Create the DynamicEntity for everyone on the map
            if (_users.Count > 0)
            {
                DynamicEntity de = entity as DynamicEntity;
                if (de != null)
                {
                    using (PacketWriter pw = ServerPacket.CreateDynamicEntity(de))
                    {
                        Send(pw);
                    }
                }
            }

            // Handle the different types of entities
            CharAdded(entity);
        }

        /// <summary>
        /// When overridden in the derived class, allows for additional processing on Entities removed from the map.
        /// This is called after the Entity has finished being removed from the map.
        /// </summary>
        /// <param name="entity">Entity that was removed from the map.</param>
        protected override void EntityRemoved(Entity entity)
        {
            base.EntityRemoved(entity);

            // Handle the different types of entities
            DynamicEntity dynamicEntity;
            if ((dynamicEntity = entity as DynamicEntity) != null)
            {
                CharRemoved(entity);

                // Destroy the DynamicEntity for everyone on the map
                if (_users.Count > 0)
                {
                    using (PacketWriter pw = ServerPacket.RemoveDynamicEntity(dynamicEntity))
                    {
                        Send(pw);
                    }
                }
            }
        }

        /// <summary>
        /// Send a message to every user in the map. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send.</param>
        public void Send(BitStream data)
        {
            Send(data, null, true);
        }

        /// <summary>
        /// Send a message to every user in the map. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send.</param>
        /// <param name="reliable">Whether or not the data should be sent over a reliable stream.</param>
        public void Send(BitStream data, bool reliable)
        {
            Send(data, null, reliable);
        }

        /// <summary>
        /// Send a packet to every user in the map. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send.</param>
        /// <param name="skipUser">User to skip sending to.</param>
        public void Send(BitStream data, User skipUser)
        {
            Send(data, skipUser, true);
        }

        /// <summary>
        /// Send a packet to every user in the map. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send.</param>
        /// <param name="skipUser">User to skip sending to.</param>
        /// <param name="reliable">Whether or not the data should be sent over a reliable stream.</param>
        public void Send(BitStream data, User skipUser, bool reliable)
        {
            // Check for valid data
            if (data == null || data.Length < 1)
            {
                const string errmsg = "Attempted to send null or invalid data to the map.";
                if (log.IsWarnEnabled)
                    log.Warn(errmsg);
                Debug.Fail(errmsg);
                return;
            }

            // Send the data to all users in the map
            foreach (User user in Users)
            {
                if (user != null)
                {
                    if (user != skipUser)
                        user.Send(data, reliable);
                }
                else
                {
                    const string errmsg = "Null User found in the Map's User list. This should never happen.";
                    Debug.Fail(errmsg);
                    if (log.IsErrorEnabled)
                        log.Error(errmsg);
                }
            }
        }

        /// <summary>
        /// Sends the data to the specified user of all existing content on the map
        /// </summary>
        /// <param name="user">User to send the map data to</param>
        void SendMapData(User user)
        {
            using (PacketWriter pw = ServerPacket.GetWriter())
            {
                // Tell the user to change the map
                ServerPacket.SetMap(pw, Index);
                user.Send(pw);

                // Send dynamic entities
                foreach (DynamicEntity dynamicEntity in DynamicEntities)
                {
                    pw.Reset();
                    ServerPacket.CreateDynamicEntity(pw, dynamicEntity);
                    user.Send(pw);
                }

                // Now that the user know about the map and every character on it, tell them which one is theirs
                pw.Reset();
                ServerPacket.SetUserChar(pw, user.MapEntityIndex);
                user.Send(pw);
            }
        }

        /// <summary>
        /// Send a packet to every user in the map within a reasonable
        /// range from the origin. Use this for packets that only affect
        /// those who are already in view from the origin such as brief
        /// visual effects.
        /// </summary>
        /// <param name="origin">Position in which the event creating the packet triggered</param>
        /// <param name="data">BitStream containing the data to send</param>
        public void SendToArea(Vector2 origin, BitStream data)
        {
            if (data == null)
                return;

            Vector2 screenSize = GameData.ScreenSize * 1.25f;
            Vector2 min = origin - screenSize;
            Vector2 max = origin + screenSize;

            foreach (User user in Users)
            {
                if (user != null)
                {
                    Vector2 p = user.Position;
                    if (p.X > min.X && p.Y > min.Y && p.X < max.X && p.Y < max.Y)
                        user.Send(data);
                }
                else
                    Debug.Fail("Null user found in Map's Users list.");
            }
        }

        /// <summary>
        /// Synchronizes all of the DynamicEntities.
        /// </summary>
        void SynchronizeDynamicEntities()
        {
            int currentTime = GetTime();

            foreach (DynamicEntity dynamicEntity in DynamicEntities)
            {
                if (!dynamicEntity.IsSynchronized)
                {
                    using (PacketWriter pw = ServerPacket.SynchronizeDynamicEntity(dynamicEntity))
                    {
                        Send(pw);
                    }
                }

                if (dynamicEntity.NeedSyncPositionAndVelocity(currentTime))
                {
                    using (PacketWriter pw = ServerPacket.UpdateVelocityAndPosition(dynamicEntity, currentTime))
                    {
                        // TODO: Send to visual area, plus a little more
                        Send(pw, false);
                    }
                }
            }
        }

        /// <summary>
        /// How long, in milliseconds, it takes from when the last User in a Map leaves it for the Map to stop
        /// updating all-together until a User enters the Map again.
        /// </summary>
        const int _emptyMapNoUpdateDelay = 60000;

        /// <summary>
        /// How much time, in milliseconds, remaining until the Map goes inactive. When this value is less than
        /// or equal to 0, the Map should be considered inactive.
        /// </summary>
        int _inactiveCounter;

        /// <summary>
        /// Gets if the Map is currently inactive.
        /// </summary>
        bool IsInactive { get { return _inactiveCounter <= 0; } }

        public override void Update(int deltaTime)
        {
            // If there are no Users on the Map, update the inactive counter or skip updating if already inactive
            if (_users.Count == 0)
            {
                if (IsInactive)
                    return;

                _inactiveCounter -= deltaTime;
            }

            base.Update(deltaTime);

            SynchronizeDynamicEntities();
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of the map and all of the Entities on it.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                Debug.Fail("Map is already disposed.");
                return;
            }

            _disposed = true;

            // Dispose of all the disposable entities
            var disposableEntities = Entities.OfType<IDisposable>();
            foreach (IDisposable entity in disposableEntities)
            {
                _world.DisposeStack.Push(entity);
            }
        }

        #endregion
    }
}