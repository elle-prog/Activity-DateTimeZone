using System;
using System.Collections.Generic;

// ──────────────────────────────────────────────
// Abstract base class (OOP: Abstraction + Encapsulation)
// ──────────────────────────────────────────────
abstract class TimeRecord
{
    public string EmployeeNumber { get; protected set; }
    public string EmployeeName   { get; protected set; }
    public string OfficeLocation { get; protected set; }
    public DateTime? TimeIn      { get; protected set; }
    public DateTime? TimeOut     { get; protected set; }

    protected TimeRecord(string empNum, string empName, string location)
    {
        EmployeeNumber = empNum;
        EmployeeName   = empName;
        OfficeLocation = location;
    }

    public abstract double ComputeTotalHours();

    public string GetNote()
    {
        double hours = ComputeTotalHours();
        if (hours < 9)
        {
            double remaining = 9 - hours;
            return $"Early Out. Hours left: {remaining} hour(s)";
        }
        else if (hours > 9)
        {
            double extra = hours - 9;
            return $"Overtime. Hours extended: {extra} hour(s)";
        }
        return string.Empty;
    }
}

// ──────────────────────────────────────────────
// Concrete class (OOP: Inheritance)
// ──────────────────────────────────────────────
class EmployeeTimeRecord : TimeRecord
{
    private readonly string _dateFormat;
    private readonly TimeSpan _utcOffset;

    public EmployeeTimeRecord(string empNum, string empName, string location,
                              string dateFormat, TimeSpan utcOffset)
        : base(empNum, empName, location)
    {
        _dateFormat = dateFormat;
        _utcOffset  = utcOffset;
    }

    private DateTime GetLocalNow()
        => DateTime.UtcNow + _utcOffset;

    public void RecordTimeIn()  => TimeIn  = GetLocalNow();
    public void RecordTimeOut() => TimeOut = GetLocalNow();

    public override double ComputeTotalHours()
    {
        if (TimeIn == null || TimeOut == null) return 0;
        return (TimeOut.Value - TimeIn.Value).TotalHours;
    }

    public string FormatDateTime(DateTime dt)
        => $"{dt.ToString(_dateFormat)} {dt:hh:mm:ss tt}";

    public void PrintTimeInLog()
    {
        Console.WriteLine("\nEmployee Log:");
        Console.WriteLine($"Name:{EmployeeName}");
        Console.WriteLine($"Location:{OfficeLocation}");
        Console.WriteLine($"Time-In:{FormatDateTime(TimeIn!.Value)}");
    }

    public void PrintFullLog()
    {
        Console.WriteLine("\n========== EMPLOYEE TIME RECORD ==========");
        Console.WriteLine($"Employee Number : {EmployeeNumber}");
        Console.WriteLine($"Name            : {EmployeeName}");
        Console.WriteLine($"Location        : {OfficeLocation}");
        Console.WriteLine($"Time-In         : {FormatDateTime(TimeIn!.Value)}");
        Console.WriteLine($"Time-Out        : {FormatDateTime(TimeOut!.Value)}");
        Console.WriteLine($"Total Hours     : {ComputeTotalHours():F2} hour(s)");
        string note = GetNote();
        Console.WriteLine($"Note            : {(string.IsNullOrEmpty(note) ? "(none)" : note)}");
        Console.WriteLine("==========================================");
    }
}

// ──────────────────────────────────────────────
// Office Location Config (UTC offsets — no timezone DB needed)
// Philippines = UTC+8, United States Eastern = UTC-4 (DST) / UTC-5, India = UTC+5:30
// ──────────────────────────────────────────────
static class OfficeConfig
{
    public static (string dateFormat, TimeSpan utcOffset) GetConfig(string location)
    {
        return location.ToLower() switch
        {
            "philippines"   => ("M/d/yy", TimeSpan.FromHours(8)),
            "united states" => ("M/d/yy", TimeSpan.FromHours(-4)),   // Eastern Daylight Time
            "india"         => ("M/d/yy", new TimeSpan(5, 30, 0)),   // IST = UTC+5:30
            _ => throw new ArgumentException("Invalid location")
        };
    }
}

