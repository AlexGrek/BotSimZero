using SimuliEngine.Basic;

namespace SImuliTests
{
    [TestClass]
    public sealed class Hypercell
    {
        readonly int size = 128;
        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
        }

        [TestMethod]
        public void TestHypercellAccessEmpty()
        {
            var m = new HyperMap<float>(size);
            var all = m.AllHypercells();
            Assert.AreEqual(0, all.Count());
        }

        [TestMethod]
        public void TestHypercellAccess()
        {
            var m = new HyperMap<float>(size);
            m.GetOrCreateHypercell(4, -1);
            var all = m.AllHypercells();
            Assert.AreEqual(1, all.Count());
        }
    }
}
