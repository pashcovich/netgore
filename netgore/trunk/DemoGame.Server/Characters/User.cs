using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DemoGame.Server.DbObjs;
using log4net;
using Microsoft.Xna.Framework;
using NetGore;
using NetGore.IO;
using NetGore.Network;
using NetGore.NPCChat;

namespace DemoGame.Server
{
    /// <summary>
    /// A user-controlled character.
    /// </summary>
    public class User : Character
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly UserChatDialogState _chatState;

        /// <summary>
        /// The socket used to communicate with the User.
        /// </summary>
        readonly IIPSocket _conn;

        readonly UserShoppingState _shoppingState;

        readonly SocketSendQueue _unreliableBuffer;
        readonly UserInventory _userInventory;
        readonly UserStats _userStatsBase;
        readonly UserStats _userStatsMod;

        /// <summary>
        /// Not used by User.
        /// </summary>
        public override NPCChatDialogBase ChatDialog
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the UserChatDialogState for this User.
        /// </summary>
        public UserChatDialogState ChatState
        {
            get { return _chatState; }
        }

        /// <summary>
        /// Gets the socket connection info for the user
        /// </summary>
        public IIPSocket Conn
        {
            get { return _conn; }
        }

        /// <summary>
        /// When overridden in the derived class, gets this <see cref="Character"/>'s <see cref="Character.Shop"/>.
        /// </summary>
        public override Shop Shop
        {
            get { return null; }
        }

        public UserShoppingState ShoppingState
        {
            get { return _shoppingState; }
        }

        /// <summary>
        /// User constructor.
        /// </summary>
        /// <param name="conn">Connection to the user's client.</param>
        /// <param name="world">World the user belongs to.</param>
        /// <param name="characterID">User's CharacterID.</param>
        public User(IIPSocket conn, World world, CharacterID characterID) : base(world, true)
        {
            // Set the connection information
            _conn = conn;

            // Create some objects
            _shoppingState = new UserShoppingState(this);
            _chatState = new UserChatDialogState(this);
            _userStatsBase = (UserStats)BaseStats;
            _userStatsMod = (UserStats)ModStats;
            _unreliableBuffer = new SocketSendQueue(conn.MaxUnreliableMessageSize);

            // Load the character data
            Load(characterID);

            // Ensure the correct Alliance is being used
            Alliance = AllianceManager["user"];

            // Attach to some events
            OnKillCharacter += User_OnKillCharacter;
            OnChangeStatPoints += User_OnChangeStatPoints;
            OnChangeExp += User_OnChangeExp;
            OnChangeCash += User_OnChangeCash;
            OnChangeLevel += User_OnChangeLevel;

            _userInventory = (UserInventory)Inventory;

            // Activate the user
            IsAlive = true;

            // Send the initial information
            User_OnChangeLevel(this, Level, Level);
            User_OnChangeCash(this, Cash, Cash);
            User_OnChangeExp(this, Exp, Exp);
            User_OnChangeStatPoints(this, StatPoints, StatPoints);
        }

