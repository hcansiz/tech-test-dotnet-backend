namespace Moonpig.PostOffice.Tests
{
    using System;
    using System.Collections.Generic;
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
        /// Verifies that a single product with 1-day lead time is dispatched next working day.
        /// </summary>
        [Fact]
        public void OneProductWithLeadTimeOfOneDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(1));
        }

        /// <summary>
        /// Verifies that a single product with 2-day lead time is dispatched after 2 working days.
        /// </summary>
        [Fact]
        public void OneProductWithLeadTimeOfTwoDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 2 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(2));
        }

        /// <summary>
        /// Verifies that a single product with 3-day lead time is dispatched after 3 working days.
        /// </summary>
        [Fact]
        public void OneProductWithLeadTimeOfThreeDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 3 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(3));
        }

        /// <summary>
        /// Verifies that orders arriving on Friday (at Moonpig) are dispatched on Monday due to weekend closure.
        /// </summary>
        [Fact]
        public void SaturdayHasExtraTwoDays()
        {
            var orderDate = new DateTime(2018, 1, 26); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 29)); // Monday
        }

        /// <summary>
        /// Verifies that null product IDs list throws appropriate validation exception.
        /// </summary>
        [Fact]
        public void ThrowsExceptionWhenProductIdsIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _despatchDateService.CalculateDespatchDate(null, DateTime.Now));
            exception.Message.ShouldStartWith("At least one product must be specified");
        }

        /// <summary>
        /// Verifies that empty product IDs list throws appropriate validation exception.
        /// </summary>
        [Fact]
        public void ThrowsExceptionWhenProductIdsIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _despatchDateService.CalculateDespatchDate(new List<int>(), DateTime.Now));
            exception.Message.ShouldStartWith("At least one product must be specified");
        }

        /// <summary>
        /// Verifies that invalid product ID throws user-friendly error message.
        /// </summary>
        [Fact]
        public void ThrowsExceptionWhenProductIdNotFound()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _despatchDateService.CalculateDespatchDate(new List<int> { 999 }, DateTime.Now));
            exception.Message.ShouldStartWith("Product with ID 999 is not available");
        }

        /// <summary>
        /// Verifies that when a product is not available from any supplier, an appropriate exception is thrown.
        /// </summary>
        [Fact]
        public void ThrowsExceptionWhenSupplierNotFound()
        {

            var exception = Should.Throw<ArgumentException>(() =>
                _despatchDateService.CalculateDespatchDate(new List<int> { 8 }, DateTime.Now));
            exception.Message.ShouldStartWith("Product with ID 8 is currently unavailable");
        }

        /// <summary>
        /// Verifies that an order on Friday with two-day lead time arrives on Tuesday.
        /// </summary>
        [Fact]
        public void OrderOnFridayWithTwoDayLeadTimeArrivesOnTuesday()
        {
            var orderDate = new DateTime(2018, 1, 5); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 2 }, orderDate); // Product 2 has 2-day lead time
            result.ShouldBe(new DateTime(2018, 1, 9)); // Tuesday
        }

        /// <summary>
        /// Verifies that an order on Friday with one-day lead time arrives on Monday.
        /// </summary>
        [Fact]
        public void OrderOnFridayWithOneDayLeadTimeArrivesOnMonday()
        {
            var orderDate = new DateTime(2018, 1, 5); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, orderDate); // Product 1 has 1-day lead time
            result.ShouldBe(new DateTime(2018, 1, 8)); // Monday
        }

        /// <summary>
        /// Verifies that an order on Saturday with one-day lead time arrives on Monday.
        /// </summary>
        [Fact]
        public void OrderOnSaturdayWithOneDayLeadTimeArrivesOnMonday()
        {
            var orderDate = new DateTime(2018, 1, 6); // Saturday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, orderDate); // Product 1 has 1-day lead time
            result.ShouldBe(new DateTime(2018, 1, 8)); // Monday
        }

        /// <summary>
        /// Verifies that an order on thursday with three-day lead time skips the weekend and arrives on Tuesday.
        /// </summary>
        [Fact]
        public void OrderOnThursdayWithThreeDayLeadTimeSkipsWeekend()
        {
            var orderDate = new DateTime(2018, 1, 25); // Thursday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 3 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 30)); // Tuesday
        }

        /// <summary>
        /// Verifies that an order on Sunday with one-day lead time arrives on Monday (first working day).
        /// </summary>
        [Fact]
        public void OrderOnSundayWithOneDayLeadTimeArrivesOnMonday()
        {
            var orderDate = new DateTime(2018, 1, 7); // Sunday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 8)); // Monday (first working day)
        }

        /// <summary>
        /// Verifies that multiple products use the longest lead time for despatch calculation.
        /// </summary>
        [Fact]
        public void MultipleProductsUsesLongestLeadTime()
        {
            var orderDate = new DateTime(2018, 1, 1); // Monday
            // Product 1 = 1 day, Product 2 = 2 days, Product 3 = 3 days
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1, 2, 3 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 4)); // Thursday (3 working days)
        }

        /// <summary>
        /// Verifies that an order on Friday with six-day lead time (Disney) is calculated correctly.
        /// </summary>
        [Fact]
        public void OrderOnFridayWithSixDayLeadTimeCalculatesCorrectly()
        {
            var orderDate = new DateTime(2018, 1, 5); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 9 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 15));  // Monday (6 working days later)
        }

        /// <summary>
        /// Verifies that an order on Friday with 13-day lead time (Tacky T-Shirts) is calculated correctly.
        /// </summary>
        [Fact]
        public void OrderOnFridayWithThirteenDayLeadTimeCalculatesCorrectly()
        {
            var orderDate = new DateTime(2018, 1, 5); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 10 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 24)); // Wednesday (13 working days later)
        }
    }
}
