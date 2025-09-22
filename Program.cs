/**
 * @file Program.cs
 * @brief Example demonstrating the use of the runtime library in a console application
 */

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using ViDi2;
using ViDi2.Local;
using System.Collections.ObjectModel;
using System.Linq;
using InvalidOperationException = System.InvalidOperationException;
using Amazon.S3;

namespace Example.Runtime
{
    class Program
    {
        /**
         * @brief This example creates a ViDi control, loads the runtime workspace
         * from the cover glass tutorial. Processes the red high-detail tool from
         * that workspace and prints out the score for each match and feature
         */
        static void Main(string[] args)
        {
            // Initializes the control
            // This initialization does not allocate any gpu ressources.
            using (ViDi2.Runtime.Local.Control control = new ViDi2.Runtime.Local.Control(GpuMode.Deferred))
            {
                // Initializes all CUDA devices
                control.InitializeComputeDevices(GpuMode.SingleDevicePerTool, new List<int>() { });

                // Open a runtime workspace from file
                // the path to this file relative to the example root folder
                // and assumes the resource archive was extracted there.

                //ViDi2.Runtime.IWorkspace workspace = control.Workspaces.Add("workspace", "..\\..\\..\\..\\resources\\runtime\\Red High-detail Tool.vrws");

                //NPNP
                ViDi2.Runtime.IWorkspace workspace = control.Workspaces.Add("workspace", @"E:\temp\FrontCSInspectionFullArea3LightingsWith002DefectQuick.vrws");
                //ViDi2.Runtime.IWorkspace workspace = control.Workspaces.Add("workspace", @"E:\CognexBrains\TopTrainingPeels_09_07_25_77ImagesLabeledCheckPerformance.vrws");

                // Store a reference to the stream 'default'
                IStream stream = workspace.Streams["default"];

                // Load an image from file
                //using (IImage image = new LibraryImage("..\\..\\..\\..\\resources\\images\\defect_00000045.png")) //disposing the image when we do not need it anymore

                //NPNP
                using (IImage image = new LibraryImage(@"E:\temp\snap.jpg")) //disposing the image when we do not need it anymore
                {
                    // Allocates a sample with the image
                    using (ISample sample = stream.CreateSample(image))
                    {
                        ITool redTool = stream.Tools["Analyze"];

                        // Process the image by the tool. All upstream tools are also processed
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        sample.Process(redTool);
                        sw.Stop();

                        IRedMarking redMarking = sample.Markings[redTool.Name] as IRedMarking;

                        foreach (IRedView view in redMarking.Views)
                        {
                            System.Console.WriteLine($"This view has a score of {view.Score}");

                            string inputImagePath = @"E:\temp\snap3.png";
                            string outputImagePath = @"E:\temp\snap3WithRegions.jpg";
                            //ViDi2.IRegion region =,
                            //Color color = Color.Color,
                            float lineWidth = 2f;
                            float pointRadius = 3f;
                            bool closePath = true;
                            using (var bmp = new Bitmap(inputImagePath))
                            {

                                foreach (IRegion region in view.Regions)
                                {
                                    DrawOuterOnBitmap(bmp, region, Color.Red, lineWidth, pointRadius, closePath);

                                    Console.WriteLine($"This region has a score of {region.Score}");
                                    bmp.Save(outputImagePath); // format inferred from extension
                                }
                            }
                        }
                        //NPNP
                        Console.WriteLine($"Cognex Duration: {redMarking.Duration} , StopWatch: {sw.ElapsedMilliseconds}");
                    }
                }

                using (IImage image = new LibraryImage(@"E:\temp\snap3UpperHalf.jpg")) //disposing the image when we do not need it anymore
                {
                    // Allocates a sample with the image
                    using (ISample sample = stream.CreateSample(image))
                    {
                        ITool redTool = stream.Tools["Analyze"];

                        // Process the image by the tool. All upstream tools are also processed
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        sample.Process(redTool);
                        sw.Stop();

                        IRedMarking redMarking = sample.Markings[redTool.Name] as IRedMarking;

                        foreach (IRedView view in redMarking.Views)
                        {
                            System.Console.WriteLine($"This view has a score of {view.Score}");

                            string inputImagePath = @"E:\temp\snap3UpperHalf.jpg";
                            string outputImagePath = @"E:\temp\snap3UpperHalfWithRegions.jpg";
                            //ViDi2.IRegion region =,
                            //Color color = Color.Color,
                            float lineWidth = 2f;
                            float pointRadius = 3f;
                            bool closePath = true;
                            using (var bmp = new Bitmap(inputImagePath))
                            {
                                //EnsureDirectory(outputImagePath);


                                foreach (IRegion region in view.Regions)
                                {
                                    DrawOuterOnBitmap(bmp, region, Color.Red, lineWidth, pointRadius, closePath);
                                    Console.WriteLine($"This region has a score of {region.Score}");
                                }
                                bmp.Save(outputImagePath); // format inferred from extension
                            }
                        }

                        //NPNP
                        Console.WriteLine($"Cognex Duration: {redMarking.Duration} , StopWatch: {sw.ElapsedMilliseconds}");

                    }
                }

            }
        }

        // Convert ViDi2.Point collection to System.Drawing.PointF[]
        private static IEnumerable<PointF> ToPointF(ReadOnlyCollection<ViDi2.Point> points)
        {
            foreach (var p in points)
                yield return new PointF((float)p.X, (float)p.Y); // assumes ViDi2.Point has double X/Y
        }

        public static void DrawOuterOnBitmap(
              Bitmap bitmap,
              ViDi2.IRegion region,
              Color color,
              float lineWidth = 2f,
              float pointRadius = 3f,
              bool closePath = true)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (region.Outer == null || region.Outer.Count == 0)
                throw new InvalidOperationException("Region.Outer has no points.");

            using (var g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(color, lineWidth))
            using (var brush = new SolidBrush(color))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var pts = ToPointF(region.Outer).ToArray();

                foreach (var p in pts)
                    g.FillEllipse(brush, p.X - pointRadius, p.Y - pointRadius, pointRadius * 2, pointRadius * 2);

                if (pts.Length > 1)
                {
                    if (closePath) g.DrawPolygon(pen, pts);
                    else g.DrawLines(pen, pts);
                }
            }
        }

    }
}
