using Dastyar.Domain.Entities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Dastyar.Application.Common.Mapping;

internal static class MappingConventions
{
    internal static readonly string[] AllSuffixes = ["Dto", "Model", "Request", "Command"];
    internal static readonly string[] DtoSuffixes = ["Dto", "Model"];
    internal static readonly string[] Prefixes = ["Create", "Update", "Delete", "Upsert", "Get", "List"];

    internal static IEnumerable<Type> GetEntityTypes(Assembly assembly)
    {
        var baseType = typeof(BaseEntity<>);

        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && IsSubclassOfRawGeneric(baseType, type));
    }

    internal static IEnumerable<DtoMapping> GetDtoMappings(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            if (TryGetGenericBase(type, typeof(BaseDto<,,>), out var baseType))
            {
                var args = baseType.GetGenericArguments();
                yield return new DtoMapping(type, args[1], args[2]);
            }
        }
    }

    internal static bool IsBaseDtoType(Type type)
        => TryGetGenericBase(type, typeof(BaseDto<,,>), out _);

    internal static IEnumerable<NamedType> GetNamedTypes(Assembly assembly, string[] suffixes)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
        {
            if (TryGetBaseName(type.Name, suffixes, out var baseName))
            {
                baseName = StripKnownPrefix(baseName);
                if (!string.IsNullOrWhiteSpace(baseName))
                {
                    yield return new NamedType(baseName, type);
                }
            }
        }
    }

    internal static bool TryGetBaseName(string typeName, string[] suffixes, out string baseName)
    {
        foreach (var suffix in suffixes)
        {
            if (typeName.EndsWith(suffix, StringComparison.Ordinal))
            {
                baseName = typeName[..^suffix.Length];
                return !string.IsNullOrWhiteSpace(baseName);
            }
        }

        baseName = string.Empty;
        return false;
    }

    internal static string StripSuffix(string name)
    {
        if (TryGetBaseName(name, AllSuffixes, out var baseName))
        {
            return baseName;
        }

        return name;
    }

    internal static string StripKnownPrefix(string name)
    {
        foreach (var prefix in Prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return name[prefix.Length..];
            }
        }

        return name;
    }

    internal static string Normalize(string name)
    {
        var chars = name.Where(char.IsLetterOrDigit).ToArray();
        return new string(chars).ToLowerInvariant();
    }

    internal static bool IsSubclassOfRawGeneric(Type generic, Type? type)
    {
        while (type != null && type != typeof(object))
        {
            var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (generic == current)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    internal static bool TryGetGenericBase(Type? type, Type generic, [NotNullWhen(true)] out Type? baseType)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
            {
                baseType = type;
                return true;
            }

            type = type.BaseType;
        }

        baseType = null;
        return false;
    }

    internal readonly record struct NamedType(string BaseName, Type Type);
    internal readonly record struct DtoMapping(Type DtoType, Type EntityType, Type IdType);
}
