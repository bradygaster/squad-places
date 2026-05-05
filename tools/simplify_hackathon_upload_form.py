from pathlib import Path

p = Path(r'src/SquadPlaces.Web/Pages/Hackathons/Index.cshtml')
t = p.read_text(encoding='utf-8')

# Remove the custom JS-driven fetch upload section and make it a normal Razor Pages form.
old = '''<form method="post" asp-page-handler="UploadFolder" enctype="multipart/form-data" class="Box p-4 mb-4" id="repo-folder-form">
    @Html.AntiForgeryToken()
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 2; min-width: 300px;">
            <label asp-for="FolderUpload.Name" class="form-label">Repository name</label>
            <input asp-for="FolderUpload.Name" class="form-control" placeholder="Optional — inferred from folder name if blank" />
        </div>
        <div class="form-group" style="flex: 2; min-width: 300px;">
            <label asp-for="FolderUpload.RepositoryUrl" class="form-label">Repository URL</label>
            <input asp-for="FolderUpload.RepositoryUrl" class="form-control" placeholder="Optional — use the Git remote if known" />
        </div>
    </div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 1; min-width: 220px;">
            <label asp-for="FolderUpload.Tags" class="form-label">Tags</label>
            <input asp-for="FolderUpload.Tags" class="form-control" placeholder="dotnet,azure,playwright" />
        </div>
        <div class="form-group" style="flex: 1; min-width: 180px;">
            <label asp-for="FolderUpload.DefaultBranch" class="form-label">Default branch</label>
            <input asp-for="FolderUpload.DefaultBranch" class="form-control" placeholder="main" />
        </div>
        <div class="form-group" style="flex: 1; min-width: 180px;">
            <label asp-for="FolderUpload.CommitSha" class="form-label">Pinned SHA</label>
            <input asp-for="FolderUpload.CommitSha" class="form-control" placeholder="optional" />
        </div>
        <div class="form-group" style="min-width: 160px;">
            <label asp-for="FolderUpload.HasSquadState" class="form-label">Has squad</label>
            <div><input asp-for="FolderUpload.HasSquadState" type="checkbox" /> <span class="color-fg-muted">Yes</span></div>
        </div>
    </div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 1; min-width: 260px;">
            <label asp-for="FolderUpload.Description" class="form-label">Description</label>
            <textarea asp-for="FolderUpload.Description" class="form-control" rows="2" placeholder="What this repo is for"></textarea>
        </div>
        <div class="form-group" style="flex: 1; min-width: 260px;">
            <label asp-for="FolderUpload.AnalysisNotes" class="form-label">Analysis notes</label>
            <textarea asp-for="FolderUpload.AnalysisNotes" class="form-control" rows="2" placeholder="Optional note that will be folded into the analysis"></textarea>
        </div>
    </div>

    <div class="Box mt-3 p-4 text-center" id="repo-dropzone" style="border: 2px dashed #30363d; background: rgba(13,17,23,0.5); cursor: pointer;">
        <strong>Drag a Git repo folder here</strong>
        <div class="color-fg-muted mt-1">or click to choose a folder</div>
        <div class="color-fg-muted text-small mt-2" id="repo-folder-summary">No folder selected yet.</div>
    </div>

    <input type="file" id="repo-folder-input" name="FolderFiles" multiple webkitdirectory directory hidden />

    <div class="d-flex flex-wrap mt-3" style="gap: 0.5rem;">
        <button type="submit" class="btn btn-primary">Analyze and register</button>
        <button type="button" class="btn btn-sm" id="repo-folder-clear">Clear folder</button>
    </div>
</form>
'''
new = '''<form method="post" asp-page-handler="UploadFolder" enctype="multipart/form-data" class="Box p-4 mb-4">
    @Html.AntiForgeryToken()
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 2; min-width: 300px;">
            <label asp-for="FolderUpload.Name" class="form-label">Repository name</label>
            <input asp-for="FolderUpload.Name" class="form-control" placeholder="Optional — inferred from folder name if blank" />
        </div>
        <div class="form-group" style="flex: 2; min-width: 300px;">
            <label asp-for="FolderUpload.RepositoryUrl" class="form-label">Repository URL</label>
            <input asp-for="FolderUpload.RepositoryUrl" class="form-control" placeholder="Optional — use the Git remote if known" />
        </div>
    </div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 1; min-width: 220px;">
            <label asp-for="FolderUpload.Tags" class="form-label">Tags</label>
            <input asp-for="FolderUpload.Tags" class="form-control" placeholder="dotnet,azure,playwright" />
        </div>
        <div class="form-group" style="flex: 1; min-width: 180px;">
            <label asp-for="FolderUpload.DefaultBranch" class="form-label">Default branch</label>
            <input asp-for="FolderUpload.DefaultBranch" class="form-control" placeholder="main" />
        </div>
        <div class="form-group" style="flex: 1; min-width: 180px;">
            <label asp-for="FolderUpload.CommitSha" class="form-label">Pinned SHA</label>
            <input asp-for="FolderUpload.CommitSha" class="form-control" placeholder="optional" />
        </div>
        <div class="form-group" style="min-width: 160px;">
            <label asp-for="FolderUpload.HasSquadState" class="form-label">Has squad</label>
            <div><input asp-for="FolderUpload.HasSquadState" type="checkbox" /> <span class="color-fg-muted">Yes</span></div>
        </div>
    </div>

    <div class="d-flex flex-wrap" style="gap: 1rem;">
        <div class="form-group" style="flex: 1; min-width: 260px;">
            <label asp-for="FolderUpload.Description" class="form-label">Description</label>
            <textarea asp-for="FolderUpload.Description" class="form-control" rows="2" placeholder="What this repo is for"></textarea>
        </div>
        <div class="form-group" style="flex: 1; min-width: 260px;">
            <label asp-for="FolderUpload.AnalysisNotes" class="form-label">Analysis notes</label>
            <textarea asp-for="FolderUpload.AnalysisNotes" class="form-control" rows="2" placeholder="Optional note that will be folded into the analysis"></textarea>
        </div>
    </div>

    <div class="Box mt-3 p-4 text-center" id="repo-dropzone" style="border: 2px dashed #30363d; background: rgba(13,17,23,0.5); cursor: pointer;">
        <strong>Drag a Git repo folder here</strong>
        <div class="color-fg-muted mt-1">or click to choose a folder</div>
        <div class="color-fg-muted text-small mt-2" id="repo-folder-summary">No folder selected yet.</div>
    </div>

    <input type="file" id="repo-folder-input" name="FolderFiles" multiple webkitdirectory directory hidden />
    <input type="hidden" id="repo-folder-manifest" name="FolderManifestJson" />

    <div class="d-flex flex-wrap mt-3" style="gap: 0.5rem;">
        <button type="submit" class="btn btn-primary">Analyze and register</button>
        <button type="button" class="btn btn-sm" id="repo-folder-clear">Clear folder</button>
    </div>
</form>
'''
if old not in t:
    raise SystemExit('old upload form block not found')
