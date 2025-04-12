using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBookkeepingApp;

public class AddMemberAction : ActionBase
{
    private enum STATE
    {
        DEFAULT,
        WAIT_MEMBER_NAME,
    }
    private STATE state = STATE.DEFAULT;

    public AddMemberAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        switch (state)
        {
            case STATE.DEFAULT:
            {
                session.bot.SendMessage(message.Chat.Id, "Enter new member name:");
                state = STATE.WAIT_MEMBER_NAME;
                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_MEMBER_NAME:
            {
                session.RegisterMember(message.Text);
                session.bot.SendMessage(message.Chat.Id, $"New member '{message.Text}' is registered");
                state = STATE.DEFAULT;
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