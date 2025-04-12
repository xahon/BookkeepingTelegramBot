using System.Collections;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TelegramBookkeepingApp;


public class RecordsTable
{
    public class Row
    {
        public List<object> objects = new List<object>();
    }
    public List<Row> rows = new List<Row>();

    public class CellString
    {
        public string str = null;
        public string[] strs = null;

        public static implicit operator CellString(string str)
        {
            return new CellString{str = str, strs = null};
        }

        public static implicit operator CellString(string[] strs)
        {
            return new CellString{str = null, strs = strs};
        }
    }

    public void AddRow(params CellString[] objects)
    {
        rows.Add(new Row{objects = objects.SelectMany(o => o.str != null ? [o.str] : o.strs).Select(o => (object)o).ToList()});
    }
}

public class MemberPairBalance
{
    public string member1;
    public string member2;
    public int balance = 0;

    public MemberPairBalance(string member1, string member2)
    {
        this.member1 = member1;
        this.member2 = member2;
    }

    public bool ContainsMember(string member)
    {
        return member1 == member || member2 == member;
    }

    public void AddBalance(string payer, int amount)
    {
        if (member1 == payer)
        {
            balance -= amount;
        } else if (member2 == payer)
        {
            balance += amount;
        } else
        {
            throw new Exception("No member");
        }

        if (balance < 0)
        {
            balance *= -1;
            (member1, member2) = (member2, member1);
        }
    }
}

public class SpreadsheetsExporter
{
    private readonly Session session;

    public SpreadsheetsExporter(Session session)
    {
        this.session = session;
    }

    public string ExportToSheets()
    {
        string[] scopes = { SheetsService.Scope.Spreadsheets, DriveService.Scope.Drive };

        string applicationName = "Google Sheets API .NET Quickstart";

        // Load credentials
        GoogleCredential credential;
        using (FileStream stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
        }

        // Create Sheets API service
        SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });

        // Create Drive API service
        DriveService driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });


        // Create a new spreadsheet
        Spreadsheet spreadsheet = new Spreadsheet()
        {
            Properties = new SpreadsheetProperties()
            {
                Title = $"Bookkeeping Report {session.idRo}"
            }
        };

        Spreadsheet createdSpreadsheet = sheetsService.Spreadsheets.Create(spreadsheet).Execute();
        Console.WriteLine("Spreadsheet ID: " + createdSpreadsheet.SpreadsheetId);
        Console.WriteLine("Spreadsheet URL: " + createdSpreadsheet.SpreadsheetUrl);

        FillData(createdSpreadsheet.SpreadsheetId, sheetsService);

        // Set sharing permissions to "anyone with the link can edit"
        Permission permission = new Permission
        {
            Type = "anyone",
            Role = "writer"
        };

        PermissionsResource.CreateRequest? permissionRequest = driveService.Permissions.Create(permission, createdSpreadsheet.SpreadsheetId);
        permissionRequest.Fields = "id";
        permissionRequest.Execute();

        Console.WriteLine("Permission updated: Anyone with the link can edit.");
        Console.WriteLine("Shareable Link: " + createdSpreadsheet.SpreadsheetUrl);

        return createdSpreadsheet.SpreadsheetUrl;
    }

    private void FillData(string sheetId, SheetsService sheetsService)
    {
        string range = "Sheet1!B2";

        Dictionary<string, int> memberToIndex = new Dictionary<string, int>();
        for (int i = 0; i < session.membersRo.Count; i++)
        {
            memberToIndex.Add(session.membersRo.ElementAt(i), i);
        }

        List<MemberPairBalance> pairs = new List<MemberPairBalance>();
        for (int i = 0; i < session.membersRo.Count; i++)
        {
            for (int j = i+1; j < session.membersRo.Count; j++)
            {
                pairs.Add(new MemberPairBalance(session.membersRo.ElementAt(i), session.membersRo.ElementAt(j)));
            }
        }

        RecordsTable recordsTable = new RecordsTable();
        recordsTable.AddRow($"Generated at {DateTime.Now:U}");
        recordsTable.AddRow("", session.membersRo.ToArray());
        foreach (PayEvent payEvent in session.payEventsRo)
        {
            int memberIndex = memberToIndex[payEvent.payee];
            int commitMessageSkipColumns = session.membersRo.Count - memberIndex - 1;
            recordsTable.AddRow(payEvent.payer, Enumerable.Repeat("", memberIndex).ToArray(), payEvent.amount.ToString(), Enumerable.Repeat("", commitMessageSkipColumns).ToArray(), payEvent.commitMessage);

            MemberPairBalance balancePair = pairs.First(p => p.ContainsMember(payEvent.payer) && p.ContainsMember(payEvent.payee));
            balancePair.AddBalance(payEvent.payer, payEvent.amount);
        }

        recordsTable.AddRow("");
        recordsTable.AddRow("");

        foreach (string member in session.membersRo)
        {
            foreach (MemberPairBalance memberPairBalance in pairs.Where(p => p.member1 == member && p.balance > 0))
            {
                recordsTable.AddRow(memberPairBalance.member1, memberPairBalance.member2, memberPairBalance.balance.ToString());
            }
        }


        ValueRange valueRange = new ValueRange();
        valueRange.Values = new List<IList<object>>(recordsTable.rows.Select(r => r.objects));

        SpreadsheetsResource.ValuesResource.UpdateRequest? updateReq = sheetsService.Spreadsheets.Values.Update(valueRange, sheetId, range);
        updateReq.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        updateReq.Execute();
    }
}