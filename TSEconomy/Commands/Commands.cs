namespace TSEconomy.Commands
{
    public class Commands
    {
        public static void RegisterAll()
        {
            foreach (CommandBase cmd in List)
            {
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
