using System.Reflection;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Kernel;

namespace Example.UnitTesting.Utilities
{
    public class AutoPopulatedMoqPropertiesCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(
                new PropertiesPostprocessor(
                    new MockPostprocessor(
                        new MethodInvoker(
                            new MockConstructorQuery()))));

            fixture.ResidueCollectors.Add(
                new Postprocessor(
                    new MockRelay(),
                    new AutoPropertiesCommand(
                        new PropertiesOnlySpecification())));
        }

        private class PropertiesOnlySpecification : IRequestSpecification
        {
            public bool IsSatisfiedBy(object request)
            {
                return request is PropertyInfo;
            }
        }
    }

    public class PropertiesPostprocessor : ISpecimenBuilder
    {
        private readonly ISpecimenBuilder _builder;

        public PropertiesPostprocessor(ISpecimenBuilder builder)
        {
            _builder = builder;
        }

        public object Create(object request, ISpecimenContext context)
        {
            dynamic s = _builder.Create(request, context);

            if (s is NoSpecimen)
                return s;

            s.SetupAllProperties();

            return s;
        }
    }
}