#!/bin/bash
# Video Processing Script for Login Background
# Compresses and converts video for optimal web performance
# 
# Usage: ./process-video.sh input.mp4
#
# Output: parallax-bg.mp4 and parallax-bg.webm (both optimized for web)

INPUT="$1"
OUTPUT_DIR="$(dirname "$0")"

if [ -z "$INPUT" ]; then
    echo "Usage: $0 <input-video.mp4>"
    echo ""
    echo "Example:"
    echo "  ./process-video.sh ~/parallax_night_countryside_00001_.mp4"
    exit 1
fi

if [ ! -f "$INPUT" ]; then
    echo "Error: File '$INPUT' not found"
    exit 1
fi

echo "=========================================="
echo "Processing video for login background..."
echo "=========================================="
echo ""

# Get video info
echo "📹 Input: $INPUT"
DURATION=$(ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 "$INPUT" 2>/dev/null)
echo "⏱️  Duration: ${DURATION}s"
echo ""

# Create MP4 (H.264) - wider compatibility
echo "🔄 Creating optimized MP4..."
ffmpeg -y -i "$INPUT" \
    -c:v libx264 \
    -preset slow \
    -crf 28 \
    -vf "scale=1920:-2" \
    -an \
    -movflags +faststart \
    -t 30 \
    "$OUTPUT_DIR/parallax-bg.mp4" 2>/dev/null

MP4_SIZE=$(du -h "$OUTPUT_DIR/parallax-bg.mp4" | cut -f1)
echo "   ✅ parallax-bg.mp4 ($MP4_SIZE)"

# Create WebM (VP9) - smaller file size
echo "🔄 Creating optimized WebM..."
ffmpeg -y -i "$INPUT" \
    -c:v libvpx-vp9 \
    -crf 35 \
    -b:v 0 \
    -vf "scale=1920:-2" \
    -an \
    -t 30 \
    "$OUTPUT_DIR/parallax-bg.webm" 2>/dev/null

WEBM_SIZE=$(du -h "$OUTPUT_DIR/parallax-bg.webm" | cut -f1)
echo "   ✅ parallax-bg.webm ($WEBM_SIZE)"

echo ""
echo "=========================================="
echo "✅ Video processing complete!"
echo "=========================================="
echo ""
echo "Files created:"
echo "  - $OUTPUT_DIR/parallax-bg.mp4 ($MP4_SIZE)"
echo "  - $OUTPUT_DIR/parallax-bg.webm ($WEBM_SIZE)"
echo ""
echo "The login page will now use this video background."
echo "Restart the application to see the changes."