        /// <summary>
        /// When overridden in the derived class, lets the Character handle being given items through GiveItem().
        /// </summary>
        /// <param name="item">The <see cref="ItemEntity"/> the Character was given.</param>
        /// <param name="amount">The amount of the <paramref name="item"/> the Character was given. Will be greater
        /// than 0.</param>
        protected override void AfterGiveItem(ItemEntity item, byte amount)
        {
            // If any was added, send the notification
            using (PacketWriter pw = ServerPacket.NotifyGetItem(item.Name, amount))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// When overridden in the derived class, checks if enough time has elapesd since the Character died
        /// for them to be able to respawn.
        /// </summary>
        /// <param name="currentTime">Current game time.</param>
        /// <returns>True if enough time has elapsed; otherwise false.</returns>
        protected override bool CheckRespawnElapsedTime(int currentTime)
        {
            // Users don't need to wait for nuttin'!
            return true;
        }

        /// <summary>
        /// When overridden in the derived class, creates the CharacterEquipped for this Character.
        /// </summary>
        /// <returns>
        /// The CharacterEquipped for this Character.
        /// </returns>
        protected override CharacterEquipped CreateEquipped()
        {
            return new UserEquipped(this);
        }

        /// <summary>
        /// When overridden in the derived class, creates the CharacterInventory for this Character.
        /// </summary>
        /// <returns>
        /// The CharacterInventory for this Character.
        /// </returns>
        protected override CharacterInventory CreateInventory()
        {
            return new UserInventory(this);
        }

        /// <summary>
        /// When overridden in the derived class, creates the CharacterSPSynchronizer for this Character.
        /// </summary>
        /// <returns>
        /// The CharacterSPSynchronizer for this Character.
        /// </returns>
        protected override CharacterSPSynchronizer CreateSPSynchronizer()
        {
            return new UserSPSynchronizer(this);
        }

        /// <summary>
        /// When overridden in the derived class, creates the CharacterStatsBase for this Character.
        /// </summary>
        /// <param name="statCollectionType">The type of StatCollectionType to create.</param>
        /// <returns>
        /// The CharacterStatsBase for this Character.
        /// </returns>
        protected override CharacterStatsBase CreateStats(StatCollectionType statCollectionType)
        {
            return new UserStats(this, statCollectionType);
        }

        /// <summary>
        /// Sends all the data buffered for the unreliable channel by SendUnreliableBuffered() to the User.
        /// </summary>
        public void FlushUnreliableBuffer()
        {
            if (IsDisposed)
                return;

            if (_conn == null || !_conn.IsConnected)
            {
                const string errmsg =
                    "Send to `{0}` failed - Conn is null or not connected." +
                    " Connection by client was probably not closed properly. Usually not a big deal. Disposing User...";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, this);
                DelayedDispose();

                return;
            }

            byte[] data;
            while ((data = _unreliableBuffer.Dequeue()) != null)
            {
                _conn.Send(data, false);
            }
        }

        /// <summary>
        /// Gives the kill reward.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="cash">The cash.</param>
        protected override void GiveKillReward(uint exp, uint cash)
        {
            base.GiveKillReward(exp, cash);

            using (PacketWriter pw = ServerPacket.NotifyExpCash(exp, cash))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// When overridden in the derived class, handles additional loading stuff.
        /// </summary>
        /// <param name="v">The ICharacterTable containing the database values for this Character.</param>
        protected override void HandleAdditionalLoading(ICharacterTable v)
        {
            base.HandleAdditionalLoading(v);

            World.AddUser(this);
        }

        /// <summary>
        /// Performs the actual disposing of the Entity. This is called by the base Entity class when
        /// a request has been made to dispose of the Entity. This is guarenteed to only be called once.
        /// All classes that override this method should be sure to call base.DisposeHandler() after
        /// handling what it needs to dispose.
        /// </summary>
        protected override void HandleDispose()
        {
            if (log.IsInfoEnabled)
                log.InfoFormat("Disposing User `{0}`.", this);

            base.HandleDispose();

            // Remove the User from being the active User in the account
            UserAccount account = World.GetUserAccount(Conn);
            if (account != null)
                account.CloseUser();
        }

        /// <summary>
        /// Kills the user
        /// </summary>
        public override void Kill()
        {
            base.Kill();

            // TODO: Respawn the user to the correct respawn location
            Teleport(new Vector2(100, 100));

            UpdateModStats();

            HP = ModStats[StatType.MaxHP];
            MP = ModStats[StatType.MaxMP];
        }

        /// <summary>
        /// Makes the Character's level increase. Does not alter the experience in any way since it is assume that,
        /// when this is called, the Character already has enough experience for the next level.
        /// </summary>
        protected override void LevelUp()
        {
            base.LevelUp();

            // Notify users on the map of the level-up
            using (PacketWriter pw = ServerPacket.NotifyLevel(MapEntityIndex))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Sends data to the User. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send to the User.</param>
        public void Send(BitStream data)
        {
            Send(data, true);
        }

        /// <summary>
        /// Sends data to the User. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send to the User.</param>
        /// <param name="reliable">Whether or not the data should be sent over a reliable stream.</param>
        public void Send(BitStream data, bool reliable)
        {
            if (IsDisposed)
            {
                const string errmsg = "Tried to send data to disposed User `{0}` [reliable = `{1}`]";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, this, reliable);
                return;
            }

            if (_conn == null || !_conn.IsConnected)
            {
                const string errmsg = "Send to `{0}` failed - Conn is null or not connected. Disposing User...";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, this);
                Debug.Fail(string.Format(errmsg, this));
                DelayedDispose();

                return;
            }

            _conn.Send(data, reliable);
        }

