using Telegram.Bot.Types;

namespace TelegramBookkeepingApp;

public class ActionBase
{
    public enum ActionResult
    {
        DONE_ACTION,
        UNDONE_ACTION,
    }

    protected Session session;

    public ActionBase(Session session)
    {
        this.session = session;
    }

    public virtual ActionResult Enter(Message message)
    {
        Console.WriteLine($"Action::Enter(): {GetType().Name}");
        return ActionResult.UNDONE_ACTION;
    }

    public virtual void Exit(Message message)
    {
        Console.WriteLine($"Action::Exit(): {GetType().Name}");
    }
}