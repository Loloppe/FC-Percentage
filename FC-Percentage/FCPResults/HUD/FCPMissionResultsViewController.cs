﻿#nullable enable

using FCPercentage.FCPCore;
using FCPercentage.FCPCore.Configuration;
using FCPercentage.FCPResults.Configuration;
using SiraUtil.Logging;
using UnityEngine;

namespace FCPercentage.FCPResults.HUD
{
	class FCPMissionResultsViewController : ResultsController
	{
		internal override string ResourceNameFCPercentage => "FCPercentage.FCPResults.HUD.BSML.MissionResultsPercentageResult.bsml";
		internal override string ResourceNameFCScore => "FCPercentage.FCPResults.HUD.BSML.MissionResultsScoreResult.bsml";

		internal override ResultsSettings config { get; set; }
		internal override ResultsTextFormattingModel textModel { get; set; }

		public FCPMissionResultsViewController(SiraLog logger, ScoreManager scoreManager, MissionResultsViewController resultsViewController) : base(logger, scoreManager, resultsViewController)
		{
			config = PluginConfig.Instance.MissionResultsSettings;
			textModel = new ResultsTextFormattingModel(scoreManager, config, GetDiffCalculationModel());
		}

		internal override LevelCompletionResults? GetLevelCompletionResults()
		{
			MissionResultsViewController resViewController = (MissionResultsViewController)resultsViewController;
			MissionCompletionResults missionCompletionResults = Accessors.MissionCompletionResults(ref resViewController);
			if (missionCompletionResults == null)
				return null;

			return missionCompletionResults.levelCompletionResults;
		}

		internal override GameObject GetViewControllerGameObject()
		{
			return ((MissionResultsViewController)resultsViewController).gameObject;
		}
	}
}
