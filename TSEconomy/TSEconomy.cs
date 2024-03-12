using Terraria;
using TerrariaApi.Server;
using TSEconomy.Configuration.Models;
using TSEconomy.Lang;
using TSEconomy.Logging;
using TShockAPI;

namespace TSEconomy
{
    /// <summary>
    /// TSEconomy's main plugin class, where the API version is specified and loading / hooking initialization is handled
    /// </summary>
    [ApiVersion(2, 1)]
    public class TSEconomy : TerrariaPlugin
    {

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
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += (x) =>
            {
                Configuration.Configuration.Load();
                Commands.Commands.Refresh();
                x.Player.SendSuccessMessage(Localization.TryGetString("[i:855]Reloaded config.", "plugin"));
            };

            // Load currencies

            // The system currency should always be first
            Currency sysCurrency = new("System-Cash", "sys", "*", "System-Cash", false);

            Api.Currencies.Add(sysCurrency);

            Api.Currencies.AddRange(Config.Currencies);

            // Cache acounts
            Api.LoadAccounts();

            // If some accounts are bound to other worlds they shall reset
            if (Config.ResetBalancesOnNewWorld) HandleBalanceReset();

            TransactionLogging.PurgeOldLogs();

        }

        private static void HandleBalanceReset()
        {
            var selectedAccounts = Api.BankAccounts.Where(i => i.WorldID != Main.worldID
                                                               && i.UserID != -1
                                                               && !TSPlayer.FindByNameOrID(Api.GetAccountName(i.UserID)).First().
                                                                  HasPermission(Permissions.ResetIgnoreBindingToWorld));

            if (!selectedAccounts.Any()) return;
           
            
            TShock.Log.ConsoleWarn("[TSEconomy] New world dectected! Found {0} accounts not belonging to this world! Do you want to reset their balance? (Y/N):", selectedAccounts.Count());
            var str = Console.ReadLine();

            if (!str.ToUpper().StartsWith("Y"))
            {
                TShock.Log.ConsoleWarn("[TSEconomy] Canceling reset...");
                return;
            }
            

            for ( int i = 0; i < selectedAccounts.Count(); i++)
            {
                selectedAccounts.TryGetValue(i, out var account);

                account.Reset();
            }

            TShock.Log.ConsoleInfo("[TSEconomy] Sucessfully reseted {0} accounts!", selectedAccounts.Count());

        }


    }
}