using FluentValidation.Results;

namespace GotExplorer.BLL.Services.Results
{
    public class ValidationWithEntityModel<TData> : ValidationResult
    {
        public TData? Entity { get; set; }
        
        public ValidationWithEntityModel() : base() { }

        public ValidationWithEntityModel(TData? entity)
        {
            Entity = entity;
        }

        public ValidationWithEntityModel(ValidationResult validationResult)
        {
            Errors = validationResult.Errors;
            RuleSetsExecuted = validationResult.RuleSetsExecuted;
        }
    }
}
