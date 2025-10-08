#!/bin/sh
#
# Pre-commit hook to ensure code compiles cleanly and YAML is valid before allowing commit
# This prevents committing broken code that would fail CI
#
# Installation:
#   cp docs/pre-commit-hook.sh .git/hooks/pre-commit
#   chmod +x .git/hooks/pre-commit
#

echo "Running pre-commit checks..."

# Check YAML files in .github directory
echo "Checking YAML syntax..."
YAML_ERROR=0
for yaml_file in $(git diff --cached --name-only --diff-filter=ACM | grep '\.github/.*\.yml$\|\.github/.*\.yaml$'); do
    if [ -f "$yaml_file" ]; then
        echo "  Validating $yaml_file..."
        # Use Ruby's YAML parser to validate syntax (Ruby is pre-installed on macOS)
        if ! ruby -ryaml -e "YAML.load_file('$yaml_file')" 2>&1; then
            echo "❌ YAML syntax error in $yaml_file"
            YAML_ERROR=1
        fi
    fi
done

if [ $YAML_ERROR -eq 1 ]; then
    echo ""
    echo "❌ YAML validation FAILED - commit rejected"
    echo "Please fix YAML syntax errors before committing"
    exit 1
fi

echo "✅ YAML validation passed"

# Build the solution
echo "Running build check..."
if ! dotnet build --no-restore > /dev/null 2>&1; then
    echo "❌ Build FAILED - commit rejected"
    echo ""
    echo "Please fix compilation errors before committing:"
    dotnet build --no-restore
    exit 1
fi

echo "✅ Build succeeded - proceeding with commit"
exit 0
