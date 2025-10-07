#!/bin/bash
#
# Install Git hooks for SWTSharp project
# Run this script after cloning the repository to set up local development hooks
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
HOOKS_DIR="$PROJECT_ROOT/.git/hooks"

echo "Installing Git hooks for SWTSharp..."

# Pre-commit hook - ensures code compiles before commit
cat > "$HOOKS_DIR/pre-commit" <<'EOF'
#!/bin/sh
#
# Pre-commit hook to ensure code compiles cleanly before allowing commit
# This prevents committing broken code that would fail CI
#

echo "Running pre-commit build check..."

# Build the solution
if ! dotnet build --no-restore > /dev/null 2>&1; then
    echo "❌ Build FAILED - commit rejected"
    echo ""
    echo "Please fix compilation errors before committing:"
    dotnet build --no-restore
    exit 1
fi

echo "✅ Build succeeded - proceeding with commit"
exit 0
EOF

# Pre-push hook - ensures code compiles before push
cat > "$HOOKS_DIR/pre-push" <<'EOF'
#!/bin/sh
#
# Pre-push hook to ensure code compiles cleanly before allowing push
# This prevents pushing broken code that would fail CI
#

echo "Running pre-push build check..."

# Build the solution
if ! dotnet build --no-restore > /dev/null 2>&1; then
    echo "❌ Build FAILED - push rejected"
    echo ""
    echo "Please fix compilation errors before pushing:"
    dotnet build --no-restore
    exit 1
fi

echo "✅ Build succeeded - proceeding with push"
exit 0
EOF

# Make hooks executable
chmod +x "$HOOKS_DIR/pre-commit"
chmod +x "$HOOKS_DIR/pre-push"

echo "✅ Git hooks installed successfully!"
echo ""
echo "Hooks installed:"
echo "  - pre-commit: Ensures code compiles before commit"
echo "  - pre-push: Ensures code compiles before push"
echo ""
echo "To bypass hooks (not recommended): git commit --no-verify"
