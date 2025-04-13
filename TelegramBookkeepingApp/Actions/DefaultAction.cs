using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBookkeepingApp;

public class DefaultAction : ActionBase
{
    public DefaultAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        if (session.membersRo.Count == 0)
        {
            session.bot.SendMessage(message.Chat.Id, "No members in current session. Define some members using /add_member command", replyMarkup: new ReplyKeyboardRemove());
        } else
        {
            session.bot.SendMessage(message.Chat.Id, $"Members: {string.Join(", ", session.membersRo)}", replyMarkup: new ReplyKeyboardRemove());
        }

        return ActionResult.UNDONE_ACTION;
    }
}