# ULE4JIS + Alt-IME for .NET 9 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a lightweight .NET 9 Windows system tray application that emulates US keyboard layout on JIS keyboards (ULE4JIS feature) and toggles IME ON/OFF on left/right Alt single-taps (alt-ime-ahk feature).

**Architecture:** Use Win32 Low-Level Keyboard Hook (`SetWindowsHookEx(WH_KEYBOARD_LL)`) in a .NET 9 WinForms application (`ApplicationContext` without main window, running only in the system tray). Intercept keys for JIS-to-US mapping and track Alt key press/release states for IME switching.

**Tech Stack:** C#, .NET 9.0 (`net9.0-windows`), Win32 User32 APIs via P/Invoke.

---

### Task 1: Project Initialization

**Files:**
- Create: `src/Ule4Jis.Net/Ule4Jis.Net.csproj`
- Create: `src/Ule4Jis.Net/NativeMethods.cs`

- [ ] **Step 1: Create .NET 9 WinForms project**

Run: `dotnet new winforms -o src/Ule4Jis.Net -f net9.0-windows`
Expected: Project created with `Ule4Jis.Net.csproj`.

- [ ] **Step 2: Define Win32 P/Invoke constants and structs in NativeMethods.cs**

Create `src/Ule4Jis.Net/NativeMethods.cs` with `SetWindowsHookEx`, `UnhookWindowsHookEx`, `CallNextHookEx`, `SendInput`, `GetModuleHandle`, `KBDLLHOOKSTRUCT`, `INPUT`, `KEYBDINPUT`, etc.

- [ ] **Step 3: Build to verify compilation**

Run: `dotnet build src/Ule4Jis.Net`
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/Ule4Jis.Net
git commit -m "feat: initialize .NET 9 WinForms project and NativeMethods P/Invoke definitions"
```

---

### Task 2: US-on-JIS Keyboard Mapper Module

**Files:**
- Create: `src/Ule4Jis.Net/UsOnJisMapper.cs`

- [ ] **Step 1: Implement UsOnJisMapper class**

Create `UsOnJisMapper.cs` containing mapping logic for:
- Shift+2 -> `@`
- Shift+6 -> `^`
- Shift+7 -> `&`
- Shift+8 -> `*`
- Shift+9 -> `(`
- Shift+0 -> `)`
- Shift+- -> `_`
- `=` -> `+` / `Shift+=`
- `[` `]` `{` `}` `;` `:` `'` `"` `\` `|` `` ` `` `~`

- [ ] **Step 2: Build project**

Run: `dotnet build src/Ule4Jis.Net`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/Ule4Jis.Net/UsOnJisMapper.cs
git commit -m "feat: add UsOnJisMapper for US keyboard layout emulation"
```

---

### Task 3: Alt-IME Switcher Module

**Files:**
- Create: `src/Ule4Jis.Net/AltImeSwitcher.cs`

- [ ] **Step 1: Implement AltImeSwitcher class**

Track Left Alt (`VK_LMENU` / `0xA4`) and Right Alt (`VK_RMENU` / `0xA5`) down/up states. Detect single tap (no other key pressed while Alt was down). Send `VK_IME_OFF` / `VK_IME_ON` or `VK_KANJI` using `SendInput`.

- [ ] **Step 2: Build project**

Run: `dotnet build src/Ule4Jis.Net`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/Ule4Jis.Net/AltImeSwitcher.cs
git commit -m "feat: add AltImeSwitcher for Left/Right Alt IME toggle"
```

---

### Task 4: Keyboard Hook Integration & Tray App Context

**Files:**
- Create: `src/Ule4Jis.Net/KeyboardHook.cs`
- Create: `src/Ule4Jis.Net/TrayApplicationContext.cs`
- Modify: `src/Ule4Jis.Net/Program.cs`

- [ ] **Step 1: Implement KeyboardHook class**

Set up `WH_KEYBOARD_LL` hook, route key events through `AltImeSwitcher` and `UsOnJisMapper`.

- [ ] **Step 2: Implement TrayApplicationContext class**

Create system tray icon (`NotifyIcon`), context menu (Toggle Emulation, Toggle Alt-IME, Startup Registry Toggle, Exit).

- [ ] **Step 3: Update Program.cs**

Run `Application.Run(new TrayApplicationContext())`.

- [ ] **Step 4: Build & Publish Release EXE**

Run: `dotnet publish src/Ule4Jis.Net -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true`
Expected: Single executable published in `src/Ule4Jis.Net/bin/Release/net9.0-windows/win-x64/publish/Ule4Jis.Net.exe`.

- [ ] **Step 5: Commit**

```bash
git add src/Ule4Jis.Net
git commit -m "feat: integrate KeyboardHook and TrayApplicationContext, complete ULE4JIS + Alt-IME app"
```
