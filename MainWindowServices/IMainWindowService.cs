using MahApps.Metro.Controls;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Text;

namespace MainWindowServices
{
	/// <summary>MainWindowの値を中継するサービスのインタフェースを表します。</summary>
	public interface IMainWindowService
	{
		/// <summary>TransitioningContentControlのTransitionを取得・設定します。</summary>
		ReactivePropertySlim<TransitionType> ContentControlTransition { get; set; }

		/// <summary>HamburgerMenuのDisplayModeを取得・設定します。</summary>
		ReactivePropertySlim<SplitViewDisplayMode> HamburgerMenuDisplayMode { get; set; }

		/// <summary>HamburgerMenuのIsPaneOpenを取得・設定します。</summary>
		ReactivePropertySlim<bool> IsHamburgerMenuPanelOpened { get; set; }
	}
}
