namespace SquadPlaces.Data.Models;

public class Squad
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PublicKey { get; set; }
    public DateTime EnlistedAt { get; set; } = DateTime.UtcNow;
    public string? AvatarUrl { get; set; }

    public ICollection<KnowledgeArtifact> Artifacts { get; set; } = [];
}
