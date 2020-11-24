using Foundation.ConfigurationResolver;
using Foundation.Logger;
using UnityEngine;

namespace Foundation.Network
{
	[CreateAssetMenu(fileName = "InternetConnectionConfig", menuName = "Foundation/Config/Create Internet Connection Config")]
	public class InternetConnectionConfig : BaseConfig
	{
		public string DNS;
		[SerializeField] [Range(5, 10000)] public int Interval;
		[SerializeField] [Range(5, 100)] public int PingMaxIterations;
		[SerializeField] [Range(5, 100)] public int DisconnectionTreshold;
		[SerializeField] [Range(5, 10000)] public int ConnectionSpeedThreshold;
		[SerializeField] [Range(5, 10000)] public int ConnectionCheckInterval;
	}
}
