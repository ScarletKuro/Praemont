////////////////////////////////////////////////////////////////////////////////
// StickyWindows
// 
// Copyright (c) 2004 Corneliu I. Tusnea
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the author be held liable for any damages arising from 
// the use of this software.
// Permission to use, copy, modify, distribute and sell this software for any 
// purpose is hereby granted without fee, provided that the above copyright 
// notice appear in all copies and that both that copyright notice and this 
// permission notice appear in supporting documentation.
//
// Notice: Check CodeProject for details about using this class
//
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using FirstFloor.ModernUI.Windows.Controls;
using NoteCore.Annotations;
using Point = System.Drawing.Point;

#region Win32Imports

namespace NoteCore.Feature.StickyWindow
{
    /// <summary>
    ///     Win32 is just a placeholder for some Win32 imported definitions
    /// </summary>
    [UsedImplicitly]
    public class Win32
    {
        [UsedImplicitly]
        public class Bit
        {
            public static int HiWord(int iValue)
            {
                return ((iValue >> 16) & 0xFFFF);
            }

            public static int LoWord(int iValue)
            {
                return (iValue & 0xFFFF);
            }
        }

        /// <summary>
        ///     HT is just a placeholder for HT (HitTest) definitions
        /// </summary>
        [UsedImplicitly]
        public class Ht
        {
            public const int HTCAPTION = 2;
            public const int HTLEFT = 10;
            public const int HTRIGHT = 11;
            public const int HTTOP = 12;
            public const int HTTOPLEFT = 13;
            public const int HTTOPRIGHT = 14;
            public const int HTBOTTOM = 15;
            public const int HTBOTTOMLEFT = 16;
            public const int HTBOTTOMRIGHT = 17;
        }

        /// <summary>
        ///     VK is just a placeholder for VK (VirtualKey) general definitions
        /// </summary>
        [UsedImplicitly]
        public class Vk
        {
            public const int VkEscape = 0x1B;
        }

        /// <summary>
        ///     WM is just a placeholder class for WM (WindowMessage) definitions
        /// </summary>
        [UsedImplicitly]
        public class Wm
        {
            public const int WmMousemove = 0x0200;
            public const int WmNclbuttondown = 0x00A1;
            public const int WmLbuttonup = 0x0202;
            public const int WmKeydown = 0x0100;
        }
    }
}

#endregion

namespace NoteCore.Feature.StickyWindow
{
    /// <summary>
    ///     A windows that Sticks to other windows of the same type when moved or resized.
    ///     You get a nice way of organizing multiple top-level windows.
    ///     Quite similar with WinAmp 2.x style of sticking the windows
    /// </summary>
    public class StickyWindow : NativeWindow
    {

        // public properties
        private const int _stickGap = 20; // distance to stick

        #region ResizeDir

        [Flags]
        private enum ResizeDir
        {
            Top = 2,
            Bottom = 4,
            Left = 8,
            Right = 16
        };

        #endregion

        #region Message Processor

        // Internal Message Processor

        // Messages processors based on type
        private readonly ProcessMessage _defaultMessageProcessor;
        private readonly ProcessMessage _moveMessageProcessor;
        private readonly ProcessMessage _resizeMessageProcessor;
        private ProcessMessage _messageProcessor;

        private delegate bool ProcessMessage(ref Message m);

        #endregion

        #region Internal properties

        // Move stuff
        private readonly WpfFormAdapter _originalForm; // the form
        private Point _formOffsetPoint; // calculated offset rect to be added !! (min distances in all directions!!)
        private Rectangle _formOffsetRect; // calculated rect to fix the size
        private Rectangle _formOriginalRect; // bounds before last operation started
        private Rectangle _formRect; // form bounds
        private Point _mousePoint; // mouse position
        private bool _movingForm;
        private Point _offsetPoint; // primary offset
        private ResizeDir _resizeDirection;
        private bool _resizingForm;

        #endregion

        private bool _stickOnMove;
        private bool _stickOnResize;
        private bool _stickToScreen;

        #region Public operations and properties

        /// <summary>
        ///     Allow the form to stick while resizing
        ///     Default value = true
        /// </summary>
        public bool StickOnResize
        {
            set { _stickOnResize = value; }
        }

        /// <summary>
        ///     Allow the form to stick while moving
        ///     Default value = true
        /// </summary>
        public bool StickOnMove
        {
            set { _stickOnMove = value; }
        }

        /// <summary>
        ///     Allow sticking to Screen Margins
        ///     Default value = true
        /// </summary>
        public bool StickToScreen
        {
            set { _stickToScreen = value; }
        }

