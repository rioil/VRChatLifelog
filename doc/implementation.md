# 各機能の実装の概要

## ログファイルのリアルタイム解析

> [!Note]
> 主な実装場所
> <https://github.com/rioil/VRChatLifelog/blob/master/VRChatLifelog/Models/VRChatLogWatcher.cs>

### ファイルシステムの変更通知の購読

VRChat のログファイルが作成されたことを検出するために、ファイルシステムの変更通知イベントを購読しています。イベントの購読には、[FileSystemWatcher](https://learn.microsoft.com/ja-jp/dotnet/api/system.io.filesystemwatcher?view=net-8.0)クラスを使用しています

### ログファイルを開く際の権限

ログファイルは、他のプロセスによる読み書きと削除を許可する設定で開きます。これにより、VRChat のプロセスとファイルアクセスの権限が衝突することを防ぎます。VRChat は古いログファイルを自動削除するため、読み書きだけでなく、削除も許可する必要があります。

### リアルタイムに追記されるログの解析

EOF に達するまでログの各行を読み取って解析します。一度 EOF に達すると、「一定時間待機して追記された内容があれば読み取り」という処理を繰り返して、リアルタイムに追記されるログを解析していきます。

VRChat のログファイルは、VRChat のプロセスが起動される毎に新しいファイルが作成されます。あるログファイルを使用していた VRChat のプロセスが終了していれば、そのファイルに新しくログが出力されることは無いため、読み取り処理を終了します。ログファイルを使用している VRChat のプロセスの情報は、Windows の[再起動マネージャー](https://learn.microsoft.com/ja-jp/windows/win32/rstmgr/about-restart-manager)を使用して、ファイルをロックしているプロセスを取得することで行います。

## 特定の場所で撮影した写真の表示

> [!Note]
> 主な実装場所
> <https://github.com/rioil/VRChatLifelog/blob/master/VRChatLifelog/Utils/SearchMsUtil.cs>

写真の一覧表示は Windows Search の機能を用います。
指定された絞り込み条件に一致する訪問履歴の入退出時刻から検索クエリを作成して、条件に一致する画像をエクスプローラー上で表示します。
