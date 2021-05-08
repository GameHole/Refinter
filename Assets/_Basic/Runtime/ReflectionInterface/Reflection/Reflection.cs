using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refinter
{
    public class Reflection : MonoBehaviour
    {
        internal static readonly Dictionary<Type, IInterface> interfaces = new Dictionary<Type, IInterface>();
        public static Dictionary<Type, IInterface> Interfaces => interfaces;
        static Queue<IInitializable> initializables = new Queue<IInitializable>();
        static HashSet<Type> injected = new HashSet<Type>();
        static HashSet<MonoBehaviour> monoInjected = new HashSet<MonoBehaviour>();
        public bool debug;
        public bool isInjectScene = true;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InjectSence();
            if (isInjectScene)
            {
                SceneManager.sceneLoaded += (a, b) =>
                {
                    InjectSence();
                };
            }
        }
        public static T Get<T>()where T: IInterface
        {
            interfaces.TryGetValue(typeof(T), out var v);
            return (T)v;
        }
        public static void Set<T>(IInterface inst)where T : IInterface
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException($"type = {typeof(T)} is not a interface");
            if (inst == null)
                throw new NullReferenceException($"inst is null");
            if(!interfaces.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"type = {typeof(T)} is not found in interface list");
            }
            interfaces[typeof(T)] = inst;
            ReInjectInterface(typeof(T), inst);
        }
        void InjectSence()
        {
            Stopwatch stopwatch = null;
            if (debug)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            ClearNullObj();
            foreach (var assembly in ReflectEx.workAssemblies)
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (item == typeof(IInterface) || interfaces.ContainsKey(item)) continue;
                    if (typeof(IInterface).IsAssignableFrom(item) && item.IsInterface)
                    {
                        var instence = ReflectEx.Instance(item) as IInterface;
                        //UnityEngine.Debug.Log($"{item},{instence}");
                        if (instence != null)
                        {
                            interfaces.Add(item, instence);
                            var init = (instence as IInitializable);
                            if (init != null)
                            {
                                //UnityEngine.Debug.Log($"find init {init}");
                                initializables.Enqueue(init);
                            }
                        }
                    }
                }
            }
          
            
            foreach (var obj in interfaces)
            {
                if (injected.Contains(obj.Key)) continue;
                injected.Add(obj.Key);
                foreach (var inter in interfaces.Values)
                {
                    ReflectEx.Inject(obj.Value, inter);
                }
            }
            if (isInjectScene)
            {
                foreach (var obj in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
                {
                    if (!obj.gameObject.scene.IsValid() || obj.GetType().FullName.Contains("UnityEngine")) continue;
                    if (monoInjected.Contains(obj)) continue;
                    monoInjected.Add(obj);
                    Inject(obj);
                    //Debug.Log(obj);
                }
            }
            while (initializables.Count > 0)
            {
                initializables.Dequeue().Initialize();
            }

            if (debug)
            {
                stopwatch.Stop();
                UnityEngine.Debug.Log(stopwatch.Elapsed);
            }
        }
        static void ReInjectInterface(Type type,IInterface inter)
        {
            foreach (var item in injected)
            {
                ReflectEx.Inject(interfaces[item], inter);
            }
            foreach (var item in monoInjected)
            {
                ReflectEx.Inject(item, inter);
            }
        }
        public static void Inject(MonoBehaviour mono)
        {
            foreach (var item in interfaces.Values)
            {
                ReflectEx.Inject(mono, item);
            }
        }
        void ClearNullObj()
        {
            List<Type> removed = new List<Type>();
            foreach (var item in interfaces.Keys)
            {   
                var v = interfaces[item];
                //由于mono destroy后并不会立即置为null，故需要使用is判断是不是mono脚本
                if (v is MonoBehaviour)
                {
                    //Debug.Log($"contain::key::{item},v::{v},isnull::{v == null}");
                    //而 mono destroy 后使用as转换得到的结果却是null，因此转换后判断即可知道脚本是否被destroy
                    if ((v as MonoBehaviour)==null)
                    {
                        removed.Add(item);
                    }
                }
            }
            foreach (var item in removed)
            {
                interfaces.Remove(item);
            }
            HashSet<MonoBehaviour> injected = new HashSet<MonoBehaviour>();
            foreach (var item in monoInjected)
            {
                if (item)
                {
                    injected.Add(item);
                }
            }
            monoInjected = injected;
        }
    }

}
