# AI Codex: Documentation Maintenance

## Overview
This codex provides comprehensive guidance for maintaining and updating GitHub Copilot instructions and project documentation within the WorkMood application, focusing on triggers, processes, and responsibilities for keeping documentation accurate and useful.

**When to Reference**: Updating documentation after structural changes; managing instruction accuracy; planning documentation maintenance; validating documentation completeness; assigning documentation responsibilities.

## Maintenance Philosophy

### Documentation as Living Code
Documentation should be treated with the same rigor and attention as source code:
- **Version Controlled**: All changes tracked through git with meaningful commit messages
- **Reviewed**: Documentation changes should be reviewed for accuracy and completeness
- **Tested**: Documentation should be validated against actual project state
- **Maintained**: Regular updates to prevent documentation drift from reality

### Proactive vs. Reactive Maintenance
**Proactive Maintenance** (Preferred):
- Documentation updated as part of the same commit that introduces changes
- Prevents documentation debt from accumulating
- Ensures instructions are always current and accurate
- Reduces cognitive load on future developers

**Reactive Maintenance** (Fallback):
- Periodic reviews to identify and fix documentation gaps
- Quarterly comprehensive audits of instruction accuracy
- Community feedback integration and issue resolution
- Legacy documentation cleanup and modernization

## Detailed Maintenance Triggers

### 1. Project Structure Changes
**When to Update**: Any modification to the fundamental organization of the codebase

#### Adding/Removing Projects
- **New Projects**: Update project count, solution structure, build commands
- **Removed Projects**: Clean up references, update dependency trees, modify build scripts
- **Project Renaming**: Update all references throughout documentation
- **Project Type Changes**: Update framework targeting, build processes, testing strategies

**Example Changes Requiring Updates**:
```
// Before: 4-project solution
WorkMood.sln
├── MauiApp/
├── WorkMood.MauiApp.Tests/  
├── whats-your-version/
└── whats-your-version-tests/

// After: Adding new shared library project
WorkMood.sln
├── MauiApp/
├── WorkMood.MauiApp.Tests/
├── WorkMood.Shared/           // NEW PROJECT
├── WorkMood.Shared.Tests/     // NEW PROJECT  
├── whats-your-version/
└── whats-your-version-tests/
```

#### New Folders/Organization
- **Folder Structure**: Update architecture diagrams and folder explanations
- **Namespace Changes**: Modify import examples and code samples
- **File Organization**: Update file location references and search patterns

**Documentation Sections to Update**:
- Architecture codex folder structure
- Build command examples
- Test project organization
- Import/reference examples

### 2. Service Architecture Evolution
**When to Update**: Changes to the core service layer and dependency injection patterns

#### New Services
- **Interface Definitions**: Add to core services list with brief descriptions
- **Dependency Patterns**: Update DI registration examples
- **Usage Examples**: Provide code samples for new service integration
- **Testing Patterns**: Document mocking and testing approaches

**Example Service Addition**:
```csharp
// New service interface
public interface IAudioNotificationService
{
    Task PlayNotificationSoundAsync(NotificationSound sound);
    Task<bool> IsSoundEnabledAsync();
    Task SetVolumeAsync(float volume);
}

// Required documentation updates:
// 1. Add to "Key Service Interfaces" list
// 2. Update DI registration examples in architecture codex
// 3. Add testing patterns for audio service mocking
// 4. Update service interaction examples
```

#### Interface Changes
- **Breaking Changes**: Document migration patterns and compatibility notes
- **New Methods**: Update interface definitions and usage examples
- **Deprecated Methods**: Add deprecation warnings and replacement guidance
- **Parameter Changes**: Update all code examples using affected methods

#### Dependency Injection Pattern Updates
- **New Registration Patterns**: Update MauiProgram.cs examples
- **Service Lifetime Changes**: Document scoping decisions and rationale
- **Factory Pattern Evolution**: Update factory registration and usage examples
- **Circular Dependency Resolution**: Document new resolution strategies

### 3. Build Process Modifications
**When to Update**: Changes to compilation, testing, or deployment procedures

#### New Commands/Scripts
- **Build Commands**: Add new framework targets or build configurations
- **Test Commands**: Update test execution patterns and coverage collection
- **Deployment Scripts**: Document new publication or packaging procedures
- **Development Tools**: Add new tool requirements and setup instructions

