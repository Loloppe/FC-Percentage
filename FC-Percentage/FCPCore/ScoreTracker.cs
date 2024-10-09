#nullable enable

using System;
using Zenject;
using FCPercentage.FCPCore.Configuration;
using System.Collections.Generic;
using static NoteData;
using SiraUtil.Logging;

namespace FCPercentage.FCPCore
{
	public class ScoreTracker : IInitializable, IDisposable, ICutScoreBufferDidChangeReceiver, ICutScoreBufferDidFinishReceiver
	{
		private readonly SiraLog logger;
		[InjectOptional] private GameplayCoreSceneSetupData sceneSetupData = null!;
		[Inject] private PlayerDataModel playerDataModel = null!;
		private readonly ScoreController scoreController;
		private readonly ScoreManager scoreManager;
		private readonly ComboController comboController;

		private Dictionary<CutScoreBuffer, int> CutScoreBufferNoteCount;

		private static readonly Func<int, int> MultiplierAtNoteCount = noteCount => noteCount > 13 ? OptimiseGetMultiplier() : (noteCount > 5 ? 4 : (noteCount > 1 ? 2 : 1));
		private static readonly Func<int, int> MultiplierAtMax = noteCount => 8;
		private static Func<int, int> GetMultiplier = x => 69; // Assign something random to stop compiler complaining ¯\_(ツ)_/¯
		private static int OptimiseGetMultiplier() { GetMultiplier = MultiplierAtMax; return 8; }

		private int noteCount;

		private PlayerLevelStatsData GetPlayerLevelStatsData(PlayerDataModel playerDataModel, BeatmapKey beatmap) => playerDataModel.playerData.TryGetPlayerLevelStatsData(beatmap);

		public ScoreTracker(SiraLog logger, [InjectOptional] ScoreController scoreController, [InjectOptional] ComboController comboController, ScoreManager scoreManager)
		{
			this.logger = logger;
			this.scoreManager = scoreManager;
			this.scoreController = scoreController;
			this.comboController = comboController;

			CutScoreBufferNoteCount = new Dictionary<CutScoreBuffer, int>();
			noteCount = 0;
		}

		private void ComboController_comboBreakingEventHappenedEvent() => scoreManager.BreakCombo();

		public void Initialize()
		{
			// Do not initialize if one of those are null
			if (playerDataModel == null || sceneSetupData == null || scoreManager == null)
				return;

			// Reset ScoreManager at level start
			PlayerLevelStatsData stats = GetPlayerLevelStatsData(playerDataModel, sceneSetupData.beatmapKey);
			if (stats == null || sceneSetupData.transformedBeatmapData == null)
				return;
			scoreManager.ResetScoreManager(stats, sceneSetupData.transformedBeatmapData, sceneSetupData.colorScheme);

			// Set function for multiplier according to setting
			GetMultiplier = PluginConfig.Instance.IgnoreMultiplier ? MultiplierAtMax : MultiplierAtNoteCount;

			// Assign events
			if (scoreController != null)
				scoreController.scoringForNoteStartedEvent += ScoreController_scoringForNoteStartedEvent;
			if (comboController != null)
				comboController.comboBreakingEventHappenedEvent += ComboController_comboBreakingEventHappenedEvent;
		}

		public void Dispose()
		{
			// Unassign events
			if (scoreController != null)
				scoreController.scoringForNoteStartedEvent -= ScoreController_scoringForNoteStartedEvent;
			if (comboController != null)
				comboController.comboBreakingEventHappenedEvent -= ComboController_comboBreakingEventHappenedEvent;
		}

