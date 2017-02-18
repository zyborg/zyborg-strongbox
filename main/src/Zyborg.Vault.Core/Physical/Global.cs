using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Physical
{
	public class Global
	{
		//// BuiltinBackends is the list of built-in physical backends that can
		//// be used with NewBackend.
		//var builtinBackends = map[string]Factory{
		//	"inmem": func(_ map[string]string, logger log.Logger) (Backend, error) {
		//		return NewInmem(logger), nil
		//	},
		//	"inmem_ha": func(_ map[string]string, logger log.Logger) (Backend, error) {
		//		return NewInmemHA(logger), nil
		//	},
		//	"consul":     newConsulBackend,
		//	"zookeeper":  newZookeeperBackend,
		//	"file":       newFileBackend,
		//	"s3":         newS3Backend,
		//	"azure":      newAzureBackend,
		//	"dynamodb":   newDynamoDBBackend,
		//	"etcd":       newEtcdBackend,
		//	"mysql":      newMySQLBackend,
		//	"postgresql": newPostgreSQLBackend,
		//	"swift":      newSwiftBackend,
		//	"gcs":        newGCSBackend,
		//}

		public static readonly IReadOnlyDictionary<string, IBackendFactory> BuiltinBackends =
				new Dictionary<string, IBackendFactory>
				{
					["inmem"] = InmemBackendFactory.INSTANCE,
					["file"] = FileBackendFactory.INSTANCE,
				};


		// NewBackend returns a new backend with the given type and configuration.
		// The backend is looked up in the builtinBackends variable.
		// func NewBackend(t string, logger log.Logger, conf map[string]string) (Backend, error) {
		//     f, ok := builtinBackends[t]
		//     if !ok {
		//         return nil, fmt.Errorf("unknown physical backend type: %s", t)
		//     }
		//     return f(conf, logger)
		// }
		public static IBackend NewBackend(string t, ILogger logger, IReadOnlyDictionary<string, string> conf)
		{
			if (!BuiltinBackends.TryGetValue(t, out var f))
				throw new Exception($"unknown physical backend type: {t}");

			return f.CreateBackend(conf, logger);
		}
	}
}
