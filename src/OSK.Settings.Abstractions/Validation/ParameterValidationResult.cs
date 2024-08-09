using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Settings.Abstractions.Validation
{
    public readonly struct ParameterValidationResult
    {
        #region Static

        public static readonly ParameterValidationResult Successful = new(ParameterValidationResultType.Valid);

        #endregion

        #region Variables

        public ParameterValidationResultType ResultType { get; }

        public IEnumerable<string> Errors { get; }

        #endregion

        #region Constructors

        public ParameterValidationResult(params string[] errors)
            : this(errors.Any() ? ParameterValidationResultType.Invalid : ParameterValidationResultType.Valid, errors)
        {
        }

        public ParameterValidationResult(ParameterValidationResultType validationResultType, params string[] errors)
            : this()
        {
            if (validationResultType == ParameterValidationResultType.Valid && errors.Any())
            {
                throw new InvalidOperationException($"Unable to set a parameter validation result type to {ValidationResultType.Valid} if there are errors provided.");
            }

            ResultType = validationResultType;
            Errors = errors;
        }

        #endregion
    }
}
