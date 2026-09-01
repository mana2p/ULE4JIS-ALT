# ULE4JIS-ALT

オリジナル版 [dezz/ULE4JIS](https://github.com/dezz/ULE4JIS) を、最新の **.NET 9 (C#)** 環境へ移植・再構築したアプリケーションです。

Windowsのキーボード設定を **JIS (日本語配列)** にしたまま **US (英語配列)** キーボードを入力・エミュレートするオリジナル機能に加え、**左右Altキーの単体押しによるIME切り替え・カタカナ変換機能** を追加しています。

---

## ⚡ 主な特徴 (Features)

- **.NET 9 (C#) での再構築**
  - 最新の .NET 9 環境に対応し、約160KBの超軽量・単一実行ファイル (`.exe`) として動作します。
- **JIS設定のままUS配列入力を再現 (ULE4JIS)**
  - `Shift + 2` で `@`、`Shift + 6` で `^` など、JISキーボード設定のままUSキーボードの印字通りに入力できます。
- **左右 Alt 空打ちによる IME 切り替え & カタカナ変換 (alt-ime-ahk 互換)**
  - **右 Alt キー単押し**: 日本語入力 (IME ON)
  - **左 Alt キー単押し**: 入力中は **無変換（全角/半角カタカナ変換）**、未入力時は **英語入力 (IME OFF)**
- **CapsLock の挙動拡張**
  - 短押しで **IMEトグル**、0.5秒長押しで **本来のCapsLock (大文字固定)** として動作。起動時の CapsLock LED 自動消灯にも対応。
- **ドット絵キーキャップアイコン**
  - タスクトレイおよび実行ファイルアイコンにドット絵キーキャップデザインを採用。

---

## 🛠️ ビルド手順 (How to Build)

### 必須環境
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Windows x64)

```powershell
# 単一実行ファイルのパブリッシュ
dotnet publish src/Ule4Jis.Net -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

出力先:
`src/Ule4Jis.Net/bin/Release/net9.0-windows/win-x64/publish/Ule4Jis.Net.exe`

---

## 🙏 クレジット (Credits)

- **オリジナル C++ 版**: [dezz/ULE4JIS](https://github.com/dezz/ULE4JIS)
- **Alt IME 切り替え**: [karakaram/alt-ime-ahk](https://github.com/karakaram/alt-ime-ahk)
