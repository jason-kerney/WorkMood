#!/bin/bash
# Bash script to generate coverage report with proper filtering
# Usage: ./generate-coverage-report.sh

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}Starting coverage report generation...${NC}"

# Clean up previous results
if [ -d "WorkMood.MauiApp.Tests/TestResults" ]; then
    rm -rf WorkMood.MauiApp.Tests/TestResults
    echo -e "${GREEN}Removed WorkMood.MauiApp.Tests/TestResults folder${NC}"
fi

if [ -d "CoverageReport" ]; then
    rm -rf CoverageReport
    echo -e "${GREEN}Removed CoverageReport folder${NC}"
fi

# Run tests with coverage collection
echo -e "${CYAN}Running tests with coverage collection...${NC}"
dotnet test WorkMood.MauiApp.Tests --collect:"XPlat Code Coverage"

# Check if coverage data was generated
coverage_files=$(find WorkMood.MauiApp.Tests/TestResults -name "coverage.cobertura.xml" 2>/dev/null | wc -l)

if [ "$coverage_files" -eq 0 ]; then
    echo -e "${RED}ERROR: No coverage files found!${NC}"
    exit 1
fi

echo -e "${GREEN}Found $coverage_files coverage file(s)${NC}"

# Generate the coverage report with filters
echo -e "${CYAN}Generating filtered coverage report...${NC}"
reportgenerator \
    "-reports:WorkMood.MauiApp.Tests/TestResults/**/coverage.cobertura.xml" \
    "-targetdir:CoverageReport" \
    "-reporttypes:TextSummary" \
    "-verbosity:Off" \
    "-assemblyfilters:-whats-your-version" \
    "-classfilters:-Microsoft.*;-__XamlGeneratedCode__.*;-WinRT.*;-*.Tests.*;-*.XamlTypeInfo.*;-*.WinUI.*"

if [ $? -ne 0 ]; then
    echo -e "${RED}ERROR: reportgenerator failed with exit code $?${NC}"
    exit $?
fi

# Display the summary
if [ -f "CoverageReport/Summary.txt" ]; then
    echo ""
    echo -e "${YELLOW}Coverage Report:${NC}"
    echo -e "${YELLOW}=================${NC}"
    cat CoverageReport/Summary.txt
else
    echo -e "${RED}ERROR: Summary.txt not found in CoverageReport folder${NC}"
    exit 1
fi