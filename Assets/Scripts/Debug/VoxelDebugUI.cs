using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoxelDebugUI : MonoBehaviour
{
	public Font textFont;
    public int lineHeight = 20;
    public int spacing = 5;

    private VoxelViewport vp;
    private int currentLine;
	private float lineWidth;

	private Dictionary<ChunkStage, Text> labels;

    // Use this for initialization
    void Start()
    {
		if (textFont == null)
		{
			textFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		var rectT = GetComponent<RectTransform>();
		rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, ((int)ChunkStage.DONE + 1) * (lineHeight + spacing));
		lineWidth = rectT.sizeDelta.x;

		labels = new Dictionary<ChunkStage, Text>();

        for (int i = 0; i <= (int)ChunkStage.DONE; i++)
        {
            AddChunkStageToPanel((ChunkStage)i);
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (vp == null)
		{
			var vpGO = GameObject.Find("Voxel Viewport");

			if (vpGO == null)
				return;

			vp = vpGO.GetComponent<VoxelViewport>();
		}

		var stages = vp.GetChunkStageSnapshot();

		foreach(KeyValuePair<ChunkStage, int> pair in stages)
		{
			labels[pair.Key].text = pair.Key + ": " + pair.Value;
		}
    }

    private void AddChunkStageToPanel(ChunkStage stage)
    {
        var labelGO = new GameObject("" + stage);
        labelGO.transform.SetParent(this.transform);
        labelGO.layer = LayerMask.NameToLayer("UI");

        var text = labelGO.AddComponent<Text>();

        text.text = stage + ": ";
        text.rectTransform.pivot = new Vector2(0, 1);
        text.rectTransform.anchorMin = new Vector2(0, 1);
        text.rectTransform.anchorMax = new Vector2(0, 1);
        text.rectTransform.sizeDelta = new Vector2(lineWidth, lineHeight);
		text.rectTransform.anchoredPosition = new Vector2(0, (lineHeight + spacing) * -currentLine);

		text.color = Color.black;
		text.font = textFont;
		text.fontSize = 13;

		labels[stage] = text;

        currentLine++;
    }
}
