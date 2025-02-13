using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Il2CppDumper.Il2CppConstants;

namespace Il2CppDumper
{
    public class Il2CppDecompiler
    {
        private readonly Il2CppExecutor executor;
        private readonly Metadata metadata;
        private readonly Il2Cpp il2Cpp;
        private readonly Dictionary<Il2CppMethodDefinition, string> methodModifiers;

        public Il2CppDecompiler(Il2CppExecutor il2CppExecutor)
        {
            executor = il2CppExecutor;
            metadata = il2CppExecutor.metadata;
            il2Cpp = il2CppExecutor.il2Cpp;
            methodModifiers = new();
        }

        public void Decompile(Config config, string outputDir)
        {
            using var writer = new StreamWriter(new FileStream(Path.Combine(outputDir, "dump.cs"), FileMode.Create), new UTF8Encoding(false));

            // Dump images
            for (var imageIndex = 0; imageIndex < metadata.imageDefs.Length; imageIndex++)
            {
                var imageDef = metadata.imageDefs[imageIndex];
                writer.WriteLine($"// Image {imageIndex}: {metadata.GetStringFromIndex(imageDef.nameIndex)} - {imageDef.typeStart}");
            }

            // Dump types
            foreach (var imageDef in metadata.imageDefs)
            {
                try
                {
                    var imageName = metadata.GetStringFromIndex(imageDef.nameIndex);
                    var typeEnd = imageDef.typeStart + imageDef.typeCount;
                    for (int typeDefIndex = imageDef.typeStart; typeDefIndex < typeEnd; typeDefIndex++)
                    {
                        var typeDef = metadata.typeDefs[typeDefIndex];
                        var extends = new List<string>();

                        if (typeDef.parentIndex >= 0)
                        {
                            var parentMetadataIndex = typeDef.parentIndex;
                            var parentName = metadata.GetStringFromIndex((uint)parentMetadataIndex); // Fetch name directly from metadata
                            if (!typeDef.IsValueType && !typeDef.IsEnum && parentName != "object")
                            {
                                extends.Add(parentName);
                            }
                        }


                        if (typeDef.parentIndex >= 0)
                        {
                            // Fetch the parent type metadata index
                            var parentMetadataIndex = typeDef.parentIndex;

                            // Fetch the parent type name directly from the metadata
                            var parentName = metadata.GetStringFromIndex((uint)parentMetadataIndex); // Cast to uint if necessary

                            // Add the parent name to extends if it's not a value type, enum, or object
                            if (!typeDef.IsValueType && !typeDef.IsEnum && parentName != "object")
                            {
                                extends.Add(parentName);
                            }
                        }





                        writer.WriteLine($"\n// Namespace: {metadata.GetStringFromIndex(typeDef.namespaceIndex)}");

                        if (config.DumpAttribute)
                        {
                            writer.Write((imageDef, typeDef.customAttributeIndex, typeDef.token));
                        }

                        if (config.DumpAttribute && (typeDef.flags & TYPE_ATTRIBUTE_SERIALIZABLE) != 0)
                            writer.WriteLine("[Serializable]");

                        var visibility = typeDef.flags & TYPE_ATTRIBUTE_VISIBILITY_MASK;
                        switch (visibility)
                        {
                            case TYPE_ATTRIBUTE_PUBLIC:
                            case TYPE_ATTRIBUTE_NESTED_PUBLIC:
                                writer.Write("public ");
                                break;
                            case TYPE_ATTRIBUTE_NOT_PUBLIC:
                            case TYPE_ATTRIBUTE_NESTED_FAM_AND_ASSEM:
                            case TYPE_ATTRIBUTE_NESTED_ASSEMBLY:
                                writer.Write("internal ");
                                break;
                            case TYPE_ATTRIBUTE_NESTED_PRIVATE:
                                writer.Write("private ");
                                break;
                            case TYPE_ATTRIBUTE_NESTED_FAMILY:
                                writer.Write("protected ");
                                break;
                            case TYPE_ATTRIBUTE_NESTED_FAM_OR_ASSEM:
                                writer.Write("protected internal ");
                                break;
                        }

                        if ((typeDef.flags & TYPE_ATTRIBUTE_ABSTRACT) != 0 && (typeDef.flags & TYPE_ATTRIBUTE_SEALED) != 0)
                            writer.Write("static ");
                        else if ((typeDef.flags & TYPE_ATTRIBUTE_INTERFACE) == 0 && (typeDef.flags & TYPE_ATTRIBUTE_ABSTRACT) != 0)
                            writer.Write("abstract ");
                        else if (!typeDef.IsValueType && !typeDef.IsEnum && (typeDef.flags & TYPE_ATTRIBUTE_SEALED) != 0)
                            writer.Write("sealed ");

                        if ((typeDef.flags & TYPE_ATTRIBUTE_INTERFACE) != 0)
                            writer.Write("interface ");
                        else if (typeDef.IsEnum)
                            writer.Write("enum ");
                        else if (typeDef.IsValueType)
                            writer.Write("struct ");
                        else
                            writer.Write("class ");

                        // Fetch the type name directly from the metadata
                        var typeName = metadata.GetStringFromIndex((uint)typeDef.nameIndex); // Assuming 'nameIndex' holds the index to the type's name

                        // Write the type name to the output
                        writer.WriteLine(typeName);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error decompiling image {metadata.GetStringFromIndex(imageDef.nameIndex)} !! {ex.Message}");
                }
            }
        }
    }
}
