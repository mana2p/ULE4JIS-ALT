# ULE4JIS + Alt-IME for .NET 9 Design Document

## 1. 概要 (Overview)
Windows の JIS (日本語) キーボード環境において、US (英語) キーボード配列の入力をエミュレートする機能（ULE4JIS 互換）と、左右 Alt キーの空打ち（単体押し）による IME ON/OFF 切り替え機能（alt-ime-ahk 互換）を統合した、超軽量な .NET 9 常駐アプリケーションを作成する。

## 2. システム構成 (Architecture)
- **ターゲットフレームワーク**: `.NET 9.0 (Windows / WinForms / タスクトレイ常駐)`
- **主要モジュール**:
  1. `Program.cs` / `TrayApplicationContext.cs`: タスクトレイ常駐（通知アイコン、コンテキストメニュー、スタートアップ登録設定、終了処理）。
  2. `KeyboardHook.cs`: Windows Low-Level Keyboard Hook (`SetWindowsHookEx(WH_KEYBOARD_LL)`) によるグローバルキー監視・フックモジュール。
  3. `UsOnJisMapper.cs`: ULE4JIS 互換の JIS->US キーマッピングエンジン。
  4. `AltImeSwitcher.cs`: 左右 Alt キーの押し下げ・離されたタイミングを検知し、単体空打ち時に IME ON / IME OFF 命令を送信するモジュール。

## 3. 機能詳細 (Detailed Features)

### 3.1. ULE4JIS 互換マッピング (US Layout Emulation)
Low-Level キーフックにより、指定キー押下時に本来のキー入力をキャンセル (`return 1`) し、`SendInput` API で US 配列に対応する仮想キーコード / Shift 状態に変換して送信する。

マッピングテーブル例:
- `Shift + 2` -> `@` (`VK_OEM_3`)
- `Shift + 6` -> `^` (`VK_OEM_7`)
- `Shift + 7` -> `&` (`Shift + '6'`)
- `Shift + 8` -> `*` (`Shift + VK_OEM_1`)
- `Shift + 9` -> `(` (`Shift + '8'`)
- `Shift + 0` -> `)` (`Shift + '9'`)
- `Shift + -` -> `_` (`Shift + VK_OEM_102`)
- `=` (`^`キー) -> `Shift + VK_OEM_MINUS`
- `+` (`Shift + ^`キー) -> `Shift + VK_OEM_PLUS`
- `` ` `` (`半角/全角`) -> `Shift + VK_OEM_3`
- `~` (`Shift + 半角/全角`) -> `Shift + VK_OEM_7`
- `[` (`@`キー) -> `VK_OEM_4`
- `]` (`[`キー) -> `VK_OEM_6`
- `{` (`Shift + @`キー) -> `Shift + VK_OEM_4`
- `}` (`Shift + [`キー) -> `Shift + VK_OEM_6`
- `:` (`Shift + ;`キー) -> `VK_OEM_1`
- `'` (`:`キー) -> `Shift + '7'`
- `"` (`Shift + :`キー) -> `Shift + '2'`
- `\` (`]`キー) -> `VK_OEM_102`
- `|` (`Shift + ]`キー) -> `Shift + VK_OEM_5`

### 3.2. 左右 Alt キー空打ちによる IME 切り替え (Alt-IME Switching)
- **左 Alt (`VK_LMENU` / `0xA4`)**:
  - `WM_KEYDOWN` / `WM_SYSKEYDOWN`: 「左Alt押下」フラグをオン。
  - Alt 押下中に他のキーが押された場合: 「他キー組み合わせ」フラグをオン。
  - `WM_KEYUP` / `WM_SYSKEYUP`: 他キー組み合わせフラグがオフなら「空打ち」と判断。`VK_IME_OFF` (0x1A) または `VK_KANJI` 等のイベントを発行して IME を OFF (英数) に変更。
- **右 Alt (`VK_RMENU` / `0xA5`)**:
  - 左 Alt 同様、単体で離された場合は `VK_IME_ON` (0x16) または `VK_KANJI` 等のイベントを発行して IME を ON (かな) に変更。

### 3.3. タスクトレイ & UI (Tray App)
- タスクトレイ常駐アイコンを表示。
- コンテキストメニュー項目:
  - **エミュレーション有効 / 無効 (Toggle)**
  - **Alt-IME有効 / 無効 (Toggle)**
  - **Windows起動時に自動起動 (Startup Checkbox)**: レジストリ (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) への登録 / 解除。
  - **終了 (Exit)**

## 4. ビルド & 配信手順 (Build & Delivery)
- `dotnet publish -c Release -r win-x64 --self-contained false`
- 単一の軽量 `.exe` ファイルまたはフォルダ構成としてビルド。
