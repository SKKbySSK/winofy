# Winofy(仮)のサーバー&クライアント（アプリ）サイド用リポジトリ

## サーバーサイド
サーバーサイドではCentOS + nginx + mariaDBの環境でGo言語のサーバーをリバースプロキシして構築しています。
なお、通知の送信にはFirebase Cloud Messagingを用いています

APIは基本的にログイン時に発行される認証トークン（以下トークン）を用いて認証して通信します。
サーバーからのレスポンスは全てjson形式です。表のGETやPOST等にある名前がjsonのプロパティ名です

リクエストヘッダとリクエストボディのContent-Typeはapplication/x-www-form-urlencodedです。

サーバーアドレスは以下のアドレスです
 - https://4eiot.ksprogram.work/

 ## APIエンドポイント

| アドレス（相対） | 説明 | リクエストヘッダ | リクエストボディ | GET | POST | PUT |
|:-----------|:------------|:------------|:------------|:------------|:------------|:------------|
| internal/register | 新規アカウントを登録します |  | username, password |  |  | success, messages |
| internal/login | トークンを発行します |  | username, password |  | success, token |  |
| internal/notification | 通知の有効無効の設定 | token | device_token, type |  | | success |
| devices/list | アカウントに紐づいているデバイス一覧を取得します | token |  | devices |  |  |
| devices/register | デバイスを新規登録し、ユーザーと紐付けます | token | device_id, name, *description |  |  | success, message |
| devices/device | デバイスの詳細情報を取得します | token |  | 未定 |  |  |
| devices/record | 震度、温度、湿度を記録します。必要に応じてアプリに通知が送信されます | token | device_id, axes, SI, temp, humidity |  | httpステータスコード |  |

\*:オプションの値（与えなくても良い）

| パラメータ名 | 説明 |
|:-----------|:------------|
| username | ユーザー名 |
| password | 暗号化されていないパスワード |
| success | ブール型で成功か失敗かを表す |
| messages | RegisterMessage文字列の配列で、登録結果の詳細を知らせる |
| token | 認証が必要な操作で使用するトークン文字列 |
| device_token | アプリごとに用意される通知用トークン |
| type | 通知タイプ(0=無効, 1=APNs(未対応), 2=FCM(実装済)) |
| devices | Device型の配列 |
| device_id | 地震や温度、湿度を検知するIoTデバイス自身のID |
| name | IoTデバイスのユーザーが付ける名前 |
| description | IoTデバイスのユーザーが付ける詳細情報 |
| message | DeviceRegisterMessage文字列で、IoTデバイスの登録結果の詳細を知らせる |
| axes | SI値の算出に使用した軸情報(0=YZ, 1=XZ, 2=XY) |
| SI | SI値 |
| temp | 温度(摂氏) |
| humidity | 湿度(0~1) |

## RegisterMessage
| パラメータ名 | 説明 |
|:-----------|:------------|
| Unknown |  不明な結果で失敗 |
| Ok | 成功 |
| InvalidRequest | 無効なリクエスト。パラメータ不足等 |
| UserExists | 既に同名のユーザーが存在しているため失敗 |
| IncorrectUsernameFormat | ユーザー名のフォーマットが正しくない（文字数等） |
| IncorrectPasswordFormat | パスワードのフォーマットが正しくない（文字数等） |

## DeviceRegisterMessage
| パラメータ名 | 説明 |
|:-----------|:------------|
| Unknown |  不明な結果で失敗 |
| Ok | 成功 |
| Unauthorized | ユーザー認証に失敗したため、登録に失敗（ヘッダにtokenを与えたか確認してください） |
| DeviceExists | 既に同名のデバイスIDが存在しているため失敗 |

## Device
| パラメータ名 | 説明 |
|:-----------|:------------|
| name | デバイス名 |
| description | デバイス詳細情報 |

## サーバー詳細
Azure Virtual MachinesのStandard B1sプランで構築。

RAM 1GB, vCPU 1コアなのであまりAPI叩きすぎないでください。お願いします。

## クライアントサイド
Xamarin.Formsを用いて、iOS, Androidのクロスプラットフォームに対応しています。アプリ上で通知の有効無効を切り替える機能を搭載します。　