﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Kalk.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Scriban;

namespace Kalk.CodeGen
{
    class Program
    {
        internal class ModuleToGenerate
        {
            public ModuleToGenerate()
            {
                Members = new List<KalkGeneratedMember>();
            }

            public string Namespace { get; set; }

            public string ClassName { get; set; }

            public List<KalkGeneratedMember> Members { get; }
        }

        public class IntrinsicParameter : KalkParamDescriptor
        {
            public IntrinsicParameter()
            {
            }

            public string Type { get; set; }
            
            public string RealType { get; set; }
            
            public string GenericCompatibleRealType { get; set; }

            public string BaseNativeType { get; set; }
            
            public string NativeType { get; set; }
        }

        public class KalkGeneratedMember : KalkDescriptor
        {
            public string Name { get; set; }

            public string CSharpName { get; set; }

            public bool IsFunc { get; set; }
            
            public bool IsAction { get; set; }

            public bool IsConst { get; set; }

            public string Cast { get; set; }
        }

        public class KalkIntrinsic : KalkGeneratedMember
        {
            public KalkIntrinsic()
            {
                Parameters = new List<IntrinsicParameter>();
            }

            public string Class { get; set; }
            
            public string ReturnType { get; set; }
            
            public string RealReturnType { get; set; }

            public string GenericCompatibleRealReturnType { get; set; }
            
            public List<IntrinsicParameter> Parameters { get; } 
            
            public bool HasPointerArguments { get; set; }
            
            public MethodInfo Method { get; set; }
            
            public IMethodSymbol MethodSymbol { get; set; }
            
            public string MethodDeclaration { get; set; }
            
            public string IndirectMethodDeclaration { get; set; }

            public string BaseNativeReturnType { get; set; }

            public string NativeReturnType { get; set; }
            
            public bool IsSupported { get; set; }
        }

        private static readonly HashSet<string> IntrinsicsSupports = new HashSet<string>()
        {
            "SSE",
            "SSE2",
            "SSE3",
            "SSSE3",
            "SSE4.1",
            "SSE4.2",
            "AVX",
            "AVX2",
        };

        /// <summary>
        /// Instructions requiring alignment (via parameter mem_addr)
        /// </summary>
        private static readonly Dictionary<string, int> RequiredAlignments = new Dictionary<string, int>()
        {
            {"mm_load_ps", 16},
            {"mm_loadr_ps", 16},
            {"mm_stream_ps", 16},
            {"mm_store1_ps", 16},
            {"mm_store_ps1", 16},
            {"mm_store_ps", 16},
            {"mm_storer_ps", 16},
            {"mm_load_si128", 16},
            {"mm_store_si128", 16},
            {"mm_stream_si128", 16},
            {"mm_load_pd", 16},
            {"mm_loadr_pd", 16},
            {"mm_stream_pd", 16},
            {"mm_store1_pd", 16},
            {"mm_store_pd1", 16},
            {"mm_store_pd", 16},
            {"mm_storer_pd", 16},
            {"mm_stream_load_si128", 16},
            {"mm256_load_pd", 32},
            {"mm256_store_pd", 32},
            {"mm256_load_ps", 32},
            {"mm256_store_ps", 32},
            {"mm256_load_si256", 32},
            {"mm256_store_si256", 32},
            {"mm256_stream_si256", 32},
            {"mm256_stream_pd", 32},
            {"mm256_stream_ps", 32},
            {"mm256_stream_load_si256", 32},
        };
       
