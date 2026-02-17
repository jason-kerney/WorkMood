---
name: release-engineer
description: >
  Step-by-step guidance for user-driven release engineering in WorkMood. Covers commit review since last tag, semantic versioning, version bumping, changelog, tagging, and packaging for local deployment.
user-invokable: true
argument-hint: "[release scenario] [platform]"
disable-model-invocation: false
---

# Release Engineer Skill

## When to Use This Skill
- Preparing a new release for WorkMood (user-built, no CI/CD)
- Reviewing changes since the last release
- Bumping version numbers and tagging releases
- Generating changelogs and packaging for deployment

---

## Core Concepts
- **Semantic Versioning**: Bump major, minor, or patch based on functional changes
- **Commit Review**: Review all commits since the last tag to determine release scope
- **Single-Commit Rule**: Version bump and changelog must be committed before tagging
- **Tagging**: Tag the release in git and push to remote
- **Packaging**: Build and package the app for user deployment

---

## How-To: User-Driven Release Process
1. **Review Commits Since Last Tag**
   - `git fetch --tags`
   - `git tag --list` (find latest tag)
   - `git log <last-tag>..HEAD --oneline` (review changes)
2. **Determine Version Bump**
   - Major: breaking changes or incompatible save file/data
   - Minor: new features, backward-compatible changes
   - Patch: bugfixes, small improvements
3. **Update Version in Code**
   - Edit `MauiApp/WorkMood.MauiApp.csproj` (`ApplicationDisplayVersion`, `ApplicationVersion`)
   - Update any other version references (e.g., package.json if needed)
4. **Update Changelog (optional)**
   - Summarize changes since last release
5. **Commit Version Bump and Changelog**
   - `git add .`
   - `git commit -m "Bump version to vX.Y.Z for release"`
6. **Tag the Release**
   - `git tag vX.Y.Z`
   - `git push --tags`
7. **Build and Package**
   - `dotnet publish MauiApp/WorkMood.MauiApp.csproj -c Release -f net9.0-windows10.0.19041.0 --output ./publish-win`
   - (or for macOS: use net9.0-maccatalyst)

---

## Checklist
- [ ] All commits since last tag reviewed
- [ ] Version updated in code
- [ ] Changelog updated (if used)
- [ ] Version bump and changelog committed
- [ ] Release tagged and pushed
- [ ] App built and packaged for deployment

---

## Anti-Patterns/When NOT to Use
- Don’t skip commit review—always check for breaking changes
- Don’t tag before committing version bump
- Don’t forget to push tags to remote
- Don’t use CI/CD-specific steps—this skill is for user-driven releases

---

## References
- [Semantic Versioning](https://semver.org/)
- [WorkMood BUILD.md](../../../../BUILD.md)
- [Build Engineer Skill](../build-engineer/SKILL.md)
