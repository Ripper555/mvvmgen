// ***********************************************************************
// ⚡ MvvmGen => https://github.com/thomasclaudiushuber/mvvmgen
// Copyright © by Thomas Claudius Huber
// Licensed under the MIT license => See LICENSE file in repository root
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MvvmGen.ViewModels;

public class ViewModelBaseWithValidation : ViewModelBase, INotifyDataErrorInfo
{
    protected readonly Dictionary<string, List<string>> Errors = new();

    protected ViewModelBaseWithValidation()
    {
    }

    public bool HasErrors => Errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        return propertyName is not null && Errors.ContainsKey(propertyName)
            ? Errors[propertyName]
            : Enumerable.Empty<string>();
    }

    protected virtual void OnErrorsChanged(string propertyName) =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

    protected void ValidateProperty(string property, params (Func<bool>, string)[] checks)
    {
        if (Errors.ContainsKey(property))
            Errors.Remove(property);

        var results = checks.Where(x => x.Item1.Invoke()).Select(x => x.Item2).ToList();
        if (results.Any())
            Errors.Add(property, results);
        OnErrorsChanged(property);
        OnPropertyChanged(nameof(HasErrors));
    }
}
