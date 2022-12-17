// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using MvvmGen.Extensions;
using MvvmGen.Model;

namespace MvvmGen.Generators;

internal static class ConstructorGenerator
{
    internal static void GenerateConstructor(this ViewModelBuilder vmBuilder, ViewModelToGenerate viewModelToGenerate)
    {
        if (viewModelToGenerate.GenerateConstructor)
        {
            Generate(vmBuilder, viewModelToGenerate.ViewModelClassSymbol.Name,
                viewModelToGenerate.InjectionsToGenerate,
                viewModelToGenerate.CommandsToGenerate,
                viewModelToGenerate.IsEventSubscriber);
        }
    }

    private static void Generate(ViewModelBuilder vmBuilder, string viewModelClassName,
        ICollection<InjectionToGenerate> injectionsToGenerate,
        ICollection<CommandToGenerate> commands, bool isEventSubscriber)
    {
        vmBuilder.AppendLineBeforeMember();
        vmBuilder.Append($"public {viewModelClassName}(");

        var first = true;
        string? eventAggregatorAccessForSubscription = null;
        if (isEventSubscriber)
        {
            var eventAggregatorInjection = injectionsToGenerate.FirstOrDefault(x => x.Type == "MvvmGen.Events.IEventAggregator");
            if (eventAggregatorInjection is not null)
            {
                eventAggregatorAccessForSubscription = $"this.{eventAggregatorInjection.PropertyName}";
            }
            else
            {
                eventAggregatorAccessForSubscription = "eventAggregator";
                first = false;
                vmBuilder.Append($"MvvmGen.Events.IEventAggregator {eventAggregatorAccessForSubscription}");
            }
        }
        if (commands.Any(x => x.IsSafeCommand))
        {
            if (!injectionsToGenerate.Any(x => x.Type.Contains("ILogger")))
            {
                injectionsToGenerate.Add(new InjectionToGenerate($"Microsoft.Extensions.Logging.ILogger<{viewModelClassName}>", "Logger"));
            }
            if (!injectionsToGenerate.Any(x => x.Type == "MvvmGen.Commands.IExceptionHandler"))
            {
                injectionsToGenerate.Add(new InjectionToGenerate("MvvmGen.Commands.IExceptionHandler", "ExceptionHandler"));
            }
        }
        foreach (var injectionToGenerate in injectionsToGenerate)
        {
            if (!first)
            {
                vmBuilder.Append(", ");
            }
            first = false;
            vmBuilder.Append($"{injectionToGenerate.Type} {injectionToGenerate.PropertyName.ToCamelCase()}");
        }

        vmBuilder.AppendLine(")");
        vmBuilder.OpenBrace();
        foreach (var injectionToGenerate in injectionsToGenerate)
        {
            vmBuilder.AppendLine($"this.{injectionToGenerate.PropertyName} = {injectionToGenerate.PropertyName.ToCamelCase()};");
        }

        if (isEventSubscriber)
        {
            vmBuilder.AppendLine($"{eventAggregatorAccessForSubscription}.RegisterSubscriber(this);");
        }

        if (commands.Any())
        {
            vmBuilder.GenerateCommandInitializeMethod(commands, injectionsToGenerate);
        }

        vmBuilder.AppendLine($"this.OnInitialize();");
        vmBuilder.CloseBrace();
        vmBuilder.AppendLine();
        vmBuilder.AppendLine($"partial void OnInitialize();");

    }
}