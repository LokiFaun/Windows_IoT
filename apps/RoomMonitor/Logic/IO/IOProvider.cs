using Dashboard.Logic.News;
using Dashboard.View;
using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Dashboard.Logic.IO
{
    internal class IOProvider : IDisposable
    {
        private const int NextPagePin = 27;
        private const int NextSubRedditPin = 22;
        private const int ShutdownPin = 17;
        private static readonly GpioController m_Controller = GpioController.GetDefault();

        private readonly TimeSpan DebounceTimeout = new TimeSpan(500);
        private readonly Container m_Container;
        private readonly GpioPin m_NextPagePin;
        private readonly GpioPin m_NextSubRedditPin;
        private readonly GpioPin m_ShutdownPin;

        private int m_CurrentPage = 0;
        private int m_CurrentSubReddit = 0;
        private bool m_IsBlanked = false;
        private bool m_IsDisposed = false;

        private IList<Type> m_Pages = new List<Type>
        {
            typeof(MainPage), typeof(NewsPage), typeof(SensorPage)
        };

        private IList<string> m_SubReddits = new List<string>
        {
            "todayilearned", "worldnews", "nfl", "witcher", "technology"
        };

        public IOProvider(Container container)
        {
            m_Container = container;

            if (m_Controller != null)
            {
                m_NextPagePin = m_Controller.OpenPin(NextPagePin);
                m_NextSubRedditPin = m_Controller.OpenPin(NextSubRedditPin);
                m_ShutdownPin = m_Controller.OpenPin(ShutdownPin);
            }

            if (m_ShutdownPin != null)
            {
                m_ShutdownPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                m_ShutdownPin.DebounceTimeout = DebounceTimeout;
                m_ShutdownPin.ValueChanged += InputValueChanged;
            }

            if (m_NextPagePin != null)
            {
                m_NextPagePin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                m_NextPagePin.DebounceTimeout = DebounceTimeout;
                m_NextPagePin.ValueChanged += InputValueChanged;
            }

            if (m_NextSubRedditPin != null)
            {
                m_NextSubRedditPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                m_NextSubRedditPin.DebounceTimeout = DebounceTimeout;
                m_NextSubRedditPin.ValueChanged += InputValueChanged;
            }
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instance references
        /// </summary>
        /// <param name="disposing"><c>true</c> to dispose references</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (m_ShutdownPin != null)
                {
                    m_ShutdownPin.Dispose();
                }

                if (m_NextPagePin != null)
                {
                    m_NextPagePin.Dispose();
                }

                if (m_NextSubRedditPin != null)
                {
                    m_NextSubRedditPin.Dispose();
                }
            }

            m_IsDisposed = true;
        }

        private void InputValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge != GpioPinEdge.FallingEdge)
            {
                return;
            }

            if (sender == m_ShutdownPin)
            {
                DispatcherHelper.RunOnUIThread(() =>
                {
                    var frame = m_Container.Resolve<Window>().Content as Frame;
                    frame.Navigate(m_IsBlanked ? typeof(MainPage) : typeof(BlankPage));
                    m_IsBlanked = !m_IsBlanked;
                });
            }

            if (sender == m_NextSubRedditPin)
            {
                var rssProvider = m_Container.Resolve<RssProvider>();
                rssProvider.Subreddit = NextSubReddit();
            }

            if (sender == m_NextPagePin)
            {
                DispatcherHelper.RunOnUIThread(() =>
                {
                    var frame = m_Container.Resolve<Window>().Content as Frame;
                    frame.Navigate(NextPage());
                    m_IsBlanked = false;
                });
            }
        }

        private Type NextPage()
        {
            if (m_CurrentPage == m_Pages.Count)
            {
                m_CurrentPage = 0;
            }

            return m_Pages[m_CurrentPage++];
        }

        private string NextSubReddit()
        {
            if (m_CurrentSubReddit == m_SubReddits.Count)
            {
                m_CurrentSubReddit = 0;
            }

            return m_SubReddits[m_CurrentSubReddit++];
        }
    }
}