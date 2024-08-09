using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Validation
{
    public class ParameterValidationResultTests
    {
        #region Constructors

        [Fact]
        public void Constructors_SuccessfulValidationWithErrors_ThrowsInvalidOperationException()
        {
            // Arrange/Act/Assert
            Assert.Throws<InvalidOperationException>(() => new ParameterValidationResult(ParameterValidationResultType.Valid, "Error!"));
        }

        #endregion
    }
}
