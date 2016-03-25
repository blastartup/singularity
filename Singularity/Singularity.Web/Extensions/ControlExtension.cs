﻿using System.IO;
using System.Text;
using System.Web.UI;

// ReSharper disable once CheckNamespace

namespace Singularity.Web
{
	public static class ControlExtension
	{
		public static string ToHtml(this Control selectedControl)
		{
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb))
			{
				using (HtmlTextWriter textWriter = new HtmlTextWriter(sw))
				{
					try { selectedControl.RenderControl(textWriter); }
					catch (System.Web.HttpException) { }
				}
			}
			return sb.ToString();
		}

	}
}