using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITemplate : MonoBehaviour {
	public virtual void SetData(string name, string status) {}
}
public class UITemplateList : MonoBehaviour
{
	[SerializeField] UITemplate prefab;
	List<string> usernames = new List<string>();
	List<UITemplate> profiles = new List<UITemplate>();
	[SerializeField] Vector3 offset = Vector3.down * 55f;

    public void CreateProfile(string data) {
		string test = data.Substring(data.IndexOf('$') + 1);
		//add it
		usernames.Add(test);
		int index = profiles.Count;
		profiles.Add(Instantiate(prefab, transform));
		((RectTransform)profiles[index].transform).localPosition = offset * (index + 0.5f);
		profiles[index].SetData(test, data.Substring(0, data.IndexOf('$')));
	}

	public void Clear() {
		usernames.Clear();
		while (profiles.Count > 0) {
			Destroy(profiles[0].gameObject);
			profiles.RemoveAt(0);
		}
	}

	public void RemoveProfile(string data) {
		string test = data.Substring(data.IndexOf('$') + 1);
		if (usernames.Contains(test)) {
			//remove it
			int index = usernames.IndexOf(test);
			usernames.RemoveAt(index);
			Destroy(profiles[index].gameObject);
			profiles.RemoveAt(index);
		}
	}
}
