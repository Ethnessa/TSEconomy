using TShockAPI;

namespace TSEconomy.Commands
{
    public abstract class CommandBase
    {
        public abstract string[] Aliases { get; set; }
        public abstract string[] PermissionNodes { get; set; }
        public string ShortestAlias => Aliases.OrderBy(x => x.Length).FirstOrDefault();
        public abstract void Execute(CommandArgs args);

        public static implicit operator Command(CommandBase cmd) => new(String.Join(", ", cmd.PermissionNodes),
    cmd.Execute, cmd.Aliases);
    }
}
