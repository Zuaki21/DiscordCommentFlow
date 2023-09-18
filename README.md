# Unityプロジェクト用テンプレート

私がゲーム制作にサッと取り掛かるためのテンプレートプロジェクトです。

このリポジトリを複製してからゲーム制作することをお勧めします。

## 機能

- 自動ビルド機能
- 自動デプロイ機能 ([公開先](https://github.com/Zuaki21/develop))

## 新しくプロジェクトとして始める手順

1. "Use This template" → "Create a new repository" と選択
1. リポジトリ名はプロジェクト名として好きに決める
1. Github Actionsの"Acquire activation file"ワークフローを実行し、alfを取得([参考](https://zenn.dev/nikaera/articles/unity-gameci-github-actions))
1. [ここ](https://license.unity3d.com/)でUnity License のアクティベーションを行いulfファイルをダウンロードする(PersonalEdition想定でワークフロー書いてるので、PersonalEditionを選ぶ)
1. リポジトリ内のSettingへ移動、シークレットキーとして"UNITY_LICENSE"にulfファイルの中身をペーストする
1. [ここ](https://github.com/settings/tokens)でトークンを生成(Select scopesの"workflow","write:packages"にチェックを入れる)してGithubActionsのシークレットキーに設定する
1. リポジトリ内のSettingへ移動、シークレットキーとして"PERSONAL_ACCESS_TOKEN"にトークンをペーストする
1. ローカルにcloneしてUnityHubから選択して開く
1. UnityEditorのPlayerSettingsでProductNameを入力する(公開ページでの表示名になります)
1. あとはMainにPushしたら自動ビルドされます！素敵なゲー制ライフを！
