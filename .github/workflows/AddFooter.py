import os
import json
import re


def add_footer(html_file_path):
    with open(html_file_path, 'r', encoding='utf-8') as f:
        html = f.read()

    # フッターが存在しなければ、追加
    footer_pattern = r'<footer[^>]*>(.*?)<\/footer>'
    footer_match = re.search(footer_pattern, html, re.DOTALL)
    if footer_match is None:
        # フッターのHTMLを追加
        html = html.replace('</html>', get_footer() + '</html>')

        # スタイルシートを追加
        css = '''
        <style>
            /* フッターのスタイル */
            footer {
                position: fixed;
                left: 0;
                bottom: 0;
                width: 100%;
                background-color: #f5f5f5;
                color: #333;
                text-align: center;
                padding: 10px 0;
            }

            /* ページの本文をフッターの上に表示するためのスタイル */
            body {
                margin-bottom: 60px;
            }
        </style>
        '''
        html = html.replace('</head>', css + '</head>')

    else:
        pattern = r'<footer[\s\S]*?</footer>'
        # 置換できたかどうかを返す
        if re.search(pattern, html):
            html = re.sub(pattern, get_footer(), html)
        else:
            print("置換できませんでした。")

    with open(html_file_path, 'w', encoding='utf-8') as f:
        f.write(html)


def get_footer():
    footer_html = f'''
    <footer style="display: flex; justify-content: space-between; align-items: center;">
        <a href="../" style="margin-left: 10px;">ホームへ戻る</a>
        <a style="margin-right: 10px; margin-left: auto"> 最終更新 {get_last_updated()} </a>
    </footer>
    '''
    return footer_html


def get_last_updated():
    with open("build/WebGL/WebGL/repo_info.json", "r", encoding="utf-8") as f:
        data = json.load(f)
    last_updated = data["last_updated"]
    return last_updated


if __name__ == "__main__":
    add_footer("build/WebGL/WebGL/index.html")
