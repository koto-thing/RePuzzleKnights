@echo off
REM Windows向けビルドスクリプト
REM 使用方法: build-windows.bat [debug|release]

setlocal enabledelayedexpansion

cd /d "%~dp0native_pathfinder"

set BUILD_TYPE=%1
if "%BUILD_TYPE%"=="" set BUILD_TYPE=release

if "%BUILD_TYPE%"=="debug" (
    echo Building for Windows (Debug)...
    cargo build
    echo.
    echo Built: target\debug\native_pathfinder.dll
) else if "%BUILD_TYPE%"=="release" (
    echo Building for Windows (Release)...
    cargo build --release
    echo.
    echo Built: target\release\native_pathfinder.dll
) else (
    echo Usage: %0 [debug^|release]
    exit /b 1
)

echo.
echo Done!

