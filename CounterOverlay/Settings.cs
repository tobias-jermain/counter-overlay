using System.IO;
using System.Text.Json;

namespace CounterOverlay;

public class AppSettings
{
    public uint IncrementModifiers { get; set; } = 0; // MOD_NONE
    public uint IncrementKey { get; set; } = 0x76; // F7
    public uint ResetModifiers { get; set; } = 0;
    public uint ResetKey { get; set; } = 0x77; // F8

    public double Left { get; set; } = 40;
    public double Top { get; set; } = 40;
    public double FontSize { get; set; } = 48;
    public string TextColor { get; set; } = "#FFFFFF";
    public string Label { get; set; } = "Count";
    public bool ClickThrough { get; set; } = true;

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CounterOverlay", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch
        {
            // fall through to defaults
        }
        return new AppSettings();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}
