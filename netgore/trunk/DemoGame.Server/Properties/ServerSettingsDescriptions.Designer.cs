﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DemoGame.Server.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ServerSettingsDescriptions {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ServerSettingsDescriptions() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DemoGame.Server.Properties.ServerSettingsDescriptions", typeof(ServerSettingsDescriptions).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to When true, if a user logs into an account that is already in use, the existing connection will be kicked so that the user will log into the account. When false, they will be unable to log into the account until the existing connection is dropped..
        /// </summary>
        public static string AccountDropExistingConnectionWhenInUse {
            get {
                return ResourceManager.GetString("AccountDropExistingConnectionWhenInUse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The amount of time, in milliseconds, an item may remain on the map before it is removed automatically..
        /// </summary>
        public static string DefaultMapItemLife {
            get {
                return ResourceManager.GetString("DefaultMapItemLife", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The map to use for a persistent NPC who does not have a valid MapID to use for their loading position. This should point to an isolated region not accessible by players. Ideally, no NPC will ever end up here. A good place is some sort of admin-only room..
        /// </summary>
        public static string InvalidPersistentNPCLoadMap {
            get {
                return ResourceManager.GetString("InvalidPersistentNPCLoadMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The position to use for when using InvalidPersistentNPCLoadMap..
        /// </summary>
        public static string InvalidPersistentNPCLoadPosition {
            get {
                return ResourceManager.GetString("InvalidPersistentNPCLoadPosition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The map to use for a User who does not have a valid MapID or position to use for their load or respawn position. This should be set to some very general place, such as the default user spawn location. Shouldn&apos;t ever be needed, but prevents a user&apos;s character from breaking in case it ever does happen..
        /// </summary>
        public static string InvalidUserLoadMap {
            get {
                return ResourceManager.GetString("InvalidUserLoadMap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The position to use for when using InvalidUserLoadMap..
        /// </summary>
        public static string InvalidUserLoadPosition {
            get {
                return ResourceManager.GetString("InvalidUserLoadPosition", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The minimum amount of time in milliseconds that may elapse between checks for expired items. The lower this value, the closer the time the items are removed will be to the actual sepcified time, but the greater the performance cost. It is recommended to keep this value greater than at least 10 seconds to avoid unneccesary performance overhead..
        /// </summary>
        public static string MapItemExpirationUpdateRate {
            get {
                return ResourceManager.GetString("MapItemExpirationUpdateRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum number of connections that can be made to the server..
        /// </summary>
        public static string MaxConnections {
            get {
                return ResourceManager.GetString("MaxConnections", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum number of connections allowed for a single IP address. Set to 0 to disable this check and not limit the number of connections per IP (not recommended!)..
        /// </summary>
        public static string MaxConnectionsPerIP {
            get {
                return ResourceManager.GetString("MaxConnectionsPerIP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum allowed distance allowed between two group members (on the same map) for them to be allowed to share rewards with the other group members..
        /// </summary>
        public static string MaxGroupShareDistance {
            get {
                return ResourceManager.GetString("MaxGroupShareDistance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum accounts that can be created for a single IP address over a given period of time. The period of time is defined by the query itself (CountRecentlyCreatedAccounts)..
        /// </summary>
        public static string MaxRecentlyCreatedAccounts {
            get {
                return ResourceManager.GetString("MaxRecentlyCreatedAccounts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Message of the Day. Displayed to the user when they log into the game. Can be empty to display nothing.
        /// </summary>
        public static string MOTD {
            get {
                return ResourceManager.GetString("MOTD", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How frequently, in milliseconds, to wait between checks to respawn IRespawnable entities. Lower values will result in IRespawnables respawning closer to their desired time, but will require more overhead. In contrast, a higher value means more things can end up spawning at once since there is a larger time frame to cover..
        /// </summary>
        public static string RespawnablesUpdateRate {
            get {
                return ResourceManager.GetString("RespawnablesUpdateRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How frequently, in milliseconds, the server will auto-save the world state. The lower this value, the less the server will &quot;roll-back&quot; when it crashes. World saves can be expensive since it is done all at once, so it is recommended to keep this value relatively high unless you are experiencing frequent crashes..
        /// </summary>
        public static string RoutineServerSaveRate {
            get {
                return ResourceManager.GetString("RoutineServerSaveRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How frequently, in milliseconds, the server updates. The server update rate does not affect the rate at which physics is update, so modifying the update rate will not affect the game speed. Server update rate is used to determine how frequently the server checks for performing updates and how long it is able to &quot;sleep&quot;. It is recommended a high update rate is used to allow for more precise updating..
        /// </summary>
        public static string ServerUpdateRate {
            get {
                return ResourceManager.GetString("ServerUpdateRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to How frequently, in milliseconds, to call User.SynchronizeExtraUserInformation. Lower values will result in smaller delays for certain things (such as changes to stats and inventory) to update, but requires more overhead..
        /// </summary>
        public static string SyncExtraUserInformationRate {
            get {
                return ResourceManager.GetString("SyncExtraUserInformationRate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The ItemTemplateID that represents the template of the item used for attacking when no weapon is specified (see: World.UnarmedWeapon)..
        /// </summary>
        public static string UnarmedItemTemplateID {
            get {
                return ResourceManager.GetString("UnarmedItemTemplateID", resourceCulture);
            }
        }
    }
}