        /// <summary>
        /// Sends data to the User.
        /// </summary>
        /// <param name="message">GameMessage to send to the User.</param>
        public void Send(GameMessage message)
        {
            using (PacketWriter pw = ServerPacket.SendMessage(message))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Sends data to the User.
        /// </summary>
        /// <param name="message">GameMessage to send to the User.</param>
        /// <param name="parameters">Message parameters.</param>
        public void Send(GameMessage message, params object[] parameters)
        {
            using (PacketWriter pw = ServerPacket.SendMessage(message, parameters))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Sends the item information for an item in a given equipment slot to the client.
        /// </summary>
        /// <param name="slot">Equipment slot of the ItemEntity to send the info for.</param>
        public void SendEquipmentItemStats(EquipmentSlot slot)
        {
            // Check for a valid slot
            if (!slot.IsDefined())
            {
                const string errmsg = "User `{0}` attempted to access invalid equipment slot `{1}`.";
                Debug.Fail(string.Format(errmsg, this, slot));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, this, slot);
                return;
            }

            // Get the item
            ItemEntity item = Equipped[slot];
            if (item == null)
            {
                const string errmsg = "User `{0}` requested info for equipment slot `{1}`, but the slot has no ItemEntity.";
                if (log.IsInfoEnabled)
                    log.InfoFormat(errmsg, this, slot);
                return;
            }

            // Send the item info
            using (PacketWriter pw = ServerPacket.SendEquipmentItemInfo(slot, item))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Sends the item information for an item in a given inventory slot to the client.
        /// </summary>
        /// <param name="slot">Inventory slot of the ItemEntity to send the info for.</param>
        public void SendInventoryItemStats(InventorySlot slot)
        {
            // Check for a valid slot
            if (slot >= InventoryBase.MaxInventorySize)
            {
                const string errmsg = "User `{0}` attempted to access invalid inventory slot `{1}`.";
                Debug.Fail(string.Format(errmsg, this, slot));
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, this, slot);
                return;
            }

            // Get the item
            ItemEntity item = Inventory[slot];
            if (item == null)
            {
                const string errmsg = "User `{0}` requested info for inventory slot `{1}`, but the slot has no ItemEntity.";
                if (log.IsInfoEnabled)
                    log.InfoFormat(errmsg, this, slot);
                return;
            }

            // Send the item info
            using (PacketWriter pw = ServerPacket.SendInventoryItemInfo(slot, item))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Sends data to the User. The data is actually buffered indefinately until FlushUnreliableBuffer() is
        /// called. This method is thread-safe.
        /// </summary>
        /// <param name="data">BitStream containing the data to send to the User.</param>
        public void SendUnreliableBuffered(BitStream data)
        {
            _unreliableBuffer.Enqueue(data);
        }

        /// <summary>
        /// Handles when an ActiveStatusEffect is added to this Character's StatusEffects.
        /// </summary>
        /// <param name="effects">The CharacterStatusEffects the event took place on.</param>
        /// <param name="ase">The ActiveStatusEffect that was added.</param>
        protected override void StatusEffects_HandleOnAdd(CharacterStatusEffects effects, ActiveStatusEffect ase)
        {
            base.StatusEffects_HandleOnAdd(effects, ase);

            int currentTime = GetTime();
            int timeLeft = ase.GetTimeRemaining(currentTime);

            using (PacketWriter pw = ServerPacket.AddStatusEffect(ase.StatusEffect.StatusEffectType, ase.Power, timeLeft))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Handles when an ActiveStatusEffect is removed from this Character's StatusEffects.
        /// </summary>
        /// <param name="effects">The CharacterStatusEffects the event took place on.</param>
        /// <param name="ase">The ActiveStatusEffect that was removed.</param>
        protected override void StatusEffects_HandleOnRemove(CharacterStatusEffects effects, ActiveStatusEffect ase)
        {
            base.StatusEffects_HandleOnRemove(effects, ase);

            using (PacketWriter pw = ServerPacket.RemoveStatusEffect(ase.StatusEffect.StatusEffectType))
            {
                Send(pw);
            }
        }

        /// <summary>
        /// Updates the Entity.
        /// </summary>
        /// <param name="imap">Map that this Entity is on.</param>
        /// <param name="deltaTime">Time elapsed (in milliseconds) since the last update.</param>
        public override void Update(IMap imap, float deltaTime)
        {
            // Don't allow movement while chatting
            if (!GameData.AllowMovementWhileChattingToNPC)
            {
                if (ChatState.IsChatting && Velocity != Vector2.Zero)
                    SetVelocity(Vector2.Zero);
            }

            // Perform the general character updating
            base.Update(imap, deltaTime);

            // Synchronize the User's stats
            _userStatsBase.UpdateClient();
            _userStatsMod.UpdateClient();

            // Synchronize the Inventory
            _userInventory.UpdateClient();
        }

        public void UseInventoryItem(InventorySlot slot)
        {
            // Get the ItemEntity to use
            ItemEntity item = Inventory[slot];
            if (item == null)
            {
                const string errmsg = "Tried to use inventory slot `{0}`, but it contains no ItemEntity.";
                Debug.Fail(string.Format(errmsg, slot));
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, slot);
                return;
            }

            // Try to use the ItemEntity
            if (!UseItem(item, slot))
            {
                // Check if the failure to use the ItemEntity was due to an invalid amount of the ItemEntity
                if (item.Amount <= 0)
                {
                    const string errmsg = "Tried to use inventory ItemEntity `{0}` at slot `{1}`, but it had an invalid amount.";
                    Debug.Fail(string.Format(errmsg, item, slot));
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, item, slot);

                    // Destroy the ItemEntity
                    Inventory.RemoveAt(slot);
                    item.Dispose();
                }
            }

            // Lower the count of use-once items
            if (item.Type == ItemType.UseOnce)
                Inventory.DecreaseItemAmount(slot);
        }

        void User_OnChangeCash(Character character, uint oldCash, uint cash)
        {
            using (PacketWriter pw = ServerPacket.SetCash(cash))
            {
                Send(pw);
            }
        }

        public bool TryBuyItem(IItemTemplateTable itemTemplate, byte amount)
        {
            if (itemTemplate == null || amount <= 0)
                return false;

            int totalCost = itemTemplate.Value * amount;
            
            // Check for enough money to buy
            if (Cash < totalCost)
            {
                if (amount == 1)
                {
                    using (var pw = ServerPacket.SendMessage(GameMessage.ShopInsufficientFundsToPurchaseSingular, itemTemplate.Name))
                        Send(pw);
                }
                else
                {
                    using (var pw = ServerPacket.SendMessage(GameMessage.ShopInsufficientFundsToPurchasePlural, amount, itemTemplate.Name))
                        Send(pw);
                }
                return false;
            }

            // Create the item
            ItemEntity itemEntity = new ItemEntity(itemTemplate, amount);

            // Check for room in the inventory
            if (!Inventory.CanAdd(itemEntity))
            {
                itemEntity.Dispose();
                return false;
            }

            // Add to the inventory
            var remainderItem = Inventory.Add(itemEntity);

            // Find the number of remaining items (in case something went wrong and not all was added)
            int remainderAmount = remainderItem == null ? 0 : (int)remainderItem.Amount;

            // Find the difference in the requested amount and remaining amount to get the amount added, and
            // only charge the character for that (so they pay for what they got)
            int amountPurchased = amount - remainderAmount;

            if (amountPurchased < 0)
            {
                const string errmsg = "Somehow, amountPurchased was negative ({0})!"
                    + " User: `{1}`. Item template: `{2}`. Requested amount: `{3}`. ItemEntity: `{4}`";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, amountPurchased, this, itemTemplate, amountPurchased, itemEntity);
                Debug.Fail(string.Format(errmsg, amountPurchased, this, itemTemplate, amountPurchased, itemEntity));

                // Raise the amount purchased to 0. They will get it for free, but that is better than PAYING them.
                amountPurchased = 0;
            }

            // Charge them
            uint chargeAmount = (uint)Math.Max(0, amountPurchased * itemEntity.Value);
            Cash -= chargeAmount;

            // Send purchase message
            if (amountPurchased <= 1)
            {
                using (var pw = ServerPacket.SendMessage(GameMessage.ShopPurchaseSingular, itemTemplate.Name,chargeAmount))
                    Send(pw);
            }
            else
            {
                using (var pw = ServerPacket.SendMessage(GameMessage.ShopPurchasePlural, amountPurchased, itemTemplate.Name, chargeAmount))
                    Send(pw);
            }

            return true;
        }

