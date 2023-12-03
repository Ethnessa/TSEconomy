using Terraria;
using TerrariaApi.Server;
using TSEconomy.Commands;
using TSEconomy.Configuration.Models;
using TSEconomy.Lang;
using TShockAPI;

namespace TSEconomy
{
    [ApiVersion(2,1)]
    public class TSEconomy : TerrariaPlugin
    {
        public override string Author => "TCN";
        public override string Description => "A plugin that adds an economy to your server.";
        public override string Name => "TSEconomy";
        public override Version Version => new Version(1, 0);

        public static Configuration.Configuration Config => Configuration.Configuration.Instance;
        public static Database.Database DB { get; set; } = new();

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

        public void OnInitialize(EventArgs args)
        {
            Localization.SetupLanguage();

            // register commands
            Commands.Commands.RegisterAll();

            // register hooks
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += (x) => {
                Configuration.Configuration.Load();
                x.Player.SendSuccessMessage(Localization.TryGetString("[i:855]Reloaded config.", "plugin"));
            };

        }
    }
}