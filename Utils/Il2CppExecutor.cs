using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Il2CppDumper
{
    public class Il2CppExecutor
    {
        public Metadata metadata;
        public Il2Cpp il2Cpp;
        private static readonly Dictionary<int, string> TypeString = new()
        {
            {1,"void"},
            {2,"bool"},
            {3,"char"},
            {4,"sbyte"},
            {5,"byte"},
            {6,"short"},
            {7,"ushort"},
            {8,"int"},
            {9,"uint"},
            {10,"long"},
            {11,"ulong"},
            {12,"float"},
            {13,"double"},
            {14,"string"},
            {22,"TypedReference"},
            {24,"IntPtr"},
            {25,"UIntPtr"},
            {28,"object"},
        };
        public ulong[] customAttributeGenerators;

        public Il2CppExecutor(Metadata metadata, Il2Cpp il2Cpp)
        {
            this.metadata = metadata;
            this.il2Cpp = il2Cpp;

            if (il2Cpp.Version >= 27 && il2Cpp.Version < 30) // Adjust for Unity 6 (2025)
            {
                customAttributeGenerators = new ulong[metadata.imageDefs.Sum(x => x.customAttributeCount)];
                foreach (var imageDef in metadata.imageDefs)
                {
                    var imageDefName = metadata.GetStringFromIndex(imageDef.nameIndex);
                    var codeGenModule = il2Cpp.codeGenModules[imageDefName];
                    if (imageDef.customAttributeCount > 0)
                    {
                        var pointers = il2Cpp.ReadClassArray<ulong>(il2Cpp.MapVATR(codeGenModule.customAttributeCacheGenerator), imageDef.customAttributeCount);
                        pointers.CopyTo(customAttributeGenerators, imageDef.customAttributeStart);
                    }
                }
            }
            else if (il2Cpp.Version < 27)
            {
                customAttributeGenerators = il2Cpp.customAttributeGenerators;
            }
        }

        public Il2CppTypeDefinition GetTypeDefinitionFromIl2CppType(Il2CppType il2CppType)
        {
            if (il2Cpp.Version >= 30 && il2Cpp.IsDumped) // Unity 6 adjustment
            {
                var offset = il2CppType.data.typeHandle - metadata.ImageBase - metadata.header.typeDefinitionsOffset;
                var index = offset / (ulong)metadata.SizeOf(typeof(Il2CppTypeDefinition));
                return metadata.typeDefs[index];
            }
            else
            {
                return metadata.typeDefs[il2CppType.data.klassIndex];
            }
        }

        public Il2CppGenericParameter GetGenericParameteFromIl2CppType(Il2CppType il2CppType)
        {
            if (il2Cpp.Version >= 30 && il2Cpp.IsDumped) // Unity 6 adjustment
            {
                var offset = il2CppType.data.genericParameterHandle - metadata.ImageBase - metadata.header.genericParametersOffset;
                var index = offset / (ulong)metadata.SizeOf(typeof(Il2CppGenericParameter));
                return metadata.genericParameters[index];
            }
            else
            {
                return metadata.genericParameters[il2CppType.data.genericParameterIndex];
            }
        }

        internal bool TryGetDefaultValue(int typeIndex, int dataIndex, out object value)
        {
            throw new NotImplementedException();
        }

        internal Il2CppTypeDefinition GetGenericClassTypeDefinition(Il2CppGenericClass genericClass)
        {
            throw new NotImplementedException();
        }
    }
}
