using System;
using BeatSaberMarkupLanguage.Settings;
using Zenject;

namespace FCPercentage.FCPResults.Configuration
{
	class ResultsConfigManager : IInitializable, IDisposable
	{
		private ResultsConfigController configHost;

		[Inject]
		public ResultsConfigManager(ResultsConfigController configHost)
		{
			this.configHost = configHost;
		}

		public void Initialize()
		{
			BSMLSettings.Instance.AddSettingsMenu(Plugin.PluginName, "FCPercentage.FCPResults.Configuration.BSML.ResultsSettings.bsml", configHost);
		}

		public void Dispose()
		{
			if (configHost == null)
				return;

			BSMLSettings.Instance.RemoveSettingsMenu(configHost);
			configHost = null!;
		}
	}
}
