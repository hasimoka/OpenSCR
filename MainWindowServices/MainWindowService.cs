using HalationGhost;
using MahApps.Metro.Controls;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using CameraClient.Models;

namespace MainWindowServices
{
	/// <summary>
	/// MainWindowの値を中継するサービスを表します。
	/// </summary>
	public class MainWindowService : BindableModelBase, IMainWindowService
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindowService()
		{
			HamburgerMenuDisplayMode = new ReactivePropertySlim<SplitViewDisplayMode>(SplitViewDisplayMode.CompactOverlay)
				.AddTo(this.Disposable);

			IsHamburgerMenuPanelOpened = new ReactivePropertySlim<bool>(false)
				.AddTo(this.Disposable);

			ContentControlTransition = new ReactivePropertySlim<TransitionType>(TransitionType.Default)
				.AddTo(this.Disposable);

			IsProgressRingDialogOpen = new ReactivePropertySlim<bool>(false)
				.AddTo(this.Disposable);

            CaptureCameraClients = new Dictionary<int, CaptureCameraClient>();
        }

        /// <summary>HamburgerMenuのDisplayModeを取得・設定します</summary>
        public ReactivePropertySlim<SplitViewDisplayMode> HamburgerMenuDisplayMode { get; set; }

        /// <summary>HamburgerMenuのIsPaneOpenを取得・設定します</summary>
        public ReactivePropertySlim<bool> IsHamburgerMenuPanelOpened { get; set; }

        /// <summary>TransitioningContentControlのTransitionを取得・設定します</summary>
        public ReactivePropertySlim<TransitionType> ContentControlTransition { get; set; }

        /// <summary>ProgressRingDialogのIsOpenを取得・設定します</summary>
        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public Dictionary<int, CaptureCameraClient> CaptureCameraClients { get; }
    }
}