        void User_OnChangeExp(Character character, uint oldExp, uint exp)
        {
            using (PacketWriter pw = ServerPacket.SetExp(exp))
            {
                Send(pw);
            }
        }

        void User_OnChangeLevel(Character character, byte oldLevel, byte level)
        {
            using (PacketWriter pw = ServerPacket.SetLevel(level))
            {
                Send(pw);
            }
        }

        void User_OnChangeStatPoints(Character character, uint oldValue, uint newValue)
        {
            using (PacketWriter pw = ServerPacket.SetStatPoints(newValue))
            {
                Send(pw);
            }
        }

        void User_OnKillCharacter(Character killed, Character killer)
        {
            Debug.Assert(killer == this);
            Debug.Assert(killed != null);

            NPC killedNPC = killed as NPC;

            // Handle killing a NPC
            if (killedNPC != null)
                GiveKillReward(killedNPC.GiveExp, killedNPC.GiveCash);
        }

        public class UserShoppingState
        {
            readonly User _user;
            readonly object _changeShopLock = new object();

            Map _shopMap;
            Entity _shopOwner;
            Shop _shoppingAt;

            public Entity ShopOwner
            {
                get { return _shopOwner; }
            }

            public Shop ShoppingAt
            {
                get { return _shoppingAt; }
            }

