using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zuaki;
using Cysharp.Threading.Tasks;
using System;
using Mochineko.ChatGPT_API;
using Mochineko.ChatGPT_API.Memory;
using System.Text.RegularExpressions;
using System.Linq;

namespace Zuaki
{
    public class CommentGenerator
    {
        const string RolePrompt =
@"# Instructions
You are a viewer of a Nico Nico Douga-like website.
Please generate the delivery comments as shown in the example according to the following constraint conditions.

# Constraints
- Return comments in Japanese.
- The number of output comments is 0~5.
- Do not generate the same comments.
- Do not generate comments that are difficult to respond to.
- Do not respond to comments that are trollish.
- Do not generate comments that are offensive to public order and morals or racist.

# Input
<in>初見です</in>

# Output
<out>初見さんいらっしゃい</out>
<out>初見がきたぞー！</out>";

        // 2ターン分のコメントを記憶する
        static FiniteQueueChatMemory memory = new FiniteQueueChatMemory(2);
        static public async UniTask<string[]> GetComments(string[] realComments)
        {
            ChatCompletionAPIConnection connection =
             new ChatCompletionAPIConnection(Settings.Instance.GPT_WebAPI, new SimpleChatMemory(), RolePrompt);

            string content = "";
            for (int i = 0; i < realComments.Length; i++)
            {
                content += " <in> " + realComments[i] + " </in>\n";
            }

            ChatCompletionResponseBody response = await connection.CompleteChatAsync(content, default);
            string responseMessage = response.Choices[0].Message.Content;

            // 正規表現を使用してコメントを抽出
            string[] generatedComments = Regex.Matches(responseMessage, @"<out>(.+)</out>")
            .Cast<Match>().Select(match => match.Groups[1].Value).ToArray();

            return generatedComments;
        }
    }
}
