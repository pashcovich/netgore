﻿using System.Linq;
using NetGore;

namespace DemoGame.Server
{
    /// <summary>
    /// <see cref="SayHandlerCommands"/> for any permission level and no specific feature.
    /// </summary>
    public partial class SayHandlerCommands
    {
        /// <summary>
        /// Sends a message to everyone online.
        /// </summary>
        /// <param name="message">The message to send.</param>
        [SayHandlerCommand("Shout")]
        public void Shout(string message)
        {
            using (var pw = ServerPacket.SendMessage(GameMessage.CommandShout, User.Name, message))
            {
                World.Send(pw, ServerMessageType.GUIChat);
            }
        }

        /// <summary>
        /// Sends a message to a single user.
        /// </summary>
        /// <param name="userName">The name of the user to whisper to.</param>
        /// <param name="message">The message to send to the user.</param>
        [SayHandlerCommand("Tell")]
        [SayHandlerCommand("Whisper")]
        public void Tell(string userName, string message)
        {
            // Check for a message to tell
            if (string.IsNullOrEmpty(userName))
            {
                // Invalid message
                User.Send(GameMessage.CommandTellNoName, ServerMessageType.GUIChat);
                return;
            }

            // Find the user to tell
            if (string.IsNullOrEmpty(message))
            {
                // No or invalid message
                User.Send(GameMessage.CommandTellNoMessage, ServerMessageType.GUIChat);
                return;
            }

            var target = World.FindUser(userName);

            // Check if the target user is available or not
            if (target != null)
            {
                // Message to sender ("You tell...")
                User.Send(GameMessage.CommandTellSender, ServerMessageType.GUIChat, target.Name, message);

                // Message to receivd ("X tells you...")
                target.Send(GameMessage.CommandTellReceiver, ServerMessageType.GUIChat, User.Name, message);
            }
            else
            {
                // User not found
                User.Send(GameMessage.CommandTellInvalidUser, ServerMessageType.GUIChat, userName);
            }
        }

        /// <summary>
        /// Starts a trade with another user.
        /// </summary>
        /// <param name="userName">The name of the user to trade with.</param>
        [SayHandlerCommand("Trade")]
        public void Trade(string userName)
        {
            var target = World.FindUser(userName);
            if (target == null)
            {
                User.Send(GameMessage.PeerTradingInvalidTarget, ServerMessageType.GUI);
                return;
            }

            User.TryStartPeerTrade(target);
        }
    }
}