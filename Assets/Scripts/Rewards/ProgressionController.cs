using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionController : MonoBehaviour
{
	public RewardTrack track;
	public Transform content;
	public GameObject rewardItemPrefab;

	public TextMeshProUGUI currentLevelText;

	private PlayerProgress pp;

	void Start()
	{
		pp = PlayerProgress.Instance;

		// Cargar progreso previo
		string saved = PlayerPrefs.GetString("ClaimedRewards", "");
		if (!string.IsNullOrEmpty(saved))
			pp.claimedRewards = new List<int>(Array.ConvertAll(saved.Split(','), int.Parse));

		pp.heroPoints = PlayerPrefs.GetInt("HeroPoints", pp.heroPoints);

		LoadUI();
	}

	void LoadUI()
	{
		foreach (Transform t in content)
			Destroy(t.gameObject);

		int currentLevel = pp.heroPoints;
		currentLevelText.text = currentLevel.ToString();

		for (int i = 0; i < track.trackRewards.Length; i++)
		{
			var go = Instantiate(rewardItemPrefab, content);
			go.GetComponent<RewardItemUI>().Setup(track.trackRewards[i], i, this);
		}
	}

	// ======================================================  
	// === LÓGICA DE PROGRESO  
	// ======================================================  

	public bool IsRewardUnlocked(int index)
	{
		int required = 0;
		for (int i = 0; i <= index; i++)
			required += track.trackRewards[i].costToUnlock;

		return pp.heroPoints >= required;
	}

	public bool IsRewardClaimed(int index)
	{
		return pp.claimedRewards.Contains(index);
	}

	public void ClaimReward(int index)
	{
		if (!IsRewardUnlocked(index)) return;
		if (IsRewardClaimed(index)) return;

		pp.claimedRewards.Add(index);

		RewardData r = track.trackRewards[index];

		switch (r.type)
		{
		case RewardType.Credits:
			pp.heroPoints += r.amount;
			break;

		case RewardType.Boosters:
			// Aquí implementas tu lógica de boosters si lo deseas
			break;

		case RewardType.Card:
			GiveCardReward(r);
			break;
		}

		SaveProgress();

		LoadUI();
	}
	
	void GiveCardReward(RewardData reward)
	{
		var pp = PlayerProgress.Instance;

		// Caso A: Carta específica definida
		if (reward.specificCard != null)
		{
			pp.AddCardToCollection(reward.specificCard);
			Debug.Log("Carta obtenida: " + reward.specificCard.cardName);
			return;
		}

		// Caso B: Carta aleatoria
		if (reward.giveRandomCard)
		{
			// Busca cartas que aún no tenga
			var allCards = Resources.LoadAll<CardData>("Cards");
			var unowned = allCards.Where(c => !pp.ownedCards.Contains(c)).ToList();

			if (unowned.Count > 0)
			{
				var randomCard = unowned[UnityEngine.Random.Range(0, unowned.Count)];
				pp.AddCardToCollection(randomCard);
				Debug.Log("Carta aleatoria obtenida: " + randomCard.cardName);
			}
			else
			{
				Debug.Log("No hay cartas nuevas para asignar.");
			}
		}
	}
	
	void SaveProgress()
	{
		// Por ahora solo PlayerPrefs de los índices reclamados
		string data = string.Join(",", pp.claimedRewards);
		PlayerPrefs.SetString("ClaimedRewards", data);
		PlayerPrefs.SetInt("HeroPoints", pp.heroPoints);
		PlayerPrefs.Save();
	}

}
