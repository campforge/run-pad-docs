---
title: Installation and first run
description: Install RunPad on Windows, macOS or Linux and set up the .NET SDK and netcoredbg.
---

# Installation and first run

RunPad is a desktop application. To compile and run scripts it uses a .NET SDK installed
on your machine, and to debug them it uses the open-source
[netcoredbg](https://github.com/Samsung/netcoredbg) debugger. The first-run setup can
install both for you.

## Download

Download RunPad from the [releases page](https://github.com/campforge/run-pad/releases).
The newest release is at the top; pick the file for your system from its **Assets**
list:

=== "Windows"

    | File | What it is |
    | --- | --- |
    | `RunPad-win-x64.msi` | Installer. Adds a Start Menu shortcut and an entry in *Add or remove programs*. A newer version installs over the old one. |
    | `RunPad-win-x64.zip` | Portable build. Unzip it anywhere and run `RunPad.exe`. Nothing is installed. |

=== "macOS"

    | File | What it is |
    | --- | --- |
    | `RunPad-osx-arm64.dmg` | Disk image for Apple Silicon Macs (M1 and later). |
    | `RunPad-osx-x64.dmg` | Disk image for Intel Macs. |

    Apple menu → **About This Mac** shows which kind of Mac you have.

=== "Linux"

    | File | What it is |
    | --- | --- |
    | `runpad_<version>_amd64.deb` | Debian/Ubuntu package. Install it with `sudo dpkg -i runpad_*.deb` or your software center. It puts `runpad` on `PATH` and adds a menu entry. |
    | `RunPad-x86_64.AppImage` | Single-file portable build. Make it executable (`chmod +x`) and run it. Nothing is installed system-wide. |
    | `RunPad-linux-x64.tar.gz` | Archive. Unpack it and run `RunPad`. |

## macOS

For architecture selection, first-launch approval, SDK setup and troubleshooting, see
[Installing RunPad on macOS](install-macos.md).

1. Open the disk image for your Mac.
2. Drag `RunPad.app` onto the `Applications` folder shown next to it.
3. Eject the disk image and start RunPad from Applications.

The build is not yet signed or notarized with an Apple Developer certificate, so on the
first launch macOS may refuse to open it because it cannot verify the developer. To open
it anyway:

1. Open **System Settings → Privacy & Security**.
2. Click **Open Anyway** next to the message about RunPad, then confirm.

macOS offers that button only for a while after a blocked launch. Depending on your
macOS version and security settings, it may not offer it at all. The disk image also
contains a `Read Me First.txt` with the same steps.

## First run: .NET SDK and debugger

On slower launches, a startup screen stays visible while RunPad checks the installed tools.
If startup fails, it shows the log location and a **Close** button.

When RunPad starts, it checks two prerequisites:

- **.NET SDK** — found when `dotnet --list-sdks` (or the dotnet path you configured)
  reports at least one SDK of version 6 or newer.
- **netcoredbg** — found when a debugger path is configured and the file exists.

If both are present, the main window opens directly. If either is missing, a setup
window opens first. For each item it shows ✅ or ❌, a **Browse…** button to pick an
existing executable, and an **Install** button:

- **Install** for the .NET SDK downloads Microsoft's official `dotnet-install` script
  and installs the .NET 10 SDK channel for your user account: `%LOCALAPPDATA%\Microsoft\dotnet`
  on Windows, `~/.dotnet` on macOS and Linux.
- **Install** for netcoredbg downloads the matching release from the
  [Samsung/netcoredbg](https://github.com/Samsung/netcoredbg/releases) GitHub releases
  and extracts it into RunPad's data folder (see
  [Where RunPad stores data](reference.md#where-runpad-stores-data)).

Both installers need internet access. A long-running install can be cancelled. You can
also click **Continue to RunPad** without installing anything and configure the paths
later in **Preferences → Languages → .NET** (fields *Dotnet path* and *Debugger path*).

!!! note "What works without each prerequisite"
    Without a .NET SDK, RunPad cannot compile or run scripts. Without netcoredbg,
    normal runs work but debugging does not.

## Choosing the SDK a script runs with

A script runs on the .NET version its `targetSdk` names, and the SDK drop-down in the
toolbar above the editor shows and changes it:

- **latest (.NET 10.0.100)** — the newest installed stable SDK; the file keeps
  `targetSdk: latest`, so the script moves to a newer SDK when you install one.
- **.NET 8.0.400** and the other entries — the newest installed SDK of each .NET version
  from .NET 6 upwards; picking one writes its version (`targetSdk: net8.0`) to the file.

If the version a script names isn't installed, it runs on the newest SDK: the Results tab
starts with a warning, and the status bar shows *net6.0 (not installed)*. The
[command-line runner](cli.md) follows the same rule. See [Front matter](scripts.md#front-matter).

## Updating and uninstalling

RunPad does not update itself. To update, download the new version from the
[releases page](https://github.com/campforge/run-pad/releases):

- **Windows installer**: install the new `.msi` over the old one. Uninstall from
  *Add or remove programs*.
- **Portable builds** (zip, AppImage, tar.gz): replace or delete the files.
- **Debian package**: install the new `.deb` the same way; remove it with your package
  manager.
- **macOS**: replace or delete `RunPad.app` in Applications.

Your scripts and settings live outside the application folder, so updating or removing
RunPad does not delete them. See [Where RunPad stores data](reference.md#where-runpad-stores-data).
