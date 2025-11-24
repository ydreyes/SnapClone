using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
	public Image icon;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI statusText;
	public Button claimButton;

	private int index;
	private RewardData data;
	private ProgressionController controller;

	public void Setup(RewardData d, int idx, ProgressionController ctrl)
	{
		data = d;
		index = idx;
		controller = ctrl;

		nameText.text = d.rewardName;

		if (d.type == RewardType.Card)
			icon.sprite = d.specificCard != null ? d.specificCard.artwork : d.icon;

		bool unlocked = controller.IsRewardUnlocked(idx);
		bool claimed  = controller.IsRewardClaimed(idx);

		if (claimed)
		{
			statusText.text = "Reclamado";
			statusText.color = Color.green;
			claimButton.gameObject.SetActive(false);
		}
		else if (!unlocked)
		{
			statusText.text = "Bloqueado";
			statusText.color = Color.red;
			claimButton.gameObject.SetActive(false);
		}
		else
		{
			statusText.text = "Disponible!";
			statusText.color = Color.yellow;
			claimButton.gameObject.SetActive(true);

			claimButton.onClick.RemoveAllListeners();
			claimButton.onClick.AddListener(() => controller.ClaimReward(idx));
		}
	}
}

