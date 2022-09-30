using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


// bool Near(this Vector3, Vector3 other, float distance)
// bool NearXZ(this Vector3, Vector3 other, float distance)
//
//
//
//
// T            FindParent<T>       (this GameObject) where T : Component
// GameObject   RecursiveFindChild  (this GameObject, string childName)
// List<T>      FindChildren<T>     (this GameObject, bool getInactive)
// T            FindChild<T>        (this GameObject, bool getInactive, Predicate<T> fn)
// GameObject   FindChild           (this GameObject, bool getInactive, Predicate<GameObject> fn)
//
// TraverseChildren     (this GameObject, bool recursive, Action<GameObject> fn)
// Traverse             (this GameObject, Action<GameObject> fn)
// Traverse<T>          (this GameObject, Action<T> fn)

public static class Util
{
    public static string GetNamePath(this GameObject go)
    {
        string s = "";
        while (go != null)
        {
            s = "/" + go.name + s;
            go = go.transform.parent?.gameObject;
        }
        return s;
    }
    public static bool GameObjectExists(GameObject gameObject)
    {
        try
        {
            if (gameObject == null) { }
        }
        catch
        {
            return false;
        }
        return true;
    }
    public static bool stillExists(this MonoBehaviour m)
    {
        return GameObjectExists(m.gameObject);
    }
    public static bool Near(this Vector3 origin, Vector3 other, float distance)
    {
        return (origin - other).sqrMagnitude <= distance * distance;
    }
    public static bool NearXZ(this Vector3 origin, Vector3 other, float distance)
    {
        float dx = other.x - origin.x;
        float dz = other.z - origin.z;
        return dx * dx + dz * dz < distance * distance;
    }
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
    public static Vector3 Change(this Vector3 org, float x = Single.PositiveInfinity, float y = Single.PositiveInfinity, float z = Single.PositiveInfinity)
    {
        Vector3 newPosition = new Vector3(
            x == Single.PositiveInfinity ? org.x : x,
            y == Single.PositiveInfinity ? org.y : y,
            z == Single.PositiveInfinity ? org.z : z
        );
        return newPosition;
    }
    public static void Toggle(this ParticleSystem pfx, bool state)
    {
        var em = pfx.emission;
        em.enabled = state;
    }
    public static void TogglePfx(this GameObject gameObject, bool state)
    {
        gameObject.GetComponent<ParticleSystem>().Toggle(state);
    }
    public static void Move(this Vector3 position, float dx, float dy, float dz)
    {
        position.x += dx;
        position.y += dy;
        position.z += dz;
    }
    public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
    public static void Periodic(this MonoBehaviour m, float frequency, int iterations, Action actionFn)
    {
        IEnumerator ExecutePeriodic()
        {
            while (iterations > 0)
            {
                yield return new WaitForSeconds(frequency);
                actionFn();
                --iterations;
            }
        }
        m.StartCoroutine(ExecutePeriodic());
    }
    public static void UntilDone(this MonoBehaviour m, float frequency, Func<bool> fn)
    {
        IEnumerator ExecuteUntilDone()
        {
            float timer = 0;
            float reps = 10000;
            bool done = false;
            do
            {
                timer += Time.deltaTime;
                if (timer >= frequency)
                {
                    done = fn();
                    timer -= frequency;
                }
                yield return null;
            } while (!done && --reps > 0);
        }
        m.StartCoroutine(ExecuteUntilDone());
    }
    public static void Defer(this MonoBehaviour m, float duration, Action actionFn)
    {
        IEnumerator ExecuteAfterTime(float time, Action actionFn)
        {
            yield return new WaitForSeconds(time);
            actionFn();
        }
        m.StartCoroutine(ExecuteAfterTime(duration, actionFn));
    }

    //
    // GameObject Overloads
    //
    public static T FindParent<T>(this GameObject source) where T : Component
    {
        if (source.GetComponent<T>() != null)
            return source.GetComponent<T>();

        Transform parent = source.transform.parent;
        while (parent != null)
        {
            if (parent.gameObject.GetComponent<T>() != null)
                return parent.gameObject.GetComponent<T>();
            parent = parent.parent;
        }
        return null;
    }


    public static GameObject RecursiveFindChild(this GameObject parent, string childName)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }
            else
            {
                GameObject found = child.gameObject.RecursiveFindChild(childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }
    public static T FindChild<T>(this GameObject source, bool getInactive, Predicate<T> fn = null) where T : Component
    {
        Component[] raw = source.GetComponentsInChildren(typeof(T), getInactive);
        List<T> list = new List<T>();
        foreach (var item in raw)
        {
            if (fn == null || fn(item as T))
                return item as T;
        }
        return null;
    }
    public static GameObject FindChild(this GameObject source, bool getInactive, Predicate<GameObject> fn)
    {
        Component[] raw = source.GetComponentsInChildren(typeof(Transform), getInactive);
        foreach (var item in raw)
        {
            if (fn(item.gameObject))
                return item.gameObject;
        }
        return null;
    }
    public static List<T> FindChildren<T>(this GameObject source, bool getInactive) where T : Component
    {
        Component[] raw = source.GetComponentsInChildren(typeof(T), getInactive);
        List<T> list = new List<T>();
        foreach (var item in raw)
        {
            list.Add(item as T);
        }
        return list;
    }

    public static void TraverseChildren<T>(this GameObject source, bool getInactive, Action<T> fn) where T : Component
    {
        Component[] raw = source.GetComponentsInChildren(typeof(T), getInactive);
        foreach (var item in raw)
        {
            fn(item as T);
        }
    }

    public static void TraverseChildren(this GameObject gameObject, bool recursive, Action<GameObject> fn)
    {
        foreach (Transform child in gameObject.transform)
        {
            fn(child.gameObject);
            if (recursive)
                TraverseChildren(child.gameObject, true, fn);
        }
    }
    public static void Traverse(this GameObject gameObject, Action<GameObject> fn)
    {
        fn(gameObject);
        gameObject.TraverseChildren(true, fn);
    }
    public static void Traverse<T>(this GameObject gameObject, Action<T> fn) where T : Component
    {
        Component[] list = gameObject.GetComponents(typeof(T));
        foreach (Component c in list)
        {
            fn((T)c);
        }
    }
    public static void FilterInPlace<T>(this List<T> list, Predicate<T> testFn)
    {
        list.RemoveAll((item) => !testFn(item));
    }
    public static List<T> Filter<T>(this List<T> list, Predicate<T> testFn)
    {
        List<T> result = new List<T>();
        foreach (T item in list)
            if (testFn(item))
                result.Add(item);
        return result;
    }
    public static List<GameObject> Filter(this List<GameObject> list, Predicate<GameObject> testFn)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (GameObject item in list)
            if (testFn(item))
                result.Add(item);
        return result;
    }
    public static void Shuffle<T>(System.Random rng, IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static float PerlinNoise3D(float x, float y, float z)
    {
        y += 1;
        z += 2;
        float xy = _perlin3DFixed(x, y);
        float xz = _perlin3DFixed(x, z);
        float yz = _perlin3DFixed(y, z);
        float yx = _perlin3DFixed(y, x);
        float zx = _perlin3DFixed(z, x);
        float zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }
    public static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}