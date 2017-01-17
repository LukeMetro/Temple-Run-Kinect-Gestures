using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using Microsoft.Kinect;
using MoreLinq;

namespace Kinect_Keystroke_Emulation
{
    class Program
    {
        // Structs to hold skeleton data
        private static Skeleton[] skeletonData;
        private static long frameCount;

        static void Main(string[] args)
        {
            // Create object for keystroke emulation
            InputSimulator sim = new InputSimulator();

            frameCount = 0; // Initialize frame counter

            // Connect to Kinect , enable skeletal tracking, & start stream
            var kinect = KinectSensor.KinectSensors.Single();
            Console.WriteLine("Kinect ID: {0}", kinect.DeviceConnectionId);
            kinect.SkeletonStream.Enable(); // Enable skeletal tracking

            // Allocate data struct for skeletal tracking
            skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data

            // Attach handler to skeleton ready event
            kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

            kinect.Start(); // Begin streaming data  
            // Infinite loop so program doesn't stop execution
            while (true)
            {

            }
                     
        }

        // Whenever a frame of skeletal data is ready, this method is called
        private static void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(skeletonData); // get the skeletal information in this frame
                    // Get tracked skeleton from skeletonData
                    // If more than one is being tracked, pick the closest one to the Kinect.
                    var foundSkeletons = skeletonData.Where(i => i.Position.Z > 0);
                    
                    // If a skeleton is found, process the data. TODO: Process data for real.
                    if (foundSkeletons.Count() != 0)
                    {
                        var trackedSkeleton = foundSkeletons.MinBy(i => i.Position.Z);
                        Console.WriteLine("Frame {3}: TrackedSkeleton position: {0} {1} {2}", 
                            trackedSkeleton.Position.X, trackedSkeleton.Position.Y, trackedSkeleton.Position.Z,
                            frameCount);
                        frameCount++;
                    }
                }
            }
        }
    }
}
// How to simulate key presses:
// sim.Keyboard.KeyPress(VirtualKeyCode.VK_W);
// sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
// sim.Keyboard.KeyPress(VirtualKeyCode.VK_S);
// sim.Keyboard.KeyPress(VirtualKeyCode.VK_D);
