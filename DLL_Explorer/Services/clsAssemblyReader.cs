using System;
using System.IO;
using System.Reflection;

namespace DLL_Explorer.Services
{
    internal class clsAssemblyReader
    {

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
    }
}
