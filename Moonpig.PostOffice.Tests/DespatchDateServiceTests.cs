namespace Moonpig.PostOffice.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Services;
    using Services.Interfaces;
    using Shouldly;
    using Xunit;

    public class DespatchDateServiceTests
    {
        private readonly IDespatchDateService _despatchDateService;

        public DespatchDateServiceTests()
        {
            _despatchDateService = new DespatchDateService();
        }

        /// <summary>
        /// Verifies that despatch dates are calculated correctly for various scenarios.
        /// </summary>
        [Theory]
        // Simple weekday orders
        [InlineData("2018-01-01", new int[] { 1 }, "2018-01-02")] // Monday + 1 day = Tuesday
        [InlineData("2018-01-01", new int[] { 2 }, "2018-01-03")] // Monday + 2 days = Wednesday
        [InlineData("2018-01-01", new int[] { 3 }, "2018-01-04")] // Monday + 3 days = Thursday
        // Weekend handling - Moonpig closure
        [InlineData("2018-01-26", new int[] { 1 }, "2018-01-29")] // Friday order arrives Saturday, dispatched Monday
        // Weekend handling - Supplier doesn't work weekends
        [InlineData("2018-01-05", new int[] { 1 }, "2018-01-08")] // Friday + 1 working day = Monday
        [InlineData("2018-01-05", new int[] { 2 }, "2018-01-09")] // Friday + 2 working days = Tuesday
        [InlineData("2018-01-06", new int[] { 1 }, "2018-01-08")] // Saturday + 1 working day = Monday
        [InlineData("2018-01-07", new int[] { 1 }, "2018-01-08")] // Sunday + 1 working day = Monday
        [InlineData("2018-01-25", new int[] { 3 }, "2018-01-30")] // Thursday + 3 working days = Tuesday (skip weekend)
        // Multiple products - max lead time wins
        [InlineData("2018-01-01", new int[] { 1, 2, 3 }, "2018-01-04")] // Max lead time of 3 days
        // Multi-week lead times
        [InlineData("2018-01-05", new int[] { 9 }, "2018-01-15")]  // Friday + 6 working days = Monday
        [InlineData("2018-01-05", new int[] { 10 }, "2018-01-24")] // Friday + 13 working days = Wednesday
        public void CalculateDespatchDate_ValidInputs_ReturnsCorrectDate(string orderDateStr, int[] productIds, string expectedDateStr)
        {
            var orderDate = DateTime.Parse(orderDateStr);
            var expectedDate = DateTime.Parse(expectedDateStr);
            var result = _despatchDateService.CalculateDespatchDate(productIds.ToList(), orderDate);
            result.ShouldBe(expectedDate);
        }

        /// <summary>
        /// Verifies that appropriate exceptions are thrown for invalid inputs.
        /// </summary>
        [Theory]
        [InlineData(null, "At least one product must be specified")]
        [InlineData(new int[] { }, "At least one product must be specified")]
        [InlineData(new int[] { 999 }, "Product with ID 999 is not available")]
        [InlineData(new int[] { 8 }, "Product with ID 8 is currently unavailable")]
        public void CalculateDespatchDate_InvalidInputs_ThrowsException(int[] productIds, string expectedErrorMessage)
        {
            var exception = Should.Throw<ArgumentException>(() =>
            _despatchDateService.CalculateDespatchDate(productIds?.ToList(), DateTime.Now));
            exception.Message.ShouldStartWith(expectedErrorMessage);
        }
    }
}