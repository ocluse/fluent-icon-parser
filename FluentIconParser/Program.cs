
using Ocluse.LiquidSnow.Extensions;
using System.Xml.Linq;

Console.WriteLine("Starting Parser...");

string? path = Console.ReadLine();

if (string.IsNullOrWhiteSpace(path))
{
    Console.WriteLine("Please enter a path");
    return;
}

var directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

Dictionary<string, IconData> elements = new();

foreach (var directory in directories)
{
    string name = Path.GetFileName(directory)!.Replace(" ", "");

    IconData? icon = GetIcon(directory, name);

    if(icon is null)
    {
        continue;
    }

    if(elements.ContainsKey(name))
    {
        Console.WriteLine($"Jumping {name}");
        continue;
    }
    elements.Add(name, icon);
}

Dictionary<Template, List<EntryData>> regularDataItems = new();
Dictionary<Template, List<EntryData>> filledDataItems = new();

foreach(var element in elements)
{
    var templateValue = element.Value.Template;

    if (element.Value.Regular != null)
    {
        if(!regularDataItems.TryGetValue(templateValue, out var entryData))
        {
            entryData = new List<EntryData>();
            regularDataItems.Add(templateValue, entryData);
        }

        entryData.Add(new EntryData(element.Key, element.Value.Regular));
    }

    if (element.Value.Filled != null)
    {
        if (!filledDataItems.TryGetValue(templateValue, out var entryData))
        {
            entryData = new List<EntryData>();
            filledDataItems.Add(templateValue, entryData);
        }
        entryData.Add(new EntryData(element.Key, element.Value.Filled));
    }
}

foreach(var fileData in filledDataItems)
{
    string fileName = $"FluentIcons{fileData.Key}Filled.cs";

    File.WriteAllLines(fileName, fileData.Value.Select(x => $"public const string {x.Name} = @\"{x.Value.Replace("\"", "\"\"")}\";"));
}

foreach (var fileData in regularDataItems)
{
    string fileName = $"FluentIcons{fileData.Key}Regular.cs";

    File.WriteAllLines(fileName, fileData.Value.Select(x => $"public const string {x.Name} = @\"{x.Value.Replace("\"", "\"\"")}\";"));
}

static IconData? GetIcon(string path, string name)
{

    //string path24 = $"{path}\\SVG\\ic_fluent_{name.ToSnakeCase()}_24_{regular}.svg";
    //path = $"{path}\\SVG\\ic_fluent_{name.ToSnakeCase()}_16_{regular}.svg";

    foreach(var templateValue in Enum.GetValues<Template>())
    {
        
        string regularPath = GetPath(path, templateValue, false);
        string filledPath = GetPath(path, templateValue, true);

        bool hasRegular = File.Exists(regularPath);
        bool hasFilled = File.Exists(filledPath);

        if (!hasRegular && !hasFilled)
        {
            continue;
        }

        string? regular = hasRegular ? GetContent(regularPath) : null;
        string? filled = hasFilled ? GetContent(filledPath) : null;

        return new IconData(filled, regular, templateValue);
    }

    return null;
}

static string GetContent(string path)
{
    var doc = XDocument.Load(path);
    var svg = doc.Root;
    return string.Concat(svg.Nodes()).Replace(@"xmlns=""http://www.w3.org/2000/svg""", "").Replace("fill=\"#212121\"", "").Replace("   ", "").Replace("  ", "");
    //svgContent = svgContent;
    //return svgContent;
}

static string GetPath(string path, Template template, bool isFill)
{
    string templateString = template.ToString().ToLower().Replace("icon", "");
    string fillString = isFill ? "filled" : "regular";
    string name = Path.GetFileName(path)!.Replace("WiFi", "Wifi").Replace("iOS", "Ios").ToLower();

    return $"{path}\\SVG\\ic_fluent_{name.ToSnakeCase()}_{templateString}_{fillString}.svg";
}

public enum Template
{
    Icon24,
    Icon48,
    Icon32,
    Icon28,
    Icon20,
    Icon16,
    Icon12
}

public record IconData(string? Filled, string? Regular, Template Template);

public record EntryData(string Name, string Value);

public record FileData(bool IsFilled, List<EntryData> Entries);