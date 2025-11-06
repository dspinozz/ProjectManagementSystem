#!/bin/bash

# Script to remove AI-generated checkpoint/documentation files
# These are not needed for the portfolio repository

echo "Cleaning up AI-generated files..."

# List of AI-generated files to remove (already in .gitignore)
FILES_TO_REMOVE=(
    "ANSWERS.md"
    "FINAL_ASSESSMENT.md"
    "FINAL_TEST_VERIFICATION.md"
    "GAPS_FIXED.md"
    "IMPROVEMENTS.md"
    "INTEGRATION_TESTS_NOTE.md"
    "PORTFOLIO_READINESS.md"
    "PRE_GITHUB_CHECKLIST.md"
    "STABILITY_IMPROVEMENTS.md"
    "SWAGGER_EXPLANATION.md"
    "TEST_ANALYSIS_REPORT.md"
    "TEST_COVERAGE_ANALYSIS.md"
    "TEST_COVERAGE_ASSESSMENT.md"
    "TEST_DATA_SEEDING_RECOMMENDATION.md"
    "TEST_EXPANSION_SUMMARY.md"
    "TEST_RUN_INSTRUCTIONS.md"
    "TEST_SEEDING_SUMMARY.md"
    "TEST_STATUS.md"
    "TEST_VERIFICATION.md"
    "TEST_VERIFICATION_COMPLETE.md"
    "TESTING_STATUS.md"
)

# Keep these files (they're useful documentation)
KEEP_FILES=(
    "README.md"
    "TESTING.md"
    "QUICK_START.md"
    "SWAGGER_UI_EXPLANATION.md"
    "FRONTEND_ANALYSIS.md"
)

for file in "${FILES_TO_REMOVE[@]}"; do
    if [ -f "$file" ]; then
        echo "Removing: $file"
        rm "$file"
    fi
done

echo "Cleanup complete!"
echo ""
echo "Kept files:"
for file in "${KEEP_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "  ✓ $file"
    fi
done

