using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBookkeepingApp;

public class RemoveMemberAction : ActionBase
{
    private enum STATE
    {
        DEFAULT,
        WAIT_FOR_SELECTION,
    }
    private STATE state;

    public RemoveMemberAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        switch (state)
        {
            case STATE.DEFAULT:
            {
                if (session.membersRo.Count == 0)
                {
                    session.bot.SendMessage(message.Chat.Id, "No members to remove");
                    return ActionResult.DONE_ACTION;
                }

                ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();
                foreach (string memberName in session.membersRo)
                {
                    rkm.AddNewRow(new KeyboardButton(memberName));
                }

                session.bot.SendMessage(message.Chat.Id, "Which member do you want to remove?", replyMarkup: rkm);
                state = STATE.WAIT_FOR_SELECTION;
                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_FOR_SELECTION:
            {
                bool result = session.TryUnregisterMember(message.Text);
                if (result)
                {
                    session.bot.SendMessage(message.Chat.Id, $"Removed member '{message.Text}'", replyMarkup: new ReplyKeyboardRemove());
                } else
                {
                    session.bot.SendMessage(message.Chat.Id, $"Couldn't remove member '{message.Text}'", replyMarkup: new ReplyKeyboardRemove());
                }
                return ActionResult.DONE_ACTION;
            }
        }
        return ActionResult.DONE_ACTION;
    }

    public override void Exit(Message message)
    {
        base.Exit(message);

        state = STATE.DEFAULT;
    }
}