using System.Collections.Generic;
using Ploeh.AutoFixture;

namespace Example.UnitTesting.Utilities
{
    public static class AutoDataUtility
    {
        private static readonly IFixture Fixture = new Fixture().Customize(
            new AutoPopulatedMoqPropertiesCustomization());

        public static T GetInstanceOfType<T>()
        {
            return Fixture.Create<T>();
        }

        public static IEnumerable<T> GetCollectionOfType<T>()
        {
            return Fixture.CreateMany<T>();
        }

        public static IEnumerable<T> GetCollectionOfType<T>(int numberOfItems)
        {
            return Fixture.CreateMany<T>(numberOfItems);
        }
    }
}