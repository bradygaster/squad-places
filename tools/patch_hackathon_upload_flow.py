from pathlib import Path

# Update the page model partial to accept a manifest and return JSON on upload.
p = Path(r'src/SquadPlaces.Web/Pages/Hackathons/Index.Upload.cs')
t = p.read_text(encoding='utf-8')

t = t.replace('    [BindProperty]\n    public List<IFormFile> FolderFiles { get; set; } = [];\n', '    [BindProperty]\n    public List<IFormFile> FolderFiles { get; set; } = [];\n\n    [BindProperty]\n    public string? FolderManifestJson { get; set; }\n')

t = t.replace('    public async Task<IActionResult> OnPostUploadFolderAsync()\n    {\n        if (FolderFiles.Count == 0)\n            ModelState.AddModelError(nameof(FolderFiles), "Select a git repo folder to upload.");\n\n        if (!ModelState.IsValid)\n        {\n            await LoadAsync();\n            return Page();\n        }\n\n        var repo = await AnalyzeUploadedFolderAsync(FolderFiles, FolderUpload);\n        await _storage.SaveHackathonRepositoryAsync(repo);\n\n        StatusMessage = $"Uploaded and analyzed {repo.Name}.";\n        FolderFiles = [];\n        FolderUpload = new RepositoryFolderUploadForm();\n        await LoadAsync();\n        return Page();\n    }\n', '    public async Task<IActionResult> OnPostUploadFolderAsync()\n    {\n        if (FolderFiles.Count == 0)\n            ModelState.AddModelError(nameof(FolderFiles), "Select a git repo folder to upload.");\n\n        if (!ModelState.IsValid)\n        {\n            var errors = ModelState\n                .Where(kvp => kvp.Value?.Errors.Count > 0)\n                .ToDictionary(\n                    kvp => kvp.Key,\n                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());\n            return new JsonResult(new { success = false, message = "Upload validation failed.", errors }) { StatusCode = StatusCodes.Status400BadRequest };\n        }\n\n        var manifestPaths = ParseManifestPaths(FolderManifestJson, FolderFiles.Count);\n        var repo = await AnalyzeUploadedFolderAsync(FolderFiles, manifestPaths, FolderUpload);\n        await _storage.SaveHackathonRepositoryAsync(repo);\n\n        StatusMessage = $"Uploaded and analyzed {repo.Name}.";\n        FolderFiles = [];\n        FolderManifestJson = null;\n        FolderUpload = new RepositoryFolderUploadForm();\n        await LoadAsync();\n        return new JsonResult(new { success = true, message = $"Uploaded and analyzed {repo.Name}.", repo = new { id = repo.Id, name = repo.Name } });\n    }\n')

t = t.replace('    private async Task<HackathonRepository> AnalyzeUploadedFolderAsync(List<IFormFile> files, RepositoryFolderUploadForm form)\n    {\n        var uploaded = files\n            .Select(f => new UploadedFile(NormalizeRelativePath(f.FileName), f))\n            .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))\n            .OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)\n            .ToList();\n', '    private async Task<HackathonRepository> AnalyzeUploadedFolderAsync(List<IFormFile> files, List<string?> manifestPaths, RepositoryFolderUploadForm form)\n    {\n        var uploaded = files\n            .Select((f, i) => new UploadedFile(NormalizeRelativePath(manifestPaths.ElementAtOrDefault(i) ?? f.FileName), f))\n            .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))\n            .OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)\n            .ToList();\n')

after = '''
    private static List<string?> ParseManifestPaths(string? manifestJson, int count)
    {
        if (string.IsNullOrWhiteSpace(manifestJson))
            return Enumerable.Repeat<string?>(null, count).ToList();

        try
        {
            var paths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(manifestJson);
            if (paths is null || paths.Count == 0)
                return Enumerable.Repeat<string?>(null, count).ToList();

            if (paths.Count < count)
                paths.AddRange(Enumerable.Repeat<string?>(null, count - paths.Count).Select(x => x));

            return paths.Take(count).Cast<string?>().ToList();
        }
        catch
        {
            return Enumerable.Repeat<string?>(null, count).ToList();
        }
    }

'''
marker = '    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();\n}'
if marker in t:
    t = t.replace(marker, after + '    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();\n}')
else:
    raise SystemExit('marker not found')

p.write_text(t, encoding='utf-8')

# Update the page view with a manifest hidden field and better JS error handling.
p = Path(r'src/SquadPlaces.Web/Pages/Hackathons/Index.cshtml')
t = p.read_text(encoding='utf-8')
t = t.replace('<input type="file" id="repo-folder-input" name="FolderFiles" multiple webkitdirectory directory hidden />', '<input type="file" id="repo-folder-input" name="FolderFiles" multiple webkitdirectory directory hidden />\n    <input type="hidden" id="repo-folder-manifest" name="FolderManifestJson" />')
t = t.replace("    const summary = document.getElementById('repo-folder-summary');\n    const nameInput = form.querySelector('[name=\"FolderUpload.Name\"]');\n", "    const summary = document.getElementById('repo-folder-summary');\n    const manifestInput = document.getElementById('repo-folder-manifest');\n    const nameInput = form.querySelector('[name=\"FolderUpload.Name\"]');\n")
t = t.replace("        if (selectedFiles.length === 0) {\n            summary.textContent = 'Select or drop a folder first.';\n            return;\n        }\n\n        const formData = new FormData(form);\n        formData.delete('FolderFiles');\n        for (const entry of selectedFiles) {\n            formData.append('FolderFiles', entry.file, entry.path);\n        }\n\n        const response = await fetch(form.action, {\n", "        if (selectedFiles.length === 0) {\n            summary.textContent = 'Select or drop a folder first.';\n            return;\n        }\n\n        if (manifestInput) {\n            manifestInput.value = JSON.stringify(selectedFiles.map(entry => entry.path));\n        }\n\n        const formData = new FormData(form);\n        formData.delete('FolderFiles');\n        for (const entry of selectedFiles) {\n            formData.append('FolderFiles', entry.file, entry.path);\n        }\n\n        const response = await fetch(form.action, {\n")
t = t.replace("        if (response.ok) {\n            window.location.reload();\n        } else {\n            window.location.reload();\n        }\n", "        if (response.ok) {\n            const data = await response.json();\n            summary.textContent = data.message || 'Repository uploaded.';\n            window.location.reload();\n            return;\n        }\n\n        let message = 'Upload failed.';\n        try {\n            const errorData = await response.json();\n            message = errorData.message || errorData.title || message;\n        } catch {\n            try {\n                message = await response.text();\n            } catch {\n                // ignore\n            }\n        }\n        summary.textContent = message;\n")

p.write_text(t, encoding='utf-8')
print('patched upload flow')
