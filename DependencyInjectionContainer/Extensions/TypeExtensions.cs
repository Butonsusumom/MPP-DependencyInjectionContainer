using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionContainer.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsAssignableFromAsOpenGeneric(this Type type, Type c)
        {
            if (!type.IsGenericTypeDefinition || !c.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Specified types should be generic");
            }

            Type comparedType, baseType;

            Queue<Type> baseTypes = new Queue<Type>();
            baseTypes.Enqueue(c);

            bool result;

            do
            {
                comparedType = baseTypes.Dequeue();
                baseType = comparedType.BaseType;
                if ((baseType != null) && (baseType.IsGenericType || baseType.IsGenericTypeDefinition))
                {
                    baseTypes.Enqueue(baseType.GetGenericTypeDefinition());
                }
                foreach (Type baseInterface in comparedType.GetInterfaces()
                    .Where((intf) => intf.IsGenericType || intf.IsGenericTypeDefinition))
                {
                     baseTypes.Enqueue(baseInterface.GetGenericTypeDefinition());
                }
                result = comparedType == type;
            } while (!result && (baseTypes.Count > 0));

            return result;
        }
    }
}
