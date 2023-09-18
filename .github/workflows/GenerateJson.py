import os
import json
from datetime import datetime

import pytz


def main():
    # リポジトリ情報を収集する
    product_name = os.environ.get("PRODUCT_NAME")
    repo_name = os.environ.get("GITHUB_REPOSITORY")
    version = os.environ.get("UNITY_VERSION")

    # GitHub ActionsのタイムゾーンはUTCなので、日本時間に変換する
    last_updated = datetime.now(pytz.timezone(
        'Asia/Tokyo')).strftime("%Y/%m/%d %H:%M")

    # リポジトリ情報を辞書として保存する
    repo_info = {
        "name": product_name,
        "description": repo_name,
        "version": version,
        "last_updated": last_updated
    }

    # リポジトリ情報をJSONファイルに保存する
    file_path = "build/WebGL/WebGL/repo_info.json"
    with open(file_path, "w") as f:
        json.dump(repo_info, f)


if __name__ == "__main__":
    main()
