using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineReturnData
{
    public Coroutine coroutine
    {
        get;
        private set;
    }
    public object result;
    public IEnumerator target;
    public bool running;

    public CoroutineReturnData(MonoBehaviour routineOwner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = routineOwner.StartCoroutine(RunCoroutine());
        running = true;
    }

    private IEnumerator RunCoroutine()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
        running = false;
    }
}
