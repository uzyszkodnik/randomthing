﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace MapEditor
{
    public sealed class EditorEngine
    {
        public event EventHandler InitializedEngine;
        private const double FRAP_TIME = 1000.0 / 60.0;
        #region Singleton
        private static EditorEngine instance;

        public static EditorEngine Instance
        {
            get { return instance; }
            set { instance = value; }
        }

        static EditorEngine()
        {
            instance = new EditorEngine();
        }

        private EditorEngine()
        {

        }
        #endregion

        private GLControl glControl;
        private Map map;
        private Camera camera;
        private bool running;
        private Stopwatch stopwatch;
        private double offset;
        private MouseController mouseController;

        private bool initialized;

        public bool Initialized { get { return initialized; } }
        public MouseController MouseController { get { return mouseController; } }
        public Map Map { get { return map; } set { map = value; } }
        public Camera Camera { get { return camera; } set { camera = value; } }


        public void Initialize()
        {
            //map = new Map(@"C:\Repo\randomthing\MapEditor\MapEditor\bin\Debug\maps\test");
            if (initialized)
                return;
            initialized = true;
            Camera = new Camera();
            Camera.Height = glControl.Height;
            Camera.Widht = glControl.Width;
            mouseController = new MouseController();
            SetupOpengl();
            if (InitializedEngine != null) InitializedEngine(this, EventArgs.Empty);
        }

        private void SetupOpengl()
        {
            GL.Enable(EnableCap.Texture2D);

        }
        private void Stop()
        {
            running = false;
        }

        private void temp()
        {
            if (glControl == null) return;
            int w = glControl.Width;
            int h = glControl.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
            GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
        }


        public void Associate(GLControl control)
        {
            this.glControl = control;
        }
        public void Run()
        {
            running = true;
            stopwatch = new Stopwatch();
            offset = 0.0;
            stopwatch.Start();

        }
        public void Update()
        {
            if (!running) return;
            stopwatch.Stop();
            double ms = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Reset();
            stopwatch.Start();
            ms += offset;
            while (ms-FRAP_TIME > 0.0)
            {
               


                ms -= FRAP_TIME;
            }
            initActions();
            camera.Set();
            beforeRendering();
            mouseController.Update();
            camera.Look();
            map.Update();


            offset = ms;
            glControl.SwapBuffers();
            glControl.Invalidate();
        }





        private void initActions()
        {
            GL.ClearColor(Color.Pink);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
        }
        private void beforeRendering()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }



        
    }
}
