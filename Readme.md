# Winofy(仮)のサーバー&クライアント（アプリ）サイド用リポジトリ

## サーバーサイド
サーバーサイドではCentOS + nginx + mariaDBの環境でGo言語のサーバーをリバースプロキシして構築しています。
なお、通知の送信にはFirebase Cloud Messagingを用いています

APIは基本的にログイン時に発行される認証トークン（以下トークン）を用いて認証して通信します。
サーバーからのレスポンスは全てjson形式です。表のGETやPOST等にある名前がjsonのプロパティ名です

リクエストヘッダとリクエストボディのContent-Typeはapplication/x-www-form-urlencodedです。

サーバーアドレスは以下のアドレスです
 - https://4eiot.ksprogram.work/

| アドレス（相対） | 説明 | リクエストヘッダ | リクエストボディ | GET | POST | PUT |
|:-----------|:------------|:------------|:------------|:------------|:------------|:------------|
| internal/register | 新規アカウントを登録します |  | username, password |  |  | success, messages |
| internal/login | トークンを発行します |  | username, password |  | success, token |  |
| internal/notification | デバイスによるPOSTで震度や温度の送信、アプリのPUTで通知の有効無効の設定 | token | 未定 |  | 未定 | 未定 |
| devices/list | アカウントに紐づいているデバイス一覧を取得します | token |  | devices |  |  |
| devices/register | デバイスを新規登録し、ユーザーと紐付けます | token | device_id, name, *description |  |  | success, message |
| devices/device | デバイスの詳細情報を取得します | token |  | 未定 |  |  |

\*:オプションの値（与えなくても良い）

### サーバー
Azure Virtual MachinesのStandard B1sプランで構築。

RAM 1GB, vCPU 1コアなのであまりAPI叩きすぎないでください。お願いします。

## クライアントサイド
Xamarin.Formsを用いて、iOS, Androidのクロスプラットフォームに対応しています。アプリ上で通知の有効無効を切り替える機能を搭載します。　