using System;
using Foundation.ConfigurationResolver;

namespace Foundation.ServicesResolver
{
	public interface IService : IDisposable
	{
		void Init(ServiceResolver serviceResolver, ConfigResolver configResolver);
	}
}
