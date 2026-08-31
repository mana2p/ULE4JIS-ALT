# ULE4JIS + Alt-IME (.NET 9 Edition)

JIS (日本語) キーボードで US (英語) 配列の入力をエミュレートする **ULE4JIS** をモダンな **.NET 9 (C#)** へ完全移植・再構築し、さらに左右 Alt キーの単体押し（空打ち）で IME の OFF/ON を切り替える機能（**alt-ime-ahk 互換**）を追加した統合ツールです。

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

3. **超軽量＆タスクトレイ常駐**
   - 画面を持たないタスクトレイ常駐型アプリケーションです。
   - トレイアイコンの右クリックメニューから各機能の ON/OFF 切り替えや「Windows起動時の自動起動」を簡単に設定できます。

---

## 🔄 オリジナル (dezz/ULE4JIS) との主な違い

| 項目 | オリジナル (dezz/ULE4JIS) | 本プロジェクト (ULE4JIS + Alt-IME) |
| :--- | :--- | :--- |
| **開発言語 / FW** | C++ (MFC / Boost ライブラリ依存) | **C# (.NET 9.0 / WinForms / P/Invoke)** |
| **ビルド環境** | Visual Studio 2005/2008 (古いC++環境が必要) | **.NET 9.0 SDK (`dotnet build` / `dotnet publish` で一発ビルド可能)** |
| **配布形態** | 複数DLL・依存ファイルあり | **約160KBの超軽量・単一実行ファイル (`.exe`)** |
| **Altキー切り替え** | なし | **あり (左Alt: IME OFF / 右Alt: IME ON)** |
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
- **Alt IME 切り替えロジックの参照元**: [karakaram/alt-ime-ahk](https://github.com/karakaram/alt-ime-ahk)
