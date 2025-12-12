using Service.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace Presentation
{
    public class ScoreUpdater : ITickable, IInitializable
    {
        private float displayScore = 0;
        private float scoreSpeed;
        private int lastDisplayedScoreInt = -1;

        private readonly TextMeshProUGUI scoreText;
        private readonly IScoreService scoreService;
        private readonly ISettings settings;

        public ScoreUpdater(
            [Inject(Id = "ScoreText")] TextMeshProUGUI scoreText,
            IScoreService scoreService,
            ISettings settings)
        {
            this.scoreText = scoreText;
            this.scoreService = scoreService;
            this.settings = settings;
        }

        public void Tick()
        {
            displayScore = Mathf.Lerp(displayScore, scoreService.Score, scoreSpeed * Time.deltaTime);

            int currentScoreInt = Mathf.RoundToInt(displayScore);
            if (currentScoreInt != lastDisplayedScoreInt)
            {
                scoreText.text = currentScoreInt.ToString();
                lastDisplayedScoreInt = currentScoreInt;
            }
        }

        public void Initialize()
        {
            var score = scoreService.Score;
            scoreText.text = score.ToString();

            scoreSpeed = settings.ScoreSpeed;
            displayScore = score;
            lastDisplayedScoreInt = score;
        }
    }
}