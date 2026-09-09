# ULE4JIS-ALT

日本語配列 (JIS) キーボード設定の Windows 上で、外付け英語 (US) 配列キーボードを物理印字通りに打鍵できるようにする Windows 用ユーティリティです。  
`ULE4JIS` の設計思想をリスペクトし、.NET 9 で再構築しました。

---

## 🌟 主な特徴

1. **JIS配列キーボードのUS配列化 (ULE4JIS エミュレーション)**
   - OS側のキーボード設定が「JIS配列」のままでも、外付けUSキーボードの印字通りに記号入力を行えるよう変換します。

2. **キーボードデバイス自動識別**
   - 入力された最初の1文字目で「ノートPC本体のJISキーボード」か「外付けUSキーボード」かを取得します。
   - 外付けUSキーボードを打鍵すると変換エミュレーションが自動適用され、ノートPC本体のキーボードを打鍵すると一切変換せずスルーされます。

3. **左右 Alt 空打ちによるスマート IME 切り替え (macOS風)**
   - **左 Alt の空打ち**: 無変換キー (`VK_NONCONVERT`) を送信し、英語入力 (IME OFF) に切り替え。変換中ならカタカナ変換。
   - **右 Alt の空打ち**: 直接 IME ON 信号を送信し、日本語入力に切り替え。
   - `Alt + Tab` や `Alt + F4` などのショートカット操作時は通常の Alt として動作するため邪魔になりません。

4. **管理者権限アプリでも無効化されない自動起動 (UIPI回避)**
   - 管理者権限で起動したアプリケーションでキー入力が効かなくなる問題（Windows の UIPI 仕様）を回避。
   - トレイメニューから **「Windows起動時に自動起動 (管理者権限)」** を選択するだけで、タスクスケジューラの最上位特権タスクを自動作成・管理します。

---

## ⚙️ 事前設定 (Microsoft IME)

左右 Alt 空打ちによる IME 切り替え（macOS風）を正しく動作させるには、Windows の IME 設定でキーの割り当てを変更してください。

1. タスクバー右下の IME アイコン（「あ」/「A」）を右クリック ➔ **「設定」** を選択
2. **「キーとタッチのカスタマイズ」** をクリック
3. **「キーの割り当て」** を **オン** に変更
4. 以下のようにキーを設定：
   - **無変換キー**: `IME-オフ`
   - **変換キー**: `IME-オン`

---

## 🚀 使い方 / ダウンロード

1. **[最新バージョンのダウンロード (ULE4JIS-ALT.exe)](https://raw.githubusercontent.com/mana2p/ULE4JIS-ALT/master/publish/ULE4JIS-ALT.exe)**
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

- **ULE4JIS-ALT**: Copyright (c) 2026 ManatsuP
- **オリジナル ULE4JIS**: Copyright (c) 2009 DEZZ Networks
