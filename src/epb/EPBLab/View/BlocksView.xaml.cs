using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EPBLab.View
{
    /// <summary>
    /// Interaction logic for BlocksView.xaml
    /// </summary>
    public partial class BlocksView : UserControl
    {
        public BlocksView()
        {
            InitializeComponent();
        }

        private enum CameraDragMode
        {
            None,
            Rotate,
            Pan
        }

        private CameraDragMode cameraDragMode = CameraDragMode.None;
        private Point oldMousePos;
        private Vector3D rotation = new Vector3D();

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                cameraDragMode = CameraDragMode.Pan;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                cameraDragMode = CameraDragMode.Rotate;
            }

            oldMousePos = e.GetPosition(Viewport);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            cameraDragMode = CameraDragMode.None;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            ProjectionCamera camera = (ProjectionCamera) Viewport.Camera;
            Vector delta = (e.GetPosition(Viewport) - oldMousePos);
            Vector3D left = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
            left.Normalize();
            switch (cameraDragMode)
            {
                case CameraDragMode.Pan:
                    delta *= 0.01;
                    Vector3D deltaMove = Vector3D.Add(left * -delta.X, camera.UpDirection * delta.Y);
                    Point3D newPos = new Point3D(camera.Position.X + deltaMove.X, camera.Position.Y + deltaMove.Y, camera.Position.Z + deltaMove.Z);
                    camera.Position = newPos;
                    break;
                case CameraDragMode.Rotate:
                    delta *= 0.5;
                    rotation.X += delta.X;
                    rotation.Y += delta.Y;
                    Transform3DGroup t = new Transform3DGroup();
                    t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotation.X)));
                    t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), rotation.Y)));
                    Model.Transform = t;
                    SelectionModel.Transform = t;
                    break;
            }
            oldMousePos = e.GetPosition(Viewport);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta * 0.01;
            ProjectionCamera camera = (ProjectionCamera)Viewport.Camera;
            Vector3D deltaMove = camera.LookDirection * delta;
            Point3D newPos = new Point3D(camera.Position.X + deltaMove.X, camera.Position.Y + deltaMove.Y, camera.Position.Z + deltaMove.Z);
            camera.Position = newPos;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            OnMouseUp(sender, null);
        }
    }
}
