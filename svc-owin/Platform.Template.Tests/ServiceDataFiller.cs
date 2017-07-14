namespace Platform.Template.Tests
{
    using System;
    using Data.Repositories;
    using Utils.Ioc;

    public static class ArrayExtensions
    {
        public static object GetRandomItem(this string[] arr)
        {
            return arr[new Random().Next(0, arr.Length - 1)];
        }
    }

    public class ServiceDataFillerTest : TestBase
    {
        private ProxyMock proxyMock;
        private TransactionSlaveRepository transactionSlaveRepository;

        public override void SetUp()
        {
            base.SetUp();
            this.transactionSlaveRepository = IocContainerProvider.CurrentContainer.GetInstance<TransactionSlaveRepository>();
            this.proxyMock = IocContainerProvider.CurrentContainer.GetInstance<ProxyMock>();
        }

        #region Dictionaries

        public string[] FirstNames = new[]
        {
            "Joe",
            "Bill",
            "William",
            "Max",
            "Samantha",
            "Collin",
            "Nad",
            "Laura",
            "Lacey",
            "Thomas",
            "Jack",
            "Kate",
            "Daniel",
            "Jason",
            "Lilly",
        };

        public string[] LastNames = new[]
        {
            "Smith",
            "Jackson",
            "Jonson",
            "Armstrong",
            "Doe",
            "Snow",
            "Fast",
            "Lee",
        };

        #endregion

    }
}
