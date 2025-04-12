using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBookkeepingApp;

public abstract class SessionBase
{
    private Guid id;
    public  Guid idRo => id;

    private ActionBase defaultAction;
    private ActionBase currentAction;
    private Dictionary<string, ActionBase> actionsMap;
    public TelegramBotClient bot { get; }

    public SessionBase(TelegramBotClient bot)
    {
        id = Guid.NewGuid();
        actionsMap = new Dictionary<string, ActionBase>();
        this.bot = bot;
    }

    protected void RegisterDefaultAction(ActionBase action)
    {
        defaultAction = action;
        currentAction = defaultAction;
    }

    protected void RegisterAction(string command, ActionBase action)
    {
        actionsMap.Add(command, action);
    }

    public void OnMessage(Message message, UpdateType updateType)
    {
        Console.WriteLine($"Session[{id}]::OnMessage(): {message.Text ?? "<NONE>"}");

        if (message.Text != null)
        {
            if (message.Text.StartsWith("/"))
            {
                if (actionsMap.TryGetValue(message.Text, out ActionBase action))
                {
                    if (currentAction != action) {
                        currentAction.Exit(message);
                        currentAction = action;
                    }
                }
            }
            ActionBase.ActionResult result = currentAction.Enter(message);
            if (result == ActionBase.ActionResult.DONE_ACTION)
            {
                currentAction.Exit(message);
                currentAction = defaultAction;
            }
        } else
        {
            defaultAction.Enter(message);
        }
    }

    public void OnUpdate(Update update)
    {
        Console.WriteLine($"Session[{id}]::OnUpdate(): {update.Type}");
    }
}