# VRChatのログ解析

## それぞれのイベントで発生するログの種類

|イベント|ログの種類|
|---|---|
|ワールドへのJoin| `[Behaviour] Joining <ロケーションID>` |
|| `[Behaviour] Joining or Creating Room: <ワールド名>` |
|プレイヤーのJoin| `[Behaviour] Initialized PlayerAPI "<プレイヤー名>" is <remote or local>`|
|プレイヤーのLeft| `[Behaviour] Unregistering <プレイヤー名>` |

## 解析上の注意

- プレイヤーのLeftは自分のログが一番最初に出力されるため，このログをワールドのLeftとして扱う場合，ワールドのLeftより後に他のプレイヤーのLeftログが発生する

search-ms:displayname=検索場所%3A%202022-05&crumb=日付：(>%3D2022%2F05%2F06%2015%3A00%3A00%20<2022%2F05%2F21%2015%3A00%3A00)&crumb=location:D%3A%5Crio%5CPictures%5CVRChat%5C2022-05
