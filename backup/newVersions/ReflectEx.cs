using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
namespace Refinter
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple =false,Inherited =false)]
    public class ImportantAttribute : Attribute
    {
        internal int index;
        public ImportantAttribute(int index)
        {
            this.index = index;
        }
    }
    public static class ReflectEx
    {
        static Assembly[] _workAssemblies;
        public static Assembly[] workAssemblies
        {
            get
            {
                if (_workAssemblies == null)
                    LoadCSharp();
                return _workAssemblies;
            }
        }
        public static void Inject(object obj, object inter)
        {
            if (inter == null) return;
            var itrType = inter.GetType();
            foreach (var item in obj.GetType().GetFields(BindingFlags.Instance| BindingFlags.Public| BindingFlags.NonPublic))
            {
                if (item.FieldType.IsInterface && typeof(IInterface).IsAssignableFrom(item.FieldType) && item.FieldType.IsAssignableFrom(itrType))
                {
                    item.SetValue(obj, inter);
                }
            }

        }
        public static T Instance<T>()where T:class,IInterface
        {
            return Instance(typeof(T)) as T;
        }
        public static int GetIndex(Type type)
        {
            var att = type.GetCustomAttribute<ImportantAttribute>();
            if (att != null)
                return att.index;
            return 0;
        }
        public static object Instance(Type type)
        {
            var find = FindImpl(type);
            find.Sort((a,b) =>
            {
                return Compare(b, a);// GetIndex(b) - GetIndex(a);
            });
            //foreach (var item in find)
            //{
            //    Debug.Log($"find::{item}");
            //}
            for (int i = 0; i < find.Count; i++)
            {
                var f = find[i];
                if (find[i].IsSubclassOf(typeof(MonoBehaviour)))
                {
                    var m = FindIMonoImpl(f);
                    if (m != null)
                        return m;
                }
                else
                {
                    return Activator.CreateInstance(f);
                }
            }
            
            return null;
        }
        public static int Compare(Type a, Type b)
        {
            return GetIndex(a) - GetIndex(b);
        }
        public static object FindIMonoImpl(Type type)
        {
            var f = Resources.FindObjectsOfTypeAll(type);
            foreach (var item in f)
            {
                if ((item as MonoBehaviour).gameObject.scene.IsValid())
                    return item;
            }
            return null;
        }
        public static List<Type> FindImpl(Type inter)
        {
            List<Type> finds = new List<Type>();
            foreach (var assembly in workAssemblies)
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (isIgnore(item)) continue;
                    if (inter.IsAssignableFrom(item))
                        finds.Add(item);
                }
            }
            return finds;
        }
        static void LoadCSharp()
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (isIgnoredAssembliy(assembly)) continue;
                assemblies.Add(assembly);
            }
            _workAssemblies = assemblies.ToArray();
        }
        static string[] ignoreStr = new string[]
        {
            "mscorlib","UnityEngine","UnityEngine.","UnityEditor","UnityEditor.","Newtonsoft.","SyntaxTree.","ExCSS.","Microsoft.","Unity.","System","System.","Mono.","nunit.","netstandard"
        };
        static bool isIgnoredAssembliy(Assembly assembly)
        {
            for (int i = 0; i < ignoreStr.Length; i++)
            {
                if (assembly.FullName.Contains(ignoreStr[i]))
                    return true;
            }
            return false;
        }
        static bool isIgnore(Type type)
        {
            return type.IsInterface || type.IsAbstract || type.IsGenericType;
        }
    }
}
