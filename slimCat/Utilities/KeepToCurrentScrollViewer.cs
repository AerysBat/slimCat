﻿#region Copyright

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeepToCurrentScrollViewer.cs">
//     Copyright (c) 2013, Justin Kadrovach, All rights reserved.
//  
//     This source is subject to the Simplified BSD License.
//     Please see the License.txt file for more information.
//     All other rights reserved.
// 
//     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
//     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//     PARTICULAR PURPOSE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#endregion

namespace slimCat.Utilities
{
    #region Usings

    using slimCat.Models;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    #endregion

    /// <summary>
    ///     This handles scroll management for async loading objects.
    /// </summary>
    public class KeepToCurrentScrollViewer
    {
        #region Fields

        private readonly ScrollViewer scroller;

        private bool hookedToBottom = true;

        private double lastHeight;

        private double lastValue;

        private double distanceToBottom;

        #endregion

        #region Constructors and Destructors

        public KeepToCurrentScrollViewer(DependencyObject toManage)
        {
            toManage.ThrowIfNull("toManage");

            scroller = StaticFunctions.FindChild<ScrollViewer>(toManage);
            if (scroller != null)
                scroller.ScrollChanged += OnScrollChanged;
        }

        #endregion

        #region Public Methods and Operators

        public void ScrollToStick()
        {
            var change = scroller.ScrollableHeight - lastValue;
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset + change);
        }

        public void Stick()
        {
            lastValue = scroller.ScrollableHeight;
        }

        public void Scroll(int scrollTicks)
        {
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - StaticFunctions.GetScrollDistance(scrollTicks, ApplicationSettings.FontSize));
        }

        public void StabilizeNextScroll()
        {
            distanceToBottom = scroller.ExtentHeight - scroller.VerticalOffset;
        }

        #endregion

        #region Methods

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var difference = Math.Abs(scroller.ScrollableHeight - lastHeight);
            lastHeight = scroller.ScrollableHeight;

            if (Math.Abs(difference - 0) > 0.01)
            {
                if (hookedToBottom)
                    scroller.ScrollToBottom();
                else if (distanceToBottom != 0.0d)
                    scroller.ScrollToVerticalOffset(scroller.ExtentHeight - distanceToBottom);
            }
            else if (Math.Abs(e.VerticalOffset - scroller.ScrollableHeight) < 20)
                hookedToBottom = true;
            else
                hookedToBottom = false;

            distanceToBottom = 0.0d;
        }

        #endregion
    }
}