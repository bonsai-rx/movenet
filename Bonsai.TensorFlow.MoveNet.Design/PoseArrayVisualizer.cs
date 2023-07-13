﻿using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.TensorFlow.MoveNet;
using Bonsai.TensorFlow.MoveNet.Design;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(PoseArrayVisualizer), Target = typeof(Pose[]))]

namespace Bonsai.TensorFlow.MoveNet.Design
{
    /// <summary>
    /// Provides a type visualizer that draws a visual representation of the
    /// collection of poses extracted from each image in the sequence.
    /// </summary>
    public class PoseArrayVisualizer : IplImageVisualizer
    {
        Pose[] poseArray = null;
        LabeledImageLayer labeledImage;
        ToolStripButton drawLabelsButton;

        /// <summary>
        /// Gets or sets a value indicating whether to show the names of body parts.
        /// </summary>
        public bool DrawLabels { get; set; }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            drawLabelsButton = new ToolStripButton("Draw Labels");
            drawLabelsButton.CheckState = CheckState.Checked;
            drawLabelsButton.Checked = DrawLabels;
            drawLabelsButton.CheckOnClick = true;
            drawLabelsButton.CheckedChanged += (sender, e) => DrawLabels = drawLabelsButton.Checked;
            StatusStrip.Items.Add(drawLabelsButton);

            VisualizerCanvas.Load += (sender, e) =>
            {
                labeledImage = new LabeledImageLayer();
                GL.Enable(EnableCap.PointSmooth);
            };
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            poseArray = (Pose[])value;
            IplImage ret = poseArray.Length > 0 ? poseArray[0].Image : null;
            base.Show(ret);
        }

        /// <inheritdoc/>
        protected override void ShowMashup(IList<object> values)
        {
            base.ShowMashup(values);
            if (poseArray.Length > 0)
            {
                foreach (var pose in poseArray) 
                {
                    if (pose != null)
                    {
                        if (DrawLabels)
                        {
                            labeledImage.UpdateLabels(pose.Image.Size, VisualizerCanvas.Font, (graphics, labelFont) =>
                            {
                                DrawingHelper.DrawLabels(graphics, labelFont, pose);
                            });
                        }
                        else labeledImage.ClearLabels();
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void RenderFrame()
        {
            GL.Color4(Color4.White);
            base.RenderFrame();
            if (poseArray != null) 
            { 
                if (poseArray.Length > 0)
                { 
                    foreach (var pose in poseArray)
                    {
                        if (pose != null)
                        {
                            DrawingHelper.SetDrawState(VisualizerCanvas);
                            DrawingHelper.DrawPose(pose);
                            labeledImage.Draw();
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            base.Unload();
            labeledImage?.Dispose();
            labeledImage = null;
        }
    }
}
