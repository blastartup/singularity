using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Singularity.FileService
{
	/// <summary>
	/// Extension class for the FileInfo.
	/// </summary>
	public static class FileInfoExtension
	{
		public static FileInfo GetUniqueName(this FileInfo fileInfo)
		{
			if (fileInfo.Exists)
			{
				FileInfo uniqueFileInfo;
				var fileVersionCounter = 1;
				do
				{
					uniqueFileInfo = new FileInfo(Path.Combine(fileInfo.DirectoryName, $"{fileInfo.NameSansExtension()} ({fileVersionCounter}){fileInfo.Extension}"));
					fileVersionCounter++;
				} while (uniqueFileInfo.Exists);

				return uniqueFileInfo;
			}

			return fileInfo;
		}

		public static Boolean CopyToRetry(this FileInfo sourceFileInfo, String destinationFileName)
		{
			var retryCounter = 0;
			var exitRetry = false;
			do
			{
				try
				{
					sourceFileInfo.CopyTo(destinationFileName);
					return true;
				}
				catch (IOException e)
				{
					if (e.HResult == -2147024864)
					{
						retryCounter++;
						if (retryCounter < 4)
						{
							Thread.Sleep(60);
							continue;
						}
					}

					exitRetry = true;
				}

			} while (!exitRetry);

			return false;
		}

		public static Boolean MoveToRetry(this FileInfo sourceFileInfo, String destinationFileName)
		{
			var retryCounter = 0;
			var exitRetry = false;
			do
			{
				try
				{
					sourceFileInfo.MoveTo(destinationFileName);
					return true;
				}
				catch (IOException e)
				{
					if (e.HResult == -2147024864)
					{
						retryCounter++;
						if (retryCounter < 4)
						{
							Thread.Sleep(60);
							continue;
						}
					}

					exitRetry = true;
				}

			} while (!exitRetry);

			return false;
		}

		public static Boolean DeleteRetry(this FileInfo sourceFileInfo)
		{
			var retryCounter = 0;
			var exitRetry = false;
			do
			{
				try
				{
					sourceFileInfo.Delete();
					return true;
				}
				catch (IOException e)
				{
					if (e.HResult == -2147024864)
					{
						retryCounter++;
						if (retryCounter < 4)
						{
							Thread.Sleep(60);
							continue;
						}
					}

					exitRetry = true;
				}

			} while (!exitRetry);

			return false;
		}

		public static Int32 Unzip(this FileInfo zipFileInfo, DirectoryInfo destinationDirectoryInfo, Action<String> addIssue)
		{
			ZipFile zipFile;
			try
			{
				zipFile = new ZipFile(zipFileInfo.FullName);
			}
			catch (ZipException e)
			{
				addIssue?.Invoke(e.Message);
				return 0;
			}

			var unzippedFileCounter = 0;
			FileInfo zipEntryFileInfo;
			foreach (ZipEntry zipEntry in zipFile)
			{
				if (zipEntry.IsFile)
				{
					zipEntryFileInfo = new FileInfo(zipEntry.Name.Left(100));
					zipEntryFileInfo = GetUniqueName(new FileInfo(Path.Combine(destinationDirectoryInfo.FullName, zipEntryFileInfo.Name)));
					if (!zipEntryFileInfo.Directory.Exists)
					{
						zipEntryFileInfo.Directory.CreateSafely();
					}

					var buffer = new Byte[4096];
					Stream zipStream;
					FileStream streamWriter = null;
					try
					{
						zipStream = zipFile.GetInputStream(zipEntry);
						streamWriter = File.Create(zipEntryFileInfo.FullName);
						StreamUtils.Copy(zipStream, streamWriter, buffer);
						unzippedFileCounter++;
					}
					catch (Exception e)
					{
						addIssue?.Invoke(e.Message);
						continue;
					}
					finally
					{
						streamWriter?.Dispose();
					}
				}
			}

			return unzippedFileCounter;
		}

		public static Encoding GetEncoding(this FileInfo sourceFileInfo)
		{
			var bom = new byte[4];
			using (var file = new FileStream(sourceFileInfo.FullName, FileMode.Open, FileAccess.Read))
			{
				file.Read(bom, 0, 4);
			}

			// Analyze the BOM
			if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0xff && bom[1] == 0xfe)
			{
				if (bom[2] != 0 || bom[3] != 0)
				{
					//UTF-16LE (I think).
					return new UnicodeEncoding(false, true);
				}
				return new UTF32Encoding(false, true);
			}
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
			if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
			return Encoding.ASCII;
		}

		public static async Task CopyToAsync(this FileInfo sourceFileInfo, FileInfo targetFileInfo)
		{
			using (FileStream sourceStream = System.IO.File.Open(sourceFileInfo.FullName, FileMode.Open))
			{
				using (FileStream destinationStream = System.IO.File.Create(targetFileInfo.FullName))
				{
					await sourceStream.CopyToAsync(destinationStream);
				}
			}
		}

		public static async Task WriteAllTextAsync(this FileInfo fileInfo, String text)
		{
			Byte[] plainText = text.ToByteArray();

			using (FileStream sourceStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None,
				 4096, true))
			{
				await sourceStream.WriteAsync(plainText, 0, plainText.Length);
			};
		}

		public static Task WriteAllTextTaskAsync(this FileInfo fileInfo, String text, out FileStream sourceStream)
		{
			Byte[] plainText = text.ToByteArray();

			sourceStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.None,
				4096, true);
			return sourceStream.WriteAsync(plainText, 0, plainText.Length);
		}

		public static async Task<String> ReadAllTextAsync(this FileInfo fileInfo)
		{
			using (FileStream sourceStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read,
				 4096, true))
			{
				StringBuilder sb = new StringBuilder();

				Byte[] buffer = new Byte[0x1000];
				Int32 numRead;
				while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
				{
					String text = Encoding.Unicode.GetString(buffer, 0, numRead);
					sb.Append(text);
				}

				return sb.ToString();
			}

		}

		// Todo - replace switch table by looking up a T4 auto generated enum from a MimeType reference table.
		public static IFileContentType GetFileContentType(this FileInfo documentFile)
		{
			ContentAndFileType result = new ContentAndFileType();
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

		public static Boolean IsValidFilename(this FileInfo fileInfo)
		{
			return fileInfo.Name.IsValidFilename();
		}

		public static Boolean IsFileLocked(this FileInfo fileinfo)
		{
			FileStream stream = null;
			try
			{
				stream = fileinfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				stream?.Close();
			}
			return false;
		}
	}
}
