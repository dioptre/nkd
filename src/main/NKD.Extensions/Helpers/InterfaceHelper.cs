using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Dynamic;
using System.Threading;
using System.Collections.Concurrent;

namespace NKD.Helpers
{
    public class InterfaceHelper
    {
        public static class ClassBuilder
        {
            private static readonly ConcurrentDictionary<string, Type> _interfaceTypeCache = new ConcurrentDictionary<string, Type>();

            public static Type Build(Type interfaceType)
            {
                try
                {
                    Type type;
                    if (_interfaceTypeCache.TryGetValue(interfaceType.FullName, out type))
                        return type;
                    AssemblyName assemblyName = new AssemblyName("NKD.Helpers.Interfaces");
                    AssemblyBuilder assemBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                    ModuleBuilder moduleBuilder = assemBuilder.DefineDynamicModule("NKDDataBuilderModule", true);
                    TypeBuilder typeBuilder = moduleBuilder.DefineType("NKD.Helpers.Interfaces." + interfaceType.Name, TypeAttributes.Class | TypeAttributes.Public);
                    //typeBuilder.AddInterfaceImplementation(interfaceType);
                    BuildProperty(typeBuilder, "Value", interfaceType);                    
                    type = typeBuilder.CreateType();
                    return _interfaceTypeCache.GetOrAdd(interfaceType.FullName, type); 
                }
                catch
                {
                    return null;
                }

            }

            private static void BuildProperty(TypeBuilder typeBuilder, string name, Type type)
            {
                FieldBuilder field = typeBuilder.DefineField("m" + name, type, FieldAttributes.Private);
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(name, PropertyAttributes.None, type, null);

                MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

                MethodBuilder getter = typeBuilder.DefineMethod("get_" + name, getSetAttr, type, Type.EmptyTypes);

                ILGenerator getIL = getter.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, field);
                getIL.Emit(OpCodes.Ret);

                MethodBuilder setter = typeBuilder.DefineMethod("set_" + name, getSetAttr, null, new Type[] { type });

                ILGenerator setIL = setter.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, field);
                setIL.Emit(OpCodes.Ret);


                propertyBuilder.SetGetMethod(getter);
                propertyBuilder.SetSetMethod(setter);
            }
        }
    }
}