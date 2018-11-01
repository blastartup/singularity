using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Singularity.WinForm.Extensions
{
	public static class ControlExtension
	{
		public static void InitTag(this Control control, params Object[] objects)
		{
			control.Tag = objects;
		}

		public static Object GetTag(this Control control, Int32 index)
		{
			if (control.Tag.GetType() != typeof(Object[]))
			{
				return null;
			}

			var objects = control.Tag as Object[];

			if (index < 0
			    || index >= objects.Length)
			{
				return null;
			}

			return objects[index];
		}

		public static void SetTag(this Control control, Int32 index, Object value)
		{
			if (control.Tag.GetType() != typeof(Object[]))
			{
				return;
			}

			var objects = control.Tag as Object[];

			if (index < 0 || index >= objects.Length)
			{
				return;
			}

			objects[index] = value;
		}
	}
}
