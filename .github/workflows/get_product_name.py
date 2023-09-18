import sys
import re


def get_product_name(file_path):
    # YAMLファイルの読み込み
    with open(file_path, 'r') as f:
        contents = f.read()

    # productNameの文字列を取得
    product_name = ""

    pattern = r'productName:\s+(\S+)\n'
    match = re.search(pattern, contents)
    if match:
        product_name = match.group(1)
        # もし、"で囲まれていたら、"を削除
        if product_name.startswith('"') and product_name.endswith('"'):
            product_name = product_name[1:-1]

    # Unicodeエスケープシーケンスをデコードして文字列に変換
    decoded_product_name = bytes(
        product_name, 'utf-8').decode('unicode_escape')

    # 結果を返す
    return decoded_product_name


if __name__ == "__main__":
    # コマンドライン引数を取得
    args = sys.argv
    # 引数があれば、実行
    if len(args) > 1:
        product_name = get_product_name(args[1])
        sys.stdout.write(product_name)
