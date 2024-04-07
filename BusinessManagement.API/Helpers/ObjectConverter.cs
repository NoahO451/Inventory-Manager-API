using System.Reflection.Emit;
using System.Reflection;

namespace App.Helpers
{
    /// <summary>
    /// Class for experimenting with reflection toautomatically flatten any object passed in. Does not currently work. 
    /// </summary>
    public static class ObjectConverter
    {
        public static object ConvertToAnonymousObject<T>(T obj)
        {
            var anonymousObjectProperties = new List<PropertyInfo>();

            // Get properties of the object
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // If property is a nested object, recursively flatten it
                if (prop.PropertyType.Namespace != typeof(string).Namespace)
                {
                    PropertyInfo[] nestedProperties = prop.PropertyType.GetProperties();
                    anonymousObjectProperties.AddRange(nestedProperties);
                }
                else // Otherwise, add property to the anonymous object properties list
                {
                    anonymousObjectProperties.Add(prop);
                }
            }

            // Create an anonymous type dynamically
            Type anonymousType = CreateType(anonymousObjectProperties.ToArray());

            // Create an instance of the anonymous type and set its properties
            object anonymousObject = Activator.CreateInstance(anonymousType);
            foreach (var prop in anonymousType.GetProperties())
            {
                PropertyInfo sourceProp = properties.FirstOrDefault(p => p.Name == prop.Name);
                if (sourceProp != null)
                {
                    object value = sourceProp.GetValue(obj);
                    prop.SetValue(anonymousObject, value);
                }
            }

            return anonymousObject;
        }

        public static Type CreateType(params PropertyInfo[] properties)
        {
            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var typeBuilder = moduleBuilder.DefineType("AnonymousType", TypeAttributes.Public);

            foreach (var propInfo in properties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(propInfo.Name, PropertyAttributes.HasDefault, propInfo.PropertyType, null);
                var fieldBuilder = typeBuilder.DefineField("_" + propInfo.Name, propInfo.PropertyType, FieldAttributes.Private);
                var getMethodBuilder = typeBuilder.DefineMethod("get_" + propInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propInfo.PropertyType, Type.EmptyTypes);
                var getIL = getMethodBuilder.GetILGenerator();

                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, fieldBuilder);
                getIL.Emit(OpCodes.Ret);

                var setMethodBuilder = typeBuilder.DefineMethod("set_" + propInfo.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propInfo.PropertyType });
                var setIL = setMethodBuilder.GetILGenerator();

                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, fieldBuilder);
                setIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getMethodBuilder);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder.CreateType();
        }
    }
}
