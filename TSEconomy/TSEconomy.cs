using Terraria;
using TerrariaApi.Server;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy
{
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
        internal static Database.Database s_DB { get; set; } = new();

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
            s_DB.InitializeDB(Config.UseMySQL);

        }

        public void OnInitialize(EventArgs args)
        {
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

        }
    }
}