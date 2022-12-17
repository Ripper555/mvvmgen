// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System.Collections.Generic;
using MvvmGen.Model;

namespace MvvmGen.Generators;

internal static class PropertyGenerator
{
    internal static void GenerateProperties(this ViewModelBuilder vmBuilder, IEnumerable<PropertyToGenerate>? propertiesToGenerate)
    {
        if (propertiesToGenerate is not null)
        {
            foreach (var propertyToGenerate in propertiesToGenerate)
            {
                GenerateProperty(vmBuilder, propertyToGenerate);
            }
        }
    }

    private static void GenerateProperty(ViewModelBuilder vmBuilder, PropertyToGenerate p)
    {
        vmBuilder.AppendLineBeforeMember();
        vmBuilder.Append($"public {p.PropertyType} {p.PropertyName}");

        if (p.IsReadOnly)
        {
            vmBuilder.AppendLine($" => {p.BackingField};");
            return;
        }
        else
        {
            vmBuilder.AppendLine();
        }

        vmBuilder.OpenBrace();
        vmBuilder.AppendLine($"get => {p.BackingField};");
        vmBuilder.AppendLine("set");
        vmBuilder.OpenBrace();
        vmBuilder.AppendLine($"if ({p.BackingField} != value)");
        vmBuilder.OpenBrace();
        vmBuilder.AppendLine($"{p.BackingField} = value;");
        vmBuilder.AppendLine($"OnPropertyChanged(\"{p.PropertyName}\");");
        if (p.PropertiesToInvalidate is not null)
        {
            foreach (var propertyToInvalidate in p.PropertiesToInvalidate)
            {
                vmBuilder.AppendLine($"OnPropertyChanged(\"{propertyToInvalidate}\");");
            }
        }
        if (p.EventsToPublish is not null)
        {
            foreach (var eventToPublish in p.EventsToPublish)
            {
                var createPublishCondition = eventToPublish.PublishCondition is { Length: > 0 };
                if (createPublishCondition)
                {
                    vmBuilder.AppendLine($"if ({eventToPublish.PublishCondition})");
                    vmBuilder.OpenBrace();
                }
                vmBuilder.AppendLine($"{eventToPublish.EventAggregatorMemberName}.Publish(new {eventToPublish.EventType}({eventToPublish.EventConstructorArgs}));");

                if (createPublishCondition)
                {
                    vmBuilder.CloseBrace();
                }
            }
        }
        if (p.MethodsToCall is not null)
        {
            foreach (var methodToCall in p.MethodsToCall)
            {
                vmBuilder.AppendLine($"{methodToCall.MethodName}({methodToCall.MethodArgs});");
            }
        }
        vmBuilder.CloseBrace();
        vmBuilder.CloseBrace();
        vmBuilder.CloseBrace();
    }
}