using Foundation.ConfigurationResolver;
using Foundation.ServicesResolver;

namespace Foundation.Logger
{
	public class LoggerInitializer : BaseService
	{
		protected override void Initialize()
		{
			Debugger.IsErrorsEnabled = GetConfig<LoggerConfig>().IsErrorsEnabled;
		}

		public override void Dispose() { }
	}
}
