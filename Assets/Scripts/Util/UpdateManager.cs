using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UniRx;
using UniRx.Triggers;

public class UpdateManager : MonoBehaviour
{
    private static Subject<Unit> updateEvent = new Subject<Unit>();
    private void Start()
    {
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                updateEvent.OnNext(Unit.Default);
            });
    }

    public static IDisposable Subscribe(Action<Unit> onNext)
    {
        return updateEvent.Subscribe(_ => onNext.Invoke(Unit.Default));
    }
}
