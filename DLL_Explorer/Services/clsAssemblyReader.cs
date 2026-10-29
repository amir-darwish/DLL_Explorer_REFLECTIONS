using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DLL_Explorer.Services
{
    internal class clsAssemblyReader
    {
        private const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static |
                                          BindingFlags.DeclaredOnly;
        public Assembly LoadFile(string path, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(path))
            {
                errorMessage = "File path cannot be null or empty.";
                return null;
            }

            try
            {
                return Assembly.LoadFrom(path);
            }
            catch (BadImageFormatException)
            {
                errorMessage = "The selected file is not a valid .NET assembly (it may be a native C/C++ DLL or corrupted).";
                return null;
            }
            catch (FileNotFoundException)
            {
                errorMessage = "The file was not found, or one of its dependent assemblies (dependencies) is missing.";
                return null;
            }
            catch (FileLoadException)
            {
                errorMessage = "The assembly was found but could not be loaded (it might be locked or incompatible).";
                return null;
            }
            catch (Exception ex)
            {
                errorMessage = $"An unexpected error occurred: {ex.Message}";
                return null;
            }
        }

        public Type[] GetAssemblyTypes(Assembly assembly)
        {
            Type[] types = null;
            if (assembly == null)
            {
                return Array.Empty<Type>();
            }
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }
            catch {
                return Array.Empty<Type>();
            }
            return types;
        }

        public MethodInfo[] GetMethodByType(Type type)
        {
            if (type == null)
            {
                return Array.Empty<MethodInfo>();
            }

            try
            {
                return type.GetMethods(DefaultFlags).Where(m => !m.IsSpecialName).ToArray(); // to hide _get and _set methods
            }
            catch
            {
                return Array.Empty<MethodInfo>();
            }
        }

        
    }
}
