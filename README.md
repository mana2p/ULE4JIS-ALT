# ULE4JIS-ALT

日本語配列 (JIS) キーボード設定の Windows 上で、外付け英語 (US) 配列キーボードを物理印字通りに打鍵できるようにする Windows 用ユーティリティです。  
名作ユーティリティ `ULE4JIS` の設計思想をリスペクトし、現代の **.NET 9** で完全再構築しました。

---

## 🌟 主な特徴

1. **JIS配列キーボードのUS配列化 (ULE4JIS エミュレーション)**
   - OS側のキーボード設定が「JIS配列」のままでも、外付けUSキーボードの印字通りに `@` `[` `]` `:` `^` `_` などの記号入力を行えるよう変換します。
   - `Shift` との組み合わせ記号（`@` `&` `*` `(` `)` `+` `:` `"` など）もUS配列通りに完璧にシミュレートします。

2. **キーボードデバイス自動識別 (内蔵JIS / 外付けUS ハイブリッド対応)** 🚀 *NEW*
   - Win32 Raw Input API を使用し、キーが叩かれた瞬間に「ノートPC本体のJISキーボード」か「自席の外付けUSキーボード」かを1ミリ秒単位で識別！
   - 自席で外付けUSキーボードを打鍵している時は自動でUS変換エミュレーションを適用し、ノートPC本体で作業する時は一切変換せずそのまま素通り（スルー）させます。

3. **左右 Alt 空打ちによるスマート IME 切り替え (macOS風)**
   - **左 Alt の空打ち**: 無変換キー (`VK_NONCONVERT`) を送信し、文字入力中はカタカナ変換、未入力時は英語入力 (IME OFF) へ変換。
   - **右 Alt の空打ち**: 日本語入力 (IME ON) へ変換。
   - `Alt + Tab` や `Alt + F4` などのショートカット操作時は通常の Alt として動作するため邪魔になりません。

4. **管理者権限アプリでも無効化されない完全自動起動 (UIPI完全回避)**
   - 管理者権限で起動した Visual Studio や Terminal / PowerShell でもキー入力が効かなくなる問題（Windows の UIPI 仕様）を全自動で回避。
   - トレイメニューから **「Windows起動時に自動起動 (管理者権限)」** を選択するだけで、タスクスケジューラの最上位特権タスクを自動作成・管理します。

---

## 🚀 使い方 / ダウンロード

1. **[最新バージョンのダウンロード (ULE4JIS-ALT.exe)](https://raw.githubusercontent.com/ntakeshitgcom/ULE4JIS/master/publish/ULE4JIS-ALT.exe)**
2. ダウンロードした `ULE4JIS-ALT.exe` を実行するだけで、タスクバーの通知領域（トレイアイコン）に常駐します。
3. トレイアイコンを右クリックすることで、各機能の ON / OFF 切り替えや自動識別、Windows 起動時の自動起動を設定できます。

---

## 🛠️ 開発・ビルド環境

- **言語 / 構成**: C# (.NET 9.0 Windows Forms / Single File Executable)
- **ビルドコマンド**:
  ```powershell
  dotnet publish src/Ule4JisAlt -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
  ```

---

## 📜 クレジット

- **オリジナル ULE4JIS**: Copyright (c) 2010 sgk
