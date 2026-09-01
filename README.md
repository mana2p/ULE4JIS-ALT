# ULE4JIS

Windowsのキーボード設定を **JIS (日本語配列)** にしたまま、物理的な **US (英語配列)** キーボードでの入力を快適に行うための常駐アプリケーションです。

名作ツールであるオリジナル [dezz/ULE4JIS](https://github.com/dezz/ULE4JIS) に深いリスペクトを捧げ、モダンな **.NET 9 (C#)** 環境へ再構築するとともに、左右AltキーによるワンタッチIME切替やカタカナ変換、CapsLockの柔軟な制御機能を統合しました。

---

## ✨ 主な機能 (Features)

### 1. JIS設定のままUS配列入力を再現 (ULE4JIS)
- `Shift + 2` で `@`、`Shift + 6` で `^`、`Shift + 7` で `&` など、JIS設定のままUSキーボードの印字通りのキー入力を実現します。
- `` ` `` `~` `!` `@` `#` `$` `%` `^` `&` `*` `(` `)` `_` `+` `=` `[` `]` `{` `}` `;` `:` `'` `"` `\` `|` の全キーマッピングに対応しています。

### 2. 左右 Alt 空打ちによるスマートな IME / カタカナ変換
- **右 Alt キーの単体押し (空打ち)**
  - 強制的に **日本語入力 (IME ON)** に切り替えます。
- **左 Alt キーの単体押し (空打ち)**
  - **未確定文字の入力中**: 無変換キー (`VK_NONCONVERT`) として機能し、一発で **全角/半角カタカナ変換** を行います。
  - **何も入力していない状態**: 強制的に **英語入力 (IME OFF)** に切り替えます。

### 3. CapsLock キーの長押し対応 & 自動消灯
- **短押し (0.5秒未満)**
  - **IMEのトグル切り替え** (ON / OFF) として軽快に動作します。
- **長押し (0.5秒以上)**
  - 本来の **CapsLock 機能 (大文字固定 ON/OFF)** が発動します。
- **起動時自動消灯**
  - アプリ起動時、CapsLock LED が点灯している場合は自動的に解除・消灯します。

### 4. ドット絵（ピクセルアート）キーキャップアイコン
- タスクトレイ（16x16ピクセル）およびアプリケーションアイコンに、レトロで親しみやすい **ドット絵キーキャップ風アイコン** を採用。
- **有効 (ON)**: クラシックブルーのピクセル「U」キーキャップ
- **一時停止 (OFF)**: シックなダークグレーのピクセルキーキャップ

---

## 🛠️ ビルドと実行 (How to Build & Run)

### 必須環境
- [Windows 10 / 11 (x64)](https://www.microsoft.com/windows)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### ビルド手順
リポジトリ直下で以下のコマンドを実行することで、単一の実行ファイル (`.exe`) を生成できます。

```powershell
# リリース用単一実行ファイルのパブリッシュ
dotnet publish src/Ule4Jis.Net -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

出力先:
`src/Ule4Jis.Net/bin/Release/net9.0-windows/win-x64/publish/Ule4Jis.Net.exe`

---

## 🙏 クレジット・謝辞 (Credits & Acknowledgments)

本プロジェクトは、以下の素晴らしいオープンソースソフトウェアおよびアイデアに強くインスパイアされ、感謝とともに開発されました。

- **オリジナルの ULE4JIS 開発者様**: [dezz/ULE4JIS](https://github.com/dezz/ULE4JIS)
  - JIS設定下でUS配列入力を実現する素晴らしい構想とマッピングロジックの原点です。
- **Alt IME 切り替えのアイデア**: [karakaram/alt-ime-ahk](https://github.com/karakaram/alt-ime-ahk)
  - 左右Altキーの空打ちによる直感的なIME切替スタイルの原点です。
- **CapsLock 拡張のアイデア**: [sgk/ULE4JIS](https://github.com/sgk/ULE4JIS)
  - CapsLockキーへの機能割り当てとLED制御のアイデアの原点です。
