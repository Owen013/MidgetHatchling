using System.Reflection;
using System;

namespace ScaleManipulator;

public static class Extensions
{
    public static void RaiseEvent<T>(this T instance, string eventName, params object[] args)
    {
        const BindingFlags flags = BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;
        if (typeof(T)
                .GetField(eventName, flags)?
                .GetValue(instance) is not MulticastDelegate multiDelegate)
        {
            return;
        }

        multiDelegate.DynamicInvoke(args);
    }
}