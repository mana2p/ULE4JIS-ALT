# ULE4JIS + Alt-IME (.NET 9 Edition)

JIS (日本語) キーボードで US (英語) 配列の入力をエミュレートする **ULE4JIS** をモダンな **.NET 9 (C#)** へ完全移植・再構築し、さらに左右 Alt キーの単体押し（空打ち）での IME 切り替え（**alt-ime-ahk 互換**）や **CapsLock カスタマイズ機能 (sgk/ULE4JIS 互換)**、そして **レトロなドット絵（ピクセルアート）キーキャップアイコン** を追加した統合ツールです。

---

## ✨ 主な特徴 (Features)

1. **JISキーボードでUS配列入力を再現 (ULE4JIS 互換)**
   - `Shift + 2` で `@`、`Shift + 6` で `^`、`Shift + 7` で `&` など、JISキーボードのままUSキーボード配列と同じ打鍵感で入力できます。
   - `[` `]` `{` `}` `;` `:` `'` `"` `\` `|` `` ` `` `~` の全マッピングに対応。

2. **左右 Alt キーの空打ちで IME ON/OFF 切り替え (alt-ime-ahk 互換)**
   - **左 Alt キーの単体押し (空打ち)** → **英語入力 (IME OFF)** に切り替え
   - **右 Alt キーの単体押し (空打ち)** → **日本語入力 (IME ON)** に切り替え
   - ※ `Alt + Tab` や `Alt + F4` などのショートカット操作時は通常の Alt キーとして動作し、IME切り替えは発動しません。
   - `WM_IME_CONTROL` メッセージ通信により、あらゆる Windows アプリケーション（ブラウザ、エディタ、ターミナル等）で高い互換性と安定性を実現。

3. **CapsLock キーの動作カスタマイズ & 自動消灯 (sgk/ULE4JIS 互換)**
   - **CapsLock 単体押しで IME トグル切り替え**: CapsLock キーを押すたびに IME の ON / OFF を交互に切り替え可能。
   - **CapsLock LED 自動解除**: アプリ起動時に間違えて CapsLock がON（点灯）になっていた場合、自動的に消灯・解除します。
   - **動作モードの切り替え**: トレイメニューから「IMEトグル切り替え」と「無効（通常のCapsLock）」をいつでも切り替えられます。

4. **ドット絵（ピクセルアート）キーキャップアイコン & 常駐機能**
   - タスクトレイ（16x16ピクセル）の限られた領域でも一目で状態が分かる、レトロでかわいいドット絵キーキャップ風アイコンを採用！
   - **有効 (ON)**: 鮮やかなクラシックブルーのピクセル「U」キーキャップ
   - **一時停止 (OFF)**: シックなダークグレーのピクセルキーキャップ
   - 機能の有効/無効に応じてトレイアイコンがリアルタイムに切り替わります。

---

## 🔄 オリジナル (dezz/ULE4JIS) との主な違い

| 項目 | オリジナル (dezz/ULE4JIS) | 本プロジェクト (ULE4JIS + Alt-IME) |
| :--- | :--- | :--- |
| **開発言語 / FW** | C++ (MFC / Boost ライブラリ依存) | **C# (.NET 9.0 / WinForms / P/Invoke)** |
| **ビルド環境** | Visual Studio 2005/2008 (古いC++環境が必要) | **.NET 9.0 SDK (`dotnet build` / `dotnet publish` で一発ビルド可能)** |
| **配布形態** | 複数DLL・依存ファイルあり | **約160KBの超軽量・単一実行ファイル (`.exe`)** |
| **Altキー切り替え** | なし | **あり (左Alt: IME OFF / 右Alt: IME ON)** |
| **CapsLock制御** | なし | **あり (IMEトグル切り替え & 起動時LED自動消灯)** |
| **アイコンデザイン** | 平坦な青枠アイコン | **ドット絵（ピクセルアート）キーキャップアイコン (16x16最適化)** |
| **自動起動設定** | 手動でスタートアップフォルダへ配置 | **トレイメニューからワンクリックでレジストリ登録可能** |

---

## 🛠️ ビルド方法 (How to Build)

### 必須環境
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Windows x64)

### コンパイル手順
プロジェクトルートディレクトリで以下のコマンドを実行します。

```powershell
# デバッグビルド
dotnet build src/Ule4Jis.Net

# リリース用単一実行ファイル (.exe) のパブリッシュ
dotnet publish src/Ule4Jis.Net -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

パブリッシュされた実行ファイルは以下に出力されます：
`src/Ule4Jis.Net/bin/Release/net9.0-windows/win-x64/publish/Ule4Jis.Net.exe`

---

## 🙏 クレジット・謝辞 (Credits & Acknowledgments)

- **オリジナル ULE4JIS**: [dezz/ULE4JIS](https://github.com/dezz/ULE4JIS)
- **Alt IME 切り替えロジック**: [karakaram/alt-ime-ahk](https://github.com/karakaram/alt-ime-ahk)
- **CapsLock 機能拡張の参考**: [sgk/ULE4JIS](https://github.com/sgk/ULE4JIS)
