# サーバー、アプリケーション、IoTサイド、回路の説明
## サーバーサイド
IoTデバイスからの震度、湿度、窓の開閉情報をユーザーごとにデータベースで管理している。
窓開閉時にはFirebase Cloud Messagingというサービスでプッシュ通知を出す。
ユーザーのパスワードは暗号化（bcryptアルゴリズムでハッシュ化）されて保存されているので安全。
サーバーはGo言語で書かれている

## アプリケーションサイド
iOSとAndroidに対応。プッシュ通知は現在iOSのみに対応している。
ユーザーの登録またはログインをすると、そのユーザーと紐付いているIoTデバイスの管理と震度、温度、窓情報の取得が可能
アプリはC#とXamarinで書かれている。

## Iotサイド
ユーザーのログイン処理等はPython側だが、データの送信処理はC#で書かれている。


## モーター駆動用回路
2つの入力端子があり、PWM信号を入力することで回転方向の制御が可能。デューティ比によって速度を制御
ラズパイから大電流は流せないので外部から直流電源を入力して、PWM信号を増幅させることで動作する

### サーバーの認証に関する簡単な説明
ユーザー名とパスワードでログインに成功すると、サーバーから認証トークンが発行される。
アプリ側はこのトークンを使用してデータをやり取りすることで、パスワードをログイン時の一度しか送信する必要が無くなるので安全性が高まる。
また、仮に不正ログイン等が起きてもサーバーが認証トークンを無効化することで悪意ある攻撃を遮断することもできる