﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using Microsoft.Kinect;
using MoreLinq;
using WindowsInput.Native;

namespace Kinect_Keystroke_Emulation
{
    class Program
    {
        // Structs to hold skeleton data
        private static Skeleton[] skeletonData;
        private static bool waitingForGest;
        private static float savedYPosition;
        private static float savedXPosition;
        private static InputSimulator sim;

        static void Main(string[] args)
        {
            // Create object for keystroke emulation
            sim = new InputSimulator();

            // Initialize tracking variable
            waitingForGest = false;

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
                        if (waitingForGest == false) // Take note of skeleton Y-coordinate
                        {
                            savedYPosition = trackedSkeleton.Joints[JointType.Head].Position.Y;
                            savedXPosition = trackedSkeleton.Joints[JointType.Head].Position.X;
                            waitingForGest = true;
                            Thread.Sleep(200);

                        } else // waiting for gesture
                        {
                            waitingForGest = false;
                            var newYPosition = trackedSkeleton.Joints[JointType.Head].Position.Y;
                            var newXPosition = trackedSkeleton.Joints[JointType.Head].Position.X;
                            var gestureDetected = false;
                            //Console.WriteLine("Saved X Position: {0}", savedXPosition);
                            //Console.WriteLine("New X Position: {0}", newXPosition);
                            if (newYPosition >= savedYPosition + 0.1)
                            {
                                Console.WriteLine("Jump Detected!");
                                sim.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                                gestureDetected = true;
                            }
                            else if (newYPosition <= savedYPosition - 0.1)
                            {
                                Console.WriteLine("Duck Detected!");
                                sim.Keyboard.KeyPress(VirtualKeyCode.VK_S);
                                gestureDetected = true;
                            }
                            if (newXPosition >= savedXPosition + 0.1)
                            {
                                Console.WriteLine("Right Dodge Detected!");
                                sim.Keyboard.KeyPress(VirtualKeyCode.VK_D);
                                gestureDetected = true;
                            }
                            else if (newXPosition <= savedXPosition - 0.1)
                            {
                                Console.WriteLine("Left Dodge Detected!");
                                sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                                gestureDetected = true;
                            }
                            // Prevent double detection (such as jump when returning from duck)
                            if (gestureDetected)
                                Thread.Sleep(600);
                        }
                    }
                }
            }
        }
    }
}