**Example Build Process Change**:
```bash
# Old build process
dotnet build --framework net9.0-windows10.0.19041.0

# New multi-target build process
dotnet build --framework net9.0-windows10.0.19041.0
dotnet build --framework net9.0-maccatalyst
dotnet build --framework net9.0-android  # NEW TARGET

# Documentation updates required:
# 1. Update "Framework Targeting" section in build-testing codex
# 2. Add Android-specific build requirements
# 3. Update CI/CD pipeline documentation
# 4. Add cross-platform testing procedures
```

#### Target Framework Changes
- **New Frameworks**: Document requirements, limitations, and platform-specific considerations
- **Deprecated Frameworks**: Add sunset timelines and migration guidance
- **Version Updates**: Update minimum requirements and compatibility matrices
- **Platform Support**: Document new platform capabilities and restrictions

#### Testing Procedure Evolution
- **New Test Categories**: Update test execution filters and organization
- **Coverage Requirements**: Document new coverage thresholds and reporting
- **Performance Testing**: Add benchmarking and profiling procedures
- **Integration Testing**: Update end-to-end testing scenarios and data

### 4. Development Workflow Changes
**When to Update**: Modifications to tools, processes, or developer experience

#### Tool Changes
- **New Development Tools**: Document installation, configuration, and usage
- **Version Updates**: Update tool version requirements and compatibility
- **Deprecated Tools**: Provide migration paths and alternative workflows
- **IDE Extensions**: Document required extensions and configuration

#### Dependency Updates
- **Major Version Updates**: Document breaking changes and migration procedures
- **New Dependencies**: Add to dependency list with purpose and configuration
- **Removed Dependencies**: Clean up references and update alternative approaches
- **Security Updates**: Document security-related changes and implications

#### Refactoring Pattern Evolution
- **New Patterns**: Document pattern implementation and usage guidelines
- **Pattern Deprecation**: Provide migration guidance and timeline
- **Anti-Pattern Updates**: Add new anti-patterns and avoidance strategies
- **Tooling Changes**: Update automated refactoring tool configurations

#### Commit Convention Updates
- **New Intention Types**: Add to Arlo's notation examples and guidance
- **Risk Assessment Changes**: Update risk level criteria and examples
- **Commit Message Templates**: Update .gitmessage and snippet files
- **Automation Updates**: Document new commit message generation tools

## Comprehensive Update Process

### Immediate Update Workflow
**Principle**: Documentation changes should be included in the same commit that introduces the structural change

#### Step-by-Step Process
1. **Make Structural Change**: Implement the actual code/project modification
2. **Identify Documentation Impact**: Review all documentation that might be affected
3. **Update Relevant Documentation**: Make necessary changes to instructions and codexes
4. **Validate Documentation**: Ensure documentation matches new reality
5. **Commit Together**: Include both structural and documentation changes in same commit

#### Commit Message Pattern
```bash
# Primary change with documentation update
^f - add new AudioNotificationService with DI registration

# Updates both:
# - New service implementation
# - Documentation: service interface list, DI examples, testing patterns
```

#### Documentation Impact Assessment
Before committing any structural change, check these documentation locations:
- **Main Instructions**: Core workflows and essential information
- **Architecture Codex**: Service interfaces, folder structure, patterns
- **Build-Testing Codex**: Build commands, test procedures, framework targeting
- **Refactoring Codex**: New abstractions, refactoring targets, patterns
- **Examples Codex**: Code transformation examples, historical references

### Validation Procedures

#### Accuracy Validation
**Principle**: Documentation must reflect actual project state, not intended or outdated state

**Validation Checklist**:
- [ ] **Build Commands Work**: All documented commands execute successfully
- [ ] **File Paths Exist**: All referenced files and folders are present
- [ ] **Code Examples Compile**: All code samples are syntactically correct
- [ ] **Dependencies Current**: All listed dependencies match actual requirements
- [ ] **Version Numbers Match**: All version references are current and accurate

#### Completeness Validation
**Principle**: Documentation should cover all essential aspects without overwhelming detail

**Completeness Checklist**:
- [ ] **New Features Documented**: All significant additions have documentation
- [ ] **Breaking Changes Noted**: All breaking changes have migration guidance
- [ ] **Examples Provided**: Code examples exist for complex or new concepts
- [ ] **Links Work**: All internal and external links are functional
- [ ] **Cross-References Accurate**: References between documents are correct

