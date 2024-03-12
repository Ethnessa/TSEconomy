namespace TSEconomy.Commands
{
    internal class Commands
    {
        public static void RegisterAll()
        {
            foreach (CommandBase cmd in List.Where(x => x.Disabled == false))
            {
                TShockAPI.Commands.ChatCommands.Add(cmd);
            }
        }

        public static void Refresh()
        {
            foreach (CommandBase cmd in List.Where(x => x.Disabled == false))
            {
                TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == cmd.Execute);
                TShockAPI.Commands.ChatCommands.Add(cmd);
            }
        }

        public static List<CommandBase> List { get; set; } = new()
        {
            new BalanceCommand(),
            new SendCommand(),
            new ListCurrenciesCommand(),
            new BankAdminCommand()
        };
    }
}