// ──────────────────────────────────────────────
// Program Entry Point
// ──────────────────────────────────────────────
class Program
{
    static Dictionary<string, EmployeeTimeRecord> EmployeeTimeinTimeoutRecord = new();

    static void Main()
    {
        Console.WriteLine("===== Employee Time-In / Time-Out System =====\n");

        bool running = true;
        while (running)
        {
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  1 - Time In");
            Console.WriteLine("  2 - Time Out");
            Console.WriteLine("  3 - View All Records");
            Console.WriteLine("  4 - Exit");
            Console.Write("Choose: ");
            string choice = Console.ReadLine()?.Trim() ?? "";

            switch (choice)
            {
                case "1": DoTimeIn();       break;
                case "2": DoTimeOut();      break;
                case "3": ViewAllRecords(); break;
                case "4": running = false;  break;
                default:  Console.WriteLine("Invalid option."); break;
            }
        }
    }

    static void DoTimeIn()
    {
        Console.Write("\nEmployee Number: ");
        string empNum = Console.ReadLine()?.Trim() ?? "";

        if (EmployeeTimeinTimeoutRecord.ContainsKey(empNum))
        {
            Console.WriteLine("This employee has already clocked in.");
            return;
        }

        Console.Write("Employee Name: ");
        string empName = Console.ReadLine()?.Trim() ?? "";

        string location = PromptLocation();

        var (dateFormat, utcOffset) = OfficeConfig.GetConfig(location);
        DateTime localNow = DateTime.UtcNow + utcOffset;

        var record = new EmployeeTimeRecord(empNum, empName, location, dateFormat, utcOffset);
        record.RecordTimeIn();

        Console.WriteLine($"\nYou clocked in at:");
        Console.WriteLine($"Date:{localNow.ToString(dateFormat)}");
        Console.WriteLine($"Time: {localNow:hh:mm:ss tt}");

        record.PrintTimeInLog();

        EmployeeTimeinTimeoutRecord[empNum] = record;
    }

    static void DoTimeOut()
    {
        Console.Write("\nEmployee Number: ");
        string empNum = Console.ReadLine()?.Trim() ?? "";

        if (!EmployeeTimeinTimeoutRecord.TryGetValue(empNum, out EmployeeTimeRecord? record))
        {
            Console.WriteLine("No time-in record found for this employee.");
            return;
        }

        if (record.TimeOut != null)
        {
            Console.WriteLine("This employee has already clocked out.");
            return;
        }

        record.RecordTimeOut();
        record.PrintFullLog();
    }

    static void ViewAllRecords()
    {
        if (EmployeeTimeinTimeoutRecord.Count == 0)
        {
            Console.WriteLine("\nNo records found.");
            return;
        }

        Console.WriteLine("\n====== Employee Timein-TimeOut Record ======");
        foreach (var kvp in EmployeeTimeinTimeoutRecord)
        {
            var r = kvp.Value;
            Console.WriteLine($"\nEmployee #: {r.EmployeeNumber}");
            Console.WriteLine($"  Name      : {r.EmployeeName}");
            Console.WriteLine($"  Location  : {r.OfficeLocation}");
            Console.WriteLine($"  Time-In   : {(r.TimeIn.HasValue  ? r.FormatDateTime(r.TimeIn.Value)  : "N/A")}");
            Console.WriteLine($"  Time-Out  : {(r.TimeOut.HasValue ? r.FormatDateTime(r.TimeOut.Value) : "N/A")}");
            if (r.TimeOut.HasValue)
            {
                Console.WriteLine($"  Hours     : {r.ComputeTotalHours():F2}");
                string note = r.GetNote();
                Console.WriteLine($"  Note      : {(string.IsNullOrEmpty(note) ? "(none)" : note)}");
            }
        }
        Console.WriteLine("\n============================================");
    }

    static string PromptLocation()
    {
        string[] valid = { "Philippines", "United States", "India" };
        while (true)
        {
            Console.Write("Which office are you located? (Philippines / United States / India): ");
            string input = Console.ReadLine()?.Trim() ?? "";
            foreach (var v in valid)
                if (string.Equals(v, input, StringComparison.OrdinalIgnoreCase))
                    return v;
            Console.WriteLine("Invalid location. Please enter Philippines, United States, or India.");
        }
    }
}