#### Consistency Validation
**Principle**: Documentation should use consistent terminology, formatting, and organization

**Consistency Checklist**:
- [ ] **Terminology Consistent**: Same concepts use same terms throughout
- [ ] **Format Consistent**: Consistent heading styles, code formatting, lists
- [ ] **Organization Logical**: Information is organized predictably
- [ ] **Voice Consistent**: Documentation maintains consistent tone and perspective
- [ ] **Standards Followed**: Markdown, naming, and style standards are applied

### Periodic Review Procedures

#### Quarterly Comprehensive Reviews
**Schedule**: Every three months, conduct comprehensive documentation audit

**Review Scope**:
1. **Accuracy Audit**: Verify all instructions match current project state
2. **Completeness Audit**: Identify gaps in documentation coverage
3. **Usability Audit**: Assess whether documentation effectively serves users
4. **Redundancy Audit**: Identify and eliminate duplicate or conflicting information

**Review Process**:
1. **Preparation**: Gather list of all changes since last review
2. **Systematic Review**: Go through each document section by section
3. **Testing**: Execute all documented procedures to verify accuracy
4. **Feedback Integration**: Incorporate any user feedback or issues
5. **Update Implementation**: Make necessary corrections and improvements
6. **Review Documentation**: Update this maintenance documentation as needed

#### Annual Strategic Reviews
**Schedule**: Once per year, conduct strategic documentation assessment

**Strategic Questions**:
- Is the documentation structure still optimal for current project size/complexity?
- Are there new documentation needs emerging from project evolution?
- Should documentation be reorganized or restructured for better usability?
- Are there opportunities to automate documentation maintenance tasks?
- What documentation patterns from other successful projects could be adopted?

## Responsibility Matrix

### Contributors (All Team Members)
**Primary Responsibility**: Be aware of documentation impact when making changes

**Specific Duties**:
- **Impact Assessment**: Before making changes, consider documentation implications
- **Proactive Updates**: Include documentation updates in the same commit when possible
- **Flag for Review**: When uncertain about documentation impact, flag for maintainer review
- **Issue Reporting**: Report discovered documentation inaccuracies or gaps
- **Usage Feedback**: Provide feedback on documentation effectiveness and clarity

**When Making Changes**:
```bash
# Good: Documentation updated with structural change
^f - add new mood export feature with documentation updates

# Acceptable: Documentation flagged for separate update  
^f - add new mood export feature (docs TODO: update export section)
```

### Maintainers (Project Leads)
**Primary Responsibility**: Ensure documentation accuracy and completeness through review process

**Specific Duties**:
- **Review Validation**: During code reviews, validate documentation changes
- **Gap Identification**: Identify missing or incomplete documentation during reviews
- **Standards Enforcement**: Ensure documentation follows established patterns and quality
- **Strategic Planning**: Plan documentation improvements and reorganization
- **Process Improvement**: Refine documentation maintenance processes based on experience

**Review Checklist for Maintainers**:
- [ ] Does this change affect any documented workflows?
- [ ] Are new features or changes properly documented?
- [ ] Do code examples remain accurate after changes?
- [ ] Are any references or links broken by this change?
- [ ] Is the documentation change complete and accurate?

### GitHub Copilot (AI Assistant)
**Primary Responsibility**: Proactively identify and suggest documentation updates

**Specific Duties**:
- **Mismatch Detection**: Identify when documentation doesn't match current project state
- **Update Suggestions**: Suggest specific documentation changes when structural changes occur
- **Consistency Monitoring**: Flag inconsistencies between documentation and implementation
- **Best Practice Guidance**: Recommend documentation best practices during interactions
- **Process Adherence**: Ensure documentation update processes are followed

**AI Assistant Behaviors**:
- Always suggest documentation updates when helping with structural changes
- Flag outdated information when encountered during conversations
- Provide specific suggestions for documentation improvements
- Reference current documentation accurately and note when updates are needed

## Documentation Maintenance Tools

### Automated Validation
**Tools for Automatic Documentation Checking**:

#### Link Validation
```bash
# Check for broken internal links
find .github -name "*.md" -exec markdown-link-check {} \;

# Validate code block syntax
markdownlint .github/*.md --config .markdownlint.json
```

