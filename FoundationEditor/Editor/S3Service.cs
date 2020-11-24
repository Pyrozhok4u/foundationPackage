using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Foundation.ConfigurationResolver;
using Foundation.Logger;
using Foundation.ServicesResolver;
using Foundation.Utils.OperationUtils;
using UnityEngine;

namespace FoundationEditor.AWSUtils.Editor
{
	public class S3Service : BaseService, ICloudStorage
	{
		protected string _bucketName;
		protected string _localFolderName;
		protected string _identityPoolId;
		private string _cognitoIdentityRegion = RegionEndpoint.USEast1.SystemName;
		private string _s3Region = RegionEndpoint.USEast1.SystemName;
		private AWSCredentials _credentials;
		protected S3Config _config;
		private IAmazonS3 _client;
		protected GameObject Initializer;

		private string S3Dirrectory => Application.persistentDataPath + Path.AltDirectorySeparatorChar + _localFolderName;
		private RegionEndpoint CognitoIdentityRegion => RegionEndpoint.GetBySystemName(_cognitoIdentityRegion);
		private RegionEndpoint S3Region => RegionEndpoint.GetBySystemName(_s3Region);

		#region Service base methods
		protected override void Initialize()
		{
			CreateUnityInitializer();
			
			AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
			_config = GetConfig<S3Config>();
			_identityPoolId = _config.IdentityPoolId;
			_bucketName = _config.BucketName;
			_localFolderName = _config.LocalFolderName;
			_credentials = new CognitoAWSCredentials(_identityPoolId, CognitoIdentityRegion);
			_client = new AmazonS3Client(_credentials, S3Region);
			
		}

		protected virtual void CreateUnityInitializer()
		{
			Initializer = new GameObject("Unity Initializer");
			UnityInitializer.AttachToGameObject(Initializer);
		}

		public override void Dispose()
		{
			if (Initializer != null) { GameObject.Destroy(Initializer); }
			_identityPoolId = _bucketName = string.Empty;
			_config = null;
			_credentials = null;
			_client = null;
		}
		
		#endregion

		#region Service API

		/// <summary>
		/// Download file content from S3
		/// </summary>
		/// <param name="bucketFilePath">Should contain full path to file on storage including  bucket folders and file extension</param>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void DownloadFile(string bucketFilePath, Action<Result<string>> onComplete = null)
		{
			string name = NameFromPath(bucketFilePath);
			_client.GetObjectAsync(_bucketName, bucketFilePath, (responseObject) =>
			{
				Result<string> result = new Result<string>();
				if (responseObject.Exception != null)
				{
					this.LogException(responseObject.Exception);
					result.SetFailure("Upload file failed - see logs for full exception " + responseObject.Exception.Message);
					onComplete?.Invoke(result);
					return;
				}

				var response = responseObject.Response;
				if (response.ResponseStream != null)
				{
					using (StreamReader reader = new StreamReader(response.ResponseStream))
					{
						string data = reader.ReadToEnd();
						result.Data = data;
					}
				}
				else
				{
					result.SetFailure("Responce stream is empty!");
				}
				onComplete?.Invoke(result);
			});
		}

		public void DownloadBytes(string bucketFilePath, Action<Result<byte[]>> onComplete = null)
		{
			string name = NameFromPath(bucketFilePath);
			_client.GetObjectAsync(_bucketName, bucketFilePath, (responseObject) =>
			{
				Result<byte[]> result = new Result<byte[]>();
				if (responseObject.Exception != null)
				{
					this.LogException(responseObject.Exception);
					result.SetFailure("Upload file failed - see logs for full exception " + responseObject.Exception.Message);
					onComplete?.Invoke(result);
					return;
				}

				var response = responseObject.Response;
				if (response.ResponseStream != null)
				{
					using (MemoryStream reader = new MemoryStream())
					{
						response.ResponseStream.CopyTo(reader);
						result.Data = reader.ToArray();
					}
				}
				else
				{
					result.SetFailure("Responce stream is empty!");
				}
				onComplete?.Invoke(result);
			});
		}

		public void UploadFiles(List<string> filesPaths, string bucketFolderPath, S3CannedACL permissions, Action<Result> onComplete)
		{
			OperationsQueue queue = new OperationsQueue();
			foreach (string path in filesPaths)
			{
				queue.And(delegate(Action<Result> complete)
				{
					UploadFileFromDisk(path, bucketFolderPath, permissions, complete);
				});
			}

			queue.Finally(onComplete);
			queue.Run();
		}
		
		/// <summary>
		/// Upload any file to S3
		/// </summary>
		/// <param name="localFilePath">Should contain full path to file on local machine including folders and file extension</param>
		/// <param name="bucketFolderPath">Should contain folder full path without the file name</param>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void UploadFileFromDisk(string localFilePath, string bucketFolderPath, S3CannedACL permissions, Action<Result> onComplete)
		{
			localFilePath = localFilePath.Replace('\\', '/');
			string fileName = Path.GetFileName(localFilePath);
			Stream stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			UploadFile(stream, fileName, bucketFolderPath, permissions, onComplete);
		}

