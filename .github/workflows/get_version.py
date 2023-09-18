import re
import sys


def get_editor_version(file_path):
    with open(file_path, 'r') as f:
        contents = f.read()

    pattern = r'm_EditorVersion:\s+(\S+)\n'
    match = re.search(pattern, contents)
    if match:
        version = match.group(1)
        return version
    else:
        return None


if __name__ == "__main__":
    # コマンドライン引数を取得
    args = sys.argv
    # 引数があれば、実行
    if len(args) > 1:
        version = get_editor_version(args[1])
        sys.stdout.write(version)
