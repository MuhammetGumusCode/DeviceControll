using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class OvalButton : Button
{
    public int BorderRadius { get; set; } = 20;   // yuvarlaklık derecesi

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using (GraphicsPath path = GetRoundedPath(new Rectangle(0, 0, this.Width, this.Height), BorderRadius))
        {
            this.Region = new Region(path);

            using (Pen pen = new Pen(Color.Black, 1))  // istersen border rengini değiştir
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        int d = radius * 2;

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();

        return path;
    }
}

