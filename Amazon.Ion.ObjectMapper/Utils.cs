using System;
using System.Reflection;
using Amazon.IonDotnet;

namespace Amazon.Ion.ObjectMapper
{
    internal static class Utils
    {
        private const BindingFlags BINDINGS = BindingFlags.NonPublic | BindingFlags.Static;

        internal static bool IsSerializerValid(Type type, dynamic serializer)
        {
            return (bool)InvokeGenericMethod(nameof(GenericIsSerializerValid), type, new [] { serializer });
        }
        
        internal static bool GenericIsSerializerValid<T>(dynamic serializer)
        {
            return serializer is IonSerializer<T>;
        }
        
        internal static void Serialize(Type type, dynamic serializer, IIonWriter writer, object item)
        {
            InvokeGenericMethod(nameof(GenericSerialize), type, new [] { serializer, writer, item });
        }

        internal static void GenericSerialize<T>(dynamic serializer, IIonWriter writer, T item)
        {
            serializer.Serialize(writer, item);
        }

        private static object InvokeGenericMethod(string methodName, Type type, object[] arguments)
        {
            MethodInfo method = typeof(Utils).GetMethod(methodName, BINDINGS);
            if (method == null)
            {
                throw new NotSupportedException($"Method {methodName} not found in class Utils");
            }

            MethodInfo genericMethod = method.MakeGenericMethod(type);
            return genericMethod.Invoke(null, arguments);
        }
    }
}
