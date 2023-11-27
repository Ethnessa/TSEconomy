using Terraria;
using TerrariaApi.Server;
using TSEconomy.Commands;
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

        public TSEconomy(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            // load our config file
            Configuration.Configuration.Load();

            // init db
            DB.InitializeDB(Config.UseMySQL);

            // register commands
            Commands.Commands.RegisterAll();

            // register hooks
            TShockAPI.Hooks.GeneralHooks.ReloadEvent += (x) => {
                Configuration.Configuration.Load();
                x.Player.SendSuccessMessage("[TSEconomy] Reloaded config.");
            };
        }
    }
}