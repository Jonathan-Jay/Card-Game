using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewCard : MonoBehaviour
{
	[SerializeField]	Vector3 offset = Vector3.up + Vector3.forward * 0.5f;
	[SerializeField]	Camera target;
	[SerializeField]	GameObject image;
	[SerializeField]	float imageOffsetRight;
	[SerializeField]	LayerMask mask;
	[SerializeField]	float maxDist = 15;
	[SerializeField]	Shader unlitShader;

	private void Start() {
		target.SetReplacementShader(unlitShader, "RenderType");

		image.SetActive(false);
		target.enabled = false;
		anchorPosTarget = ((RectTransform)image.transform).anchoredPosition;
		anchorTarget = ((RectTransform)image.transform).anchorMax;
	}

	static Quaternion ninetyX = Quaternion.Euler(90f, 0f, 0f);
	void LateUpdate() {
		bool dirty = true;

		//if holding right click
		if (Input.GetMouseButton(1)) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;
			if (Physics.Raycast(ray, out rayHit, maxDist, mask)) {
				//we got a hit, check if the thing is a card then teleport the camera
				Card cardTest = rayHit.transform.GetComponent<Card>();
				if (cardTest) {
					target.enabled = true;
					//manual position parenting, prob not that bad actually
					target.transform.rotation = rayHit.transform.rotation * ninetyX;
					target.transform.position = rayHit.transform.position + rayHit.transform.rotation * offset;

					//render the image
					if (!image.activeInHierarchy)
						image.SetActive(true);

					//do the left right anchor stuff
					if (Input.mousePosition.x > (Camera.main.pixelWidth * 0.5f)) {
						if (anchorPosTarget.x < 0) {
							anchorTarget = Vector2.up * anchorTarget.y;
							anchorPosTarget = Vector2.right * imageOffsetRight
									+ Vector2.up * anchorPosTarget.y;
						}
					}
					else if (anchorPosTarget.x > 0) {
						anchorTarget = Vector2.right + Vector2.up * anchorTarget.y;
						anchorPosTarget = Vector2.left * imageOffsetRight
								+ Vector2.up * anchorPosTarget.y;
					}

					if (!transitioning)
						StartCoroutine(TransitionCardPreview());

				}
				dirty = false;
			}
		}
		
		if (target.enabled && dirty) {
			target.enabled = false;
			transitioning = false;
		}
	}

	bool transitioning = false;
	Vector2 anchorPosTarget;
	Vector2 anchorTarget;
	[SerializeField]	float transitionSpeed = 1000f;
	IEnumerator TransitionCardPreview() {
		transitioning = true;

		RectTransform rectTrans = (RectTransform)image.transform;
		//if left side
		if (anchorPosTarget.x > 0) {
			rectTrans.anchorMax = rectTrans.anchorMin = Vector2.up * anchorTarget.y;
			rectTrans.anchoredPosition = Vector2.left * imageOffsetRight
					+ Vector2.up * anchorPosTarget.y;
		}
		else {
			rectTrans.anchorMax = rectTrans.anchorMin = Vector2.right + Vector2.up * anchorTarget.y;
			rectTrans.anchoredPosition = Vector2.right * imageOffsetRight
					+ Vector2.up * anchorPosTarget.y;
		}

		while (transitioning) {
			if (rectTrans.anchorMax != anchorTarget)
				rectTrans.anchorMax = rectTrans.anchorMin = Vector2.MoveTowards(
					rectTrans.anchorMax, anchorTarget, transitionSpeed * 0.002f * Time.deltaTime);
			
			if (rectTrans.anchoredPosition != anchorTarget)
				rectTrans.anchoredPosition = Vector2.MoveTowards(
					rectTrans.anchoredPosition, anchorPosTarget, transitionSpeed * Time.deltaTime);
			
			yield return Card.eof;
		}

		//now make it go offscreen?, hardcoded height
		Vector2 exitPos =  Vector2.right * rectTrans.anchoredPosition.x + Vector2.up * 720;
		while (!transitioning) {
			rectTrans.anchoredPosition = Vector2.MoveTowards(
				rectTrans.anchoredPosition, exitPos, transitionSpeed * Time.deltaTime);

			if (rectTrans.anchoredPosition == exitPos)
				break;
			yield return Card.eof;
		}
		if (!transitioning)
			image.SetActive(false);
	}
}