		private void ScoreController_scoringForNoteStartedEvent(ScoringElement scoringElement)
		{
			// Ignore bombs
			if (IsBomb(scoringElement))
				return;
			
			// And ignore bad cuts. But do count them for proper application of the multiplier
			noteCount++;
			//logger.Notice($"noteCount[{noteCount}]");
			if (scoringElement is GoodCutScoringElement goodCutScoringElement)
			{
				// Track cut data
				CutScoreBuffer cutScoreBuffer = (CutScoreBuffer)goodCutScoringElement.cutScoreBuffer;

				// Add provisional score assuming it'll be a full swing to make it feel more responsive even though it may be temporarily incorrect
				if (goodCutScoringElement.isFinished)
					scoreManager.AddScore(goodCutScoringElement.noteData.colorType, cutScoreBuffer.cutScore, goodCutScoringElement.maxPossibleCutScore, GetMultiplier(noteCount));
				else
					scoreManager.AddScore(goodCutScoringElement.noteData.colorType, MaxPotentialScore(cutScoreBuffer), goodCutScoringElement.maxPossibleCutScore, GetMultiplier(noteCount));

				if (!cutScoreBuffer.isFinished) 
				{
					CutScoreBufferNoteCount.Add(cutScoreBuffer, noteCount);
					goodCutScoringElement.cutScoreBuffer.RegisterDidChangeReceiver(this);
					goodCutScoringElement.cutScoreBuffer.RegisterDidFinishReceiver(this);
				}
			}
		}

		public void HandleCutScoreBufferDidChange(CutScoreBuffer cutScoreBuffer)
		{
			if (!IsCutMaxPotentialScore(cutScoreBuffer))
				return;

			// Score has already been added during the scoringForNoteStartedEvent. So no further action is required for this note.
			cutScoreBuffer.UnregisterDidChangeReceiver(this);
			cutScoreBuffer.UnregisterDidFinishReceiver(this);

			if (CutScoreBufferNoteCount.ContainsKey(cutScoreBuffer))
				CutScoreBufferNoteCount.Remove(cutScoreBuffer);
		}

		public void HandleCutScoreBufferDidFinish(CutScoreBuffer cutScoreBuffer)
		{
			if (CutScoreBufferNoteCount.TryGetValue(cutScoreBuffer, out int noteCount))
			{
				int diffAngleCutScore;
				if ((diffAngleCutScore = DifferenceFromProvisionalScore(cutScoreBuffer)) > 0)
					scoreManager.SubtractScore(GetColorType(cutScoreBuffer), diffAngleCutScore, GetMultiplier(noteCount));

				CutScoreBufferNoteCount.Remove(cutScoreBuffer);
			}
			else
				logger.Error("HandleCutScoreBufferDidChange: Unable to get noteCount from CutScoreBufferNoteCount!");
			cutScoreBuffer.UnregisterDidChangeReceiver(this);
			cutScoreBuffer.UnregisterDidFinishReceiver(this);
		}

		private int DifferenceFromProvisionalScore(CutScoreBuffer cutScoreBuffer)
		{
			int maxAngleCutScore = cutScoreBuffer.noteScoreDefinition.maxBeforeCutScore + cutScoreBuffer.noteScoreDefinition.maxAfterCutScore;
			int ratingAngleCutScore = cutScoreBuffer.beforeCutScore + cutScoreBuffer.afterCutScore;

			return maxAngleCutScore - ratingAngleCutScore;
		}

		private ColorType GetColorType(CutScoreBuffer cutScoreBuffer) => cutScoreBuffer.noteCutInfo.noteData.colorType;

		private bool IsCutMaxPotentialScore(CutScoreBuffer cutScoreBuffer) => cutScoreBuffer.cutScore == MaxPotentialScore(cutScoreBuffer);
		private int MaxPotentialScore(CutScoreBuffer cutScoreBuffer) => cutScoreBuffer.maxPossibleCutScore - MissedCenterDistanceCutScore(cutScoreBuffer);
		private int MissedCenterDistanceCutScore(CutScoreBuffer cutScoreBuffer) => cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore - cutScoreBuffer.centerDistanceCutScore;

		private bool IsBomb(ScoringElement scoringElement) => IsBomb(scoringElement.noteData);
		private bool IsBomb(NoteData noteData) => noteData.gameplayType == GameplayType.Bomb;
	}
}
