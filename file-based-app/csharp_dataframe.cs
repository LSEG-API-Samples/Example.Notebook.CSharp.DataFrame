// #:sdk Microsoft.NET.Sdk@10.0.300

#:property TargetFramework=net10.0
#:property LogLevel=$([MSBuild]::ValueOrDefault('$(LOG_LEVEL)', 'Information'))
#:property OutputPath=./bin


#:package Microsoft.Data.Analysis@0.22.1
#:package Plotly.NET.Interactive@5.0.0
#:package Microsoft.DotNet.Interactive.Formatting@1.0.0-beta.25070.1

using Plotly.NET;
using static Plotly.NET.StyleParam;
using Plotly.NET.TraceObjects;
using Microsoft.Data.Analysis;
using System.Linq;
using Microsoft.DotNet.Interactive.Formatting;
using System.Text;

Formatter.Register<DataFrameRow>(rows =>
    {
        StringBuilder sb = new StringBuilder("<table><tr>");
        foreach (var r in rows)
        {
            sb.Append($"<td>{r}</td>");
        }

        sb.Append("</tr></table>");
        return sb.ToString();
    },
    mimeType: "text/html");


Formatter.Register<DataFrameColumn>(cols =>
    {
        StringBuilder sb = new StringBuilder("<table><tr>");
        sb.Append($"<th><b>{cols.Name}</b></th></tr>");

        foreach (var c in cols)
        {
            sb.Append($"<tr><td>{c}</td></tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    },
    mimeType: "text/html");

var start = new DateTime(2009, 1, 1);
Random rand = new Random();
var numDataPoint = 200;

PrimitiveDataFrameColumn<DateTime> date =
    new PrimitiveDataFrameColumn<DateTime>("Date",
        Enumerable.Range(0, numDataPoint)
            .Select(offset => start.AddDays(offset))
            .ToList());

PrimitiveDataFrameColumn<int> data =
    new PrimitiveDataFrameColumn<int>("Data",
        Enumerable.Range(0, numDataPoint)
            .Select(r => rand.Next(100))
            .ToList());

var df0 = new DataFrame(date, data);
var df1 = new DataFrame(date);

Console.WriteLine();
Console.WriteLine("InMemory DataFrame Loaded:");
Console.WriteLine(df0);

Console.WriteLine();
Console.WriteLine("Access data by indices:");
Console.WriteLine(df0[0, 1]);

Console.WriteLine();
Console.WriteLine("Increase the data in the first row and the second column by 10:");
Console.WriteLine("Initial value: " + df0[0, 1]);//df0.Head(1));
df0[0, 1] = int.Parse(df0[0, 1].ToString()) + 10;
Console.WriteLine("Updated value: " + df0[0, 1]);//df0.Head(1));

Console.WriteLine();
Console.WriteLine("Access row data:");
Console.WriteLine("10th row: " + df0.Rows[9]);
Console.WriteLine("10th row, 2nd column: " + df0.Rows[9][1]);
Console.WriteLine("Assign 50,000,000 to the second column: ");
df0.Rows[9][1] = 50000000;
Console.WriteLine("10th row, 2nd column: " + df0.Rows[9][1]);

Console.WriteLine();
Console.WriteLine("Access column data:");
Console.WriteLine("2nd column: ");
Console.WriteLine(df0.Columns["Data"]);
Console.WriteLine("Increase all data in the column by ten: ");
df0.Columns["Data"]= df0.Columns["Data"]+10;
Console.WriteLine("2nd column: ");
Console.WriteLine(df0.Columns[1]);

Console.WriteLine();
Console.WriteLine("Add a new column:");
df0.Columns.Add(new PrimitiveDataFrameColumn<int>("Data1", df0.Rows.Count()));
df0.Columns["Data1"].FillNulls(10, true);
Console.WriteLine(df0);

Console.WriteLine();
Console.WriteLine("Append a new row:");
df0.Append(new List<KeyValuePair<string, object>>() { 
    new KeyValuePair<string, object>("Date", DateTime.Now),
    new KeyValuePair<string, object>("Data", 12),
    new KeyValuePair<string, object>("Data1", 50)
}, true);
Console.WriteLine(df0.Tail(10));

Console.WriteLine();
Console.WriteLine("Manipulate the DataFrame...");
Console.WriteLine("Sorting: Order By 'Data'");
Console.WriteLine(df0.OrderBy("Data"));

Console.WriteLine("Grouping: Group By 'Data'");
var groupByData = df0.GroupBy("Data");
Console.WriteLine(groupByData.Count().OrderBy("Data"));

Console.WriteLine("Filtering: Filter By 'Data' greater than fifty");
Console.WriteLine(df0.Filter(df0.Columns["Data"].ElementwiseGreaterThan(50)).OrderBy("Data"));

Console.WriteLine("Merging: Merge 'Date' from two dataframes");
//var dateformat = "dd/MM/yyyy hh:mm:ss tt";
//df0.Columns["Date"] = 
    //new PrimitiveDataFrameColumn<DateTime>("Date", 
        //df0.Columns["Date"]
            //.Cast<object>()
            //.ToList()
            //.Select(x => DateTime.ParseExact(x.ToString(), dateformat, System.Globalization.CultureInfo.InvariantCulture))
            //.Cast<DateTime>()); 
                
Console.WriteLine(df1.Merge<DateTime>(df0, "Date", "Date"));

Console.WriteLine();
Console.WriteLine("Loading CSV Data...");
var df2 = DataFrame.LoadCsv("./datasets/ohlcdata.csv");

Console.WriteLine("CSV DataFrame Information:");
Console.WriteLine(df2.Info());

Console.WriteLine();
Console.WriteLine("Generating Charts...");

string chartsDir = "./charts";
if (!Directory.Exists(chartsDir))
{
    Directory.CreateDirectory(chartsDir);
}

// Line Chart
Title title0 = Title.init(X: 0.5, Text: "Open Price Line Chart");
var dateData0 = df2.Columns["Date"].Cast<DateTime>().ToArray();
var openData0 = df2.Columns["Open"].Cast<Single>().ToArray();

var chart1 = Chart2D.Chart.Line<DateTime, Single, bool>(dateData0, openData0, true)
                .WithSize(1000, 500)
                .WithMarker(Marker.init(Size: 8))
                .WithXAxisStyle(title: Title.init(Text: "Date"))
                .WithYAxisStyle(title: Title.init(Text: "Price (USD)"))
                .WithLayout(Layout.init<bool>(Title: title0));

chart1.SaveHtml(chartsDir + "/line-chart.html");
Console.WriteLine("Charts: Line Chart Created");

// Line Chart with Mulitple Lines
Title title1 = Title.init(X:0.5, Text: "Open and Close Price Line Chart");
var dateData1 = df2.Columns["Date"].Cast<DateTime>().ToArray();
var openData1 = df2.Columns["Open"].Cast<Single>().ToArray();
var closeData1 = df2.Columns["Close"].Cast<Single>();

var multiChart = Chart.Combine(new [] {
    Chart2D.Chart.Line<DateTime, Single, bool>(dateData1, openData1,true,Name: "Open")
        .WithMarkerStyle(Symbol: StyleParam.MarkerSymbol.Square),
    Chart2D.Chart.Line<DateTime, Single, bool>(dateData1, closeData1, true, Name: "Close")
}).WithSize(1000, 500)
    .WithMarker(Marker.init(Size: 8))
    .WithXAxisStyle(title: Title.init(Text:"Date"))
    .WithYAxisStyle(title: Title.init(Text:"Price (USD)"))
    .WithLayout(Layout.init<bool>(Title: title1));

multiChart.SaveHtml(chartsDir + "/line-chart-with-multiple-lines.html");
Console.WriteLine("Charts: Line Chart with Mulitple Lines Created");

// Bar Chart
Title title2 = Title.init(X:0.5, Text: "Volume");
var dateData2 = df2.Columns["Date"].Cast<DateTime>().ToArray();
var volumeData2 = df2.Columns["Volume"].Cast<Single>().ToArray();

var columnChart = Chart2D.Chart.Column<Single, DateTime, bool, bool, bool>(volumeData2, dateData2)
    .WithSize(1000, 500)
    .WithXAxisStyle(title: Title.init(Text:"Date"))
    .WithYAxisStyle(title: Title.init(Text:"Unit"))
    .WithLayout(Layout.init<bool>(Title: title2));

columnChart.SaveHtml(chartsDir + "/column-chart.html");
Console.WriteLine("Charts: Column Chart Created");

// Candlestick Chart
Title title3 = Title.init(X:0.5, Text: "Open, High, Low, Close - OHLC Candlestick Chart");
var dateData3 = df2.Columns["Date"].Cast<DateTime>().ToArray();
var openData3 = df2.Columns["Open"].Cast<Single>().ToArray();
var highData3 = df2.Columns["High"].Cast<Single>().ToArray();
var lowData3 = df2.Columns["Low"].Cast<Single>().ToArray();
var closeData3 = df2.Columns["Close"].Cast<Single>().ToArray();

var candleChart = Chart2D.Chart.Candlestick<Single, Single, Single, Single, DateTime, bool>(openData3, highData3, lowData3, closeData3, X:dateData3)
     .WithSize(1000, 500)
     .WithXAxisStyle(title: Title.init(Text:"Date"))
     .WithYAxisStyle(title: Title.init(Text:"Price (USD)"))
     .WithLayout(Layout.init<bool>(Title: title3));

candleChart.SaveHtml(chartsDir + "/candlestick-chart.html");
Console.WriteLine("Charts: Candlestick Chart Created");

Console.WriteLine();
Console.WriteLine("Program Termination: Successful");
Console.WriteLine();

// dotnet build .\csharp_dataframe.cs
// dotnet run .\csharp_dataframe.cs