using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MvvmGen.ViewModels;

public class ViewModelBaseWithValidation : ViewModelBase, INotifyDataErrorInfo
{
    protected readonly Dictionary<string, List<string>> Errors;

    protected ViewModelBaseWithValidation()
    {
        Errors = new Dictionary<string, List<string>>();
        PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(HasErrors)) return;
            Validate();
        };
    }

    public bool HasErrors => Errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName != null && Errors.ContainsKey(propertyName)
            ? Errors[propertyName]
            : Enumerable.Empty<string>();

    protected virtual void OnErrorsChanged(string propertyName) =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

    protected void ClearErrors()
    {
        foreach (var propertyName in Errors.Keys.ToList())
        {
            Errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
    }

    protected void Validate()
    {
        ClearErrors();

        var results = new List<ValidationResult>();
        var context = new ValidationContext(this);
        Validator.TryValidateObject(this, context, results, true);

        if (results.Any())
        {
            var propertyNames = results.SelectMany(r => r.MemberNames).Distinct().ToList();

            foreach (var propertyName in propertyNames)
            {
                Errors[propertyName] = results
                    .Where(r => r.MemberNames.Contains(propertyName))
                    .Select(r => r.ErrorMessage)
                    .Distinct()
                    .ToList();
                OnErrorsChanged(propertyName);
            }
        }
        OnPropertyChanged(nameof(HasErrors));
    }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
