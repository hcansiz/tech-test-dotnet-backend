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

        [Fact]
        public void OneProductWithLeadTimeOfOneDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(1));
        }

        [Fact]
        public void OneProductWithLeadTimeOfTwoDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 2 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(2));
        }

        [Fact]
        public void OneProductWithLeadTimeOfThreeDay()
        {
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 3 }, DateTime.Now);
            result.Date.ShouldBe(DateTime.Now.Date.AddDays(3));
        }

        [Fact]
        public void SaturdayHasExtraTwoDays()
        {
            var orderDate = new DateTime(2018, 1, 26); // Friday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 1 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 29)); // Monday
        }

        [Fact]
        public void SundayHasExtraDay()
        {
            var orderDate = new DateTime(2018, 1, 25); // Thursday
            var result = _despatchDateService.CalculateDespatchDate(new List<int> { 3 }, orderDate);
            result.ShouldBe(new DateTime(2018, 1, 29)); // Monday (Thursday + 3 days = Sunday, moved to Monday)
        }
    }
}
