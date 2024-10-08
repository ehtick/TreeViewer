/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace ScaleBar
{
    /// <summary>
    /// This module is used to draw a scale bar below the tree plot.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Scale bar";
        public const string HelpText = "Draws a scale bar.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "195bdabf-c5cf-4daf-992f-7a86a600beec";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAA+SURBVDhPYxgFVAJFRUX/QRjKJQiQ1TOBRSgAjCCCFNuRQV9fHyPFLmCB0mAAMhHKxAuQXUyxC0YBxYCBAQAocRFg/ulkfQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAA9SURBVEhLYxgFo2AUQEBRUdF/EIZyKQbI5jGBRWgIaG4BI4igZvAgg76+Pkaa+wAMRiN5FIyCUYAPMDAAACgiFrQoEMCjAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABESURBVFhH7dPBCQAgCEBRbWQHceVC6eACpYf/wKAuipoAAEYwsx1xr8/VfCtfGlGAxvFz/pW7a3sHEr+g04wdAAD0ETl04xa4X+VUYAAAAABJRU5ErkJggg==";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Scale", "Group:2" ),
                
                /// <param name="Scale size:">
                /// This parameter determines the length of the scale bar (expressed as a proportion of the total length from the root
                /// of the tree to the farthest tip).
                /// </param>
                ( "Scale size:", "Slider:0.2[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Digits:">
                /// This parameter determines the number of decimal digits used to show the size of the scale bar.
                /// </param>
                ( "Digits:", "NumericUpDown:2[\"1\",\"Infinity\",\"1\",\"0\"]" ),

                ( "Appearance", "Group:3"),
                
                /// <param name="Line thickness:">
                /// This parameter determines the thickness of the scale bar.
                /// </param>
                ( "Line thickness:", "NumericUpDown:1[\"0\",\"Infinity\"]" ),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to draw the scale bar.
                /// </param>
                ( "Colour:", "Colour:[0,0,0,255]" ),
                
                /// <param name="Font:">
                /// This parameter determines the font used for the label of the scale bar.
                /// </param>
                ( "Font:", "Font:[\"Helvetica\",\"10\"]" ),

                ( "Position", "Group:2" ),
                
                /// <param name="Position:">
                /// This parameter determines the offset from the default position at which the scale bar is drawn.
                /// </param>
                ( "Position:", "Point:[0,0]" ),
                
                /// <param name="Angle:">
                /// This parameter determines the rotation angle of the scale bar. If the value is `0°`, the scale bar is horizontal.
                /// </param>
                ( "Angle:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { };

            parametersToChange = new Dictionary<string, object>() { };

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            Point scalePoint;

            if (!coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out scalePoint))
            {
                if (!coordinates.TryGetValue("95b61284-b870-48b9-b51c-3276f7d89df1", out scalePoint))
                {
                    if (!coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out scalePoint))
                    {
                        throw new Exception("The coordinates module is not supported!");
                    }
                    else
                    {
                        scalePoint = new Point(scalePoint.X, 0);
                    }
                }
            }

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            void updateMaxMin(Point pt)
            {
                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static Point sumPoint(Point pt1, Point pt2)
            {
                return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
            }

            static Point rotatePoint(Point pt, double angle)
            {
                return new Point(pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle), pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                updateMaxMin(coordinates[nodes[i].Id]);
            }

            updateMaxMin(coordinates[Modules.RootNodeId]);

            double maxLength = tree.LongestDownstreamLength();

            int digits = (int)(double)parameterValues["Digits:"];

            double scaleSize = double.Parse((maxLength * (double)parameterValues["Scale size:"]).ToString(digits));

            double actualScaleLength = Math.Sqrt((scalePoint.X * scalePoint.X) + (scalePoint.Y * scalePoint.Y)) * scaleSize;

            double scaleThickness = (double)parameterValues["Line thickness:"];

            double angle = (double)parameterValues["Angle:"] * Math.PI / 180;

            while (Math.Abs(angle) > Math.PI)
            {
                angle -= 2 * Math.PI * Math.Sign(angle);
            }

            Colour colour = (Colour)parameterValues["Colour:"];

            Point delta = (Point)parameterValues["Position:"];

            Point point = new Point((minX + maxX) * 0.5 + delta.X, maxY + 15 + scaleThickness * 0.5 + 4.5 + delta.Y);

            graphics.Save();
            graphics.Translate(point);
            graphics.Rotate(angle);

            GraphicsPath scalePath = new GraphicsPath().MoveTo(-actualScaleLength * 0.5, -scaleThickness * 0.5 - 4.5).LineTo(-actualScaleLength * 0.5, scaleThickness * 0.5 + 4.5);
            scalePath.MoveTo(-actualScaleLength * 0.5, 0).LineTo(actualScaleLength * 0.5, 0);
            scalePath.MoveTo(actualScaleLength * 0.5, -scaleThickness * 0.5 - 4.5).LineTo(actualScaleLength * 0.5, scaleThickness * 0.5 + 4.5);

            graphics.StrokePath(scalePath, colour, scaleThickness, LineCaps.Round);

            Font fnt = (Font)parameterValues["Font:"];
            string scaleText = scaleSize.ToString(digits);

            if (Math.Abs(angle) < Math.PI / 2)
            {
                graphics.FillText(-fnt.MeasureText(scaleText).Width * 0.5, 5 + scaleThickness * 0.5 + 4.5, scaleText, fnt, colour);
            }
            else
            {
                graphics.Save();
                graphics.Rotate(Math.PI);
                graphics.FillText(-fnt.MeasureText(scaleText).Width * 0.5, -(5 + scaleThickness * 0.5 + 4.5) - fnt.MeasureText(scaleText).Height, scaleText, fnt, colour);
                graphics.Restore();
            }

            graphics.Restore();

            minX = double.MaxValue;
            maxX = double.MinValue;
            minY = double.MaxValue;
            maxY = double.MinValue;

            updateMaxMin(sumPoint(point, rotatePoint(new Point(-actualScaleLength * 0.5, -scaleThickness * 0.5 - 4.5), angle)));
            updateMaxMin(sumPoint(point, rotatePoint(new Point(actualScaleLength * 0.5, -scaleThickness * 0.5 - 4.5), angle)));
            updateMaxMin(sumPoint(point, rotatePoint(new Point(actualScaleLength * 0.5, scaleThickness * 0.5 + 4.5), angle)));
            updateMaxMin(sumPoint(point, rotatePoint(new Point(-actualScaleLength * 0.5, scaleThickness * 0.5 + 4.5), angle)));

            updateMaxMin(sumPoint(point, rotatePoint(new Point(-fnt.MeasureText(scaleText).Width * 0.5, 5 + scaleThickness * 0.5 + 4.5 + fnt.MeasureText(scaleText).Height), angle)));
            updateMaxMin(sumPoint(point, rotatePoint(new Point(fnt.MeasureText(scaleText).Width * 0.5, 5 + scaleThickness * 0.5 + 4.5 + fnt.MeasureText(scaleText).Height), angle)));

            if (double.IsNaN(minX))
            {
                minX = 0;
            }

            if (double.IsNaN(minY))
            {
                minY = 0;
            }

            if (double.IsNaN(maxX))
            {
                maxX = 0;
            }

            if (double.IsNaN(maxY))
            {
                maxY = 0;
            }

            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }
    }
}
