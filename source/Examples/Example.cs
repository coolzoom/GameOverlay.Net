using System;
using System.Collections.Generic;
using System.Text;

using GameOverlay.Drawing;
using GameOverlay.Windows;

namespace Examples
{
	public class Example : IDisposable
	{
		private readonly StickyWindow _window;

		private readonly Dictionary<string, SolidBrush> _brushes;
		private readonly Dictionary<string, Font> _fonts;
		private readonly Dictionary<string, Image> _images;

		private Geometry _gridGeometry;
		private Rectangle _gridBounds;

		private Random _random;
		private long _lastRandomSet;
		private List<Action<Graphics, float, float>> _randomFigures;
        private float angle = 0f;
        public Example()
		{
			_brushes = new Dictionary<string, SolidBrush>();
			_fonts = new Dictionary<string, Font>();
			_images = new Dictionary<string, Image>();

			var gfx = new Graphics()
			{
				MeasureFPS = true,
				PerPrimitiveAntiAliasing = true,
				TextAntiAliasing = true
			};

			IntPtr ip = System.Diagnostics.Process.GetProcessesByName("WoWClassicT")[0].MainWindowHandle;
			_window = new StickyWindow(ip,gfx)
			{
                AttachToClientArea = true,
				BypassTopmost = true,
			};

			_window.DestroyGraphics += _window_DestroyGraphics;
			_window.DrawGraphics += _window_DrawGraphics;
			_window.SetupGraphics += _window_SetupGraphics;
		}

