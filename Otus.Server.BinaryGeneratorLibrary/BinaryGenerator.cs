using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Otus.Server.BinaryGeneratorLibrary;

[Generator]
public class BinaryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // ищем класс по атрибуту
        var classesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Otus.Server.BinaryGeneratorLibrary.GenerateBinarySerializerAttribute", 
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (context, _) =>
                    GetSemanticTarget(context)
            );

        // создаем файл на выходе
        context.RegisterSourceOutput(classesToGenerate, (productionContext, provider) =>

            {
                
                var (classInfo, properties)= provider;

                // функции которые будут конвертировать данные
                var (fromBinary ,toBinary)= CreateDeserializeFromBinary(properties);
                
                StringBuilder sb = new StringBuilder($$"""
using System.Text;
using System;

namespace {{classInfo.Namespace}};


public partial class {{classInfo.Name}}
{
"""); 
                sb.AppendLine(fromBinary);
                sb.AppendLine(toBinary);
                sb.AppendLine("}");
                productionContext.AddSource($"{classInfo.Name}.BinaryGenerator.g.cs", sb.ToString());
            }
        );
    }

    // для свойства берем способ конвертации
    private (string toBinary, string fromBinary) GetActionProperty(
        string propertyName, TypeInfo typeInfo
        )
    {
        if (typeInfo.Type is INamedTypeSymbol type)
        {
            SpecialType specialType = type.OriginalDefinition.SpecialType;
            SpecialType specialTypeArguments = type.TypeArguments.FirstOrDefault()?.SpecialType ?? specialType;

            if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_Int32)
            {
                return CreateReadWriteBinaryFromNullableInt32(propertyName);
            }
            else if (specialType == SpecialType.System_Int32)
            {
                return CreateReadWriteBinaryFromInt32(propertyName);
            }
            if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_Int64)
            {
                return CreateReadWriteBinaryFromNullableInt64(propertyName);
            }
            else if (specialType == SpecialType.System_Int64)
            {
                return CreateReadWriteBinaryFromInt64(propertyName);
            }
            else if (specialType == SpecialType.System_String)
            {
                return CreateReadWriteBinaryFromString(propertyName);
            }
            else if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_DateTime)
            {
                return CreateReadWriteBinaryFromNullableDateTime(propertyName);
            }
            else if (specialType == SpecialType.System_DateTime)
            {
                return CreateReadWriteBinaryFromDateTime(propertyName);
            }
            else if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_Boolean)
            {
                return CreateReadWriteBinaryFromNullableBoolean(propertyName);
            }
            else if (specialType == SpecialType.System_Boolean)
            {
                return CreateReadWriteBinaryFromBoolean(propertyName);
            }
            else if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_Double)
            {
                return CreateReadWriteBinaryFromNullableDouble(propertyName);
            }
            else if (specialType == SpecialType.System_Double)
            {
                return CreateReadWriteBinaryFromDouble(propertyName);
            }
            else if (specialType == SpecialType.System_Nullable_T && specialTypeArguments == SpecialType.System_Decimal)
            {
                return CreateReadWriteBinaryFromNullableDecimal(propertyName);
            }
            else if (specialType == SpecialType.System_Decimal)
            {
                return CreateReadWriteBinaryFromDecimal(propertyName);
            }
        }
        return default;
    }

    // создаем функции для коныертиции
    private (string FromBinary, string ToBinary) CreateDeserializeFromBinary(
        (string Name, TypeInfo TypeInfo)[] properties)
    {
        List<(string toBinary, string fromBinary)> actionProperties = new(properties.Length);
        foreach (var property in properties)
        {
            (string toBinary, string fromBinary) actionProperty = GetActionProperty(property.Name, property.TypeInfo);
            if (actionProperty == default)
            {
                continue;
            }

            actionProperties.Add(actionProperty);
        }

        string fromBinary = CreateSerializeToBinary(actionProperties);
        string toBinary = CreateDeserializeFromBinary(actionProperties);
        return (fromBinary, toBinary);
    }

    private string CreateDeserializeFromBinary(List<(string toBinary, string fromBinary)> actionProperties)
    {
        
        StringBuilder sb = new StringBuilder(@"
    public void DeserializeFromBinary(Stream stream)
    {
        using (stream)
        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
        {
");

        foreach (var actionProperty in actionProperties)
        {
            sb.AppendLine(actionProperty.fromBinary);
        }
        sb.Append(@"        }
    }");
        return sb.ToString();
    }
    private string CreateSerializeToBinary(List<(string toBinary, string fromBinary)> actionProperties)
    {
        
        StringBuilder sb = new StringBuilder(@"
    public void SerializeToBinary(Stream stream)
    {
        using (stream)
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
");

        foreach (var actionProperty in actionProperties)
        {
            sb.AppendLine(actionProperty.toBinary);
        }
        sb.Append(@"        }
    }");
        return sb.ToString();
    }

    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromInt32(string propertyName)
    {
        return (
$"            writer.Write(this.{propertyName});",
$"            this.{propertyName} = reader.ReadInt32();");
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableInt32(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = reader.ReadInt32();
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromInt64(string propertyName)
    {
        return (
            $"            writer.Write(this.{propertyName});",
            $"            this.{propertyName} = reader.ReadInt64();");
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableInt64(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = reader.ReadInt64();
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableDateTime(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value.Ticks);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = new DateTime(reader.ReadInt64());;
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromString(string propertyName)
    {
        string toBinary = 
            $$"""
            writer.Write(this.{{propertyName}} != null);
            if (this.{{propertyName}} != null) { writer.Write(this.{{propertyName}});} 
""";
        string fromBinary = $$"""
            if (reader.ReadBoolean())
            {
                this.{{propertyName}} = reader.ReadString();
            } 
""";
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromDateTime(string propertyName)
    {
        return (
$"            writer.Write(this.{propertyName}.Ticks);",
$"            this.{propertyName} = new DateTime(reader.ReadInt64());");
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromBoolean(string propertyName)
    {
        return (
            $"            writer.Write(this.{propertyName});",
            $"            this.{propertyName} = reader.ReadBoolean();");
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableBoolean(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = reader.ReadBoolean();
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableDouble(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = reader.ReadDouble();
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromDouble(string propertyName)
    {
        return (
            $"            writer.Write(this.{propertyName});",
            $"            this.{propertyName} = reader.ReadDouble();");
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromNullableDecimal(string propertyName)
    {
        string toBinary = 
            $$"""
                          writer.Write(this.{{propertyName}}.HasValue);
                          if (this.{{propertyName}}.HasValue) { writer.Write(this.{{propertyName}}.Value);} 
              """;
        string fromBinary = $$"""
                                          if (reader.ReadBoolean())
                                          {
                                              this.{{propertyName}} = reader.ReadDecimal();
                                          } 
                              """;
        return (
            toBinary,
            fromBinary);
    }
    private (string toBinary, string fromBinary) CreateReadWriteBinaryFromDecimal(string propertyName)
    {
        return (
            $"            writer.Write(this.{propertyName});",
            $"            this.{propertyName} = reader.ReadDecimal();");
    }

    // берем из класса все необходимое
    private static (
        (string Name, string Namespace) classInfo, 
        (string name, TypeInfo typeInfo)[] properties)
        GetSemanticTarget(GeneratorAttributeSyntaxContext context)
    {
        
        SemanticModel model = context.SemanticModel;
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.TargetNode;
        
        var classSymbol = model.GetDeclaredSymbol(classDeclarationSyntax) ?? throw new NullReferenceException("classSymbol");
        var namespaceSymbol = classSymbol.ContainingNamespace;
        string namespaceName = namespaceSymbol.ToDisplayString();
        (string Name, string Namespace) classInfo = (classSymbol.Name, namespaceName);
       
        var propertyDeclarations =
            classDeclarationSyntax
                .Members.OfType<PropertyDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))
                .Select(x => (x.Identifier.Text, ModelExtensions.GetTypeInfo(context.SemanticModel, x.Type)))
                .ToArray();
        
        return (classInfo, propertyDeclarations);
    }
}