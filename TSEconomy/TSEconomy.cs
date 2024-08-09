using MonoMod.Utils;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using TerrariaApi.Server;
using TSEconomy.Api;
using TSEconomy.Configuration.Models;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TSEconomy.Packets;
using TShockAPI;
using TShockAPI.Hooks;

namespace TSEconomy
{
    /// <summary>
    /// TSEconomy's main plugin class, where the API version is specified and loading / hooking initialization is handled
    /// </summary>
    [ApiVersion(2, 1)]
    public class TSEconomy : TerrariaPlugin
    {

        public const string ASKING_TO_TRADE_WITH = "tseconomy_asking_to_trade_with";
        public const string CURRENTLY_TRADING_WITH = "tseconomy_currently_tading_with";
        public const string BEING_ASKED_TO_TRADE_BY = "tseconomy_being_asked_to_trade_by";
        public const string HAS_COMFIRMED_TRADE = "tseconomy_comfirmed_trade";


        public override string Author => "TCN";
        public override string Description => "A plugin that adds an economy to your server.";
        public override string Name => "TSEconomy";
        public override Version Version => new(1, 0);

        /// <summary>
        /// A static reference to our config.
        /// </summary>
        public static Configuration.Configuration Config => Configuration.Configuration.Instance;

        /// <summary>
        /// A static reference to our database, really only intended to be used by TSEconomy internally.
        /// </summary>
        internal static Database.Database DB { get; set; } = new();

        /// <summary>
        /// Directory at which our config, and lang files are stored in.
        /// </summary>
        public static readonly string PluginDirectory = Path.Combine(TShock.SavePath, "TSEconomy");

        public TSEconomy(Main game) : base(game)
        {
            Order = 1;
        }

        public override void Initialize()
        {
            // register hooks
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, PacketHandler.OnGreet);
            ServerApi.Hooks.ServerLeave.Register(this, PacketHandler.OnLeave);

            // load our config file
            Configuration.Configuration.Load();

            // init db
            DB.InitializeDB(Config.UseMySQL);

        }
        /// <summary>
        /// GameInitialize hook method
        /// </summary>
        /// <param name="args"></param>
        public static void OnInitialize(EventArgs args)
        {
            // initialize localization files
            Localization.SetupLanguage();

            // register commands
            Commands.Commands.RegisterAll();

            // register hooks
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += OnReload;

            // Load currencies

            // The system currency should always be first
            Currency sysCurrency = new("System-Cash", "sys", "*", "System-Cash", false);

            CurrencyApi.AddCurrency(sysCurrency);

            CurrencyApi.Currencies.AddRange(Config.Currencies.ToDictionary(i => i.InternalName));

            // Cache acounts
            AccountApi.LoadAccounts();
            TradeInventoryApi.LoadTradeInventories();

            // If some accounts are bound to other worlds they shall reset
            if (Config.ResetBalancesOnNewWorld) HandleBalanceReset();

            TransactionLogging.PurgeOldLogs();

        }

        private static void OnReload(ReloadEventArgs args)
        {

            Configuration.Configuration.Load();
            Commands.Commands.Refresh();
            args.Player.SendSuccessMessage(Localization.TryGetString("[i:855]Reloaded config.", "plugin"));
        }

        private static void HandleBalanceReset()
        {
            var selectedAccounts = AccountApi.BankAccounts.Values.Where(i => i.WorldID != Main.worldID
                                                               && i.UserID != -1
                                                               && !i.HasPermission(Permissions.ResetIgnoreBindingToWorld));

            if (!selectedAccounts.Any()) return;
           
            
            TShock.Log.ConsoleWarn("[TSEconomy] New world dectected! Found {0} accounts not belonging to this world! Do you want to reset their balance? (Y/N):", selectedAccounts.Count());
            var str = Console.ReadLine();

            if (!str.ToUpper().StartsWith("N"))
            {
                TShock.Log.ConsoleWarn("[TSEconomy] Canceling reset...");
                
                TileEntityID.
                return;
            }
            

            for ( int i = 0; i < selectedAccounts.Count(); i++)
            {
                selectedAccounts.TryGetValue(i, out var account);

                account.Reset();
            }

            TShock.Log.ConsoleInfo("[TSEconomy] Sucessfully reseted {0} accounts!", selectedAccounts.Count());

        }

        protected override void Dispose(bool disposing)
        {
            TransactionLogging.ForceWrite();

            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, PacketHandler.OnGreet);
                TShockAPI.Hooks.GeneralHooks.ReloadEvent -= OnReload;
            }

            base.Dispose(disposing);
        }

    }
}