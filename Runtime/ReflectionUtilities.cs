using System;
using System.Collections.Generic;
using System.Linq;

namespace CLabs.Utility {
    public static class ReflectionUtilities {
        public static IEnumerable<Type> FindImplementors(this Type type) {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type.IsAssignableFrom);
        }
        
        public static IEnumerable<Type> GetConstructorParams(this Type type) {
            return type
                .GetConstructors()[0]
                .GetParameters()
                .Select(param => param.ParameterType);
        }

        public static IEnumerable<Type> SelectInterfaces(this IEnumerable<Type> types) 
            => types.Where(type => type.IsInterface);
        
        public static string ConvertType(string type)
        {
            return type switch
            {
                "System.Boolean" => "bool",
                "System.String" => "string",
                "System.Object" => "object",
                "System.Byte" => "byte",
                "System.SByte" => "sbyte",
                "System.Char" => "char",
                "System.Decimal" => "decimal",
                "System.Double" => "double",
                "System.Single" => "float",
                "System.Int32" => "int",
                "System.UInt32" => "uint",
                "System.IntPtr" => "nint",
                "System.UIntPtr" => "nuint",
                "System.Int64" => "long",
                "System.UInt64" => "ulong",
                "System.Int16" => "short",
                "System.UInt16" => "ushort",
                _ => type
            };
        }
    }
}