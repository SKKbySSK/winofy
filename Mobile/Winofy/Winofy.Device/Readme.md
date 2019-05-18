# Winofy Client Device Application
Winofyのアカウント作成、デバイス登録、震度・温度送信を行うことのできるWindows, Linux, macOS対応のアプリケーションです。

## 使い方
1. ビルドするか、実行可能ファイル(winofyまたはwinofy.exe)をダウンロードして適当なフォルダに保存してください
2. ターミナルやコマンドプロンプト等で保存したフォルダに移動してください
3. winofy(winofy.exe) \<Parameters\>を環境と用途に合わせて呼び出してください

## 初期設定
ここではwinofyコマンドとして扱いますが、Windowsではwinofy.exeと読み替えてください。コマンドを実行すると#と書かれている行が表示されます
1. アカウントの新規作成
``` shell
winofy create
# New Account
# Username : ユーザー名を入力（3文字以上）
# Password : パスワードを入力 (3文字以上、文字は表示されません)
```
2. デバイスの新規作成
``` shell
winofy init -u ユーザー名 -p パスワード
# Name : デバイスの名前を入力
# Description (optional) : デバイスの詳細情報を入力
# Json successfully exported to device.json
```

3. デバイスをアカウントに紐づけ
``` shell
winofy register -u ユーザー名 -p パスワード
# Device was successfully registered
```

4. アカウントにデバイスが登録されているかの確認
``` shell
winofy list -u ユーザー名 -p パスワード
# ----------
# ID:自動生成されたデバイスごとに固有のID
# Name:入力したデバイス名
# Description:入力したデバイスの詳細情報
# ----------
```

4で作成したデバイスが表示されれば成功です。もし手順の途中で何か変なメッセージが出た場合には、既に登録されたユーザー名の可能性があったりユーザー名やパスワードが短すぎる可能性があるので確認してください。

それでも解決しなければLineやSlackで連絡ください。

## プログラムからの使用
既に初期設定が済んだものとして、サーバーへ震度・温度・湿度情報を送信する方法を書きます。
1. 認証トークンの取得（1度保存すれば使いまわせます)
``` shell
winofy token -u ユーザー名 -p パスワード
# 認証トークン または 失敗メッセージ
```
2. 送信
``` shell
winofy record -SI SI値 -axes SI値の軸情報 -temp 温度 (摂氏) -humidity 湿度 (0~1) -token 認証トークン
# OK
```

## ビルド（コンパイル）
### 始める前に

- ビルドには .Net Core 2.1のSDKが必要です。https://dotnet.microsoft.com/download/dotnet-core/2.1 よりダウンロードしてインストールしてください
- 場合によってdotnetコマンドの実行にパスを通す必要があるので、Windowsでは環境変数に、Linux等ではexportコマンドなどを使用してください。
- 詳しくはググってね

### 手順

1. このリポジトリをクローンまたはダウンロードしてください。
2. ターミナルやコマンドプロンプト等でこのディレクトリ（Winofy.Deviceフォルダ)に移動してください
3. 以下のコマンドを実行してください
``` shell
 dotnet publish .\Winofy.Device.csproj --self-contained -r <Platform> -c release
```

 - \<Platform\>には以下の値を環境に合わせて入力してください。詳しくは、https://docs.microsoft.com/ja-jp/dotnet/core/rid-catalog を参照してください

 | OS | 値 |
 |:---|:---|
 | Linux ARM (Raspberry Pi等) | linux-arm |
 | Windows 64bit | win-x64 |
 | macOS (Sierra以上) | osx-x64 |
 | Linux 64bit (Ubuntu等) | linux-x64 |

 4. おそらく、bin/release/netcoreapp2.1/\<Platform\>/publish/にwinofy (Windowsではwinofy.exe)というファイルが完成しています
 5. 後はこのファイルをターミナル等から実行してください（前のセクションを参照)