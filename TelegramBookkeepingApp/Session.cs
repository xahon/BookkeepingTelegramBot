using Telegram.Bot;

namespace TelegramBookkeepingApp;

public class PayEvent
{
    public string payer;
    public string payee;
    public int amount;
    public string commitMessage;
}

public class Session : SessionBase
{
    private List<string> members = new List<string>();
    public IReadOnlyList<string> membersRo => members;

    private List<PayEvent> payEvents = new List<PayEvent>();
    public IReadOnlyList<PayEvent> payEventsRo => payEvents;

    public event Action<Session> OnFinished = delegate {};


    public Session(TelegramBotClient bot) : base(bot)
    {
        RegisterDefaultAction(new DefaultAction(this));
        RegisterAction("/add_member", new AddMemberAction(this));
        RegisterAction("/remove_member", new RemoveMemberAction(this));
        RegisterAction("/new_event", new CreateEventAction(this));
        RegisterAction("/list_events", new ListEventsAction(this));
        RegisterAction("/finish_session", new FinishSessionAction(this));
    }

    public void RegisterMember(string memberName)
    {
        members.Add(memberName);
    }

    public bool TryUnregisterMember(string memberName)
    {
        return members.Remove(memberName);
    }

    public bool IsValidMember(string memberName)
    {
        return members.Contains(memberName);
    }

    public void RegisterPayEvent(string payer, string payee, int amount, string commitMessage)
    {
        payEvents.Add(new PayEvent
        {
            payer = payer,
            payee = payee,
            amount = amount,
            commitMessage = commitMessage,
        });
    }

    public void Finish()
    {
        OnFinished.Invoke(this);
    }
}