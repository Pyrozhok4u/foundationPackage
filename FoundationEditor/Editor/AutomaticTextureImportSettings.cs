using System.Collections.Generic;
using System.Reflection;
using Foundation.Enforcers;
using Foundation.Logger;
using FoundationEditor.Utils.Editor.BuildUtils;
using UnityEditor;
using UnityEngine;

namespace FoundationEditor.Enforcers.Editor
{
    public class AutomaticTextureImportSettings : AssetPostprocessor
    {
        private enum Platforms { Android, iPhone, WebGL };

        private const int MAX_TEXTURE_SIZE = 2048;
        private const int MIN_TEXTURE_SIZE = 32;
        private const int COMPRESSION_QUALITY = 50;
        private const bool ENABLE_MIPMAPS = false;
        private const bool ALLOW_ALPHA_SPLITTING = false;
        private const TextureWrapMode WRAP_MODE = TextureWrapMode.Clamp;

        // A white list of support texture compressions formats that will NOT be over-written
        private List<TextureImporterFormat> _supportedFormats = new List<TextureImporterFormat>()
        {
            TextureImporterFormat.Alpha8
        };

        private void OnPreprocessTexture()
        {
            // Make sure we are not in build mode (we don't want the build change import settings)
            if (assetImporter == null || EditorBuildUtils.IsBatchMode) { return; }

            SetTextureSetting((TextureImporter) assetImporter);
        }

        private void SetTextureSetting(TextureImporter importer)
        {
            bool firstImport = importer.importSettingsMissing;
            // Set texture settings only for the first time texture is imported (or if its meta was deleted)
            if(!firstImport) { return; }

            importer.alphaIsTransparency = importer.DoesSourceTextureHaveAlpha();
            importer.mipmapEnabled = ENABLE_MIPMAPS;
            
            // Wrap mode Repeated is needed for ConversionTool.
            importer.wrapMode = AutomaticTextureImportConfig.TextureWrapMode;
            
            // Set initial max texture size according to the native image size
            int width, height;
            GetNativeTextureSize(importer, out width, out height);
            importer.maxTextureSize = CeilToPowerOfTwo(Mathf.Max(width, height));

            SetPlatformTextureImportSettings(importer, Platforms.Android, TextureImporterFormat.ETC2_RGBA8,
                TextureImporterFormat.ETC_RGB4);
            SetPlatformTextureImportSettings(importer, Platforms.iPhone, TextureImporterFormat.PVRTC_RGBA4,
                TextureImporterFormat.PVRTC_RGB4);
            SetPlatformTextureImportSettings(importer, Platforms.WebGL, TextureImporterFormat.DXT5,
                TextureImporterFormat.DXT1);
        }

        private void SetPlatformTextureImportSettings(TextureImporter importer, Platforms platform,
            TextureImporterFormat alphaCompression, TextureImporterFormat nonAlphaCompression)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform.ToString());
            // override texture compression format only if the current format is not supported
            if (!IsCompressionFormatSupported(settings.format))
            {
                settings.format = importer.alphaIsTransparency ? alphaCompression : nonAlphaCompression;
            }

            settings.maxTextureSize = Mathf.Min(importer.maxTextureSize, settings.maxTextureSize);
            if (!settings.overridden)
            {
                settings.compressionQuality = COMPRESSION_QUALITY;
                settings.allowsAlphaSplitting = ALLOW_ALPHA_SPLITTING;
                settings.overridden = true;
            }

            // Validate texture size do not exceed max size
            if (settings.maxTextureSize > MAX_TEXTURE_SIZE)
            {
                Debugger.LogError(this,"Texture exceeds maximum size! Reverting to: " + MAX_TEXTURE_SIZE);
                settings.maxTextureSize = MAX_TEXTURE_SIZE;
            }

            // Finally, set the updated import settings
            importer.SetPlatformTextureSettings(settings);
        }

        private bool IsCompressionFormatSupported(TextureImporterFormat format) => _supportedFormats.Contains(format);

        /// <summary>
        /// Get the native image size
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private void GetNativeTextureSize(TextureImporter importer, out int width, out int height)
        {
            object[] args = new object[2] {0, 0};
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", flags);
            mi.Invoke(importer, args);
            width = (int) args[0];
            height = (int) args[1];
        }

        /// <summary>
        /// Mapping function to nearest p^2 higher or equal to the given size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int CeilToPowerOfTwo(int size)
        {
            int newSize;
            for (newSize = MAX_TEXTURE_SIZE; newSize / 2 >= size && newSize != MIN_TEXTURE_SIZE; newSize /= 2) { }

            return newSize;
        }
    }
}