#### Content Validation
```bash
# Verify build commands work
dotnet build --framework net9.0-windows10.0.19041.0 --verbosity quiet

# Check file references exist
grep -r "\.github/" --include="*.md" | while read line; do
  # Extract file paths and verify they exist
done
```

### Documentation Generation
**Automated Documentation Updates**:

#### API Documentation
```bash
# Generate service interface documentation
dotnet tool run docfx --serve

# Update service interface list in architecture codex
```

#### Dependency Documentation
```bash
# Generate current dependency list
dotnet list package --include-transitive > current-dependencies.txt

# Compare with documented dependencies
```

### Documentation Metrics
**Tracking Documentation Health**:

#### Coverage Metrics
- **Feature Coverage**: Percentage of features with documentation
- **Code Example Coverage**: Ratio of documented APIs to total APIs
- **Process Coverage**: Percentage of workflows with documentation
- **Update Frequency**: How often documentation is updated relative to code changes

#### Quality Metrics
- **Accuracy Score**: Results of validation tests and reviews
- **User Satisfaction**: Feedback scores from documentation users
- **Issue Resolution Time**: Speed of fixing documentation problems
- **Consistency Score**: Results of style and terminology checks

## Migration and Evolution Strategies

### Documentation Refactoring
**When Documentation Structure Needs Major Changes**:

#### Identifying Need for Refactoring
- Documentation becomes too long or complex to navigate
- Frequent confusion about where to find specific information
- Redundant information across multiple documents
- User feedback indicating structural problems

#### Refactoring Process
1. **Current State Analysis**: Document current structure and identify problems
2. **Target State Design**: Design improved structure based on user needs
3. **Migration Planning**: Plan incremental migration to avoid disruption
4. **Implementation**: Execute migration with validation at each step
5. **User Communication**: Inform users of changes and provide migration guidance

### Legacy Documentation Handling
**Managing Outdated Documentation**:

#### Deprecation Process
1. **Identify Legacy Content**: Content that's no longer accurate or relevant
2. **Add Deprecation Warnings**: Clear warnings about outdated information
3. **Provide Migration Path**: Guide users to current information
4. **Sunset Timeline**: Set clear timeline for removal
5. **Archive or Remove**: Clean up outdated content after sunset period

#### Historical Preservation
- **Success Stories**: Preserve documentation of successful patterns and practices
- **Lessons Learned**: Document what didn't work and why
- **Evolution Record**: Maintain record of how practices evolved over time
- **Reference Examples**: Keep examples of how problems were solved

## Success Metrics and Monitoring

### Key Performance Indicators

#### Documentation Accuracy
- **Validation Pass Rate**: Percentage of documentation that passes accuracy validation
- **User Error Rate**: Frequency of users encountering incorrect information
- **Time to Fix Issues**: Speed of correcting identified inaccuracies
- **Proactive Update Rate**: Percentage of changes that include documentation updates

#### Documentation Usability
- **User Success Rate**: Percentage of users who successfully complete documented procedures
- **Search Success Rate**: Ability of users to find relevant information
- **Question Frequency**: How often users ask questions answered in documentation
- **Feedback Quality**: Satisfaction scores and qualitative feedback

#### Maintenance Efficiency
- **Update Effort**: Time required to maintain documentation relative to code changes
- **Review Efficiency**: Time required for documentation review during code reviews
- **Automation Coverage**: Percentage of documentation validation that's automated
- **Process Adherence**: How consistently the documentation maintenance process is followed

### Continuous Improvement

#### Feedback Integration
- **User Surveys**: Regular surveys about documentation effectiveness
- **Usage Analytics**: Track which documentation is most/least used
- **Issue Tracking**: Monitor documentation-related issues and feature requests
- **Community Contributions**: Encourage and integrate external documentation contributions

#### Process Refinement
- **Regular Retrospectives**: Team discussions about documentation process effectiveness
- **Tool Evaluation**: Regular assessment of documentation tools and automation
- **Best Practice Evolution**: Continuous improvement of documentation standards and practices
- **Training Updates**: Keep team training current with documentation best practices

---

*This codex serves as the definitive guide for maintaining accurate, complete, and useful documentation within the WorkMood project. All documentation maintenance activities should follow these established procedures to ensure long-term documentation quality and developer productivity.*