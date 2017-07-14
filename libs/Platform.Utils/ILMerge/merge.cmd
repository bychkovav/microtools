CALL %~dp0\ILMerge.exe /closed^
	 %~dp0\..\..\Platform.Utils.Owin\bin\Release\Platform.Utils.Owin.dll^
	 %~dp0\..\..\Platform.Utils.ServiceBus.CassandraRetry\bin\Release\Platform.Utils.ServiceBus.CassandraRetry.dll^
	 %~dp0\..\..\Platform.Utils.ServiceBus\bin\Release\Platform.Utils.ServiceBus.dll^
	 %~dp0\..\..\Platform.Utils.Domain\bin\Release\Platform.Utils.Domain.dll^
	 %~dp0\..\..\Platform.Utils.IoC\bin\Release\Platform.Utils.IoC.dll^
     %~dp0\..\..\Platform.Utils.Cassandra\bin\Release\Platform.Utils.Cassandra.dll^
     %~dp0\..\..\Platform.Utils.Hangfire\bin\Release\Platform.Utils.Hangfire.dll^
     %~dp0\..\..\Platform.Utils.Json\bin\Release\Platform.Utils.Json.dll^
     %~dp0\..\..\Platform.Utils.Kafka\bin\Release\Platform.Utils.Kafka.dll^
     %~dp0\..\..\Platform.Utils.MongoDb\bin\Release\Platform.Utils.MongoDb.dll^
     %~dp0\..\..\Platform.Utils.Nhibernate\bin\Release\Platform.Utils.Nhibernate.dll^
     %~dp0\..\..\Platform.Utils.NLog\bin\Release\Platform.Utils.NLog.dll^
     %~dp0\..\..\Platform.Utils.Redis\bin\Release\Platform.Utils.Redis.dll^
     %~dp0\..\..\Platform.Utils.Rpc\bin\Release\Platform.Utils.Rpc.dll^
	 /out:%~dp0\..\bin\Release\Platform.Utils.dll