using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Validation
{
    public class ValidationResultTests
    {
        #region Constructors

        [Fact]
        public void Constructors_SuccessfulValidationWithErrors_ThrowsInvalidOperationException()
        {
            // Arrange/Act/Assert
            Assert.Throws<InvalidOperationException>(() => new ValidationResult(ValidationResultType.Valid, "Error!"));
        }

        #endregion
    }
}
