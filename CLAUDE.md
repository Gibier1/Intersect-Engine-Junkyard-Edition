# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Intersect Engine is a 2D MMORPG game creation suite built on .NET 8 and MonoGame. It provides a complete development platform including a client, server, editor, and plugin system for creating 2D MMORPGs without programming experience.

## Build Commands

### Initial Setup
After cloning or updating the repository:
- **All platforms**: `git submodule update --init --recursive`
- **Non-Windows platforms**: `git apply disable-windows-only.patch`

Before updating on non-Windows platforms: `git apply -R disable-windows-only.patch`

### Building
```bash
# Debug build (no single-file output)
dotnet build -p:Configuration=Debug -p:PackageVersion=0.8.0-beta -p:Version=0.8.0

# Release build (single-file output in bin/)
dotnet build -p:Configuration=Release -p:PackageVersion=0.8.0-beta -p:Version=0.8.0

# Publish for specific runtime (clean single-file output)
dotnet publish -p:Configuration=Release -p:PackageVersion=0.8.0-beta -p:Version=0.8.0 -r <runtime-id>
# Runtime IDs: linux-x64, osx-x64, win-x64
```

### Running Tests
Tests use both MSTest and NUnit frameworks. Run tests with:
```bash
dotnet test Intersect.sln
```

For a single test project:
```bash
dotnet test Intersect.Tests\Intersect.Tests.csproj
```

## Architecture

### Project Structure

```
Framework/                      # Shared framework libraries
├── Intersect.Framework         # Base framework with logging, reflection, serialization
├── Intersect.Framework.Core    # Core framework extensions
└── Intersect.Framework.Multitarget # Multitarget framework

Intersect (Core)/               # Core library (MIT licensed)
├── Network/                    # Networking abstractions and packet handling
├── Plugins/                    # Plugin system implementation
├── Serialization/              # JSON serialization
├── IO/                         # File I/O utilities
└── ...                         # Core utilities and extensions

Intersect.Client.Framework/     # Client framework (MIT licensed)
├── Configuration/              # Client configuration
├── Content/                    # Content management
├── Graphics/                   # Rendering code (MonoGame)
├── Interface/                  # GUI (Gwen-based)
├── Networking/                 # Client networking
└── Plugins/                    # Client plugin interfaces

Intersect.Client.Core/          # Client core implementation (MIT licensed)
├── MonoGame/                   # MonoGame integration
├── Networking/                 # Network packet handlers
└── ...                         # Client implementation

Intersect.Server.Framework/     # Server framework (GPLv3 licensed)
├── Entities/                   # Server-side entity definitions
├── Items/                      # Server-side item definitions
├── Maps/                       # Server-side map definitions
└── Plugins/                    # Server plugin interfaces

Intersect.Server.Core/          # Server core implementation (GPLv3 licensed)
├── Database/                   # Database context (Entity Framework Core)
├── Networking/                 # Server networking
├── Migrations/                 # Database migrations
└── ...                         # Server implementation

Intersect.Client/               # Client application entry point (MIT licensed)
Intersect.Server/               # Server application entry point (GPLv3 licensed)
Intersect.Editor/               # Editor for Windows only (DirectX, GPLv3 licensed)
Intersect.SinglePlayer/         # Single-player mode (combines client+server in-process)
Intersect.Network/              # Networking layer using LiteNetLib (MIT licensed)

Examples/                       # Plugin development examples
├── Intersect.Examples.Plugin/          # Shared plugin code
├── Intersect.Examples.Plugin.Client/    # Client-side plugin example
└── Intersect.Examples.Plugin.Server/    # Server-side plugin example
```

### Key Architectural Patterns

**Plugin System**: The engine uses a plugin architecture where plugins can extend both client and server functionality. Plugins are loaded dynamically and can hook into various game events.

**Networking**: Uses LiteNetLib (vendor/LiteNetLib) for UDP networking with a custom packet system. Network keys are generated during build for RSA encryption. The networking layer supports asymmetric encryption for handshake.

**Client-Server Separation**: Game logic is split between client (what players see) and server (authoritative game state). The SinglePlayer project combines both in-process for offline play.

**Content Management**: Game content (maps, items, spells, etc.) is managed through a shared system between editor, client, and server. Content is serialized and loaded at runtime.

**Entity System**: Game entities (players, NPCs, items) follow a component-based pattern with shared definitions between client and server.

## Platform-Specific Notes

- **Editor**: Windows-only (DirectX requirement). Cannot run on Linux/macOS.
- **Client/Server**: Cross-platform (OpenGL on non-Windows).
- **Non-Windows builds**: Require `disable-windows-only.patch` to exclude Windows-specific code.

## Configuration

- **Build configurations**: Debug, Release, DebugTests, DebugFull, DebugPlugins
- **Runtime identifiers**: linux-arm64, linux-x64, osx-arm64, osx-x64, win-x64
- **Target framework**: .NET 8.0

## Common Development Patterns

**Packet Handling**: Network packets are defined in both client and server projects with a `[PacketHandler]` attribute pattern for routing.

**Event System**: The engine uses a custom event system for game events (see `Intersect (Core)/Eventing/`).

**Localization**: Strings are stored in `.resx` files for localization support.

**Database**: Server uses Entity Framework Core with migrations in `Intersect.Server/Migrations/`.

## Licensing

The project has split licensing:
- MIT: Intersect.Core, Intersect.Client, Intersect.Client.Framework, Intersect.Network
- GPLv3: Intersect.Server, Intersect.Server.Framework, Intersect.Editor, Intersect.Utilities

When modifying code, be aware of the license implications. GPL-licensed code cannot be used in proprietary projects.