        #endregion

        #region StickyWindow Constructor

        // <summary>
        /// Make the window Sticky
        /// 
        /// <param name="window"></param>
        public StickyWindow(ModernWindow window)
            : this(new WpfFormAdapter(window))
        {
        }

        /// <summary>
        ///     Make the form Sticky
        /// </summary>
        /// <param name="form">Form to be made sticky</param>
        private StickyWindow(WpfFormAdapter form)
        {
            _resizingForm = false;
            _movingForm = false;

            _originalForm = form;

            _formRect = Rectangle.Empty;
            _formOffsetRect = Rectangle.Empty;

            _formOffsetPoint = Point.Empty;
            _offsetPoint = Point.Empty;
            _mousePoint = Point.Empty;

            _stickOnMove = true;
            _stickOnResize = true;
            _stickToScreen = true;

            _defaultMessageProcessor = DefaultMsgProcessor;
            _moveMessageProcessor = MoveMsgProcessor;
            _resizeMessageProcessor = ResizeMsgProcessor;
            _messageProcessor = _defaultMessageProcessor;

            AssignHandle(_originalForm.Handle);
        }

        #endregion

        #region OnHandleChange



        #endregion

        #region WndProc

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (!_messageProcessor(ref m))
                base.WndProc(ref m);
        }

        #endregion

        #region DefaultMsgProcessor

