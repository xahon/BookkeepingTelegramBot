using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBookkeepingApp;

public class ListEventsAction : ActionBase
{
    public ListEventsAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        session.bot.SendMessage(message.Chat.Id, $"Pay events:\n{string.Join("\n", session.payEventsRo.Select(p => $"{p.payer} -> {p.payee} = {p.amount} ({p.commitMessage})"))}");

        return ActionResult.DONE_ACTION;
    }
}