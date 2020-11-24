using Foundation.ConfigurationResolver;
using UnityEngine;

namespace FoundationEditor.AWSUtils.Editor
{
	public class S3Config : BaseConfig
	{
		public string IdentityPoolId;
		public string BucketName;
		public string LocalFolderName;
	}
}
