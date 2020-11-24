using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Foundation.Utils.ReflectionUtils
{
    public static class ReflectionUtils
    {
        
        private static readonly AssemblyTypes AssemblyTypes = new AssemblyTypes();

        /// <summary>
        /// Get all concrete classes types that inherit from the given type or interface
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static List<Type> GetConcreteDerivedTypes<T>() where T : class
        {
            Type baseType = typeof(T);
            // Get all types from the given assembly
            Type[] types = AssemblyTypes.GetAssemblyTypes();
            List<Type> derivedTypes = new List<Type>();
    
            // Iterate over all types & check if it's concrete sub-class or implementing the given interface
            for (int i = 0, count = types.Length; i < count; i++)
            {
                Type type = types[i];
                
                // Ignore the given class, abstracts or interfaces
                if (baseType == type || type.IsAbstract || type.IsInterface) { continue; }

                bool isSubClass = type.IsSubclassOf(baseType);
                bool isImplementingInterface = type.IsClass && type.GetInterface(baseType.FullName) != null;
                if (isSubClass || isImplementingInterface)
                {
                    derivedTypes.Add(type);
                }
            }
            
            return derivedTypes;
        }

        /// <summary>
        /// Returns a list of classes instances of the given base type
        /// Note: supports only classes constructors with primitive types (or optional value)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetDerivedInstancesOfType<T>() where T : class
        {
            List<Type> types = GetConcreteDerivedTypes<T>();
            return CreateInstancesFromTypes<T>(types);
        }
        
        /// <summary>
        /// Creates instance from each of the given types
        /// Abstracts & Interfaces will be ignored...
        /// </summary>
        /// <param name="types"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> CreateInstancesFromTypes<T>(List<Type> types) where T : class
        {
            
            List<T> classes = new List<T>();
            if (types != null)
            {
                foreach (Type type in types)
                {
                    T classObject = CreateInstanceFromType<T>(type);
                    if (classObject != null) classes.Add(classObject);
                }
            }

            return classes;
        }
        
        /// <summary>
        /// Create a class object (instance) from the given type
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstanceFromType<T>(Type type) where T : class
        {
            
            if (type.IsAbstract || type.IsInterface) return null;

            bool instanceCreated = false;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;
            ConstructorInfo[] constructors = type.GetConstructors(flags);

            // Iterate through all constructors until object is created successfully (or no supported constructors were found)
            for (int i = 0; i < constructors.Length && !instanceCreated; i++)
            {
                ConstructorInfo ci = constructors[i];
                ParameterInfo[] constructorParams = ci.GetParameters();
                object[] args = new object[constructorParams.Length];
                for (int j = 0; j < constructorParams.Length; j++)
                {
                    ParameterInfo pi = constructorParams[j];
                    //Log(pi.Name + " type: " + pi.ParameterType.ToString() + " is optional: " + pi.IsOptional.ToString());

                    // Note: accepts nulls (invoke constructor will take care of that if needed)
                    object o = GetParameterInfoAsGenericObject(pi);
                    args[j] = o;
                }

                try
                {
                    T classObject = ci.Invoke(args) as T;
                    if (classObject != null)
                    {
                        //Debug.Log("Reflection Utils: " + "Successfully created type: " + type.Name);
                        instanceCreated = true;
                        return classObject;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Reflection Utils: " + "------------------ Exception creating type: " +
                                         type.Name + " exception: " + e.Message);
                    Debug.Log("Reflection Utils: " + "Failed creating type: " + type.Name +
                              "\nIt's probably because it contains un-supported parameter type in the constructor without default value." +
                              "\nConsider making it as optional value similar to obj = null or enum = defaultEnumValue");
                }
            }

            return null;
        }
        
        /// <summary>
        /// A simple mapper from parameter info to respective object type with a default value.
        /// If the parameter includes a optional value it will be used, otherwise just use the type's default value
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        private static object GetParameterInfoAsGenericObject(ParameterInfo parameterInfo)
        {
            object value = null;
            if (parameterInfo.IsOptional && parameterInfo.DefaultValue != null)
            {
                value = parameterInfo.DefaultValue;
            }
            else if (parameterInfo.ParameterType.IsValueType)
            {
                value = Activator.CreateInstance(parameterInfo.ParameterType);
            }
            return value;
        }
    }
    

    /// <summary>
    /// Access all types in the assembly and caching when needed
    /// </summary>
    internal class AssemblyTypes
    {
        private Type[] _types;
        private readonly List<string> _assemblies;

        internal AssemblyTypes()
        {
            _assemblies = new List<string>();
            _assemblies.Add("Assembly-CSharp.dll");
            _assemblies.Add("AssemblyFoundation.dll");
            if(Application.isEditor) { _assemblies.Add("Assembly-CSharp-Editor.dll"); }
        }
        
        /// <summary>
        /// Returns all types from the executing assembly
        /// Result is only cached when running in batch mode or on actual device because
        /// on editor it may change between calls
        /// </summary>
        internal Type[] GetAssemblyTypes()
        {
            // Loads assembly types only if it is the first time, or it's a normal call on the editor (not during build)
            if (_types != null && Application.isBatchMode) { return _types; }
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                // Ignore irrelevant assemblies
                if (!_assemblies.Contains(assembly.ManifestModule.Name)) { continue; }
                // Get all types from this assembly
                Type[] assemblyTypes = assembly.GetTypes();
                foreach (Type type in assemblyTypes) { types.Add(type); }
            }

            _types = types.ToArray();
            return _types;
        }
    }
}
