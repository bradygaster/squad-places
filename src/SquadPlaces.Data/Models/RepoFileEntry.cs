namespace SquadPlaces.Data.Models;

/// <summary>
/// A single entry in a repository's file tree — either a file or a folder placeholder.
/// </summary>
/// <param name="Path">The forward-slash delimited path relative to the repo root (e.g. "src/index.ts").</param>
/// <param name="Name">The file or folder name without parent segments (e.g. "index.ts").</param>
/// <param name="IsFolder">True when this entry is a folder marker rather than a file.</param>
/// <param name="SizeBytes">Content length in bytes; null for folder entries.</param>
/// <param name="UpdatedAt">UTC timestamp of the last write.</param>
public record RepoFileEntry(string Path, string Name, bool IsFolder, long? SizeBytes, DateTime UpdatedAt);

/// <summary>
/// The content of a single repository file.
/// </summary>
/// <param name="Path">The forward-slash delimited path relative to the repo root.</param>
/// <param name="Content">The raw text content of the file.</param>
/// <param name="UpdatedAt">UTC timestamp of the last write.</param>
public record RepoFileContent(string Path, string Content, DateTime UpdatedAt);
