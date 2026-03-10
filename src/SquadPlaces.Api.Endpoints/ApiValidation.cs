using System.Text.RegularExpressions;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints;

/// <summary>
/// Static validation and spam detection helpers for API endpoints.
/// </summary>
public static class ApiValidation
{
    public static string Sanitize(string? input)
    {
        if (input is null) return string.Empty;
        // Strip null bytes and control characters (keep \n, \r, \t)
        var sanitized = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);
        return sanitized.Trim();
    }

    /// <summary>
    /// Returns true if the input contains script tags — first line of defense against XSS.
    /// </summary>
    public static bool ContainsScriptTags(string? input) =>
        input is not null && Regex.IsMatch(input, @"<\s*script", RegexOptions.IgnoreCase);

    /// <summary>
    /// Checks all provided fields for script tags. Returns an error message if any are found, null otherwise.
    /// </summary>
    public static string? DetectScriptInjection(params (string FieldName, string? Value)[] fields)
    {
        foreach (var (fieldName, value) in fields)
        {
            if (ContainsScriptTags(value))
                return $"{fieldName} contains prohibited script content.";
        }
        return null;
    }

    public static bool IsValidArtifactType(string type) =>
        type.Trim().Equals("decision", StringComparison.OrdinalIgnoreCase) ||
        type.Trim().Equals("pattern", StringComparison.OrdinalIgnoreCase) ||
        type.Trim().Equals("lesson", StringComparison.OrdinalIgnoreCase) ||
        type.Trim().Equals("insight", StringComparison.OrdinalIgnoreCase);

    public static Dictionary<string, string[]>? ValidateEnlistRequest(EnlistRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }
        var name = request.Name;
        if (string.IsNullOrWhiteSpace(name))
            errors["Name"] = ["Name is required and cannot be empty."];
        else if (Sanitize(name).Length == 0)
            errors["Name"] = ["Name cannot consist entirely of control characters."];
        else if (Sanitize(name).Length > 200)
            errors["Name"] = ["Name must be 200 characters or fewer."];

        if (request.Description is not null && Sanitize(request.Description).Length > 1000)
            errors["Description"] = ["Description must be 1000 characters or fewer."];
        if (request.PublicKey is not null && Sanitize(request.PublicKey).Length > 5000)
            errors["PublicKey"] = ["PublicKey must be 5000 characters or fewer."];
        if (request.AvatarUrl is not null)
        {
            var url = Sanitize(request.AvatarUrl);
            if (url.Length > 2000)
                errors["AvatarUrl"] = ["AvatarUrl must be 2000 characters or fewer."];
            else if (url.Length > 0 && !Uri.TryCreate(url, UriKind.Absolute, out _))
                errors["AvatarUrl"] = ["AvatarUrl must be a valid absolute URI."];
        }
        return errors.Count > 0 ? errors : null;
    }

    public static Dictionary<string, string[]>? ValidatePublishArtifactRequest(PublishArtifactRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }
        var title = request.Title;
        if (string.IsNullOrWhiteSpace(title))
            errors["Title"] = ["Title is required and cannot be empty."];
        else if (Sanitize(title).Length == 0)
            errors["Title"] = ["Title cannot consist entirely of control characters."];
        else if (Sanitize(title).Length > 200)
            errors["Title"] = ["Title must be 200 characters or fewer."];

        var summary = request.Summary;
        if (string.IsNullOrWhiteSpace(summary))
            errors["Summary"] = ["Summary is required and cannot be empty."];
        else if (Sanitize(summary).Length == 0)
            errors["Summary"] = ["Summary cannot consist entirely of control characters."];
        else if (Sanitize(summary).Length > 1000)
            errors["Summary"] = ["Summary must be 1000 characters or fewer."];

        if (string.IsNullOrWhiteSpace(request.ArtifactType))
            errors["ArtifactType"] = ["ArtifactType is required."];
        else if (!IsValidArtifactType(request.ArtifactType))
            errors["ArtifactType"] = ["ArtifactType must be one of: decision, pattern, lesson, insight."];

        if (request.Content is not null && Sanitize(request.Content).Length > 50000)
            errors["Content"] = ["Content must be 50000 characters or fewer."];
        if (request.Tags is not null && Sanitize(request.Tags).Length > 500)
            errors["Tags"] = ["Tags must be 500 characters or fewer."];
        if (request.GifUrl is not null)
        {
            var gifUrl = Sanitize(request.GifUrl);
            if (gifUrl.Length > 2000)
                errors["GifUrl"] = ["GifUrl must be 2000 characters or fewer."];
            else if (gifUrl.Length > 0 && !Uri.TryCreate(gifUrl, UriKind.Absolute, out _))
                errors["GifUrl"] = ["GifUrl must be a valid absolute URI."];
        }

        // Image validation — only relative URLs allowed
        if (request.ImageUrl is not null)
        {
            var imageUrl = Sanitize(request.ImageUrl);
            if (imageUrl.Length > 2000)
                errors["ImageUrl"] = ["ImageUrl must be 2000 characters or fewer."];
            else if (imageUrl.Length > 0 && !IsValidRelativeImageUrl(imageUrl))
                errors["ImageUrl"] = ["ImageUrl must be a relative URL starting with /api/images/{squadId}/{imageId}."];
        }

        if (request.ImageData is not null)
        {
            var imageValidation = ValidateImageData(request.ImageData, request.ImageContentType);
            if (imageValidation is not null)
            {
                foreach (var kvp in imageValidation)
                    errors[kvp.Key] = kvp.Value;
            }
        }
        else if (request.ImageContentType is not null)
        {
            errors["ImageContentType"] = ["ImageContentType should only be provided with ImageData."];
        }

        return errors.Count > 0 ? errors : null;
    }

    public static Dictionary<string, string[]>? ValidatePostCommentRequest(PostCommentRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }
        if (request.SquadId == Guid.Empty)
            errors["SquadId"] = ["SquadId is required."];

        var body = request.Body;
        if (string.IsNullOrWhiteSpace(body))
            errors["Body"] = ["Body is required and cannot be empty."];
        else if (Sanitize(body).Length == 0)
            errors["Body"] = ["Body cannot consist entirely of control characters."];
        else if (Sanitize(body).Length > 5000)
            errors["Body"] = ["Body must be 5000 characters or fewer."];

        if (request.GifUrl is not null)
        {
            var gifUrl = Sanitize(request.GifUrl);
            if (gifUrl.Length > 2000)
                errors["GifUrl"] = ["GifUrl must be 2000 characters or fewer."];
            else if (gifUrl.Length > 0 && !Uri.TryCreate(gifUrl, UriKind.Absolute, out _))
                errors["GifUrl"] = ["GifUrl must be a valid absolute URI."];
        }
        return errors.Count > 0 ? errors : null;
    }

    public static int LevenshteinDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
            prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = char.ToLowerInvariant(a[i - 1]) == char.ToLowerInvariant(b[j - 1]) ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[b.Length];
    }

    public static string? FindNearDuplicateSquad(string name, string? description, IEnumerable<Squad> existingSquads)
    {
        var trimmedName = name.Trim();
        var trimmedDesc = description?.Trim();

        foreach (var squad in existingSquads)
        {
            var existingName = squad.Name.Trim();
            var nameDistance = LevenshteinDistance(trimmedName, existingName);

            if (nameDistance == 0)
                return $"A squad with the name '{existingName}' already exists.";

            if (nameDistance <= 4)
            {
                // Names are very similar — check description too if both provided
                if (trimmedDesc is not null && squad.Description is not null)
                {
                    var descDistance = LevenshteinDistance(trimmedDesc, squad.Description.Trim());
                    if (descDistance <= 4)
                        return $"A squad with a similar name and description already exists: '{existingName}'";
                }

                // Even without description match, a near-duplicate name is rejected
                return $"A squad with a similar name already exists: '{existingName}'";
            }
        }
        return null;
    }

    /// <summary>
    /// Heuristic: reject content with >5 URLs or >50% identical repeated words.
    /// </summary>
    public static string? DetectSpam(params string?[] fields)
    {
        var combined = string.Join(" ", fields.Where(f => !string.IsNullOrWhiteSpace(f)));
        if (string.IsNullOrWhiteSpace(combined)) return null;

        // Check for excessive URLs (>5)
        var urlCount = Regex.Matches(combined, @"https?://\S+", RegexOptions.IgnoreCase).Count;
        if (urlCount > 5)
            return $"Content contains {urlCount} URLs (maximum 5 allowed)";

        // Check for >50% identical repeated words
        var words = combined.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 4)
        {
            var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var word in words)
            {
                wordCounts.TryGetValue(word, out var count);
                wordCounts[word] = count + 1;
            }
            var maxCount = wordCounts.Values.Max();
            if ((double)maxCount / words.Length > 0.5)
                return "Content consists of >50% identical repeated words";
        }

        return null;
    }

    public static Dictionary<string, string[]>? ValidateEditArtifactRequest(EditArtifactRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }

        if (request.SquadId == Guid.Empty)
            errors["SquadId"] = ["SquadId is required."];

        // At least one editable field must be provided
        bool hasEditableField = request.Title is not null
            || request.Summary is not null
            || request.Content is not null
            || request.ArtifactType is not null
            || request.Tags is not null
            || request.GifUrl is not null
            || request.ImageUrl is not null
            || request.ImageData is not null;

        if (!hasEditableField)
            errors[""] = ["At least one editable field must be provided."];

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                errors["Title"] = ["Title cannot be empty."];
            else if (Sanitize(request.Title).Length == 0)
                errors["Title"] = ["Title cannot consist entirely of control characters."];
            else if (Sanitize(request.Title).Length > 200)
                errors["Title"] = ["Title must be 200 characters or fewer."];
        }

        if (request.Summary is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Summary))
                errors["Summary"] = ["Summary cannot be empty."];
            else if (Sanitize(request.Summary).Length == 0)
                errors["Summary"] = ["Summary cannot consist entirely of control characters."];
            else if (Sanitize(request.Summary).Length > 1000)
                errors["Summary"] = ["Summary must be 1000 characters or fewer."];
        }

        if (request.ArtifactType is not null)
        {
            if (string.IsNullOrWhiteSpace(request.ArtifactType))
                errors["ArtifactType"] = ["ArtifactType cannot be empty."];
            else if (!IsValidArtifactType(request.ArtifactType))
                errors["ArtifactType"] = ["ArtifactType must be one of: decision, pattern, lesson, insight."];
        }

        if (request.Content is not null && Sanitize(request.Content).Length > 50000)
            errors["Content"] = ["Content must be 50000 characters or fewer."];
        if (request.Tags is not null && Sanitize(request.Tags).Length > 500)
            errors["Tags"] = ["Tags must be 500 characters or fewer."];

        if (request.GifUrl is not null)
        {
            var gifUrl = Sanitize(request.GifUrl);
            if (gifUrl.Length > 2000)
                errors["GifUrl"] = ["GifUrl must be 2000 characters or fewer."];
            else if (gifUrl.Length > 0 && !Uri.TryCreate(gifUrl, UriKind.Absolute, out _))
                errors["GifUrl"] = ["GifUrl must be a valid absolute URI."];
        }

        if (request.ImageUrl is not null)
        {
            var imageUrl = Sanitize(request.ImageUrl);
            if (imageUrl.Length > 2000)
                errors["ImageUrl"] = ["ImageUrl must be 2000 characters or fewer."];
            else if (imageUrl.Length > 0 && !IsValidRelativeImageUrl(imageUrl))
                errors["ImageUrl"] = ["ImageUrl must be a relative URL starting with /api/images/{squadId}/{imageId}."];
        }

        if (request.ImageData is not null)
        {
            var imageValidation = ValidateImageData(request.ImageData, request.ImageContentType);
            if (imageValidation is not null)
            {
                foreach (var kvp in imageValidation)
                    errors[kvp.Key] = kvp.Value;
            }
        }
        else if (request.ImageContentType is not null)
        {
            errors["ImageContentType"] = ["ImageContentType should only be provided with ImageData."];
        }

        return errors.Count > 0 ? errors : null;
    }

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp"
    };

    public static Dictionary<string, string[]>? ValidateImageData(string imageData, string? contentType)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(contentType))
        {
            errors["ImageContentType"] = ["ImageContentType is required when ImageData is provided."];
        }
        else if (!AllowedImageContentTypes.Contains(contentType))
        {
            errors["ImageContentType"] = ["ImageContentType must be one of: image/png, image/jpeg, image/gif, image/webp."];
        }

        if (string.IsNullOrWhiteSpace(imageData))
        {
            errors["ImageData"] = ["ImageData cannot be empty."];
        }
        else
        {
            try
            {
                var bytes = Convert.FromBase64String(imageData);
                if (bytes.Length > 10 * 1024 * 1024)
                    errors["ImageData"] = ["ImageData must be 10MB or smaller when decoded."];
            }
            catch (FormatException)
            {
                errors["ImageData"] = ["ImageData must be valid base64."];
            }
        }

        return errors.Count > 0 ? errors : null;
    }

    public static Dictionary<string, string[]>? ValidateUploadImageRequest(UploadImageRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }

        if (request.SquadId == Guid.Empty)
            errors["SquadId"] = ["SquadId is required."];

        var imageValidation = ValidateImageData(request.ImageData, request.ContentType);
        if (imageValidation is not null)
        {
            foreach (var kvp in imageValidation)
                errors[kvp.Key] = kvp.Value;
        }

        return errors.Count > 0 ? errors : null;
    }

    private static readonly Regex RelativeImageUrlPattern = new(
        @"^/api/images/[0-9a-fA-F\-]{36}/[0-9a-fA-F\-]{36}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates that a URL is a relative image URL in the format /api/images/{squadId}/{imageId}.
    /// </summary>
    public static bool IsValidRelativeImageUrl(string url) =>
        RelativeImageUrlPattern.IsMatch(url);

    public static Dictionary<string, string[]>? ValidateMemberRegistrationRequest(MemberRegistrationRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request is null)
        {
            errors[""] = ["Request body is required."];
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["Name"] = ["Name is required and cannot be empty."];
        else if (Sanitize(request.Name).Length == 0)
            errors["Name"] = ["Name cannot consist entirely of control characters."];
        else if (Sanitize(request.Name).Length > 200)
            errors["Name"] = ["Name must be 200 characters or fewer."];

        if (request.Role is not null && Sanitize(request.Role).Length > 200)
            errors["Role"] = ["Role must be 200 characters or fewer."];

        if (request.AvatarUrl is not null)
        {
            var url = Sanitize(request.AvatarUrl);
            if (url.Length > 2000)
                errors["AvatarUrl"] = ["AvatarUrl must be 2000 characters or fewer."];
            else if (url.Length > 0 && !Uri.TryCreate(url, UriKind.Absolute, out _))
                errors["AvatarUrl"] = ["AvatarUrl must be a valid absolute URI."];
        }

        return errors.Count > 0 ? errors : null;
    }
}
