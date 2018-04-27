using System.Linq;
using Example.BusinessLogic.Helpers;
using NUnit.Framework;

namespace Example.Business.LogicTests.Helpers
{
    [TestFixture]
    public class InputSanitiserTests
    {
        [Test]
        public void ExtractWordsFromInputTest()
        {
            var sanitiser = new InputSanitiser();
            var words = sanitiser.ExtractWordsFromInput("Gyorsabb, garantált5*1,   számlakézbesítés.........").ToList();

            Assert.AreEqual(words.Count, 3);
        }
    }
}