        /// <summary>
        ///     Processes messages during normal operations (while the form is not resized or moved)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool DefaultMsgProcessor(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.Wm.WmNclbuttondown:
                {
                    _originalForm.Activate();
                    _mousePoint.X = (short) Win32.Bit.LoWord((int) m.LParam);
                    _mousePoint.Y = (short) Win32.Bit.HiWord((int) m.LParam);
                    if (OnNclButtonDown((int) m.WParam, _mousePoint))
                    {
                        //m.Result = new IntPtr ( (resizingForm || movingForm) ? 1 : 0 );
                        m.Result = (IntPtr) ((_resizingForm || _movingForm) ? 1 : 0);
                        return true;
                    }
                    break;
                }
            }

            return false;
        }

        #endregion

        #region OnNCLButtonDown

        /// <summary>
        ///     Checks where the click was in the NC area and starts move or resize operation
        /// </summary>
        /// <param name="iHitTest"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool OnNclButtonDown(int iHitTest, Point point)
        {
            Rectangle rParent = _originalForm.Bounds;
            _offsetPoint = point;

            switch (iHitTest)
            {
                case Win32.Ht.HTCAPTION:
                {
                    // request for move
                    if (_stickOnMove)
                    {
                        _offsetPoint.Offset(-rParent.Left, -rParent.Top);
                        StartMove();
                        return true;
                    }
                    return false; // leave default processing
                }

                    // requests for resize
                case Win32.Ht.HTTOPLEFT:
                    return StartResize(ResizeDir.Top | ResizeDir.Left);
                case Win32.Ht.HTTOP:
                    return StartResize(ResizeDir.Top);
                case Win32.Ht.HTTOPRIGHT:
                    return StartResize(ResizeDir.Top | ResizeDir.Right);
                case Win32.Ht.HTRIGHT:
                    return StartResize(ResizeDir.Right);
                case Win32.Ht.HTBOTTOMRIGHT:
                    return StartResize(ResizeDir.Bottom | ResizeDir.Right);
                case Win32.Ht.HTBOTTOM:
                    return StartResize(ResizeDir.Bottom);
                case Win32.Ht.HTBOTTOMLEFT:
                    return StartResize(ResizeDir.Bottom | ResizeDir.Left);
                case Win32.Ht.HTLEFT:
                    return StartResize(ResizeDir.Left);
            }

            return false;
        }

        #endregion

        #region ResizeOperations

        private bool StartResize(ResizeDir resDir)
        {
            if (_stickOnResize)
            {
                _resizeDirection = resDir;
                _formRect = _originalForm.Bounds;
                _formOriginalRect = _originalForm.Bounds; // save the old bounds

                if (!_originalForm.Capture) // start capturing messages
                    _originalForm.Capture = true;

                _messageProcessor = _resizeMessageProcessor;

                return true; // catch the message
            }
            return false; // leave default processing !
        }

        private bool ResizeMsgProcessor(ref Message m)
        {
            if (!_originalForm.Capture)
            {
                Cancel();
                return false;
            }

            switch (m.Msg)
            {
                case Win32.Wm.WmLbuttonup:
                {
                    // ok, resize finished !!!
                    EndResize();
                    break;
                }
                case Win32.Wm.WmMousemove:
                {
                    _mousePoint.X = (short) Win32.Bit.LoWord((int) m.LParam);
                    _mousePoint.Y = (short) Win32.Bit.HiWord((int) m.LParam);
                    Resize(_mousePoint);
                    break;
                }
                case Win32.Wm.WmKeydown:
                {
                    if ((int) m.WParam == Win32.Vk.VkEscape)
                    {
                        _originalForm.Bounds = _formOriginalRect; // set back old size
                        Cancel();
                    }
                    break;
                }
            }

            return false;
        }

        private void EndResize()
        {
            Cancel();
        }

        #endregion

        #region Resize Computing

        private void Resize(Point p)
        {
            p = _originalForm.PointToScreen(p);
            var activeScr = Screen.FromPoint(p);
            _formRect = _originalForm.Bounds;

            int iRight = _formRect.Right;
            int iBottom = _formRect.Bottom;

            // no normalize required
            // first strech the window to the new position
            if ((_resizeDirection & ResizeDir.Left) == ResizeDir.Left)
            {
                _formRect.Width = _formRect.X - p.X + _formRect.Width;
                _formRect.X = iRight - _formRect.Width;
            }
            if ((_resizeDirection & ResizeDir.Right) == ResizeDir.Right)
                _formRect.Width = p.X - _formRect.Left;

            if ((_resizeDirection & ResizeDir.Top) == ResizeDir.Top)
            {
                _formRect.Height = _formRect.Height - p.Y + _formRect.Top;
                _formRect.Y = iBottom - _formRect.Height;
            }
            if ((_resizeDirection & ResizeDir.Bottom) == ResizeDir.Bottom)
                _formRect.Height = p.Y - _formRect.Top;

            // this is the real new position
            // now, try to snap it to different objects (first to screen)

            // CARE !!! We use "Width" and "Height" as Bottom & Right!! (C++ style)
            //formOffsetRect = new Rectangle ( stickGap + 1, stickGap + 1, 0, 0 );
            _formOffsetRect.X = _stickGap + 1;
            _formOffsetRect.Y = _stickGap + 1;
            _formOffsetRect.Height = 0;
            _formOffsetRect.Width = 0;

            if (_stickToScreen)
                Resize_Stick(activeScr.WorkingArea, false);


            // Fix (clear) the values that were not updated to stick
            if (_formOffsetRect.X == _stickGap + 1)
                _formOffsetRect.X = 0;
            if (_formOffsetRect.Width == _stickGap + 1)
                _formOffsetRect.Width = 0;
            if (_formOffsetRect.Y == _stickGap + 1)
                _formOffsetRect.Y = 0;
            if (_formOffsetRect.Height == _stickGap + 1)
                _formOffsetRect.Height = 0;

            // compute the new form size
            if ((_resizeDirection & ResizeDir.Left) == ResizeDir.Left)
            {
                // left resize requires special handling of X & Width acording to MinSize and MinWindowTrackSize
                int iNewWidth = _formRect.Width + _formOffsetRect.Width + _formOffsetRect.X;

                if (_originalForm.MaximumSize.Width != 0)
                    iNewWidth = Math.Min(iNewWidth, _originalForm.MaximumSize.Width);

                iNewWidth = Math.Min(iNewWidth, SystemInformation.MaxWindowTrackSize.Width);
                iNewWidth = Math.Max(iNewWidth, _originalForm.MinimumSize.Width);
                iNewWidth = Math.Max(iNewWidth, SystemInformation.MinWindowTrackSize.Width);

                _formRect.X = iRight - iNewWidth;
                _formRect.Width = iNewWidth;
            }
            else
            {
                // other resizes
                _formRect.Width += _formOffsetRect.Width + _formOffsetRect.X;
            }

            if ((_resizeDirection & ResizeDir.Top) == ResizeDir.Top)
            {
                int iNewHeight = _formRect.Height + _formOffsetRect.Height + _formOffsetRect.Y;

                if (_originalForm.MaximumSize.Height != 0)
                    iNewHeight = Math.Min(iNewHeight, _originalForm.MaximumSize.Height);

                iNewHeight = Math.Min(iNewHeight, SystemInformation.MaxWindowTrackSize.Height);
                iNewHeight = Math.Max(iNewHeight, _originalForm.MinimumSize.Height);
                iNewHeight = Math.Max(iNewHeight, SystemInformation.MinWindowTrackSize.Height);

                _formRect.Y = iBottom - iNewHeight;
                _formRect.Height = iNewHeight;
            }
            else
            {
                // all other resizing are fine 
                _formRect.Height += _formOffsetRect.Height + _formOffsetRect.Y;
            }

            // Done !!
            _originalForm.Bounds = _formRect;
        }

        private void Resize_Stick(Rectangle toRect, bool bInsideStick)
        {
            if (_formRect.Right >= (toRect.Left - _stickGap) && _formRect.Left <= (toRect.Right + _stickGap))
            {
                if ((_resizeDirection & ResizeDir.Top) == ResizeDir.Top)
                {
                    if (Math.Abs(_formRect.Top - toRect.Bottom) <= Math.Abs(_formOffsetRect.Top) && bInsideStick)
                        _formOffsetRect.Y = _formRect.Top - toRect.Bottom; // snap top to bottom
                    else if (Math.Abs(_formRect.Top - toRect.Top) <= Math.Abs(_formOffsetRect.Top))
                        _formOffsetRect.Y = _formRect.Top - toRect.Top; // snap top to top
                }

                if ((_resizeDirection & ResizeDir.Bottom) == ResizeDir.Bottom)
                {
                    if (Math.Abs(_formRect.Bottom - toRect.Top) <= Math.Abs(_formOffsetRect.Bottom) && bInsideStick)
                        _formOffsetRect.Height = toRect.Top - _formRect.Bottom; // snap Bottom to top
                    else if (Math.Abs(_formRect.Bottom - toRect.Bottom) <= Math.Abs(_formOffsetRect.Bottom))
                        _formOffsetRect.Height = toRect.Bottom - _formRect.Bottom; // snap bottom to bottom
                }
            }

            if (_formRect.Bottom >= (toRect.Top - _stickGap) && _formRect.Top <= (toRect.Bottom + _stickGap))
            {
                if ((_resizeDirection & ResizeDir.Right) == ResizeDir.Right)
                {
                    if (Math.Abs(_formRect.Right - toRect.Left) <= Math.Abs(_formOffsetRect.Right) && bInsideStick)
                        _formOffsetRect.Width = toRect.Left - _formRect.Right; // Stick right to left
                    else if (Math.Abs(_formRect.Right - toRect.Right) <= Math.Abs(_formOffsetRect.Right))
                        _formOffsetRect.Width = toRect.Right - _formRect.Right; // Stick right to right
                }

                if ((_resizeDirection & ResizeDir.Left) == ResizeDir.Left)
                {
                    if (Math.Abs(_formRect.Left - toRect.Right) <= Math.Abs(_formOffsetRect.Left) && bInsideStick)
                        _formOffsetRect.X = _formRect.Left - toRect.Right; // Stick left to right
                    else if (Math.Abs(_formRect.Left - toRect.Left) <= Math.Abs(_formOffsetRect.Left))
                        _formOffsetRect.X = _formRect.Left - toRect.Left; // Stick left to left
                }
            }
        }

        #endregion

        #region Move Operations

        private void StartMove()
        {
            _formRect = _originalForm.Bounds;
            _formOriginalRect = _originalForm.Bounds; // save original position

            if (!_originalForm.Capture) // start capturing messages
                _originalForm.Capture = true;

            _messageProcessor = _moveMessageProcessor;
        }

        private bool MoveMsgProcessor(ref Message m)
        {
            // internal message loop
            if (!_originalForm.Capture)
            {
                Cancel();
                return false;
            }

            switch (m.Msg)
            {
                case Win32.Wm.WmLbuttonup:
                {
                    // ok, move finished !!!
                    EndMove();
                    break;
                }
                case Win32.Wm.WmMousemove:
                {
                    _mousePoint.X = (short) Win32.Bit.LoWord((int) m.LParam);
                    _mousePoint.Y = (short) Win32.Bit.HiWord((int) m.LParam);
                    Move(_mousePoint);
                    break;
                }
                case Win32.Wm.WmKeydown:
                {
                    if ((int) m.WParam == Win32.Vk.VkEscape)
                    {
                        _originalForm.Bounds = _formOriginalRect; // set back old size
                        Cancel();
                    }
                    break;
                }
            }

            return false;
        }

        private void EndMove()
        {
            Cancel();
        }

        #endregion

        #region Move Computing

        private void Move(Point p)
        {
            p = _originalForm.PointToScreen(p);
            var activeScr = Screen.FromPoint(p); // get the screen from the point !!

            if (!activeScr.WorkingArea.Contains(p))
            {
                p.X = NormalizeInside(p.X, activeScr.WorkingArea.Left, activeScr.WorkingArea.Right);
                p.Y = NormalizeInside(p.Y, activeScr.WorkingArea.Top, activeScr.WorkingArea.Bottom);
            }

            p.Offset(-_offsetPoint.X, -_offsetPoint.Y);

            // p is the exact location of the frame - so we can play with it
            // to detect the new position acording to different bounds
            _formRect.Location = p; // this is the new positon of the form

            _formOffsetPoint.X = _stickGap + 1; // (more than) maximum gaps
            _formOffsetPoint.Y = _stickGap + 1;

            if (_stickToScreen)
                Move_Stick(activeScr.WorkingArea, false);


            if (_formOffsetPoint.X == _stickGap + 1)
                _formOffsetPoint.X = 0;
            if (_formOffsetPoint.Y == _stickGap + 1)
                _formOffsetPoint.Y = 0;

            _formRect.Offset(_formOffsetPoint);

            _originalForm.Bounds = _formRect;
        }

        /// <summary>
        /// </summary>
        /// <param name="toRect">Rect to try to snap to</param>
        /// <param name="bInsideStick">Allow snapping on the inside (eg: window to screen)</param>
        private void Move_Stick(Rectangle toRect, bool bInsideStick)
        {
            // compare distance from toRect to formRect
            // and then with the found distances, compare the most closed position
            if (_formRect.Bottom >= (toRect.Top - _stickGap) && _formRect.Top <= (toRect.Bottom + _stickGap))
            {
                if (bInsideStick)
                {
                    if ((Math.Abs(_formRect.Left - toRect.Right) <= Math.Abs(_formOffsetPoint.X)))
                    {
                        // left 2 right
                        _formOffsetPoint.X = toRect.Right - _formRect.Left;
                    }
                    if ((Math.Abs(_formRect.Left + _formRect.Width - toRect.Left) <= Math.Abs(_formOffsetPoint.X)))
                    {
                        // right 2 left
                        _formOffsetPoint.X = toRect.Left - _formRect.Width - _formRect.Left;
                    }
                }

                if (Math.Abs(_formRect.Left - toRect.Left) <= Math.Abs(_formOffsetPoint.X))
                {
                    // snap left 2 left
                    _formOffsetPoint.X = toRect.Left - _formRect.Left;
                }
                if (Math.Abs(_formRect.Left + _formRect.Width - toRect.Left - toRect.Width) <= Math.Abs(_formOffsetPoint.X))
                {
                    // snap right 2 right
                    _formOffsetPoint.X = toRect.Left + toRect.Width - _formRect.Width - _formRect.Left;
                }
            }
            if (_formRect.Right >= (toRect.Left - _stickGap) && _formRect.Left <= (toRect.Right + _stickGap))
            {
                if (bInsideStick)
                {
                    if (Math.Abs(_formRect.Top - toRect.Bottom) <= Math.Abs(_formOffsetPoint.Y))
                    {
                        // Stick Top to Bottom
                        _formOffsetPoint.Y = toRect.Bottom - _formRect.Top;
                    }
                    if (Math.Abs(_formRect.Top + _formRect.Height - toRect.Top) <= Math.Abs(_formOffsetPoint.Y))
                    {
                        // snap Bottom to Top
                        _formOffsetPoint.Y = toRect.Top - _formRect.Height - _formRect.Top;
                    }
                }

                // try to snap top 2 top also
                if (Math.Abs(_formRect.Top - toRect.Top) <= Math.Abs(_formOffsetPoint.Y))
                {
                    // top 2 top
                    _formOffsetPoint.Y = toRect.Top - _formRect.Top;
                }
                if (Math.Abs(_formRect.Top + _formRect.Height - toRect.Top - toRect.Height) <= Math.Abs(_formOffsetPoint.Y))
                {
                    // bottom 2 bottom
                    _formOffsetPoint.Y = toRect.Top + toRect.Height - _formRect.Height - _formRect.Top;
                }
            }
        }

        #endregion

        #region Utilities

        private int NormalizeInside(int iP1, int iM1, int iM2)
        {
            return iP1 <= iM1 ? iM1 : (iP1 >= iM2 ? iM2 : iP1);
        }

        #endregion

        #region Cancel

        private void Cancel()
        {
            _originalForm.Capture = false;
            _movingForm = false;
            _resizingForm = false;
            _messageProcessor = _defaultMessageProcessor;
        }

        #endregion
    }
}