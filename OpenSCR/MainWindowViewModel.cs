using System;
using System.Collections.ObjectModel;
using HalationGhost.WinApps;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using OpenSCR.Menus;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using MainWindowServices;
using System.Web.UI;
using Microsoft.Extensions.Logging;
using OpenSCRLib;
using Prism.Ioc;
using VideoViews.ViewModels;

namespace OpenSCR
{
	/// <summary>MainWindowのVM</summary>
	public class MainWindowViewModel : HalationGhostViewModelBase, IDisposable
	{
		#region プロパティ

		/// <summary>TransitioningContentControlのTransitionを取得・設定します。</summary>
		public ReadOnlyReactivePropertySlim<TransitionType> ContentControlTransition { get; }

		/// <summary>HamburgerMenu.IsPaneOpenを取得・設定します。</summary>
		public ReactivePropertySlim<bool> IsHamburgerMenuPanelOpened { get; set; }

		/// <summary>HamburgerMenu.DisplayModeを取得・設定します。</summary>
		public ReadOnlyReactivePropertySlim<SplitViewDisplayMode> HamburgerMenuDisplayMode { get; }

		/// <summary>HamburgerMenuで選択しているメニュー項目を取得・設定します。</summary>
		public ReactivePropertySlim<HamburgerMenuItemViewModel> SelectedMenu { get; set; }

		/// <summary>HamburgerMenuで選択しているメニュー項目のインデックスを取得・設定します。</summary>
		public ReactivePropertySlim<int> SelectedMenuIndex { get; set; }

		/// <summary>HamburgerMenuで選択しているオプションメニュー項目を取得・設定します。</summary>
		public ReactivePropertySlim<HamburgerMenuItemViewModel> SelectedOption { get; set; }

		/// <summary>HamburgerMenuで選択しているオプションメニュー項目のインデックスを取得・設定します。</summary>
		public ReactivePropertySlim<int> SelectedOptionIndex { get; set; }

		/// <summary>HamburgerMenuのメニュー項目を取得します。</summary>
		public ObservableCollection<HamburgerMenuItemViewModel> MenuItems { get; } = new ObservableCollection<HamburgerMenuItemViewModel>();

		/// <summary>HamburgerMenuのオプションメニュー項目を取得します。</summary>
		public ObservableCollection<HamburgerMenuItemViewModel> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItemViewModel>();

		public ReactiveProperty<UserControl> DialogView { get; }

		public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

		private string _title = "Prism Application";
		/// <summary>Windowのタイトルを取得・設定します。</summary>
		public string Title
		{
			get { return _title; }
			set { SetProperty(ref _title, value); }
		}

		#endregion

		/// <summary>タイトルバー上のHomeボタンのCommand。</summary>
		public ReactiveCommand HomeCommand { get; }

		/// <summary>ContentRenderedイベントハンドラ。</summary>
		public ReactiveCommand ContentRendered { get; }

		/// <summary>HamburgerMenuのメニュー項目選択通知イベントハンドラ。</summary>
		/// <param name="item">選択したメニュー項目を表すHamburgerMenuItemViewModel。</param>
		private void OnSelectedMenu(HamburgerMenuItemViewModel item)
		{
			if (item == null)
				return;
			if (string.IsNullOrEmpty(item.NavigationPanel))
				return;

			this.regionManager.RequestNavigate("ContentRegion", item.NavigationPanel);
		}

		#region コンストラクタ

		/// <summary>MainWindowサービスを表します。</summary>
		private IMainWindowService _mainWindowService;

		/// <summary>デフォルトコンストラクタ。</summary>
		/// <param name="regionMan">IRegionManager。</param>
		/// <param name="containerProvider">IContainerProvider</param>
		/// <param name="winService">IMainWindowService。</param>
		public MainWindowViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService winService) : base(regionMan)
		{
			this._mainWindowService = winService;

            // DBからカメラ設定を取得する
            var dbAccessor = containerProvider.Resolve<DatabaseAccessor>();
            var loadedSettings = dbAccessor.FindCameraSettings();
            foreach (var loadedSetting in loadedSettings)
            {
                var cameraClient = new CaptureCameraClient(loadedSetting);
				cameraClient.StartCapture();
                this._mainWindowService.CaptureCameraClients[loadedSetting.CameraChannel] = cameraClient;
            }

			this.InitializeMenu();

			this.SelectedMenu = new ReactivePropertySlim<HamburgerMenuItemViewModel>(null)
				.AddTo(this.disposable);
			this.SelectedMenu.Subscribe(i => this.OnSelectedMenu(i));

			this.SelectedMenuIndex = new ReactivePropertySlim<int>(-1)
				.AddTo(this.disposable);

			this.SelectedOption = new ReactivePropertySlim<HamburgerMenuItemViewModel>(null)
				.AddTo(this.disposable);
			this.SelectedOption.Subscribe(o => this.OnSelectedMenu(o));

			this.SelectedOptionIndex = new ReactivePropertySlim<int>(-1)
				.AddTo(this.disposable);

			this.ContentControlTransition = this._mainWindowService.ContentControlTransition
				.ToReadOnlyReactivePropertySlim()
				.AddTo(this.disposable);
			this.HamburgerMenuDisplayMode = this._mainWindowService.HamburgerMenuDisplayMode
				.ToReadOnlyReactivePropertySlim()
				.AddTo(this.disposable);
			this.IsHamburgerMenuPanelOpened = this._mainWindowService.IsHamburgerMenuPanelOpened
				.AddTo(this.disposable);

			this.ContentRendered = new ReactiveCommand()
				.WithSubscribe(() => this.regionManager.RequestNavigate("ContentRegion", "VideoViewPanel"))
				.AddTo(this.disposable);

			this.DialogView = new ReactiveProperty<UserControl>()
				.AddTo(this.disposable);

			this.IsProgressRingDialogOpen = this._mainWindowService.IsProgressRingDialogOpen
				.AddTo(this.disposable);
		}

        public new void Dispose()
        {
            foreach (var cameraClientPair in _mainWindowService.CaptureCameraClients)
            {
                cameraClientPair.Value.Dispose();                
            }

			base.Dispose();
        }

		/// <summary>HamburgerMenuのメニュー項目を初期化します。</summary>
		private void InitializeMenu()
		{
			this.MenuItems.Add(new HamburgerMenuItemViewModel(PackIconFontAwesomeKind.VideoSolid, "ビデオ一覧", "VideoViewPanel"));

			this.OptionMenuItems.Add(new HamburgerMenuItemViewModel(PackIconFontAwesomeKind.CogsSolid, "設定", "OptionCommonPanel"));
			this.OptionMenuItems.Add(new HamburgerMenuItemViewModel(PackIconFontAwesomeKind.InfoCircleSolid, "このアプリケーションについて", "AboutPanel"));
		}

		#endregion
	}
}
