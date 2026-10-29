using System;
using System.Linq;
using System.Reflection;

namespace DLL_Explorer.Services
{
    internal static class clsSignatureFormatter
    {
        public static string GetTypeName(Type type)
        {
            if (type == null) return "None";
            if (type.IsByRef) return GetTypeName(type.GetElementType());
            if (type.IsPointer) return GetTypeName(type.GetElementType()) + "*";
            if (type.IsArray) return GetTypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) return GetTypeName(nullableType) + "?";

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: if (!type.IsEnum) return "bool"; break;
                case TypeCode.Byte: if (!type.IsEnum) return "byte"; break;
                case TypeCode.SByte: if (!type.IsEnum) return "sbyte"; break;
                case TypeCode.Int16: if (!type.IsEnum) return "short"; break;
                case TypeCode.UInt16: if (!type.IsEnum) return "ushort"; break;
                case TypeCode.Int32: if (!type.IsEnum) return "int"; break;
                case TypeCode.UInt32: if (!type.IsEnum) return "uint"; break;
                case TypeCode.Int64: if (!type.IsEnum) return "long"; break;
                case TypeCode.UInt64: if (!type.IsEnum) return "ulong"; break;
                case TypeCode.Single: return "float";
                case TypeCode.Double: return "double";
                case TypeCode.Decimal: return "decimal";
                case TypeCode.Char: return "char";
                case TypeCode.String: return "string";
            }
            if (type == typeof(void)) return "void";
            if (type == typeof(object)) return "object";

            string name = type.Name.Split('`')[0];
            if (type.IsGenericType)
                name += "<" + string.Join(", ", type.GetGenericArguments().Select(GetTypeName)) + ">";
            return name;
        }

        public static string GetTypeKind(Type type)
        {
            if (type.IsEnum) return "Enum";
            if (type.IsInterface) return "Interface";
            if (typeof(Delegate).IsAssignableFrom(type)) return "Delegate";
            if (type.IsValueType) return "Struct";
            return "Class";
        }

        public static string GetAccess(MemberInfo member)
        {
            Type type = member as Type;
            if (type != null)
            {
                if (type.IsPublic || type.IsNestedPublic) return "public";
                if (type.IsNestedPrivate) return "private";
                if (type.IsNestedFamORAssem) return "protected internal";
                if (type.IsNestedFamANDAssem) return "private protected";
                if (type.IsNestedFamily) return "protected";
                return "internal";
            }
            PropertyInfo property = member as PropertyInfo;
            if (property != null)
            {
                string[] access = property.GetAccessors(true).Select(GetAccess).ToArray();
                // A property's visibility is the least restrictive visibility of its accessors.
                foreach (string candidate in new[] { "public", "protected internal", "protected", "internal", "private protected", "private" })
                    if (access.Contains(candidate)) return candidate;
                return "unknown";
            }
            MethodBase method = member as MethodBase;
            if (method != null)
            {
                if (method.IsPublic) return "public";
                if (method.IsPrivate) return "private";
                if (method.IsFamilyOrAssembly) return "protected internal";
                if (method.IsFamilyAndAssembly) return "private protected";
                if (method.IsFamily) return "protected";
                return "internal";
            }
            FieldInfo field = member as FieldInfo;
            if (field != null)
            {
                if (field.IsPublic) return "public";
                if (field.IsPrivate) return "private";
                if (field.IsFamilyOrAssembly) return "protected internal";
                if (field.IsFamilyAndAssembly) return "private protected";
                if (field.IsFamily) return "protected";
                return "internal";
            }
            return "unknown";
        }

        public static string GetMemberLabel(MemberInfo member)
        {
            MethodInfo method = member as MethodInfo;
            return method == null ? member.Name : method.Name + "(" +
                string.Join(", ", method.GetParameters().Select(p => GetTypeName(p.ParameterType))) + ")";
        }

        public static string GetSignature(MemberInfo member)
        {
            Type type = member as Type;
            if (type != null)
            {
                if (typeof(Delegate).IsAssignableFrom(type))
                {
                    MethodInfo invoke = type.GetMethod("Invoke");
                    if (invoke != null)
                        return GetAccess(type) + " delegate " + GetTypeName(invoke.ReturnType) + " " + GetTypeName(type) +
                            "(" + string.Join(", ", invoke.GetParameters().Select(GetParameter)) + ");";
                }
                string kind = GetTypeKind(type).ToLowerInvariant();
                string modifiers = type.IsClass && !typeof(Delegate).IsAssignableFrom(type)
                    ? (type.IsAbstract && type.IsSealed ? "static " : type.IsAbstract ? "abstract " : type.IsSealed ? "sealed " : "") : "";
                return GetAccess(type) + " " + modifiers + kind + " " + GetTypeName(type);
            }
            MethodInfo method = member as MethodInfo;
            if (method != null)
            {
                string generic = method.IsGenericMethod ? "<" + string.Join(", ", method.GetGenericArguments().Select(GetTypeName)) + ">" : "";
                return GetAccess(method) + " " + GetMethodModifier(method) + GetTypeName(method.ReturnType) + " " + method.Name + generic +
                    "(" + string.Join(", ", method.GetParameters().Select(GetParameter)) + ")";
            }
            PropertyInfo property = member as PropertyInfo;
            if (property != null)
            {
                string access = GetAccess(property);
                MethodInfo getter = property.GetGetMethod(true), setter = property.GetSetMethod(true);
                MethodInfo accessor = getter ?? setter;
                string name = property.GetIndexParameters().Length == 0 ? property.Name :
                    "this[" + string.Join(", ", property.GetIndexParameters().Select(GetParameter)) + "]";
                return access + " " + (accessor != null ? GetMethodModifier(accessor) : "") + GetTypeName(property.PropertyType) + " " + name + " { " +
                    GetAccessor(getter, "get", access) + GetAccessor(setter, "set", access) + "}";
            }
            FieldInfo field = member as FieldInfo;
            if (field != null)
                return GetAccess(field) + " " + (field.IsLiteral ? "const " : (field.IsStatic ? "static " : "") + (field.IsInitOnly ? "readonly " : "")) +
                    GetTypeName(field.FieldType) + " " + field.Name + ";";
            return member.Name;
        }

        private static string GetMethodModifier(MethodInfo method)
        {
            if (method.IsStatic) return "static ";
            if (method.IsAbstract) return "abstract ";
            if (method.IsVirtual && !method.IsFinal)
                return method.GetBaseDefinition().DeclaringType != method.DeclaringType ? "override " : "virtual ";
            return "";
        }

        private static string GetAccessor(MethodInfo accessor, string name, string propertyAccess)
        {
            if (accessor == null) return "";
            string access = GetAccess(accessor);
            return (access == propertyAccess ? "" : access + " ") + name + "; ";
        }

        private static string GetParameter(ParameterInfo parameter)
        {
            string modifier = parameter.IsOut ? "out " : parameter.ParameterType.IsByRef ? "ref " :
                parameter.IsDefined(typeof(ParamArrayAttribute), false) ? "params " : "";
            return modifier + GetTypeName(parameter.ParameterType) + " " + parameter.Name;
        }
    }
}
