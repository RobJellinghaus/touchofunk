﻿/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using UnityPlayer;

namespace Touchofunk
{
	class App : IFrameworkView, IFrameworkViewSource
	{
		private WinRTBridge.WinRTBridge _Bridge;
		private AppCallbacks _AppCallbacks;

        private AudioGraphImpl _audioGraphImpl;

		public App()
		{
            DebugUtil.CheckMainThread();

			SetupOrientation();
			_AppCallbacks = new AppCallbacks();

			// Allow clients of this class to append their own callbacks.
			AddAppCallbacks(_AppCallbacks);
		}

		public virtual async void Initialize(CoreApplicationView applicationView)
		{
            DebugUtil.CheckAppThread();

			applicationView.Activated += ApplicationView_Activated;
			CoreApplication.Suspending += CoreApplication_Suspending;

			// Setup scripting bridge
			_Bridge = new WinRTBridge.WinRTBridge();
			_AppCallbacks.SetBridge(_Bridge);

			_AppCallbacks.SetCoreApplicationViewEvents(applicationView);

            _audioGraphImpl = new AudioGraphImpl();
            await _audioGraphImpl.InitializeAsync();
        }

        /// <summary>
        /// This is where apps can hook up any additional setup they need to do before Unity intializes.
        /// </summary>
        /// <param name="appCallbacks"></param>
        virtual protected void AddAppCallbacks(AppCallbacks appCallbacks)
		{
		}

		private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
		{

		}

		private void ApplicationView_Activated(CoreApplicationView sender, IActivatedEventArgs args)
		{
			CoreWindow.GetForCurrentThread().Activate();
        }

        public void SetWindow(CoreWindow coreWindow)
		{
			ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;
			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
#pragma warning disable 4014
			{
				StatusBar.GetForCurrentView().HideAsync();
			}
#pragma warning restore 4014

			_AppCallbacks.SetCoreWindowEvents(coreWindow);
			_AppCallbacks.InitializeD3DWindow();
		}

		public void Load(string entryPoint)
		{
            // TODO: where can this go so it actually works?????
            // this makes it be a hand until the Unity banner finishes, but then it reverts to arrow.  :-(
            _AppCallbacks.SetCursor(new CoreCursor(CoreCursorType.Hand, (uint)0));
        }

        public void Run()
		{
            _AppCallbacks.Run();
        }

        public void Uninitialize()
		{
		}

		[MTAThread]
		static void Main(string[] args)
		{
			var app = new App();
			CoreApplication.Run(app);
		}

		public IFrameworkView CreateView()
		{
			return this;
		}

		private void SetupOrientation()
		{
			Unity.UnityGenerated.SetupDisplay();
		}
	}
}