        private static List<ModuleToGenerate> FindIntrinsics(Compilation compilation)
        {
            var regexName = new Regex(@"^\s*\w+\s+(_\w+)");
            int count = 0;
            
            var intelDoc = XDocument.Load("intel-intrinsics-data-latest.xml");
            var nameToDoc = intelDoc.Descendants("intrinsic").Where(x => IntrinsicsSupports.Contains(x.Attribute("tech").Value ?? string.Empty)).ToDictionary(x => x.Attribute("name").Value, x => x);
            
            var generatedIntrinsics = new Dictionary<string, KalkIntrinsic>();
            
            foreach (var type in new Type[]
            {
                typeof(System.Runtime.Intrinsics.X86.Sse.X64),
                typeof(System.Runtime.Intrinsics.X86.Sse),
                typeof(System.Runtime.Intrinsics.X86.Sse2),
                typeof(System.Runtime.Intrinsics.X86.Sse2.X64),
                typeof(System.Runtime.Intrinsics.X86.Sse3),
                typeof(System.Runtime.Intrinsics.X86.Ssse3),
                typeof(System.Runtime.Intrinsics.X86.Sse41),
                typeof(System.Runtime.Intrinsics.X86.Sse41.X64),
                typeof(System.Runtime.Intrinsics.X86.Sse42),
                typeof(System.Runtime.Intrinsics.X86.Sse42.X64),
                typeof(System.Runtime.Intrinsics.X86.Aes),
                typeof(System.Runtime.Intrinsics.X86.Avx),
                typeof(System.Runtime.Intrinsics.X86.Avx2),
                typeof(System.Runtime.Intrinsics.X86.Bmi1),
                typeof(System.Runtime.Intrinsics.X86.Bmi1.X64),
                typeof(System.Runtime.Intrinsics.X86.Bmi2),
                typeof(System.Runtime.Intrinsics.X86.Bmi2.X64),
            })
            {
                var x86Sse = compilation.GetTypeByMetadataName(type.FullName);
                foreach(var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (method.GetParameters().Length == 0)
                    {
                        continue;
                    }

                    var reflectionParameters = method.GetParameters();
                    IMethodSymbol sseMember = null;
                    foreach (var matchingMember in x86Sse.GetMembers(method.Name).OfType<IMethodSymbol>().Where(x => x.Parameters.Length == method.GetParameters().Length))
                    {
                        sseMember = matchingMember;
                        for (int i = 0; i < matchingMember.Parameters.Length; i++)
                        {
                            var reflectionParameter = reflectionParameters[i];
                            var isReflectionPointer = reflectionParameter.ParameterType.IsPointer;
                            var isRoslynPointer = matchingMember.Parameters[i].Type is IPointerTypeSymbol;
                            if (isReflectionPointer != isRoslynPointer)
                            {
                                sseMember = null;
                                break;
                            }
                        }

                        if (sseMember != null)
                        {
                            break;
                        }
                    }

                    if (sseMember == null)
                    {
                        Console.WriteLine($"Unable to match {method}");
                        continue;
                    }
                    
                    var groupName = type.FullName.Substring(type.FullName.LastIndexOf('.') + 1).Replace("+", string.Empty);
                    var docGroupName = type.Name == "X64" ? type.DeclaringType.Name : type.Name;
                    
                    var xmlDocStr = sseMember.GetDocumentationCommentXml();
                    if (string.IsNullOrEmpty(xmlDocStr)) continue;
                    
                    var xmlDoc = XElement.Parse($"<root>{xmlDocStr.Trim()}</root>");
                    var elements = xmlDoc.Elements().First();

                    var summary = GetCleanedString(elements);

                    var summaryTrimmed = summary.Replace("unsigned", string.Empty);
                    var match = regexName.Match(summaryTrimmed);
                    if (!match.Success) continue;
                    
                    var rawIntrinsicName = match.Groups[1].Value;

                    var intrinsicName = rawIntrinsicName.TrimStart('_');

                    generatedIntrinsics.TryGetValue(intrinsicName, out var existingDesc);
                    
                    var desc = new KalkIntrinsic
                    {
                        Name = rawIntrinsicName.TrimStart('_'),
                        Method = method,
                        Class = groupName,
                        MethodSymbol = sseMember,
                        IsFunc = true,
                    };
                    desc.CSharpName = desc.Name;
                    desc.Names.Add(desc.Name);
                    desc.Description = summary;

                    if (nameToDoc.TryGetValue(rawIntrinsicName, out var elementIntelDoc))
                    {
                        desc.Description = GetCleanedString(elementIntelDoc.Descendants("description").FirstOrDefault());

                        foreach (var parameter in elementIntelDoc.Descendants("parameter"))
                        {
                            desc.Params.Add(new KalkParamDescriptor(parameter.Attribute("varname").Value, parameter.Attribute("type").Value));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unable to find intrinsic doc for: {rawIntrinsicName}");
                    }

                    desc.RealReturnType = sseMember.ReturnType.ToDisplayString();
                    desc.ReturnType = method.ReturnType.IsConstructedGenericType ? "object" : desc.RealReturnType;
                    desc.GenericCompatibleRealReturnType = method.ReturnType.IsPointer ? "IntPtr" : desc.RealReturnType;
                    
                    (desc.BaseNativeReturnType, desc.NativeReturnType) = GetBaseTypeAndType(sseMember.ReturnType);

                    desc.HasPointerArguments = false;

                    for (int i = 0; i < sseMember.Parameters.Length; i++)
                    {
                        var intrinsicParameter = new IntrinsicParameter();
                        var parameter = sseMember.Parameters[i];
                        intrinsicParameter.Name = i < desc.Params.Count ? desc.Params[i].Name : parameter.Name;

                        var parameterType = method.GetParameters()[i].ParameterType;
                        intrinsicParameter.Type =  parameterType.IsConstructedGenericType || parameterType.IsPointer ? "object" : parameter.Type.ToDisplayString();
                        intrinsicParameter.RealType = parameter.Type.ToDisplayString();
                        intrinsicParameter.GenericCompatibleRealType = parameterType.IsPointer ? "IntPtr" : intrinsicParameter.RealType;
                        if (parameterType.IsPointer)
                        {
                            desc.HasPointerArguments = true;
                        }
                        
                        (intrinsicParameter.BaseNativeType, intrinsicParameter.NativeType) = GetBaseTypeAndType(parameter.Type);
                        desc.Parameters.Add(intrinsicParameter);
                    }
                    
                    // public object mm_add_ps(object left, object right) => ProcessArgs<float, Vector128<float>, float, Vector128<float>, float, Vector128<float>>(left, right, Sse.Add);
                    var methodDeclaration = new StringBuilder();
                    methodDeclaration.Append($"public {desc.ReturnType} {desc.Name}(");
                    for (int i = 0; i < desc.Parameters.Count; i++)
                    {
                        var parameter = desc.Parameters[i];
                        if (i > 0) methodDeclaration.Append(", ");
                        methodDeclaration.Append($"{parameter.Type} {parameter.Name}");
                    }

                    bool isAction = method.ReturnType == typeof(void);
                    desc.IsAction = isAction;
                    desc.IsFunc = !desc.IsAction;
                    
                    methodDeclaration.Append(isAction ? $") => ProcessAction<" : $") => ({desc.ReturnType})ProcessFunc<");

                    var castBuilder = new StringBuilder(isAction ? "Action<" : "Func<");
                    for (int i = 0; i < desc.Parameters.Count; i++)
                    {
                        var parameter = desc.Parameters[i];
                        if (i > 0)
                        {
                            methodDeclaration.Append(", ");
                            castBuilder.Append(", ");
                        }
                        methodDeclaration.Append($"{parameter.BaseNativeType}, {parameter.NativeType}");
                        castBuilder.Append($"{parameter.Type}");
                    }

                    if (!isAction)
                    {
                        if (desc.Parameters.Count > 0)
                        {
                            methodDeclaration.Append(", ");
                            castBuilder.Append(", ");                        
                        }
                        
                        methodDeclaration.Append($"{desc.BaseNativeReturnType}, {desc.NativeReturnType}");
                        castBuilder.Append($"{desc.ReturnType}");
                    }
                    methodDeclaration.Append($">(");
                    castBuilder.Append($">");
                    
                    RequiredAlignments.TryGetValue(desc.Name, out int memAlign);
                    
                    for (int i = 0; i < desc.Parameters.Count; i++)
                    {
                        var parameter = desc.Parameters[i];
                        methodDeclaration.Append($"{parameter.Name}, ");
                        if (memAlign > 0 && parameter.Name == "mem_addr")
                        {
                            methodDeclaration.Append($"{memAlign}, ");
                        }
                    }

                    var finalSSEMethod = $"{sseMember.ContainingType.ToDisplayString()}.{sseMember.Name}";
                    var sseMethodName = desc.HasPointerArguments ? $"{groupName}_{sseMember.Name}" : finalSSEMethod;
                    methodDeclaration.Append(sseMethodName);
                    methodDeclaration.Append(");");
                    desc.Cast = $"({castBuilder})";
                    desc.MethodDeclaration = methodDeclaration.ToString();

                    if (desc.HasPointerArguments)
                    {
                        methodDeclaration.Clear();

                        castBuilder.Clear();
                        castBuilder.Append(isAction ? "Action<" : "Func<");
                        for (int i = 0; i < desc.Parameters.Count; i++)
                        {
                            var parameter = desc.Parameters[i];
                            if (i > 0)
                            {
                                castBuilder.Append(", ");
                            }
                            castBuilder.Append($"{parameter.GenericCompatibleRealType}");
                        }

                        if (!isAction)
                        {
                            castBuilder.Append($", {desc.GenericCompatibleRealReturnType}");
                        }
                        castBuilder.Append(">");

                        methodDeclaration.Append($"private unsafe readonly static {castBuilder} {sseMethodName} = new {castBuilder}((");
                        for (int i = 0; i < desc.Parameters.Count; i++)
                        {
                            if (i > 0) methodDeclaration.Append(", ");
                            methodDeclaration.Append($"arg{i}");
                        }
                        methodDeclaration.Append($") => {finalSSEMethod}(");
                        for (int i = 0; i < desc.Parameters.Count; i++)
                        {
                            if (i > 0) methodDeclaration.Append(", ");
                            var parameter = desc.Parameters[i];
                            methodDeclaration.Append($"({parameter.RealType})arg{i}");
                        }
                        methodDeclaration.Append("));");
                        desc.IndirectMethodDeclaration = methodDeclaration.ToString();
                    }

                    desc.Category = $"Vector Hardware Intrinsics / {docGroupName.ToUpperInvariant()}";
                    
                    if (existingDesc == null || desc.Parameters[0].BaseNativeType == "float")
                    {
                        desc.Description = desc.Description.Replace("[round_note]", string.Empty).Trim().Replace("\r\n", "\n").Replace("\n", " ");
                        generatedIntrinsics[intrinsicName] = desc;
                    }
                    
                    desc.IsSupported = true;
                    
                    count++;
                }
            }
            
            Console.WriteLine($"{generatedIntrinsics.Count} intrinsics");

            var intrinsics = generatedIntrinsics.Values.Where(x => x.IsSupported).ToList();

            var intrinsicsPerClass = intrinsics.GroupBy(x => x.Class).ToDictionary(x => x.Key, y => y.OrderBy(x => x.Name).ToList());

            var modules = new List<ModuleToGenerate>();
            foreach (var keyPair in intrinsicsPerClass.OrderBy(x => x.Key))
            {
                var module = new ModuleToGenerate();
                module.Namespace = "Kalk.Core.Modules.HardwareIntrinsics";
                module.ClassName = $"{keyPair.Key}IntrinsicsModule";
                module.Members.AddRange(keyPair.Value);
                modules.Add(module);
            }
            
            return modules;
        }

        private static (string, string) GetBaseTypeAndType(ITypeSymbol type)
        {
            if (type is IPointerTypeSymbol)
            {
                return ("IntPtr", "IntPtr");
            }
            
            var typeStr = type.ToDisplayString();
            string baseType;
            var nativeType = type as INamedTypeSymbol;
            if (nativeType != null && nativeType.TypeArguments.Length > 0)
            {
                baseType = nativeType.TypeArguments[0].ToDisplayString();
            }
            else
            {
                baseType = typeStr;
            }
            return (baseType, typeStr);
        }
        
        
        static async Task Main(string[] args)
        {
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

            var x = typeof(System.Composition.CompositionContext).Name;

            var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
            {
//               {"TargetFramework", "netcoreapp3.1"},
            });

            var srcFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../.."));
            
            var pathToSolution = Path.Combine(srcFolder, @"kalk.sln");
            var solution = await workspace.OpenSolutionAsync(pathToSolution);

            // Force this assembly to reference the following types in order to compile Kalk.Core correctly
            // (this is awful but Roslyn doesn't help us here)
            Console.WriteLine($"Using {typeof(Unsafe).Assembly.FullName}");
            Console.WriteLine($"Using {typeof(CsvHelper.CsvParser).Assembly.FullName}");
            Console.WriteLine($"Using {typeof(HttpClient).Assembly.FullName}");
            Console.WriteLine($"Using {typeof(BigInteger).Assembly.FullName}");
            Console.WriteLine($"Using {typeof(HttpStatusCode).Assembly.FullName}");
            Console.WriteLine($"Using {typeof(System.Runtime.Intrinsics.Vector128).Assembly.Location}");

            
            var project = solution.Projects.First(x => x.Name == "Kalk.Core");
            //var scriban = solution.Projects.First(x => x.Name == "Scriban");
            //project = project.AddMetadataReferences(scriban.MetadataReferences);

            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic) continue;
                if (assembly.GetName().Name == "Scriban") continue;
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(assembly.Location));
            }

            {
                var refPackage = Path.Combine(homePath, ".nuget", "packages", "mathnet.numerics", "4.9.1", "lib", "netstandard2.0", "MathNet.Numerics.dll");
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(refPackage));
            }
            {
                var refPackage = Path.Combine(homePath, ".nuget", "packages", "System.Text.Encoding.CodePages", "4.7.0", "lib", "netstandard2.0", "System.Text.Encoding.CodePages.dll");
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(refPackage));
            }

            // Hack re-add System.Runtime.Intrinsics with doc
            var docProvider = XmlDocumentationProvider.CreateFromFile("System.Runtime.Intrinsics.xml");
            var intrinsicsAssembly = MetadataReference.CreateFromFile(typeof(System.Runtime.Intrinsics.X86.Sse).Assembly.Location, documentation: docProvider);
            var toRemove = project.MetadataReferences.FirstOrDefault(x => x.Display.Contains("System.Runtime.Intrinsics"));
            project = project.RemoveMetadataReference(toRemove);
            project = project.AddMetadataReference(intrinsicsAssembly);
            
            // Compile the project
            var compilation = await project.GetCompilationAsync();

            var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Count > 0)
            {
                Console.WriteLine("Compilation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                Console.WriteLine("Error, Exiting.");
                Environment.Exit(1);
                return;
            }

            var intrinsicModules = FindIntrinsics(compilation);

            //var kalkEngine = compilation.GetTypeByMetadataName("Kalk.Core.KalkEngine");

            var mapNameToModule = new Dictionary<string, ModuleToGenerate>();

            foreach (var type in compilation.GetSymbolsWithName(x => true, SymbolFilter.Type))
            {
                var typeSymbol = type as ITypeSymbol;
                if (typeSymbol == null) continue;

                foreach (var member in typeSymbol.GetMembers())
                {
                    var attr = member.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == "KalkDocAttribute");
                    if (attr == null) continue;

                    var name = attr.ConstructorArguments[0].Value.ToString();
                    var category = attr.ConstructorArguments[1].Value.ToString();

                    var className = member is ITypeSymbol ? member.Name : member.ContainingSymbol.Name;

                    if (!mapNameToModule.TryGetValue(className, out var moduleToGenerate))
                    {
                        moduleToGenerate = new ModuleToGenerate()
                        {
                            Namespace = member.ContainingNamespace.ToDisplayString(),
                            ClassName = className
                        };
                        mapNameToModule.Add(className, moduleToGenerate);
                    }

                    var method = member as IMethodSymbol;
                    var desc = new KalkGeneratedMember()
                    {
                        Category = category,
                        IsCommand = method != null && method.ReturnsVoid
                    };
                    desc.Name = name;
                    desc.Names.Add(name);
                    
                    if (method != null)
                    {
                        desc.CSharpName = method.Name;
                        
                        var builder = new StringBuilder();
                        desc.IsAction = method.ReturnsVoid;
                        desc.IsFunc = !desc.IsAction;
                        builder.Append(desc.IsAction ? "Action" : "Func");

                        if (method.Parameters.Length > 0 || desc.IsFunc)
                        {
                            builder.Append("<");
                        }

                        for (var i = 0; i < method.Parameters.Length; i++)
                        {
                            var parameter = method.Parameters[i];
                            if (i > 0) builder.Append(", ");
                            builder.Append(GetTypeName(parameter.Type));
                        }

                        if (desc.IsFunc)
                        {
                            if (method.Parameters.Length > 0)
                            {
                                builder.Append(", ");
                            }
                            builder.Append(GetTypeName(method.ReturnType));
                        }

                        if (method.Parameters.Length > 0 || desc.IsFunc)
                        {
                            builder.Append(">");
                        }

                        desc.Cast = $"({builder.ToString()})";
                    }
                    else if (member is IPropertySymbol || member is IFieldSymbol)
                    {
                        desc.CSharpName = member.Name;
                        desc.IsConst = true;
                    }

                    moduleToGenerate.Members.Add(desc);
                    
                    var xmlStr = member.GetDocumentationCommentXml();

                    if (!string.IsNullOrEmpty(xmlStr))
                    {
                        var xmlDoc = XElement.Parse(xmlStr);
                        var elements = xmlDoc.Elements().ToList();

                        foreach (var element in elements)
                        {
                            var text = GetCleanedString(element).Trim();
                            if (element.Name == "summary")
                            {
                                desc.Description = text;
                            }
                            else if (element.Name == "param")
                            {
                                var argName = element.Attribute("name").Value;
                                if (method != null)
                                {
                                    var parameterSymbol = method.Parameters.FirstOrDefault(x => x.Name == argName);
                                    bool isOptional = false;
                                    if (parameterSymbol == null)
                                    {
                                        Console.WriteLine($"Invalid XML doc parameter name {argName} not found on method {method}");
                                    }
                                    else
                                    {
                                        isOptional = parameterSymbol.IsOptional;
                                    }

                                    desc.Params.Add(new KalkParamDescriptor(element.Attribute("name").Value, text) {IsOptional = isOptional});
                                }
                            }
                            else if (element.Name == "returns")
                            {
                                desc.Returns = text;
                            }
                            else if (element.Name == "remarks")
                            {
                                desc.Remarks = text;
                            }
                        }
                    }
                }
            }

            var modules = mapNameToModule.Values.OrderBy(x => x.ClassName).ToList();
            
            var templateStr = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Date: {{ date.now }}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

