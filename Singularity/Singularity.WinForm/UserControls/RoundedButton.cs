using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Singularity.WinForm.UserControls
{
	public class RoundedButton : Button
	{
		private Int32 b_radius = 50;
		private Single b_width = 1.75f;
		private Color b_colour = Color.Blue;
		private Color _bOverColour, _bDownColour;
		private Single b_over_width = 0;
		private Single b_down_width = 0;

		public Boolean IsMouseOver { get; private set; }
		private Boolean IsMouseDown { get; set; }

		[Category("Border"), DisplayName("Border Width")]
		public Single BorderWidth
		{
			get => b_width;
			set
			{
				if (b_width.Equals(value)) return;
				b_width = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Over Width")]
		public Single BorderOverWidth
		{
			get => b_over_width;
			set
			{
				if (b_over_width.Equals(value)) return;
				b_over_width = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Down Width")]
		public Single BorderDownWidth
		{
			get => b_down_width;
			set
			{
				if (b_down_width.Equals(value)) return;
				b_down_width = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Colour")]
		public Color BorderColour
		{
			get => b_colour;
			set
			{
				if (b_colour.Equals(value)) return;
				b_colour = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Over Colour")]
		public Color BorderOverColour
		{
			get => _bOverColour;
			set
			{
				if (_bOverColour == value) return;
				_bOverColour = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Down Colour")]
		public Color BorderDownColour
		{
			get => _bDownColour;
			set
			{
				if (_bDownColour == value) return;
				_bDownColour = value;
				Invalidate();
			}
		}

		[Category("Border"), DisplayName("Border Radius")]
		public Int32 BorderRadius
		{
			get => b_radius;
			set
			{
				if (b_radius == value) return;
				b_radius = value;
				Invalidate();
			}
		}

		GraphicsPath GetRoundPath(RectangleF rect, Int32 radius, Single width = 0)
		{
			//Fix radius to rect size
			radius = (Int32)Math.Max((Math.Min(radius, Math.Min(rect.Width, rect.Height)) - width), 1);
			Single r2 = radius / 2f;
			Single w2 = width / 2f;
			GraphicsPath graphPath = new GraphicsPath();

			//Top-Left Arc
			graphPath.AddArc(rect.X + w2, rect.Y + w2, radius, radius, 180, 90);

			//Top-Right Arc
			graphPath.AddArc(rect.X + rect.Width - radius - w2, rect.Y + w2, radius, radius, 270, 90);

			//Bottom-Right Arc
			graphPath.AddArc(rect.X + rect.Width - w2 - radius,
									 rect.Y + rect.Height - w2 - radius, radius, radius, 0, 90);
			//Bottom-Left Arc
			graphPath.AddArc(rect.X + w2, rect.Y - w2 + rect.Height - radius, radius, radius, 90, 90);

			//Close line ( Left)           
			graphPath.AddLine(rect.X + w2, rect.Y + rect.Height - r2 - w2, rect.X + w2, rect.Y + r2 + w2);

			//GraphPath.CloseFigure();            

			return graphPath;
		}

		private void DrawText(Graphics g, RectangleF rect)
		{
			Single r2 = BorderRadius / 2f;
			Single w2 = BorderWidth / 2f;
			Point point = new Point();
			StringFormat format = new StringFormat();

			switch (TextAlign)
			{
				case ContentAlignment.TopLeft:
					point.X = (Int32)(rect.X + r2 / 2 + w2 + Padding.Left);
					point.Y = (Int32)(rect.Y + r2 / 2 + w2 + Padding.Top);
					format.LineAlignment = StringAlignment.Center;
					break;
				case ContentAlignment.TopCenter:
					point.X = (Int32)(rect.X + rect.Width / 2f);
					point.Y = (Int32)(rect.Y + r2 / 2 + w2 + Padding.Top);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.TopRight:
					point.X = (Int32)(rect.X + rect.Width - r2 / 2 - w2 - Padding.Right);
					point.Y = (Int32)(rect.Y + r2 / 2 + w2 + Padding.Top);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Far;
					break;
				case ContentAlignment.MiddleLeft:
					point.X = (Int32)(rect.X + r2 / 2 + w2 + Padding.Left);
					point.Y = (Int32)(rect.Y + rect.Height / 2);
					format.LineAlignment = StringAlignment.Center;
					break;
				case ContentAlignment.MiddleCenter:
					point.X = (Int32)(rect.X + rect.Width / 2);
					point.Y = (Int32)(rect.Y + rect.Height / 2);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.MiddleRight:
					point.X = (Int32)(rect.X + rect.Width - r2 / 2 - w2 - Padding.Right);
					point.Y = (Int32)(rect.Y + rect.Height / 2);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Far;
					break;
				case ContentAlignment.BottomLeft:
					point.X = (Int32)(rect.X + r2 / 2 + w2 + Padding.Left);
					point.Y = (Int32)(rect.Y + rect.Height - r2 / 2 - w2 - Padding.Bottom);
					format.LineAlignment = StringAlignment.Center;
					break;
				case ContentAlignment.BottomCenter:
					point.X = (Int32)(rect.X + rect.Width / 2);
					point.Y = (Int32)(rect.Y + rect.Height - r2 / 2 - w2 - Padding.Bottom);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Center;
					break;
				case ContentAlignment.BottomRight:
					point.X = (Int32)(rect.X + rect.Width - r2 / 2 - w2 - Padding.Right);
					point.Y = (Int32)(rect.Y + rect.Height - r2 / 2 - w2 - Padding.Bottom);
					format.LineAlignment = StringAlignment.Center;
					format.Alignment = StringAlignment.Far;
					break;
				default:
					break;
			}

			/* Debug
			using (Pen pen = new Pen(Color.Black, 1))
			{
				 g.DrawLine(pen, new Point(0, 0), point);
				 g.DrawLine(pen, point.X, 0, point.X, point.Y);
				 g.DrawLine(pen, 0, point.Y, point.X, point.Y);
			}
			*/

			using (Brush brush = new SolidBrush(ForeColor))
			{
				g.DrawString(Text, Font, brush, point, format);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			RectangleF rect = new RectangleF(0, 0, this.Width, this.Height);
			Brush brush = new SolidBrush(this.BackColor);
			//Pen pen = new Pen(BorderColor, BorderWidth);

			GraphicsPath graphPath = GetRoundPath(rect, BorderRadius);

			this.Region = new Region(graphPath);

			//Draw Back Color
			if (IsMouseDown && !FlatAppearance.MouseDownBackColor.IsEmpty)
			{
				using (Brush mouseDownBrush = new SolidBrush(FlatAppearance.MouseDownBackColor))
				{
					e.Graphics.FillPath(mouseDownBrush, graphPath);
				}
			}
			else if (IsMouseOver && !FlatAppearance.MouseOverBackColor.IsEmpty)
			{
				using (Brush overBrush = new SolidBrush(FlatAppearance.MouseOverBackColor))
				{
					e.Graphics.FillPath(overBrush, graphPath);
				}
			}
			else
			{
				e.Graphics.FillPath(brush, graphPath);
			}

			//Draw Border
			#region DrawBorder

			GraphicsPath graphInnerPath;
			Pen pen;

			if (IsMouseDown && !BorderDownColour.IsEmpty)
			{
				graphInnerPath = GetRoundPath(rect, BorderRadius, BorderDownWidth);
				pen = new Pen(BorderDownColour, BorderDownWidth);
			}
			else if (IsMouseOver && !BorderOverColour.IsEmpty)
			{
				graphInnerPath = GetRoundPath(rect, BorderRadius, BorderOverWidth);
				pen = new Pen(BorderOverColour, BorderOverWidth);
			}
			else
			{
				graphInnerPath = GetRoundPath(rect, BorderRadius, BorderWidth);
				pen = new Pen(BorderColour, BorderWidth);
			}


			pen.Alignment = PenAlignment.Inset;
			if (pen.Width > 0)
				e.Graphics.DrawPath(pen, graphInnerPath);
			#endregion

			//Draw Text
			DrawText(e.Graphics, rect);
		}// End Paint Method

		protected override void OnMouseEnter(EventArgs e)
		{
			IsMouseOver = true;
			Invalidate();
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			IsMouseOver = false;
			IsMouseDown = false;
			Invalidate();
			base.OnMouseHover(e);
		}

		protected override void OnMouseDown(MouseEventArgs mouseEvent)
		{
			IsMouseDown = true;
			Invalidate();
			base.OnMouseDown(mouseEvent);
		}

		protected override void OnMouseUp(MouseEventArgs mouseEvent)
		{
			IsMouseDown = false;
			Invalidate();
			base.OnMouseDown(mouseEvent);
		}


	}
}
