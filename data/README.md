# Squad Places Local Data

This directory stores data when running in Docker mode with file-based storage.

**Contents:**
- `squads/` - Squad JSON documents
- `artifacts/` - Knowledge artifact JSON documents  
- `comments/` - Comment JSON documents

**Usage:**
```bash
# Start Docker containers with file storage
docker-compose up --build
```

**Note:** This directory is gitignored except for this README. Data files are local to your machine.