{{~ for module in intrinsics ~}}
namespace {{ module.Namespace }}
{
    public partial class {{ module.ClassName }}
    {
        {{~ for member in module.Members ~}}
        /// <summary>
        /// {{ member.Description }}
        /// </summary>
        {{ member.MethodDeclaration }}
        {{ member.IndirectMethodDeclaration }}
        {{~ end ~}}
    }
}
{{~ end ~}}

{{~ for module in modules ~}}
namespace {{ module.Namespace }}
{
    public partial class {{ module.ClassName }}
    {
        {{~ if module.ClassName == 'KalkEngine' ~}}
        protected void RegisterFunctionsAuto()
        {{~ else ~}}
        protected override void RegisterFunctionsAuto()
        {{~ end ~}}
        {
            {{~ for member in module.Members ~}}
                {{~ if member.IsConst ~}}
            RegisterConstant(""{{ member.Name }}"", {{ member.CSharpName }});
                {{~ else if member.IsFunc ~}}
            RegisterFunction(""{{ member.Name }}"", {{member.Cast}}{{ member.CSharpName }});
                {{~ else if member.IsAction ~}}
            RegisterAction(""{{ member.Name }}"", {{member.Cast}}{{ member.CSharpName }});
                {{~ end ~}}
            {{~ end ~}}
            RegisterDocumentationAuto();
        }

