# Native Pathfinder

A* pathfinding algorithm implemented in Rust for native C interop (Unity compatible).

## Building

### Windows

```bash
# Debug build
cargo build

# Release build
cargo build --release

# または、スクリプトを使用
.\build-windows.bat release
```

Built binaries:
- **Debug**: `target/debug/native_pathfinder.dll`
- **Release**: `target/release/native_pathfinder.dll`

---

### macOS

#### Option 1: Build directly on Mac (Recommended)

```bash
# Install Rust (if not already installed)
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Build for Apple Silicon (ARM64)
cargo build --release --target aarch64-apple-darwin

# Build for Intel Mac
cargo build --release --target x86_64-apple-darwin
```

Built binaries:
- **ARM Mac**: `target/aarch64-apple-darwin/release/libnative_pathfinder.dylib`
- **Intel Mac**: `target/x86_64-apple-darwin/release/libnative_pathfinder.dylib`

#### Option 2: Use build script on Mac

```bash
# Make script executable
chmod +x build-macos.sh

# Build for ARM64
./build-macos.sh aarch64

# Build for Intel
./build-macos.sh x86_64

# Build universal binary (ARM64 + Intel)
./build-macos.sh universal
```

#### Option 3: Use GitHub Actions (CI/CD)

Push your code to GitHub, and the workflow `.github/workflows/build-macos.yml` will automatically build for both ARM64 and Intel macOS on every push.

---

### Supported Targets

| Target | Description | Command |
|--------|-------------|---------|
| `x86_64-pc-windows-msvc` | Windows (MSVC) | `cargo build --release` |
| `x86_64-pc-windows-gnu` | Windows (MinGW) | `cargo build --release --target x86_64-pc-windows-gnu` |
| `aarch64-apple-darwin` | macOS ARM64 (Apple Silicon) | `cargo build --release --target aarch64-apple-darwin` |
| `x86_64-apple-darwin` | macOS Intel | `cargo build --release --target x86_64-apple-darwin` |

---

## C FFI Interface

### Function: `find_path`

```c
extern "C" int32_t find_path(
    const Vector3* nodes,           // Array of graph nodes
    int32_t node_count,            // Number of nodes
    const int32_t* edges,          // Edge indices (triplets: from, to, cost)
    int32_t edge_count,            // Number of edges
    int32_t start_index,           // Start node index
    int32_t goal_index,            // Goal node index
    int32_t* out_path,             // Output path buffer
    int32_t max_path_length        // Maximum path length
);
```

**Returns**: Path length on success, -1 on error

---

## Development

### Run tests

```bash
cargo test
```

### Check for issues

```bash
cargo clippy
```

### Format code

```bash
cargo fmt
```

---

## Safety

This crate uses `unsafe` Rust for C interop. All unsafe operations are documented and wrapped with safety contracts.

### Important Notes

- ⚠️ All pointers must be valid and properly aligned
- ⚠️ Buffers must be large enough for output data
- ⚠️ Null pointer checks are performed at the boundary
- ⚠️ Panics in Rust code are caught and converted to error codes (-1)

---

## License

MIT

