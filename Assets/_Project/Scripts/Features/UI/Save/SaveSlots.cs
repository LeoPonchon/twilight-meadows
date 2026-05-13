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

        public SaveSlotInfo(string slotId, string filePath, DateTime lastWriteUtc)
        {
            SlotId = slotId;
            FilePath = filePath;
            LastWriteUtc = lastWriteUtc;
        }

        public string GetDisplayName(string prefix = "Partie")
        {
            var local = LastWriteUtc.ToLocalTime();
            var stamp = local.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture);
            return $"{prefix} {SlotId} ({stamp})";
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

            list.Add(new SaveSlotInfo(slotId, file, lastWriteUtc));
        }

        list.Sort((a, b) => b.LastWriteUtc.CompareTo(a.LastWriteUtc));
        return list;
    }

    public static SaveSlotInfo GetMostRecent()
        => ListSaves().FirstOrDefault();

    public static SaveSlotInfo CreateNewSave(string slotId = null)
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
        File.WriteAllText(path, "{}");
        try { File.SetLastWriteTimeUtc(path, nowUtc); } catch { }

        return new SaveSlotInfo(id, path, nowUtc);
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
        save = new SaveSlotInfo(id, path, lastWriteUtc);
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
