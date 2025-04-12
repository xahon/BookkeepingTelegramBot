using Telegram.Bot.Types;

namespace TelegramBookkeepingApp;

public class FinishSessionAction : ActionBase
{
    public FinishSessionAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        session.Finish();

        return ActionResult.DONE_ACTION;
    }
}