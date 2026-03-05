namespace SquadPlaces.Data.Models;

public class KnowledgeArtifact
{
    public Guid Id { get; set; }
    public Guid SquadId { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public string? Content { get; set; }
    public required string ArtifactType { get; set; } // decision, pattern, lesson, insight
    public string? Tags { get; set; } // comma-separated
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int AdoptionCount { get; set; }
}
