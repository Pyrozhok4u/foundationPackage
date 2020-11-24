using System;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;

namespace Foundation.AssetBundles
{
	public interface IAssetBundlesService : IService
	{
		string EmbeddedAssetsFolderPath { get; }
		string CloudStoragePath { get; }
		string CachedFolderPath { get; }

		void LoadAsset<T>(string asset, Action<Result<T>> onComplete) where T : UnityEngine.Object;
		void LoadAsset<T>(string asset, string fallbackAsset, Action<Result<T>> onComplete) where T : UnityEngine.Object;

		bool AssetExists(string assetName);
	}
}
