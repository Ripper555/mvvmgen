// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MvvmGen.Generators;
using MvvmGen.Model;
using MvvmGen.SourceGenerators.Model;

namespace MvvmGen;

/// <summary>
/// Generates ViewModels for classes that are decorated with the MvvmGen.ViewModelAttribute.
/// </summary>
[Generator]
public class ViewModelGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not ViewModelSyntaxReceiver receiver)
        {
            return;
        }
        var versionString = GetType().Assembly.GetName().Version.ToString(3);
        var viewModelBaseSymbol = context.Compilation.GetTypeByMetadataName("MvvmGen.ViewModels.ViewModelBase");
        if (viewModelBaseSymbol is not null)
        {
            foreach (var viewModelToGenerate in receiver.ViewModelsToGenerate)
            {
                var vmBuilder = new ViewModelBuilder();

                vmBuilder.GenerateCommentHeader(versionString);

                vmBuilder.GenerateNullableDirective();

                vmBuilder.GenerateUsingDirectives();

                vmBuilder.GenerateNamespace(viewModelToGenerate.ViewModelClassSymbol);

                vmBuilder.GenerateClass(viewModelToGenerate.ViewModelClassSymbol, viewModelBaseSymbol, viewModelToGenerate.Validation);

                vmBuilder.GenerateConstructor(viewModelToGenerate);

                //vmBuilder.GenerateCommandInitializeMethod(viewModelToGenerate.CommandsToGenerate, viewModelToGenerate.InjectionsToGenerate);

                vmBuilder.GenerateCommandProperties(viewModelToGenerate.CommandsToGenerate);

                vmBuilder.GenerateProperties(viewModelToGenerate.PropertiesToGenerate);

                vmBuilder.GenerateModelProperty(viewModelToGenerate.WrappedModelType);

                vmBuilder.GenerateInjectionProperties(viewModelToGenerate.InjectionsToGenerate);

                vmBuilder.GenerateInvalidateCommandsMethod(viewModelToGenerate.CommandsToInvalidateByPropertyName);

                while (vmBuilder.IndentLevel > 1) // Keep the namespace open for a factory class
                {
                    vmBuilder.DecreaseIndent();
                    vmBuilder.AppendLine("}");
                }

                vmBuilder.GenerateFactoryClass(viewModelToGenerate);

                while (vmBuilder.DecreaseIndent())
                {
                    vmBuilder.AppendLine("}");
                }

                var sourceText = SourceText.From(vmBuilder.ToString(), Encoding.UTF8);
                context.AddSource($"{viewModelToGenerate.ViewModelClassSymbol.ContainingNamespace}.{viewModelToGenerate.ViewModelClassSymbol.Name}.g.cs", sourceText);
            }

            BuildViewModelRegistration(context, receiver.ViewModelsToGenerate);
            BuildServiceRegistration(context, receiver.ViewModelsToGenerate, receiver.ServiceClasses);
        }
    }

    private static void BuildServiceRegistration(GeneratorExecutionContext context, IEnumerable<ViewModelToGenerate> vms, List<ServiceClass> services)
    {
        var file = $$"""
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;

            namespace MvvmGen;
            public static class ServiceRegister
            {
                public static IServiceCollection AddServices(this IServiceCollection services)
                {
                    {{ServiceRegistionsMethodBody(vms, services)}}
                    return services;
                }
            }
            """;

        var diSourceText = SourceText.From(file, Encoding.UTF8);
        context.AddSource($"ServiceRegister.g.cs", diSourceText);

    }

    private static string ServiceRegistionsMethodBody(IEnumerable<ViewModelToGenerate> vms, List<ServiceClass> services)
    {
        var builder = new StringBuilder();
        var injections = vms.SelectMany(x => x.InjectionsToGenerate.Select(i => i.Type))
            .Distinct()
            .ToArray();
        foreach (var injection in injections.AsSpan())
        {
            var c = services.Count(s => s.Interfaces.Any(i => i.ToString() == injection));
            if (c != 1)
                continue;

            var service = services.First(x => x.Interfaces.Any(i => i.ToString() == injection));
            builder.AppendLine($"services.TryAddScoped<{injection}, {service.Service.ToDisplayString()}>();");
        }
        return builder.ToString();
    }

    private static void BuildViewModelRegistration(GeneratorExecutionContext context, IEnumerable<ViewModelToGenerate> viewModelToGenerate)
    {
        var file = $$"""
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;

            namespace MvvmGen;
            public static class ViewModelRegister
            {
                public static IServiceCollection AddViewModels(this IServiceCollection services)
                {
                    {{VmRegistrationMethodBody(viewModelToGenerate)}}
                    return services;
                }
            }
            """;

        var diSourceText = SourceText.From(file, Encoding.UTF8);
        context.AddSource($"ViewModelRegister.g.cs", diSourceText);
    }

    private static string VmRegistrationMethodBody(IEnumerable<ViewModelToGenerate> vms)
    {
        var body = new StringBuilder();
        foreach (var viewModelToGenerate in vms)
        {
            body.AppendLine($"services.TryAddTransient<{viewModelToGenerate.ViewModelClassSymbol.ToDisplayString()}>();");
        }

        return body.ToString();
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ViewModelSyntaxReceiver());
    }
}
