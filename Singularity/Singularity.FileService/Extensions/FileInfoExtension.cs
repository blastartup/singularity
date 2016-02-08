using System;
using System.Diagnostics;
using System.IO;

// ReSharper disable once CheckNamespace

namespace Singularity.FileService
{
	/// <summary>
	/// Extension class for the FileInfo.
	/// </summary>
	public static class FileInfoExtension
	{
		// Todo - replace switch table by looking up a T4 auto generated enum from a MimeType reference table.
		public static IFileContentType GetFileContentType(this FileInfo documentFile)
		{
			var result = new ContentAndFileType();
			switch (documentFile.Extension.ToLower())
			{
				case ".doc":
				case ".docx":
					result.ContentType = "application/msword";
					result.EFileType = EFileTypes.Document;
					break;
				case ".jpg":
				case ".jpeg":
					result.ContentType = "image/jpeg";
					result.EFileType = EFileTypes.Image;
					break;
				case ".png":
					result.ContentType = "image/png";
					result.EFileType = EFileTypes.Image;
					break;
				case ".gif":
					result.ContentType = "image/gif";
					result.EFileType = EFileTypes.Image;
					break;
				case ".bmp":
					result.ContentType = "image/bmp";
					result.EFileType = EFileTypes.Image;
					break;
				case ".tif":
				case ".tiff":
					result.ContentType = "image/tiff";
					result.EFileType = EFileTypes.Document;
					break;
				case ".pdf":
					result.ContentType = "application/pdf";
					result.EFileType = EFileTypes.Document;
					break;
				case ".swf":
					result.ContentType = "application/x-shockwave-flash";
					result.EFileType = EFileTypes.Video;
					break;
				case ".xml":
					result.ContentType = "application/xml";
					result.EFileType = EFileTypes.Document;
					break;
			}
			return result;
		}

		public static IFileContent ToFileContent(this FileInfo fileInfo)
		{
			return new FileContent(new SerialisedFileInfo(fileInfo.OpenRead(), fileInfo));
		}
	}
}