		/// <summary>
		/// Upload any file to S3
		/// </summary>
		/// <param name="content">The content you wish to upload as a document</param>
		/// <param name="fileName">Should contain full of the file including extension</param>
		/// <param name="bucketFolderPath">Should contain folder full path without the file name</param>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void UploadFileFromMemory(string content, string fileName, string bucketFolderPath, S3CannedACL permissions, Action<Result> onComplete)
		{
			Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
			UploadFile(stream, fileName, bucketFolderPath, permissions, onComplete);
		}

		public void UploadFile(string localFilePath, string path, Action<Result> onComplete = null)
		{
			UploadFileFromDisk(localFilePath, path, S3CannedACL.Private, onComplete);
		}
		
		private void UploadFile(Stream stream, string fileName, string bucketFolderPath, S3CannedACL permissions, Action<Result> onComplete)
		{
			Result result = new Result();
			if (!bucketFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				result.SetFailure("Bucket folder path must end with a '/': " + bucketFolderPath);
				onComplete?.Invoke(result);
				return;
			}
			
			string bucketFilePath = bucketFolderPath + fileName;
			
			// Changed from the sample to include region to fix HttpErrorResponseException 
			var request = new PostObjectRequest()
			{
				Bucket = _bucketName,
				Key = bucketFilePath,
				InputStream = stream,
				CannedACL = permissions,
				Region = S3Region
			};
			_client.PostObjectAsync(request, (responseObject) =>
			{
				if (responseObject.Exception != null)
				{
					// Changed from sample so we can see actual error and not null pointer exception
					this.LogException(responseObject.Exception);
					result.SetFailure("Upload file failed - see logs for full exception " + responseObject.Exception.ToString());
				}
				
				stream.Dispose();
				this.Log("Finish uploading file: " + fileName + " result success: " + result.Success);
				onComplete?.Invoke(result);
			});
		}
		
		/// <summary>
		/// Upload any file to S3
		/// </summary>
		/// <param name="bucketFilePath">Should contain full path to file on storage including  bucket folders and file extension</param>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void DeleteFile(string bucketFilePath, Action<Result> onComplete)
		{
			List<KeyVersion> objects = new List<KeyVersion>();
			objects.Add(new KeyVersion() { Key = bucketFilePath });

			var request = new DeleteObjectsRequest()
			{
				BucketName = _bucketName,
				Objects = objects
			};

			_client.DeleteObjectsAsync(request, (responseObject) =>
			{
				var result = new Result();
				if (responseObject.Exception == null)
				{
					responseObject.Response.DeletedObjects.ForEach((dObj) => this.Log("Object deleted: " + dObj.Key));
				} else
				{
					this.LogException(responseObject.Exception);
					result.SetFailure("Delete file failed - see logs for full exception " + responseObject.Exception.Message);
				}
				onComplete?.Invoke(result);
			});
		}

		/// <summary>
		/// Method to receive all buckets available for configured pool id
		/// </summary>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void GetBucketList(Action<Result> onComplete)
		{
			_client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
			{
				var result = new Result();
				if (responseObject.Exception == null)
				{
					responseObject.Response.Buckets.ForEach((s3b) =>
					{
						result.AddMessage("Bucket: " + s3b.BucketName + " created date: " + s3b.CreationDate);
					});
				} else
				{
					this.LogException(responseObject.Exception);
					result.SetFailure("Get bucket list failed. See logs for more details " + responseObject.Exception.Message);
				}
				onComplete?.Invoke(result);
			});
		}

		/// <summary>
		///  Method to receive all objects in selected bucket
		/// </summary>
		/// <param name="onComplete">Will be executed after the responce received. Result can contain additional messages</param>
		public void GetObjectsList(Action<Result> onComplete, string prefix = null)
		{
			var request = new ListObjectsRequest()
			{
				BucketName = _bucketName
			};

			request.Prefix = prefix;

			_client.ListObjectsAsync(request, (responseObject) =>
			{
				var result = new Result();
				if (responseObject.Exception == null)
				{
					responseObject.Response.S3Objects.ForEach((o) =>
					{
						result.AddMessage(o.Key);
					});
				} else
				{
					this.LogException(responseObject.Exception);
					result.SetFailure(responseObject.Exception.ToString());
				}
				onComplete?.Invoke(result);
			});
		}
		#endregion

		#region Helpers
		private string GetDownloadPath(string fileName)
		{
			if (!Directory.Exists(S3Dirrectory))
			{
				Directory.CreateDirectory(S3Dirrectory);
			}
			return S3Dirrectory + Path.AltDirectorySeparatorChar + fileName;
		}

		public string NameFromPath(string bucketFilePath)
		{
			int index = bucketFilePath.LastIndexOf(Path.AltDirectorySeparatorChar) + 1;
			return bucketFilePath.Substring(index, bucketFilePath.Length - index);
		}
		#endregion
	}
}
