using TShockAPI;

namespace TSEconomy.Commands
{
    internal abstract class CommandBase
    {
        public string[] Aliases => TSEconomy.Config.Aliases.GetAliases(this);
        public abstract string[] PermissionNodes { get; set; }
        public string ShortestAlias => Aliases.OrderBy(x => x.Length).FirstOrDefault();
        public string[] GetAliases()
        {
            var aliases = TSEconomy.Config.Aliases.GetAliases(this);
            if(aliases.Count() < 1)
            {
                Disabled = true;
            }
            return aliases;
        }
        public bool Disabled { get; set; } = false;
        public abstract void Execute(CommandArgs args);

        public static implicit operator Command(CommandBase cmd) => new(String.Join(", ", cmd.PermissionNodes),
    cmd.Execute, cmd.Aliases);
    }
}
