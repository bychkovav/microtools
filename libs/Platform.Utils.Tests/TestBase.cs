namespace Platform.Utils.Tests
{
    using Ioc;
    using NUnit.Framework;

    public abstract class TestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            IocContainerProvider.InitIoc();
        }

        [TearDown]
        public virtual void TearDown()
        {
        }
    }
}
