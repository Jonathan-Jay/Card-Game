using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITemplate : MonoBehaviour {
	public virtual void SetData(string name, int id, string status) {}
}
public class UITemplateList : MonoBehaviour
{
	[SerializeField] UITemplate prefab;
	[SerializeField] Transform listParent;
	List<int> ids = new List<int>();
	List<UITemplate> profiles = new List<UITemplate>();
	//[SerializeField] Vector3 offset = Vector3.down * 55f;

    public void CreateProfile(string data) {
		//replace the / with a spliter
		//format is status/id/name
		int firstSplit = data.IndexOf(Client.spliter);

		//this is id/name
		string name = data.Substring(firstSplit + 1);
		
		//get second spliter
		int secondSplit = name.IndexOf(Client.spliter);
		//extract index from name
		int id = int.Parse(name.Substring(0, secondSplit));
		//now properly get name
		name = name.Substring(secondSplit + 1);
		
		ids.Add(id);

		//position index
		int index = profiles.Count;
		profiles.Add(Instantiate(prefab, listParent));
		//offset is now handled by scrollrect
		//((RectTransform)profiles[index].transform).localPosition = offset * (index + 0.5f);

		//dont need to duplicate firstSplit
		profiles[index].SetData(name, id, data.Substring(0, firstSplit));
	}

	public void Clear() {
		ids.Clear();
		while (profiles.Count > 0) {
			Destroy(profiles[0].gameObject);
			profiles.RemoveAt(0);
		}
	}

	//doesnt really matter, might delete
	// public void RemoveProfile(string data) {
	// 	string test = data.Substring(data.IndexOf(Client.spliter) + 1);
	// 	if (usernames.Contains(test)) {
	// 		//remove it
	// 		int index = usernames.IndexOf(test);
	// 		usernames.RemoveAt(index);
	// 		Destroy(profiles[index].gameObject);
	// 		profiles.RemoveAt(index);
	// 	}
	// }
}
