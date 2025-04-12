using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBookkeepingApp;

public class CreateEventAction : ActionBase
{
    private enum STATE
    {
        DEFAULT,
        WAIT_FOR_CHOOSE_MAIN_PAYER,
        WAIT_FOR_CHOOSE_PAYEE,
        WAIT_FOR_PAY_AMOUNT,
        WAIT_FOR_COMMIT_MESSAGE,
    }
    private STATE state = STATE.DEFAULT;
    private string mainPayer = "";
    private string currentPayee = "";
    private int payAmount = 0;

    public CreateEventAction(Session session) : base(session)
    {
    }

    public override ActionResult Enter(Message message)
    {
        base.Enter(message);

        ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();
        foreach (string memberName in session.membersRo)
        {
            rkm.AddNewRow(new KeyboardButton(memberName));
        }

        switch (state)
        {
            case STATE.DEFAULT:
            {
                if (session.membersRo.Count == 0)
                {
                    session.bot.SendMessage(message.Chat.Id, "No members to create event on");
                    return ActionResult.DONE_ACTION;
                }

                session.bot.SendMessage(message.Chat.Id, "New event. Choose main payer:", replyMarkup: rkm);
                state = STATE.WAIT_FOR_CHOOSE_MAIN_PAYER;

                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_FOR_CHOOSE_MAIN_PAYER:
            {
                string memberName = message.Text;
                if (!session.IsValidMember(memberName))
                {
                    session.bot.SendMessage(message.Chat.Id, $"Invalid member '{memberName}'. Choose main payer:", replyMarkup: rkm);
                    return ActionResult.UNDONE_ACTION;
                }

                mainPayer = memberName;

                session.bot.SendMessage(message.Chat.Id, $"'{mainPayer}' pays for:", replyMarkup: rkm);
                state = STATE.WAIT_FOR_CHOOSE_PAYEE;
                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_FOR_CHOOSE_PAYEE:
            {
                string memberName = message.Text;
                if (!session.IsValidMember(memberName))
                {
                    session.bot.SendMessage(message.Chat.Id, $"Invalid member '{memberName}'. '{mainPayer}' pays for:", replyMarkup: rkm);
                    return ActionResult.UNDONE_ACTION;
                }

                currentPayee = memberName;

                session.bot.SendMessage(message.Chat.Id, $"How much '{mainPayer}' pays for '{currentPayee}'?", replyMarkup: new ReplyKeyboardRemove());
                state = STATE.WAIT_FOR_PAY_AMOUNT;
                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_FOR_PAY_AMOUNT:
            {
                string amountStr = message.Text;
                if (!int.TryParse(amountStr, out int amount))
                {
                    session.bot.SendMessage(message.Chat.Id, $"Invalid numerical amount. How much '{mainPayer}' pays for '{currentPayee}'?");
                    return ActionResult.UNDONE_ACTION;
                }

                payAmount = amount;
                session.bot.SendMessage(message.Chat.Id, $"'{mainPayer}' pays for '{currentPayee}' {amount}. Enter commit message:");
                state = STATE.WAIT_FOR_COMMIT_MESSAGE;
                return ActionResult.UNDONE_ACTION;
            }
            case STATE.WAIT_FOR_COMMIT_MESSAGE:
            {
                session.bot.SendMessage(message.Chat.Id, $"Registered event: '{mainPayer}' pays for '{currentPayee}' {payAmount} - {message.Text}");
                session.RegisterPayEvent(mainPayer, currentPayee, payAmount, message.Text);
                return ActionResult.DONE_ACTION;
            }
        }

        return ActionResult.DONE_ACTION;
    }

    public override void Exit(Message message)
    {
        base.Exit(message);

        state = STATE.DEFAULT;
        mainPayer = "";
        currentPayee = "";
        payAmount = 0;
    }
}