t = t.replace(old, new)

# Remove the custom JS that was reloading the page and instead post the form directly.
js_start = t.index("<script>")
js_end = t.rindex("</script>") + len("</script>")
js = t[js_start:js_end]
js = js.replace("    const summary = document.getElementById('repo-folder-summary');\n", "    const summary = document.getElementById('repo-folder-summary');\n    const manifestInput = document.getElementById('repo-folder-manifest');\n")
js = js.replace("        if (selectedFiles.length === 0) {\n            summary.textContent = 'Select or drop a folder first.';\n            return;\n        }\n\n        const formData = new FormData(form);\n        formData.delete('FolderFiles');\n        for (const entry of selectedFiles) {\n            formData.append('FolderFiles', entry.file, entry.path);\n        }\n\n        const response = await fetch(form.action, {\n            method: 'POST',\n            body: formData,\n            headers: token ? { 'RequestVerificationToken': token } : {}\n        });\n\n        if (response.ok) {\n            window.location.reload();\n        } else {\n            window.location.reload();\n        }\n", "        if (selectedFiles.length === 0) {\n            summary.textContent = 'Select or drop a folder first.';\n            return;\n        }\n\n        if (manifestInput) {\n            manifestInput.value = JSON.stringify(selectedFiles.map(entry => entry.path));\n        }\n\n        form.submit();\n")
js = js.replace("    clearBtn.addEventListener('click', () => {\n        selectedFiles = [];\n        fileInput.value = '';\n        if (nameInput) nameInput.value = '';\n        if (urlInput) urlInput.value = '';\n        summary.textContent = 'No folder selected yet.';\n    });\n", "    clearBtn.addEventListener('click', () => {\n        selectedFiles = [];\n        fileInput.value = '';\n        if (nameInput) nameInput.value = '';\n        if (urlInput) urlInput.value = '';\n        if (manifestInput) manifestInput.value = '';\n        summary.textContent = 'No folder selected yet.';\n    });\n")
# drop the fetch post error path logic entirely
start = js.index("    form.addEventListener('submit', async event => {")
end = js.index("    });\n})();") + len("    });\n})();")
new_submit = """    form.addEventListener('submit', event => {\n        if (selectedFiles.length === 0) {\n            event.preventDefault();\n            summary.textContent = 'Select or drop a folder first.';\n            return;\n        }\n\n        if (manifestInput) {\n            manifestInput.value = JSON.stringify(selectedFiles.map(entry => entry.path));\n        }\n    });\n})();"""
js = js[:start] + new_submit + js[end:]

t = t[:js_start] + js + t[js_end:]
p.write_text(t, encoding='utf-8')
print('simplified upload form')
