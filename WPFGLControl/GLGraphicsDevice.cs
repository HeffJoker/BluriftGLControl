using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Timers;
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace WorldEditor
{
    public class GLGraphicsDevice : GLControl
    {

        public const string VERSION = "Version 1.0";
        public const string ABOUT = "Blurift GLControl for Monogame and WPF applications.";

        private bool m_bLoaded = false;
        private Timer m_timer = null;
        // The GraphicsDeviceService that provides and manages our GraphicsDevice
        public GraphicsDeviceService graphicsService;
        GameServiceContainer services = new GameServiceContainer();
        public ContentManager content;
        public event EventHandler<GraphicsDeviceEventArgs> RenderXna;
        public event EventHandler<GraphicsDeviceEventArgs> LoadContent;
        private bool applicationHasFocus = false;
        private bool mouseInWindow = false;
        private HwndMouseState mouseState = new HwndMouseState();
        private bool isMouseCaptured = false;

        /// <summary>
        /// Invoked when the control receives a left mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDown;

        /// <summary>
        /// Invoked when the control receives a left mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonUp;

        /// <summary>
        /// Invoked when the control receives a left mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndLButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a right mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDown;

        /// <summary>
        /// Invoked when the control receives a right mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonUp;

        /// <summary>
        /// Invoked when the control receives a right mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndRButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a middle mouse down message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDown;

        /// <summary>
        /// Invoked when the control receives a middle mouse up message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonUp;

        /// <summary>
        /// Invoked when the control receives a middle mouse double click message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the first extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX1ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse down message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDown;

        /// <summary>
        /// Invoked when the control receives a mouse up message for the second extra button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonUp;

        /// <summary>
        /// Invoked when the control receives a double click message for the first extra mouse button.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndX2ButtonDblClick;

        /// <summary>
        /// Invoked when the control receives a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseMove;

        /// <summary>
        /// Invoked when the control first gets a mouse move message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseEnter;

        /// <summary>
        /// Invoked when the control gets a mouse leave message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseLeave;

        /// <summary>
        /// Invoked when the control gets a mouse wheel message.
        /// </summary>
        public event EventHandler<HwndMouseEventArgs> HwndMouseWheel;

        public GLGraphicsDevice()
        {
            
        }

        public void Initialize(string contentDirectory)
        {
            Load += new EventHandler(OTKGLGraphicsDevice_Load);

            content = new ContentManager(services, System.AppDomain.CurrentDomain.BaseDirectory);
            content.RootDirectory = contentDirectory;

            Paint += new System.Windows.Forms.PaintEventHandler(OTKGLGraphicsDevice_Paint);
            SizeChanged += new EventHandler(OTKGLGraphicsDevice_SizeChanged);
            // We must be notified of the application foreground status for our mouse input events
            Application.Current.Activated += new EventHandler(Current_Activated);
            Application.Current.Deactivated += new EventHandler(Current_Deactivated);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (graphicsService != null)
            {
                graphicsService.Release(disposing);
                graphicsService = null;
            }

            m_timer.Stop();
        }

        void Current_Activated(object sender, EventArgs e)
        {
            applicationHasFocus = true;
        }

        void Current_Deactivated(object sender, EventArgs e)
        {
            applicationHasFocus = false;
            ResetMouseState();

            if (mouseInWindow)
            {
                mouseInWindow = false;
                if (HwndMouseLeave != null)
                    HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));
            }
        }

        private void ResetMouseState()
        {
            // We need to invoke events for any buttons that were pressed
            bool fireL = mouseState.LeftButton == MouseButtonState.Pressed;
            bool fireM = mouseState.MiddleButton == MouseButtonState.Pressed;
            bool fireR = mouseState.RightButton == MouseButtonState.Pressed;
            bool fireX1 = mouseState.X1Button == MouseButtonState.Pressed;
            bool fireX2 = mouseState.X2Button == MouseButtonState.Pressed;

            // Update the state of all of the buttons
            mouseState.LeftButton = MouseButtonState.Released;
            mouseState.MiddleButton = MouseButtonState.Released;
            mouseState.RightButton = MouseButtonState.Released;
            mouseState.X1Button = MouseButtonState.Released;
            mouseState.X2Button = MouseButtonState.Released;

            // Fire any events
            HwndMouseEventArgs args = new HwndMouseEventArgs(mouseState);
            if (fireL && HwndLButtonUp != null)
                HwndLButtonUp(this, args);
            if (fireM && HwndMButtonUp != null)
                HwndMButtonUp(this, args);
            if (fireR && HwndRButtonUp != null)
                HwndRButtonUp(this, args);
            if (fireX1 && HwndX1ButtonUp != null)
                HwndX1ButtonUp(this, args);
            if (fireX2 && HwndX2ButtonUp != null)
                HwndX2ButtonUp(this, args);
            // The mouse is no longer considered to be in our window
            mouseInWindow = false;
        }

        void OTKGLGraphicsDevice_SizeChanged(object sender, EventArgs e)
        {
            if (graphicsService != null)
            {
                graphicsService.ResetDevice((int)Width, (int)Height);
                graphicsService.GraphicsDevice.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport(0, 0, Width, Height);
            }
        }

        protected virtual void OTKGLGraphicsDevice_Load(object sender, EventArgs e)
        {
            m_bLoaded = true;
            if (graphicsService == null)
            {
                graphicsService = GraphicsDeviceService.AddRef(Handle, (int)Width, (int)Height);
            }

            graphicsService.GraphicsDevice.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport(0, 0, Width, Height);
            services.AddService(typeof(IGraphicsDeviceService), graphicsService);

            if (LoadContent != null)
            {
                LoadContent(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));
            }

            

            m_timer = new System.Timers.Timer(1000.0f / 60.0f);
            m_timer.Elapsed += OnRender;
            m_timer.Start();
        }

        private void OnRender(object sender, ElapsedEventArgs e)
        {
            if(applicationHasFocus)
                this.Invoke(new System.Windows.Forms.MethodInvoker(this.Invalidate));
        }

        void OTKGLGraphicsDevice_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!m_bLoaded)
                return;

            if (RenderXna != null)
            {
                RenderXna(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));
            }
            SwapBuffers();
        }

        public int getWidth()
        {
            return this.Width;
        }

        public int getHeight()
        {
            return this.Height;
        }

        protected override void WndProc(ref System.Windows.Forms.Message msg)
        {
            switch (msg.Msg)
            {
                case NativeMethods.WM_LBUTTONDOWN:
                    mouseState.LeftButton = MouseButtonState.Pressed;
                    if (HwndLButtonDown != null)
                        HwndLButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    mouseState.LeftButton = MouseButtonState.Released;
                    if (HwndLButtonUp != null)
                        HwndLButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_LBUTTONDBLCLK:
                    if (HwndLButtonDblClick != null)
                        HwndLButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Left));
                    break;
                case NativeMethods.WM_RBUTTONDOWN:
                    mouseState.RightButton = MouseButtonState.Pressed;
                    if (HwndRButtonDown != null)
                        HwndRButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_RBUTTONUP:
                    mouseState.RightButton = MouseButtonState.Released;
                    if (HwndRButtonUp != null)
                        HwndRButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_RBUTTONDBLCLK:
                    if (HwndRButtonDblClick != null)
                        HwndRButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Right));
                    break;
                case NativeMethods.WM_MBUTTONDOWN:
                    mouseState.MiddleButton = MouseButtonState.Pressed;
                    if (HwndMButtonDown != null)
                        HwndMButtonDown(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_MBUTTONUP:
                    mouseState.MiddleButton = MouseButtonState.Released;
                    if (HwndMButtonUp != null)
                        HwndMButtonUp(this, new HwndMouseEventArgs(mouseState));
                    break;
                case NativeMethods.WM_MBUTTONDBLCLK:
                    if (HwndMButtonDblClick != null)
                        HwndMButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.Middle));
                    break;
                case NativeMethods.WM_XBUTTONDOWN:
                    if (((int)msg.WParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        mouseState.X1Button = MouseButtonState.Pressed;
                        if (HwndX1ButtonDown != null)
                            HwndX1ButtonDown(this, new HwndMouseEventArgs(mouseState));
                    }
                    else if (((int)msg.WParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        mouseState.X2Button = MouseButtonState.Pressed;
                        if (HwndX2ButtonDown != null)
                            HwndX2ButtonDown(this, new HwndMouseEventArgs(mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONUP:
                    if (((int)msg.WParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        mouseState.X1Button = MouseButtonState.Released;
                        if (HwndX1ButtonUp != null)
                            HwndX1ButtonUp(this, new HwndMouseEventArgs(mouseState));
                    }
                    else if (((int)msg.WParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        mouseState.X2Button = MouseButtonState.Released;
                        if (HwndX2ButtonUp != null)
                            HwndX2ButtonUp(this, new HwndMouseEventArgs(mouseState));
                    }
                    break;
                case NativeMethods.WM_XBUTTONDBLCLK:
                    if (((int)msg.WParam & NativeMethods.MK_XBUTTON1) != 0)
                    {
                        if (HwndX1ButtonDblClick != null)
                            HwndX1ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton1));
                    }
                    else if (((int)msg.WParam & NativeMethods.MK_XBUTTON2) != 0)
                    {
                        if (HwndX2ButtonDblClick != null)
                            HwndX2ButtonDblClick(this, new HwndMouseEventArgs(mouseState, MouseButton.XButton2));
                    }
                    break;

                case NativeMethods.WM_MOUSEMOVE:
                    // If the application isn't in focus, we don't handle this message
                    if (!applicationHasFocus)
                        break;

                    // record the previous and new position of the mouse
                    mouseState.PreviousPosition = mouseState.Position;
                    mouseState.Position = new System.Windows.Point(
                        NativeMethods.GetXLParam((int)msg.LParam),
                        NativeMethods.GetYLParam((int)msg.LParam));

                    if (!mouseInWindow)
                    {
                        mouseInWindow = true;

                        // if the mouse is just entering, use the same position for the previous state
                        // so we don't get weird deltas happening when the move event fires
                        mouseState.PreviousPosition = mouseState.Position;

                        if (HwndMouseEnter != null)
                            HwndMouseEnter(this, new HwndMouseEventArgs(mouseState));

                        // send the track mouse event so that we get the WM_MOUSELEAVE message
                        NativeMethods.TRACKMOUSEEVENT tme = new NativeMethods.TRACKMOUSEEVENT();
                        tme.cbSize = Marshal.SizeOf(typeof(NativeMethods.TRACKMOUSEEVENT));
                        tme.dwFlags = NativeMethods.TME_LEAVE;
                        tme.hWnd = msg.HWnd;
                        NativeMethods.TrackMouseEvent(ref tme);
                    }

                    // Only fire the mouse move if the position actually changed
                    if (mouseState.Position != mouseState.PreviousPosition)
                    {
                        if (HwndMouseMove != null)
                            HwndMouseMove(this, new HwndMouseEventArgs(mouseState));
                    }

                    break;
                case NativeMethods.WM_MOUSELEAVE:

                    // If we have capture, we ignore this message because we're just
                    // going to reset the cursor position back into the window
                    if (isMouseCaptured)
                        break;

                    // Reset the state which releases all buttons and 
                    // marks the mouse as not being in the window.
                    ResetMouseState();

                    if (HwndMouseLeave != null)
                        HwndMouseLeave(this, new HwndMouseEventArgs(mouseState));
                    break;
                default:
                    break;
            }
            base.WndProc(ref msg);
        }
    }
}