		private void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
		{
			var gfx = e.Graphics;

			if (e.RecreateResources)
			{
				foreach (var pair in _brushes) pair.Value.Dispose();
				foreach (var pair in _images) pair.Value.Dispose();
			}

			_brushes["black"] = gfx.CreateSolidBrush(0, 0, 0);
			_brushes["white"] = gfx.CreateSolidBrush(255, 255, 255);
			_brushes["red"] = gfx.CreateSolidBrush(255, 0, 0);
			_brushes["green"] = gfx.CreateSolidBrush(0, 255, 0);
			_brushes["blue"] = gfx.CreateSolidBrush(0, 0, 255);
            //_brushes["background"] = gfx.CreateSolidBrush(0xff, 0xff, 0xff);
            _brushes["background"] = gfx.CreateSolidBrush(0, 0x27, 0x31, 255.0f * 0.2f);
            _brushes["grid"] = gfx.CreateSolidBrush(255, 255, 255, 0.2f);
			_brushes["random"] = gfx.CreateSolidBrush(0, 0, 0);

			if (e.RecreateResources) return;

			_fonts["arial"] = gfx.CreateFont("Arial", 12);
			_fonts["consolas"] = gfx.CreateFont("Consolas", 14);

			_gridBounds = new Rectangle(100, 200, gfx.Width - 20, gfx.Height - 20);
			_gridGeometry = gfx.CreateGeometry();

			for (float x = _gridBounds.Left; x <= _gridBounds.Right; x += 20)
			{
				var line = new Line(x, _gridBounds.Top, x, _gridBounds.Bottom);
				_gridGeometry.BeginFigure(line);
				_gridGeometry.EndFigure(false);
			}

			for (float y = _gridBounds.Top; y <= _gridBounds.Bottom; y += 20)
			{
				var line = new Line(_gridBounds.Left, y, _gridBounds.Right, y);
				_gridGeometry.BeginFigure(line);
				_gridGeometry.EndFigure(false);
			}

			_gridGeometry.Close();

			_randomFigures = new List<Action<Graphics, float, float>>()
			{
				(g, x, y) => g.DrawRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110, 2.0f),
				(g, x, y) => g.DrawCircle(GetRandomColor(), x + 60, y + 60, 48, 2.0f),
				(g, x, y) => g.DrawRoundedRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110, 8.0f, 2.0f),
				(g, x, y) => g.DrawTriangle(GetRandomColor(), x + 10, y + 110, x + 110, y + 110, x + 60, y + 10, 2.0f),
				(g, x, y) => g.DashedRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110, 2.0f),
				(g, x, y) => g.DashedCircle(GetRandomColor(), x + 60, y + 60, 48, 2.0f),
				(g, x, y) => g.DashedRoundedRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110, 8.0f, 2.0f),
				(g, x, y) => g.DashedTriangle(GetRandomColor(), x + 10, y + 110, x + 110, y + 110, x + 60, y + 10, 2.0f),
				(g, x, y) => g.FillRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110),
				(g, x, y) => g.FillCircle(GetRandomColor(), x + 60, y + 60, 48),
				(g, x, y) => g.FillRoundedRectangle(GetRandomColor(), x + 10, y + 10, x + 110, y + 110, 8.0f),
				(g, x, y) => g.FillTriangle(GetRandomColor(), x + 10, y + 110, x + 110, y + 110, x + 60, y + 10),
			};
		}

		private void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
		{
			foreach (var pair in _brushes) pair.Value.Dispose();
			foreach (var pair in _fonts) pair.Value.Dispose();
			foreach (var pair in _images) pair.Value.Dispose();
		}

		private void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
		{
			var gfx = e.Graphics;

			var padding = 16;
			var infoText = new StringBuilder()
				.Append("FPS: ").Append(gfx.FPS.ToString().PadRight(padding))
				.Append("FrameTime: ").Append(e.FrameTime.ToString().PadRight(padding))
				.Append("FrameCount: ").Append(e.FrameCount.ToString().PadRight(padding))
				.Append("DeltaTime: ").Append(e.DeltaTime.ToString().PadRight(padding))
				.ToString();

			gfx.ClearScene(_brushes["background"]);

			gfx.DrawTextWithBackground(_fonts["consolas"], _brushes["green"], _brushes["black"], 58, 20, infoText);

			gfx.DrawGeometry(_gridGeometry, _brushes["grid"], 1.0f);

			if (_lastRandomSet == 0L || e.FrameTime - _lastRandomSet > 2500)
			{
				_lastRandomSet = e.FrameTime;
			}

			_random = new Random(unchecked((int)_lastRandomSet));

			for (float row = _gridBounds.Top + 12; row < _gridBounds.Bottom - 120; row += 120)
			{
				for (float column = _gridBounds.Left + 12; column < _gridBounds.Right - 120; column += 120)
				{
					DrawRandomFigure(gfx, column, row);
				}
			}


			//test draw
			if (angle < 2 * 3.14)
			{
				angle += 0.01f;
			}
			else
			{
				angle = 0;
            }
			
			float x1 = 100, y1 = 100;
            float x2 = 80, y2 = 140;
            float x3 = 120, y3 = 140;


            //rotate calculation
            //https://stackoverflow.com/questions/61447973/how-to-calculate-positions-of-new-points-after-image-rotation
            //def rotate(pt, radians, origin):
            //    x, y = pt
            //    offset_x, offset_y = origin
            //adjusted_x = (x - offset_x)
            //adjusted_y = (y - offset_y)
            //cos_rad = math.cos(radians)
            //sin_rad = math.sin(radians)
            //qx = offset_x + cos_rad* adjusted_x + sin_rad* adjusted_y
            //qy = offset_y + -sin_rad* adjusted_x + cos_rad* adjusted_y
            //return qx, qy

            float x2r = (float)((x2 - x1) * Math.Cos(angle) + (y2 - y1) * Math.Sin(angle) + x1);
            float y2r = (float)(- (x2 - x1) * Math.Sin(angle) + (y2 - y1) * Math.Cos(angle) + y1);

            float x3r = (float)((x3 - x1) * Math.Cos(angle) + (y3 - y1) * Math.Sin(angle) + x1);
            float y3r = (float)(- (x3 - x1) * Math.Sin(angle) + (y3 - y1) * Math.Cos(angle) + y1);
            //b.x = (a.x - o.x) * cos(angle) - (a.y - o.y) * sin(angle) + o.x
            //b.y = (a.x - o.x) * sin(angle) + (a.y - o.y) * cos(angle) + o.y

            //
			gfx.DrawText(_fonts["arial"], 22, _brushes["white"], x1, y1, $"angle: {angle}");
            //vertice
            gfx.DrawCircle(GetRandomColor(), x1, y1, 10, 2.0f);
			//original
            //gfx.DrawTriangle(_brushes["red"], x1, y1, x2, y2, x3, y3, 1.0f);
			//rotated
            gfx.DrawTriangle(_brushes["green"], x1, y1, x2r, y2r, x3r, y3r, 1.0f);
            gfx.DrawText(_fonts["arial"], 22, _brushes["white"], x2r, y2r, "p2");
            gfx.DrawText(_fonts["arial"], 22, _brushes["white"], x3r, y3r, "p3");

            ////test transform
            //TransformationMatrix tm = new TransformationMatrix();
            //         TransformationMatrix.Rotation(1.2f);

            //         gfx.TransformStart(tm);
            ////gfx.TransformEnd();
        }
        //def rotate(pt, radians, origin):
    //    x, y = pt
    //    offset_x, offset_y = origin
    //adjusted_x = (x - offset_x)
    //adjusted_y = (y - offset_y)
    //cos_rad = math.cos(radians)
    //sin_rad = math.sin(radians)
    //qx = offset_x + cos_rad* adjusted_x + sin_rad* adjusted_y
    //qy = offset_y + -sin_rad* adjusted_x + cos_rad* adjusted_y
    //return qx, qy


        private void DrawRandomFigure(Graphics gfx, float x, float y)
		{
			var action = _randomFigures[_random.Next(0, _randomFigures.Count)];

			action(gfx, x, y);


		}

		private SolidBrush GetRandomColor()
		{
			var brush = _brushes["random"];

			brush.Color = new Color(_random.Next(0, 256), _random.Next(0, 256), _random.Next(0, 256));

			return brush;
		}

		public void Run()
		{
			_window.Create();
			_window.Join();
		}

		~Example()
		{
			Dispose(false);
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				_window.Dispose();

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
