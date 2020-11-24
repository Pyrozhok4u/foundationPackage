using Foundation.ConfigurationResolver;
using Foundation.Network;
using FoundationEditor.BuildPipelineService.Editor.ClientBuild;

namespace FoundationEditor.BestHTTP.Editor
{
	public class BestHTTPBaseBuilder : BaseBuilder
	{

		internal override PrebuildResult Prebuild()
		{
			PrebuildResult result = new PrebuildResult();
			result.AddDefineSymbol("BESTHTTP_DISABLE_SERVERSENT_EVENTS");
			result.AddDefineSymbol("BESTHTTP_DISABLE_SIGNALR");
			result.AddDefineSymbol("BESTHTTP_DISABLE_SIGNALR_CORE");
			result.AddDefineSymbol("BESTHTTP_DISABLE_ALTERNATE_SSL");
			return result;
		}

		internal override void PostBuild()
		{

		}

		internal override BaseConfig GetConfig()
		{
			return ConfigResolver.GetConfig<HTTPServiceConfig>();
		}
	}
}
