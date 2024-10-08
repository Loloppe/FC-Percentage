﻿namespace FCPercentage.FCPResults.Configuration
{
	public class ResultsAdvancedSettings
	{
		// Advanced settings available from config file
		public virtual string ScorePrefixText { get; set; } = DefaultScorePrefixText;
		public virtual string PercentagePrefixText { get; set; } = DefaultPercentagePrefixText;
		public virtual string PercentageTotalPrefixText { get; set; } = DefaultPercentageTotalPrefixText;
		public virtual string PercentageSplitSaberAPrefixText { get; set; } = DefaultPercentageSplitSaberAPrefixText;
		public virtual string PercentageSplitSaberBPrefixText { get; set; } = DefaultPercentageSplitSaberBPrefixText;
		public virtual string DifferencePositiveColor { get; set; } = DefaultDifferencePositiveColor;
		public virtual string DifferenceNegativeColor { get; set; } = DefaultDifferenceNegativeColor;
		public virtual bool ApplyColorsToScorePercentageModDifference { get; set; } = false;

		public static string DefaultScorePrefixText = "FC : ";
		public static string DefaultPercentagePrefixText = "FC : ";
		public static string DefaultPercentageTotalPrefixText = "";
		public static string DefaultPercentageSplitSaberAPrefixText = "";
		public static string DefaultPercentageSplitSaberBPrefixText = "";
		public static string DefaultDifferencePositiveColor = "#00B300";
		public static string DefaultDifferenceNegativeColor = "#FF0000";

		public static void RevertChanges(ResultsAdvancedSettings advancedSettings, ResultsAdvancedSettings oldAdvancedSettings)
		{
			advancedSettings.DifferencePositiveColor = oldAdvancedSettings.DifferencePositiveColor;
			advancedSettings.DifferenceNegativeColor = oldAdvancedSettings.DifferenceNegativeColor;
			advancedSettings.ApplyColorsToScorePercentageModDifference = oldAdvancedSettings.ApplyColorsToScorePercentageModDifference;

			advancedSettings.ScorePrefixText = oldAdvancedSettings.ScorePrefixText;
			advancedSettings.PercentagePrefixText = oldAdvancedSettings.PercentagePrefixText;
			advancedSettings.PercentageTotalPrefixText = oldAdvancedSettings.PercentageTotalPrefixText;
			advancedSettings.PercentageSplitSaberAPrefixText = oldAdvancedSettings.PercentageSplitSaberAPrefixText;
			advancedSettings.PercentageSplitSaberBPrefixText = oldAdvancedSettings.PercentageSplitSaberBPrefixText;
		}
	}
}
