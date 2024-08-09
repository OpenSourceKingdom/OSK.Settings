using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Settings.Abstractions.Validation
{
    public readonly struct ValidationResult()
    {
        #region Static

        public static readonly ValidationResult Successful = new(ValidationResultType.Valid);

        #endregion

        #region Variables

        public ValidationResultType ResultType { get; }

        public IEnumerable<string> Errors { get; }

        #endregion

        #region Constructors

        public ValidationResult(params string[] errors)
            : this(errors.Any() ? ValidationResultType.Invalid : ValidationResultType.Valid, errors)
        {
        }

        public ValidationResult(ValidationResultType validationResultType, params string[] errors)
            : this()
        {
            if (validationResultType == ValidationResultType.Valid && errors.Any())
            {
                throw new InvalidOperationException($"Unable to set a validation result type to {ValidationResultType.Valid} if there are errors provided.");
            }

            ResultType = validationResultType;
            Errors = errors;
        }

        #endregion
    }
}
