using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EPBLab.Helpers;
using EPBLab.ViewModel;

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

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            switch (e.ChangedButton)
            {
                case MouseButton.Middle:
                    cameraDragMode = CameraDragMode.Pan;
                    break;
                case MouseButton.Right:
                    cameraDragMode = CameraDragMode.Rotate;
                    break;
            }
            oldMousePos = e.GetPosition(Viewport);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    TestHit(e.GetPosition(Viewport));
                    break;
                case MouseButton.Middle:
                case MouseButton.Right:
                    cameraDragMode = CameraDragMode.None;
                    break;
            }
        }

        private void TestHit(Point screenPos)
        {
            VisualTreeHelper.HitTest(Viewport, null, result =>
            {
                if (result is RayMeshGeometry3DHitTestResult rayHitResult)
                {
                    if (rayHitResult.ModelHit is GeometryModel3D model)
                    {
                        BlocksViewModel vm = (BlocksViewModel)DataContext; // TODO: Ugly dependency on the model here!
                        vm.GeometryModelClicked(model);
                    }
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(screenPos));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Vector delta = (e.GetPosition(Viewport) - oldMousePos);
            BlocksViewModel vm = (BlocksViewModel)((FrameworkElement)sender).DataContext; // TODO: Ugly dependency on the model here!
            ProjectionCamera camera = (ProjectionCamera) Viewport.Camera;
            Vector3D left = Vector3D.CrossProduct(camera.LookDirection, camera.UpDirection);
            left.Normalize();
            switch (cameraDragMode)
            {
                case CameraDragMode.Pan:
                    delta *= 0.01;
                    Vector3D deltaMove = Vector3D.Add(left * -delta.X, camera.UpDirection * delta.Y);
                    Point3D newCameraPosition = vm.CameraPosition.Add(deltaMove);
                    vm.CameraPosition = newCameraPosition;
                    Point3D newCameraAimPoint = vm.CameraAimPoint.Add(deltaMove);
                    vm.CameraAimPoint = newCameraAimPoint;
                    break;
                case CameraDragMode.Rotate:
                    delta *= 0.01;
                    Vector3D v = vm.CameraAimPoint.VectorTo(vm.CameraPosition).Cartesian2Spherical();
                    v.Y -= delta.X;
                    v.Z -= delta.Y;
                    vm.CameraPosition = vm.CameraAimPoint.Add(v.Spherical2Cartesian());
                    break;
            }
            oldMousePos = e.GetPosition(Viewport);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta * 0.01;
            BlocksViewModel vm = (BlocksViewModel)((FrameworkElement)sender).DataContext; // TODO: Ugly dependency on the model here!
            ProjectionCamera camera = (ProjectionCamera)Viewport.Camera;
            Vector3D deltaMove = camera.LookDirection * delta;
            Point3D newCameraPosition = camera.Position.Add(deltaMove);
            camera.Position = newCameraPosition;
            Point3D newCameraAimPoint = vm.CameraAimPoint.Add(deltaMove);
            vm.CameraAimPoint = newCameraAimPoint;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            OnMouseUp(sender, null);
        }
    }
}
