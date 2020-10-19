using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Refinter;
public static class MonoEx
{
    public static T Instantiate<T>(this T prefab) where T : MonoBehaviour
    {
        var clone = Object.Instantiate(prefab);
        InjectPredab(clone);
        return clone;
    }
    public static T Instantiate<T>(this T prefab, Transform parent) where T : MonoBehaviour
    {
        var clone = Object.Instantiate(prefab, parent);
        InjectPredab(clone);
        return clone;
    }
    public static T Instantiate<T>(this T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour
    {
        var clone = Object.Instantiate(prefab, position, rotation, parent);
        InjectPredab(clone);
        return clone;
    }
    static void InjectPredab(MonoBehaviour mono)
    {
        foreach (var item in mono.GetComponentsInChildren<MonoBehaviour>(true))
        {
            Reflection.Inject(item);
        }
    }
    public static Coroutine Wait(this MonoBehaviour mono, float time, System.Action action)
    {
        return mono.StartCoroutine(itrWait(time, action));
    }
    static IEnumerator itrWait(float v, System.Action action)
    {
        yield return new WaitForSeconds(v);
        action?.Invoke();
    }
    public static string formatTimeStr(this int v)
    {
        return v < 10 ? "0" + v : v.ToString();
    }
    public static void Remove<T>(this GameObject game)where T : MonoBehaviour
    {
        var runner = game.GetComponent<T>();
        if (runner)
            Object.Destroy(runner);
    }
    public static void Remove<T>(this MonoBehaviour game) where T : MonoBehaviour
    {
        var runner = game.GetComponent<T>();
        if (runner)
            Object.Destroy(runner);
    }
}