            public User User
            {
                get { return _user; }
            }

            public UserShoppingState(User user)
            {
                if (user == null)
                    throw new ArgumentNullException("user");

                _user = user;
            }

            bool IsValidDistance(Entity shopKeeper)
            {
                return GameData.IsValidDistanceToShop(User, shopKeeper);
            }

            void SendStartShopping(Shop shop)
            {
                using (PacketWriter pw = ServerPacket.StartShopping(shop))
                {
                    User.Send(pw);
                }
            }

            void SendStopShopping()
            {
                using (PacketWriter pw = ServerPacket.StopShopping())
                {
                    User.Send(pw);
                }
            }

            public bool TryStartShopping(Character shopkeeper)
            {
                if (shopkeeper == null)
                    return false;

                return TryStartShopping(shopkeeper.Shop, shopkeeper, shopkeeper.Map);
            }

            public bool TryPurchase(ShopItemIndex slot, byte amount)
            {
                if (slot < 0 || slot >= GameData.MaxShopItems)
                    return false;

                Shop shop;

                lock (_changeShopLock)
                {
                    // Check for a valid shop
                    if (_shoppingAt == null || _shopOwner == null || _shopMap == null)
                        return false;

                    // Check for a valid distance
                    if (!IsValidDistance(_shopOwner))
                    {
                        // Stop shopping
                        SendStopShopping();
                        _shoppingAt = null;
                        _shopOwner = null;
                        _shopMap = null;
                        return false;
                    }

                    // Store values locally so we can finish shopping without a lock
                    // If you want to use other locals outside of the lock, like _shopOwner, apply the same pattern
                    shop = _shoppingAt;
                }

                // Get and validate the item
                var shopItem = shop.GetShopItem(slot);
                if (shopItem == null)
                    return false;

                // Try to buy the item
                return User.TryBuyItem(shopItem.ItemTemplate, amount);
            }

            public bool TryStartShopping(Shop shop, Entity shopkeeper, Map entityMap)
            {
                if (shop == null || shopkeeper == null || entityMap == null)
                    return false;

                if (User.Map != entityMap)
                    return false;

                if (!IsValidDistance(shopkeeper))
                    return false;

                lock (_changeShopLock)
                {
                    // If the User was already shopping somewhere else, stop that shopping
                    if (_shoppingAt != null)
                        SendStopShopping();

                    // Start the shopping
                    _shoppingAt = shop;
                    _shopOwner = shopkeeper;
                    _shopMap = entityMap;
                }

                SendStartShopping(shop);

                return true;
            }
        }
    }
}