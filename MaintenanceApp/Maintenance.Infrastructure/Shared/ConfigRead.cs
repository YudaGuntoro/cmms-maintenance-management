using System.Reflection;

namespace Maintenance.Infrastructure.Shared
{
    public class ConfigRead
    {
        public string PathFile { get; }

        private static readonly Lazy<ConfigRead> lazy = new(() => new ConfigRead());
        public static ConfigRead Instance => lazy.Value;

        private readonly string _defaultSection = Assembly.GetExecutingAssembly().GetName().Name ?? "Maintenance";

        public ConfigRead(string? IniPath = null)
        {
            PathFile = IniPath ?? Path.Combine(AppContext.BaseDirectory, "Settings.ini");
        }

        public string Read(string Key, string? Section = null)
        {
            if (!File.Exists(PathFile))
            {
                return string.Empty;
            }

            var targetSection = Section ?? _defaultSection;
            var currentSection = string.Empty;

            foreach (var rawLine in File.ReadLines(PathFile))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
                {
                    continue;
                }

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1].Trim();
                    continue;
                }

                if (!string.Equals(currentSection, targetSection, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex < 0)
                {
                    continue;
                }

                var name = line[..separatorIndex].Trim();
                if (string.Equals(name, Key, StringComparison.OrdinalIgnoreCase))
                {
                    return line[(separatorIndex + 1)..].Trim();
                }
            }

            return string.Empty;
        }

        public void Write(string Key, string Value, string? Section = null)
        {
            var targetSection = Section ?? _defaultSection;
            var lines = File.Exists(PathFile)
                ? File.ReadAllLines(PathFile).ToList()
                : [];

            var sectionHeader = $"[{targetSection}]";
            var sectionIndex = lines.FindIndex(line => string.Equals(line.Trim(), sectionHeader, StringComparison.OrdinalIgnoreCase));
            if (sectionIndex < 0)
            {
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                {
                    lines.Add(string.Empty);
                }

                lines.Add(sectionHeader);
                lines.Add($"{Key}={Value}");
                File.WriteAllLines(PathFile, lines);
                return;
            }

            var insertIndex = lines.Count;
            for (var index = sectionIndex + 1; index < lines.Count; index++)
            {
                var line = lines[index].Trim();
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    insertIndex = index;
                    break;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex < 0)
                {
                    continue;
                }

                var name = line[..separatorIndex].Trim();
                if (string.Equals(name, Key, StringComparison.OrdinalIgnoreCase))
                {
                    lines[index] = $"{Key}={Value}";
                    File.WriteAllLines(PathFile, lines);
                    return;
                }
            }

            lines.Insert(insertIndex, $"{Key}={Value}");
            File.WriteAllLines(PathFile, lines);
        }

        public void DeleteKey(string Key, string? Section = null)
        {
            Write(Key, string.Empty, Section ?? _defaultSection);
        }

        public void DeleteSection(string? Section = null)
        {
            Write(string.Empty, string.Empty, Section ?? _defaultSection);
        }

        public bool KeyExists(string Key, string? Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
