﻿using TSEconomy.Configuration.Models;
using TSEconomy.Database.Models;
using TShockAPI;

namespace TSEconomy.Extensions
{
    /// <summary>
    /// TSEconomy's extention to TSPlayer mostly providing ways to get the bank account
    /// and the player's UserID
    /// </summary>
    public static class TSPlayerExtensions
    {
        // these are mostly to avoid writing lots of text just to get bank accounts &
        // to avoid getting null references with manipulating Server Accounts.

        /// <summary>
        /// Get the player's UserID in TShock's Account database, returns
        /// -1 if the player is the server
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetUserId(this TSPlayer player)
        {
            if (!player.RealPlayer)
                return -1;

            return player.Account.ID;
        }

        /// <summary>
        /// Gets the player's specified bank account, returns the world account
        /// if the player is the server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static BankAccount? GetBankAccount(this TSPlayer player, Currency curr)
        {
            if (!player.RealPlayer)
                return Api.WorldAccount;

            return Api.GetBankAccount(player.Account.ID);
        }


        /// <summary>
        /// Checks if the specified player has a bank account, always
        /// true if the player is the server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="curr"></param>
        /// <returns></returns>
        public static bool HasBankAccount(this TSPlayer player, Currency curr)
        {
            if (!player.RealPlayer)
                return true;

            return Api.HasBankAccount(player.Account.ID);
        }


    }
}
