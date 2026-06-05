#!/bin/bash
# macOS向けビルドスクリプト
# 使用方法: ./build-macos.sh [aarch64|x86_64|universal]

set -e

cd "$(dirname "$0")/native_pathfinder"

TARGET="${1:-aarch64}"

case $TARGET in
  aarch64)
    echo "🍎 Building for ARM64 macOS (Apple Silicon)..."
    cargo build --release --target aarch64-apple-darwin
    echo "✅ Built: target/aarch64-apple-darwin/release/libnative_pathfinder.dylib"
    ;;
  x86_64)
    echo "🍎 Building for Intel macOS..."
    cargo build --release --target x86_64-apple-darwin
    echo "✅ Built: target/x86_64-apple-darwin/release/libnative_pathfinder.dylib"
    ;;
  universal)
    echo "🍎 Building universal binary (ARM64 + Intel)..."
    cargo build --release --target aarch64-apple-darwin
    cargo build --release --target x86_64-apple-darwin

    lipo -create \
      target/aarch64-apple-darwin/release/libnative_pathfinder.dylib \
      target/x86_64-apple-darwin/release/libnative_pathfinder.dylib \
      -output target/release/libnative_pathfinder-universal.dylib

    echo "✅ Built: target/release/libnative_pathfinder-universal.dylib"
    ;;
  *)
    echo "❌ Usage: $0 [aarch64|x86_64|universal]"
    exit 1
    ;;
esac

