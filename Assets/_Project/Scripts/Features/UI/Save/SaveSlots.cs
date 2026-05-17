using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveSlots
{
    public const string SaveDirectoryName = "saves";
    public const string LegacySaveFileName = "save.json";

    public const string ActiveSaveSlotIdPlayerPrefsKey = "ActiveSaveSlotId";

    public sealed class SaveSlotInfo
    {
        public string SlotId { get; }
        public string FilePath { get; }
        public DateTime LastWriteUtc { get; }

        public DateTime SavedAtUtc { get; }
        public int Day { get; }
        public int SeasonId { get; }
        public int Year { get; }
        public int Gold { get; }

        public SaveSlotInfo(
            string slotId,
            string filePath,
            DateTime lastWriteUtc,
            DateTime savedAtUtc,
            int day,
            int seasonId,
            int year,
            int gold)
        {
            SlotId = slotId;
            FilePath = filePath;
            LastWriteUtc = lastWriteUtc;
            SavedAtUtc = savedAtUtc;
            Day = day;
            SeasonId = seasonId;
            Year = year;
            Gold = gold;
        }

        public string GetDisplayName(string prefix = "Partie")
        {
            var local = SavedAtUtc.ToLocalTime();
            var stamp = local.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);

            var seasonName = GetSeasonNameById(SeasonId);
            if (string.IsNullOrWhiteSpace(seasonName)) seasonName = "?";

            var dayPart = Day > 0 ? $"Jour {Day}" : "Jour ?";
            var yearPart = Year > 0 ? $"Année {Year}" : "Année ?";
            var goldPart = Gold >= 0 ? $"{Gold}g" : "?g";

            return $"{prefix} - {stamp} - {dayPart} - {seasonName} - {yearPart} - {goldPart}";
        }
    }

    public static string GetSaveDirectoryPath()
        => Path.Combine(Application.persistentDataPath, SaveDirectoryName);

    public static void EnsureSaveDirectoryExists()
    {
        var dir = GetSaveDirectoryPath();
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    public static bool AnySaveExists()
    {
        TryMigrateLegacySaveIfNeeded();
        var dir = GetSaveDirectoryPath();
        if (!Directory.Exists(dir)) return false;
        return Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly).Any();
    }

    public static IReadOnlyList<SaveSlotInfo> ListSaves()
    {
        TryMigrateLegacySaveIfNeeded();
        var dir = GetSaveDirectoryPath();
        if (!Directory.Exists(dir)) return Array.Empty<SaveSlotInfo>();

        var list = new List<SaveSlotInfo>();
        foreach (var file in Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
        {
            var slotId = Path.GetFileNameWithoutExtension(file);
            DateTime lastWriteUtc;
            try
            {
                lastWriteUtc = File.GetLastWriteTimeUtc(file);
            }
            catch
            {
                lastWriteUtc = DateTime.MinValue;
            }

            var savedAtUtc = lastWriteUtc;
            var day = -1;
            var seasonId = -1;
            var year = -1;
            var gold = -1;

            if (TryReadSaveData(file, out var data) && data != null)
            {
                if (DateTime.TryParse(data.savedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
                {
                    savedAtUtc = parsed.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : parsed.ToUniversalTime();
                }

                if (data.day > 0) day = data.day;
                if (data.seasonId > 0) seasonId = data.seasonId;
                if (data.year > 0) year = data.year;
                gold = data.gold;
            }

            list.Add(new SaveSlotInfo(slotId, file, lastWriteUtc, savedAtUtc, day, seasonId, year, gold));
        }

        list.Sort((a, b) => b.LastWriteUtc.CompareTo(a.LastWriteUtc));
        return list;
    }

    public static SaveSlotInfo GetMostRecent()
        => ListSaves().FirstOrDefault();

    public static SaveSlotInfo CreateNewSave(string slotId = null)
        => CreateNewSave(initialData: null, slotId: slotId);

    public static SaveSlotInfo CreateNewSave(GameSaveData initialData, string slotId = null)
    {
        EnsureSaveDirectoryExists();

        var id = string.IsNullOrWhiteSpace(slotId)
            ? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture)
            : SanitizeSlotId(slotId);

        var path = Path.Combine(GetSaveDirectoryPath(), id + ".json");
        if (File.Exists(path))
        {
            id = id + "_" + Guid.NewGuid().ToString("N")[..8];
            path = Path.Combine(GetSaveDirectoryPath(), id + ".json");
        }

        var nowUtc = DateTime.UtcNow;
        if (initialData == null)
        {
            File.WriteAllText(path, "{}");
        }
        else
        {
            initialData.slotId = id;
            if (string.IsNullOrWhiteSpace(initialData.savedAtUtc))
            {
                initialData.savedAtUtc = nowUtc.ToString("O", CultureInfo.InvariantCulture);
            }
            File.WriteAllText(path, JsonUtility.ToJson(initialData, prettyPrint: true));
        }
        try { File.SetLastWriteTimeUtc(path, nowUtc); } catch { }

        var day = initialData != null ? initialData.day : -1;
        var seasonId = initialData != null ? initialData.seasonId : -1;
        var year = initialData != null ? initialData.year : -1;
        var gold = initialData != null ? initialData.gold : -1;
        return new SaveSlotInfo(id, path, nowUtc, nowUtc, day, seasonId, year, gold);
    }

    public static void SetActiveSlotId(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId))
        {
            PlayerPrefs.DeleteKey(ActiveSaveSlotIdPlayerPrefsKey);
            return;
        }

        PlayerPrefs.SetString(ActiveSaveSlotIdPlayerPrefsKey, slotId);
        PlayerPrefs.Save();
    }

    public static string GetActiveSlotId()
        => PlayerPrefs.GetString(ActiveSaveSlotIdPlayerPrefsKey, string.Empty);

    public static bool TryGetActive(out SaveSlotInfo save)
    {
        var id = GetActiveSlotId();
        if (string.IsNullOrWhiteSpace(id))
        {
            save = null;
            return false;
        }

        var path = Path.Combine(GetSaveDirectoryPath(), id + ".json");
        if (!File.Exists(path))
        {
            save = null;
            return false;
        }

        var lastWriteUtc = File.GetLastWriteTimeUtc(path);
        save = new SaveSlotInfo(id, path, lastWriteUtc, lastWriteUtc, -1, -1, -1, -1);
        return true;
    }

    public static void TouchLastPlayed(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId)) return;

        var path = Path.Combine(GetSaveDirectoryPath(), slotId + ".json");
        if (!File.Exists(path)) return;

        try
        {
            var nowUtc = DateTime.UtcNow;
            try { File.SetLastWriteTimeUtc(path, nowUtc); } catch { }
        }
        catch
        {
            // Best effort.
        }
    }

    public static SaveSlotInfo GetOrCreateActive()
    {
        if (TryGetActive(out var existing)) return existing;

        var created = CreateNewSave();
        SetActiveSlotId(created.SlotId);
        return created;
    }

    private static bool TryReadSaveData(string filePath, out GameSaveData data)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                data = null;
                return false;
            }

            data = JsonUtility.FromJson<GameSaveData>(json);
            return data != null;
        }
        catch
        {
            data = null;
            return false;
        }
    }

    private static string GetSeasonNameById(int id)
    {
        return id switch
        {
            1 => "Spring",
            2 => "Summer",
            3 => "Autumn",
            4 => "Winter",
            _ => string.Empty
        };
    }

    public static void TryMigrateLegacySaveIfNeeded()
    {
        try
        {
            var legacyPath = Path.Combine(Application.persistentDataPath, LegacySaveFileName);
            if (!File.Exists(legacyPath)) return;

            EnsureSaveDirectoryExists();
            var dir = GetSaveDirectoryPath();

            // If there are already saves, keep legacy file untouched (avoid duplicating/overwriting).
            if (Directory.EnumerateFiles(dir, "*.json", SearchOption.TopDirectoryOnly).Any()) return;

            var id = "legacy_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var newPath = Path.Combine(dir, id + ".json");
            File.Move(legacyPath, newPath);
        }
        catch
        {
            // Best effort.
        }
    }

    private static string SanitizeSlotId(string slotId)
    {
        var trimmed = slotId.Trim();
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            trimmed = trimmed.Replace(c, '_');
        }

        return string.IsNullOrWhiteSpace(trimmed) ? "slot" : trimmed;
    }
}
