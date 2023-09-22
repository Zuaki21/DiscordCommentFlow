using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
// using AngleSharp;
// using AngleSharp.Dom;
using Zuaki;
//AngleSharp.dll必須
//AngleSharp.Js.dll Jint.dll必須

namespace Zuaki
{
    // public class ScrapingSampleJs : MonoBehaviour
    // {
    //     public string uri;

    //     void Start()
    //     {
    //         CheckURL();
    //     }

    //     async void CheckURL()
    //     {
    //         var doc = await Parce();
    //         Debug.Log(doc.Title);
    //         //本文を取得
    //         var body = doc.Body;
    //         Debug.Log(body.TextContent);

    //     }

    //     async UniTask<IDocument> Parce()
    //     {
    //         // WithJs()で、JavaScriptを有効 動作してるか不明
    //         var config = Configuration.Default.WithDefaultLoader().WithJs();
    //         var context = BrowsingContext.New(config);
    //         return await context.OpenAsync(uri);
    //     }
    // }
}
