namespace Platform.Template.Tests
{
    using NUnit.Framework;
    using Utils.Ioc;

    public abstract class TestBase
    {
        static TestBase()
        {
            IocContainerProvider.InitIoc();
        }

        [SetUp]
        public virtual void SetUp()
        {
        }

        [TearDown]
        public virtual void TearDown()
        {
        }
    }
}
