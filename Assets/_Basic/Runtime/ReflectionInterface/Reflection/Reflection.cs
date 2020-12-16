﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refinter
{
    public class Reflection : MonoBehaviour
    {
        internal static readonly Dictionary<Type, IInterface> interfaces = new Dictionary<Type, IInterface>();
        static Dictionary<Type, bool> injected = new Dictionary<Type, bool>();
        static Dictionary<MonoBehaviour, bool> monoInjected = new Dictionary<MonoBehaviour, bool>();
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InjectSence();
            SceneManager.sceneLoaded += (a,b)=>
            {
                InjectSence();
            };
        }
        void InjectSence()
        {
            ClearNullObj();
            foreach (var assembly in ReflectEx.workAssemblies)
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (item == typeof(IInterface) || interfaces.ContainsKey(item)) continue;
                    if (typeof(IInterface).IsAssignableFrom(item) && item.IsInterface)
                    {
                        var instence = ReflectEx.Instance(item) as IInterface;
                        //Debug.Log($"{item},{instence}");
                        if (instence != null)
                            interfaces.Add(item, instence);
                    }
                }
            }
            
            foreach (var obj in interfaces)
            {
                if (injected.ContainsKey(obj.Key)) continue;
                injected.Add(obj.Key, true);
                foreach (var inter in interfaces.Values)
                {
                    ReflectEx.Inject(obj.Value, inter);
                }
            }
            foreach (var obj in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (!obj.gameObject.scene.IsValid() || obj.GetType().FullName.Contains("UnityEngine")) continue;
                if (monoInjected.ContainsKey(obj)) continue;
                monoInjected.Add(obj, true);
                Inject(obj);
                //Debug.Log(obj);
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
        }
    }

}