using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBookkeepingApp;


Dictionary<long, Session> sessions = new Dictionary<long, Session>();

CancellationTokenSource cts = new CancellationTokenSource();
TelegramBotClient bot = new TelegramBotClient(args[0], cancellationToken: cts.Token);

bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;
bot.OnError += OnError;


while (Console.ReadKey(true).Key != ConsoleKey.X)
{
    Thread.Yield();
}
cts.Cancel();

Session GetOrCreateSession(long chatId)
{
    if (!sessions.TryGetValue(chatId, out Session session))
    {
        session = new Session(bot);
        session.OnFinished += s => OnSessionFinished(chatId, s);
        sessions.Add(chatId, session);
    }
    return session;
}


async Task OnMessage(Message message, UpdateType updateType)
{
    Session session = GetOrCreateSession(message.Chat.Id);
    session.OnMessage(message, updateType);
}

async Task OnUpdate(Update update)
{
    if (update.Message == null)
        return;

    Session session = GetOrCreateSession(update.Message.Chat.Id);
    session.OnUpdate(update);
}

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.Error.WriteLine(exception.Message);
}

void OnSessionFinished(long chatId, Session session)
{
    SpreadsheetsExporter exporter = new SpreadsheetsExporter(session);
    string url = exporter.ExportToSheets();

    bot.SendMessage(chatId, $"Session is over. Report URL: {url}");
    sessions.Remove(chatId);
}