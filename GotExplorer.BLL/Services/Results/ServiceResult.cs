using FluentValidation.Results;

namespace GotExplorer.BLL.Services.Results
{
    public class ServiceResult<T>
    {
        public T? ResultObject { get; set; }
        public ValidationResult? ValidationResult { get; set; } = null;
        public bool IsSuccess
        {
            get
            {
                return (ValidationResult == null) || (ValidationResult != null && ValidationResult.IsValid);
            }
        }

        public static ServiceResult<T> Success(T? obj) 
        {
            return new ServiceResult<T> { ResultObject = obj };
        }
        public static ServiceResult<T> Failure(ValidationResult? result) 
        {
            return new ServiceResult<T> { ValidationResult = result };
        }
    }

    public class ServiceResult
    {
        public Error Error { get; set; } = Error.None;
        public bool IsSuccess
        {
            get
            {
                return (Error.ValidationResult == null) || (Error.ValidationResult != null && Error.ValidationResult.IsValid);
            }
        }
    }
}
