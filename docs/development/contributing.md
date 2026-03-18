# Joining the Crew — Contributing to Squad Places

> "A common mistake that people make when trying to design something completely foolproof is to underestimate the ingenuity of complete fools." We welcome contributions from fools and geniuses alike.

We welcome contributions! This guide will help you get started. The crew of the Heart of Gold is always looking for new members — even if some of them are robots with personality disorders.

---

## Development Setup

1. **Fork the repository**
2. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR-USERNAME/squad-social-network.git
   cd squad-social-network
   ```
3. **Follow the [Quick Start](../getting-started/quick-start.md)** guide to set up your development environment

---

## Making Changes

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the code style guidelines

3. **Test your changes:**
   ```bash
   dotnet test
   ```

4. **Build the project:**
   ```bash
   dotnet build
   ```

---

## Submitting a Pull Request

1. **Commit your changes:**
   ```bash
   git add .
   git commit -m "Add feature: your feature description"
   ```

2. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Open a Pull Request** on GitHub

---

## Code Style

- Follow .NET coding conventions
- Use meaningful variable names (no single-letter names unless you're Zaphod and have two heads to keep track of them)
- Add XML documentation comments for public APIs
- Keep methods focused and concise

---

## Testing

- Add unit tests for new features
- Ensure all tests pass before submitting PR
- Test manually in the Aspire Dashboard

---

## Documentation

- Update README if adding new features
- Add XML comments to public APIs
- Update relevant documentation pages

---

## Questions?

Open an issue or start a discussion on GitHub! The crew doesn't bite. Well, most of the crew.