        private void RegisterDocumentationAuto()
        {
            {{~ for item in module.Members ~}}
            {
                var descriptor = Descriptors[""{{ item.Names[0] }}""];
                descriptor.Category = ""{{ item.Category }}"";
                descriptor.Description = @""{{ item.Description | string.replace '""' '""""' }}"";
                descriptor.IsCommand = {{ item.IsCommand }};
            {{~ for param in item.Params ~}}
                descriptor.Params.Add(new KalkParamDescriptor(""{{ param.Name }}"", @""{{ param.Description | string.replace '""' '""""' }}"")  { IsOptional = {{ param.IsOptional }} });
            {{~ end ~}}
                {{~ if item.Returns ~}}
                descriptor.Returns = @""{{ item.Returns }}"";
                {{~ end ~}}
            }
            {{~ end ~}}
        }        
    }
}
{{~ end ~}}
";
            var template = Template.Parse(templateStr);
            
            var result = template.Render(new {modules = modules, intrinsics = new List<ModuleToGenerate>()}, x => x.Name);
            File.WriteAllText(Path.Combine(srcFolder, "Kalk.Core/KalkEngine.generated.cs"), result);
            
            result = template.Render(new {modules = intrinsicModules, intrinsics = intrinsicModules}, x => x.Name);
            File.WriteAllText(Path.Combine(srcFolder, "Kalk.Core/Modules/HardwareIntrinsics/Intrinsics.generated.cs"), result);
            
            //Console.WriteLine(result);
        }

        static string GetTypeName(ITypeSymbol typeSymbol)
        {
            //if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            //{
            //    return GetTypeName(arrayTypeSymbol.ElementType) + "[]";
            //}

            //if (typeSymbol.Name == typeof(Nullable).Name)
            //{
            //    return typeSymbol.ToDisplayString();
            //}
           
            //if (typeSymbol.Name == "String") return "string";
            //if (typeSymbol.Name == "Object") return "object";
            //if (typeSymbol.Name == "Boolean") return "bool";
            //if (typeSymbol.Name == "Single") return "float";
            //if (typeSymbol.Name == "Double") return "double";
            //if (typeSymbol.Name == "Int32") return "int";
            //if (typeSymbol.Name == "Int64") return "long";
            return typeSymbol.ToDisplayString();
        }

        private static string GetCleanedString(XNode node)
        {
            if (node.NodeType == XmlNodeType.Text)
            {
                return node.ToString();
            }

            var element = (XElement) node;
            string text;
            if (element.Name == "paramref")
            {
                text = element.Attribute("name")?.Value ?? string.Empty;
            }
            else
            {

                var builder = new StringBuilder();
                foreach (var subElement in element.Nodes())
                {
                    builder.Append(GetCleanedString(subElement));
                }

                text = builder.ToString();
            }
            return HttpUtility.HtmlDecode(text);
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
